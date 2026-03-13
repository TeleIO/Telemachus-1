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

        #region Initialisation

        public DataLinkResponsibility(KSPAPIBase kspAPI, UpLinkDownLinkRate rateTracker)
        {
            this.kspAPI = kspAPI;
            dataRates = rateTracker;
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

        public bool process(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (!request.RawUrl.StartsWith(PAGE_PREFIX)) return false;

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
