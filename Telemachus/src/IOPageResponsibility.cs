//Author: Richard Bunt

using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp.Net;
using WebSocketSharp;
using KSP.IO;
using System.IO;

namespace Telemachus
{
    class IOPageResponsibility : IHTTPRequestResponder
    {
        #region Constants

        const String PAGE_PREFIX = "/telemachus";

        #endregion

        #region IHTTPRequestResponder

        public bool process(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.RawUrl.StartsWith(PAGE_PREFIX))
            {
                try
                {
                    var requestedFile = request.RawUrl.Substring(PAGE_PREFIX.Length);
                    var contentType = GetContentType(Path.GetExtension(requestedFile));
                    // Set a mime type, if we have one for this extension
                    if (!string.IsNullOrEmpty(contentType.mimeType))
                    {
                        response.ContentType = contentType.mimeType;
                    }

                    // Read the data, and set encoding type if text. Assume that any text encoding is UTF-8 (probably).
                    byte[] contentData = System.IO.File.ReadAllBytes(buildPath(escapeFileName(requestedFile)));
                    if (contentType.contentType == HTMLContentType.TextContent)
                    {
                        response.ContentEncoding = Encoding.UTF8;
                    }
                    // Write out the response to the client
                    response.WriteContent(contentData);

                    return true;
                }
                catch
                {

                }
            }
            return false;
        }

        #endregion

        #region Content Type Determination

        /// Retrieve whether a specific extension is text, binary, and what it's mimetype is.
        private enum HTMLContentType
        {
            TextContent,
            BinaryContent,
        }
        private struct HTMLResponseContentType
        {
            public HTMLContentType contentType;
            public string mimeType;
        }
        private Dictionary<string, HTMLResponseContentType> contentTypes = null;
        private HTMLResponseContentType GetContentType(string extension)
        {
            contentTypes ??= new Dictionary<string, HTMLResponseContentType>
            {
                [".html"]  = new HTMLResponseContentType { contentType = HTMLContentType.TextContent,   mimeType = "text/html" },
                [".css"]   = new HTMLResponseContentType { contentType = HTMLContentType.TextContent,   mimeType = "text/css" },
                [".js"]    = new HTMLResponseContentType { contentType = HTMLContentType.TextContent,   mimeType = "application/x-javascript" },
                [".jpg"]   = new HTMLResponseContentType { contentType = HTMLContentType.BinaryContent, mimeType = "image/jpeg" },
                [".jpeg"]  = new HTMLResponseContentType { contentType = HTMLContentType.BinaryContent, mimeType = "image/jpeg" },
                [".png"]   = new HTMLResponseContentType { contentType = HTMLContentType.BinaryContent, mimeType = "image/png" },
                [".gif"]   = new HTMLResponseContentType { contentType = HTMLContentType.BinaryContent, mimeType = "image/gif" },
                [".svg"]   = new HTMLResponseContentType { contentType = HTMLContentType.BinaryContent, mimeType = "image/svg+xml" },
                [".eot"]   = new HTMLResponseContentType { contentType = HTMLContentType.BinaryContent, mimeType = "application/vnd.ms-fontobject" },
                [".ttf"]   = new HTMLResponseContentType { contentType = HTMLContentType.BinaryContent, mimeType = "application/font-sfnt" },
                [".woff"]  = new HTMLResponseContentType { contentType = HTMLContentType.BinaryContent, mimeType = "application/font-woff" },
                [".otf"]   = new HTMLResponseContentType { contentType = HTMLContentType.BinaryContent, mimeType = "application/font-sfnt" },
                [".mp4"]   = new HTMLResponseContentType { contentType = HTMLContentType.BinaryContent, mimeType = "video/mp4" },
                [".json"]  = new HTMLResponseContentType { contentType = HTMLContentType.TextContent,   mimeType = "application/json" },
                [".txt"]   = new HTMLResponseContentType { contentType = HTMLContentType.TextContent,   mimeType = "text/plain" },
                [""]       = new HTMLResponseContentType { contentType = HTMLContentType.BinaryContent, mimeType = null },
            };

            return contentTypes.TryGetValue(extension, out var ct) ? ct : contentTypes[""];
        }

        #endregion

        #region Methods

        static protected string buildPath(string fileName)
        {
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            const string webFiles = "PluginData/Telemachus/";
            return assemblyPath.Replace("Telemachus.dll", "") + webFiles + fileName;
        }

        static protected string escapeFileName(string fileName)
        {
            return fileName.Replace("..", "");
        }

        #endregion
    }
}
