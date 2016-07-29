using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Enrichable.LibOwin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enrichable
{
    public static class ReverseProxyOwinContextExtensions
    {
        public static HttpRequestMessage GetHttpRequestMessage(this IDictionary<string,object> environment, string rootUrl)
        {
            var owinContext = new OwinContext(environment);

            var method = owinContext.Request.Method;
            var requestPath = owinContext.Request.Path;
            var queryString = owinContext.Request.QueryString;
            var acceptHeader = owinContext.Request.Accept ?? "application/json";

            var msg = new HttpRequestMessage();
            msg.Method = new HttpMethod(method);
            msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));

            // Build the new Uri
            msg.RequestUri = new Uri(rootUrl + requestPath + queryString);

            return msg;
        }

        public static void SetHttpResponse(this IDictionary<string, object> environment,
            HttpResponseMessage responseMessage)
        {
            var owinContext = new OwinContext(environment);

            owinContext.Response.StatusCode = (int)responseMessage.StatusCode;
            owinContext.Response.ReasonPhrase = responseMessage.ReasonPhrase;
            foreach (var header in responseMessage.Headers)
            {
                owinContext.Response.Headers.Add(header.Key, header.Value.ToArray());
            }
        }

        public static async Task<JObject> GetRequestBodyAsJsonAsync(this IDictionary<string, object> environment)
        {
            var owinContext = new OwinContext(environment);
            return await ParseAsJsonAsync(owinContext.Request.Body);
        }

        private static async Task<JObject> ParseAsJsonAsync(Stream inStream)
        {
            if (inStream == null)
                return null;

            var tmpStream = new MemoryStream();
            // Get a copy of the stream asynchronously so that we don't hold up this thread
            await inStream.CopyToAsync(tmpStream);

            JObject jsonBody = null;
            if (tmpStream.Length > 0)
            {
                tmpStream.Position = 0;
                // Parse the body as Json. Maybe we could support other formats as well?
                using (var reader = new StreamReader(tmpStream))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    jsonBody = (JObject)JToken.ReadFrom(jsonReader);
                }
            }
            return jsonBody;
        }
        internal static string BuildQueryString(IDictionary<string, IList<string>> query)
        {
            if (query == null || !query.Any())
                return string.Empty;

            var queryBuilder = new StringBuilder("?");
            bool first = true;
            foreach (var q in query)
            {
                foreach (var val in q.Value)
                {
                    if (!first)
                    {
                        queryBuilder.Append("&");
                    }
                    queryBuilder.AppendFormat("{0}={1}", q.Key, val);
                    first = false;
                }
            }
            return queryBuilder.ToString();
        }

    }
}