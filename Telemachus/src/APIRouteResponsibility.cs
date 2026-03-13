using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace Telemachus
{
    /// <summary>
    /// Handles REST-style routes: GET/POST /api/{key} and GET/POST /api/{key}[arg1,arg2]
    /// Delegates to the same KSP API dispatch as DataLinkResponsibility.
    ///
    /// Supports optional scaling query parameters for embedded controllers:
    ///   ?scale=0,1024       Map input integer range to the API's expected 0.0–1.0
    ///   ?precision=2         Round numeric output to N decimal places
    ///   ?int=true            Return output multiplied by 10^precision as an integer
    /// Examples:
    ///   /api/f.setThrottle?args=512&amp;scale=0,1024   → throttle set to 0.5
    ///   /api/v.altitude?precision=2                → 14257.83
    ///   /api/v.altitude?precision=2&amp;int=true       → 1425783
    /// </summary>
    public class APIRouteResponsibility : IHTTPRequestResponder
    {
        public const string PAGE_PREFIX = "/api/";

        private readonly KSPAPIBase kspAPI;
        private readonly UpLinkDownLinkRate dataRates;

        public APIRouteResponsibility(KSPAPIBase kspAPI, UpLinkDownLinkRate rateTracker)
        {
            this.kspAPI = kspAPI;
            this.dataRates = rateTracker;
        }

        public bool process(HttpListenerRequest request, HttpListenerResponse response)
        {
            var path = request.Url.AbsolutePath;
            if (!path.StartsWith(PAGE_PREFIX) || path.Length <= PAGE_PREFIX.Length)
                return false;

            // Extract the API key from the path: /api/v.altitude → v.altitude
            var apiString = Uri.UnescapeDataString(path.Substring(PAGE_PREFIX.Length));

            // Parse query parameters
            var queryParams = ParseQuery(request.Url.Query);

            // Append bracket args from query string if present: ?args=512 → key[512]
            var args = queryParams.Get("args");
            if (args != null)
            {
                // Apply input scaling: map integer range to 0.0–1.0
                var scale = queryParams.Get("scale");
                if (scale != null)
                    args = ApiScaling.ScaleInput(args, scale);

                apiString += "[" + args + "]";
            }

            dataRates.RecieveDataFromClient(Convert.ToInt32(
                (long)request.RawUrl.Length + request.ContentLength64));

            var results = new Dictionary<string, object>();

            try
            {
                var result = kspAPI.ProcessAPIString(apiString);

                // Apply output precision/integer scaling
                var precisionStr = queryParams.Get("precision");
                if (precisionStr != null && int.TryParse(precisionStr, out int precision))
                {
                    bool asInt = string.Equals(queryParams.Get("int"), "true", StringComparison.OrdinalIgnoreCase);
                    result = ApiScaling.ApplyOutputScaling(result, precision, asInt);
                }

                results[apiString.Split('[')[0]] = result;
            }
            catch (KSPAPIBase.UnknownAPIException)
            {
                results["error"] = "Unknown API key: " + apiString;
                response.StatusCode = 404;
            }
            catch (Exception ex)
            {
                results["error"] = ex.Message;
                response.StatusCode = 500;
            }

            var returnData = Encoding.UTF8.GetBytes(Json.Encode(results));
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "application/json";
            response.WriteContent(returnData);
            dataRates.SendDataToClient(returnData.Length);
            return true;
        }

        private static NameValueCollection ParseQuery(string query)
        {
            var nvc = new NameValueCollection();
            if (string.IsNullOrEmpty(query)) return nvc;
            if (query.StartsWith("?")) query = query.Substring(1);

            foreach (var part in query.Split('&'))
            {
                var eq = part.IndexOf('=');
                if (eq < 0) { nvc.Add(Uri.UnescapeDataString(part), ""); continue; }
                nvc.Add(Uri.UnescapeDataString(part.Substring(0, eq)),
                         Uri.UnescapeDataString(part.Substring(eq + 1)));
            }
            return nvc;
        }
    }
}
