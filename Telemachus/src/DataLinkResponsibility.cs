//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Reflection;
using WebSocketSharp.Net;
using WebSocketSharp;

namespace Telemachus
{
    public class DataLinkResponsibility : IHTTPRequestResponder
    {
        /// The page prefix that this class handles
        public const String PAGE_PREFIX = "/telemachus/datalink";
        /// The KSP API to use to access variable data
        private KSPAPIBase kspAPI = null;

        private UpLinkDownLinkRate dataRates = null;

        /// Optional; CORS headers aren't emitted when null
        private readonly ServerConfiguration serverConfig;

        #region Initialisation

        public DataLinkResponsibility(KSPAPIBase kspAPI, UpLinkDownLinkRate rateTracker, ServerConfiguration serverConfig = null)
        {
            this.kspAPI = kspAPI;
            dataRates = rateTracker;
            this.serverConfig = serverConfig;
        }

        #endregion

        private static Dictionary<string, object> splitArguments(string argstring,
            out string globalScale, out int globalPrecision, out bool globalInt)
        {
            globalScale = null;
            globalPrecision = -1;
            globalInt = false;

            var ret = new Dictionary<string, object>();
            if (argstring.StartsWith("?")) argstring = argstring[1..];

            foreach (var part in argstring.Split('&'))
            {
                var subParts = part.Split('=');
                if (subParts.Length != 2) continue;
                var keyName = Uri.UnescapeDataString(subParts[0]);
                var apiName = Uri.UnescapeDataString(subParts[1]);

                // Reserved global params
                switch (keyName)
                {
                    case "_scale": globalScale = apiName; continue;
                    case "_precision":
                        int.TryParse(apiName, out globalPrecision);
                        continue;
                    case "_int":
                        globalInt = string.Equals(apiName, "true", StringComparison.OrdinalIgnoreCase);
                        continue;
                }

                ret[keyName] = apiName;
            }
            return ret;
        }

        private static IDictionary<string, object> parseJSONBody(string jsonBody)
        {
            return Json.DecodeObject(jsonBody);
        }

        private bool corsConfigured =>
            serverConfig != null && serverConfig.AllowedOrigins != null && serverConfig.AllowedOrigins.Count > 0;

        // Whenever CORS is configured the response depends on Origin — emit Vary even on non-matches
        // so shared caches don't serve a no-CORS response to a request that would have matched.
        private void applyCorsHeader(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (!corsConfigured) return;
            response.Headers["Vary"] = "Origin";

            string origin = request.Headers["Origin"];
            if (string.IsNullOrEmpty(origin)) return;
            // Defence in depth: reject any Origin containing CR/LF before echoing it into a header.
            if (origin.IndexOfAny(new[] { '\r', '\n' }) >= 0) return;
            string normalised = origin.TrimEnd('/');
            if (!serverConfig.AllowedOrigins.Contains(normalised)) return;
            response.Headers["Access-Control-Allow-Origin"] = normalised;
        }

        public bool process(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (!request.RawUrl.StartsWith(PAGE_PREFIX)) return false;

            // CORS preflight: when CORS is configured, OPTIONS never goes through the data pipeline.
            // 204 with preflight headers if the Origin is allowlisted; 204 without headers otherwise,
            // rather than serialising an empty JSON body. When CORS is not configured we leave OPTIONS
            // behaviour unchanged from before this PR (falls through to the data pipeline below).
            if (corsConfigured
                && string.Equals(request.HttpMethod, "OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                applyCorsHeader(request, response);
                if (response.Headers["Access-Control-Allow-Origin"] != null)
                {
                    response.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";
                    response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
                    response.Headers["Access-Control-Max-Age"] = "86400";
                }
                response.StatusCode = 204;
                return true;
            }

            applyCorsHeader(request, response);

            // Work out how big this request was
            long byteCount = request.RawUrl.Length + request.ContentLength64;
            // Don't count headers + request.Headers.AllKeys.Sum(x => x.Length + request.Headers[x].Length + 1);
            dataRates.RecieveDataFromClient(Convert.ToInt32(byteCount));

            string globalScale = null;
            int globalPrecision = -1;
            bool globalInt = false;

            IDictionary<string, object> apiRequests;
            if (request.HttpMethod.ToUpper() == "POST" && request.HasEntityBody)
            {
                using var streamReader = new System.IO.StreamReader(request.InputStream);
                apiRequests = parseJSONBody(streamReader.ReadToEnd());
            }
            else
            {
                apiRequests = splitArguments(request.Url.Query,
                    out globalScale, out globalPrecision, out globalInt);
            }

            var results = new Dictionary<string, object>();
            var unknowns = new List<string>();
            var errors = new Dictionary<string, string>();

            foreach (var name in apiRequests.Keys)
            {
                try
                {
                    var raw = apiRequests[name].ToString();

                    // Parse per-key pipe modifiers, falling back to global defaults
                    var apiString = ApiScaling.ParseModifiers(raw,
                        out string scale, out int precision, out bool asInt,
                        globalScale, globalPrecision, globalInt);

                    // Apply input scaling to bracket args
                    apiString = ApiScaling.ApplyInputScaling(apiString, scale);

                    var result = kspAPI.ProcessAPIString(apiString);

                    // Apply output precision/integer scaling
                    if (precision >= 0)
                        result = ApiScaling.ApplyOutputScaling(result, precision, asInt);

                    results[name] = result;
                }
                catch (KSPAPIBase.UnknownAPIException)
                {
                    unknowns.Add(apiRequests[name].ToString());
                }
                catch (Exception ex)
                {
                    errors[apiRequests[name].ToString()] = ex.ToString();
                }
            }
            // If we had any unrecognised API keys, let the user know
            if (unknowns.Count > 0) results["unknown"] = unknowns;
            if (errors.Count > 0) results["errors"] = errors;

            // Now, serialize the dictionary and write to the response
            var returnData = Encoding.UTF8.GetBytes(Json.Encode(results));
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "application/json";
            response.WriteContent(returnData);
            dataRates.SendDataToClient(returnData.Length);
            return true;
        }
    }

    public class DataSources
    {
        #region Fields

        public Vessel vessel;
        public List<String> args = new();
        protected string varName;
        #endregion

        public DataSources Clone()
        {
            var d = new DataSources();
            d.vessel = this.vessel;
            d.args = new List<string>(this.args);
            return d;
        }
    }
}
