using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace Telemachus
{
    /// <summary>
    /// Handles REST-style routes: GET/POST /api/{key} and GET/POST /api/{key}[arg1,arg2]
    /// Delegates to the same KSP API dispatch as DataLinkResponsibility.
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

            // Append bracket args from query string if present: ?args=1200.0 → key[1200.0]
            var query = request.Url.Query;
            if (!string.IsNullOrEmpty(query) && query.StartsWith("?"))
            {
                var qArgs = query.Substring(1);
                if (qArgs.Length > 0)
                    apiString += "[" + Uri.UnescapeDataString(qArgs) + "]";
            }

            dataRates.RecieveDataFromClient(Convert.ToInt32(
                (long)request.RawUrl.Length + request.ContentLength64));

            var results = new Dictionary<string, object>();

            try
            {
                results[apiString.Split('[')[0]] = kspAPI.ProcessAPIString(apiString);
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
    }
}
