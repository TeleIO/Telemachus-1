using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace Telemachus
{
    public interface IHTTPRequestResponder
    {
        bool process(HttpListenerRequest request, HttpListenerResponse response);
    }

    class KSPWebServerDispatcher
    {
        private List<IHTTPRequestResponder> responderChain = new List<IHTTPRequestResponder>();

        /// Iterate over all responders to find one that works
        public void DispatchRequest(object sender, HttpRequestEventArgs request)
        {
            ApplyConnectionPolicy(request.Request, request.Response);

            foreach (var responder in responderChain.Reverse<IHTTPRequestResponder>())
            {
                try
                {
                    if (responder.process(request.Request, request.Response))
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    PluginLogger.print("Caught exception in web handlers: " + ex.ToString());
                }
            }
            // If here, we had no responder.
            request.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }

        public void AddResponder(IHTTPRequestResponder responder)
        {
            responderChain.Add(responder);
        }

        private static void ApplyConnectionPolicy(HttpListenerRequest request, HttpListenerResponse response)
        {
            var connectionHeader = request.Headers["Connection"];
            if (connectionHeader == null) return;

            if (connectionHeader.IndexOf("close", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                response.KeepAlive = false;
            }
        }
    }
}
