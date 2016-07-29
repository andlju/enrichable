using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Enrichable
{
    /// <summary>
    /// Pick up path, query and any body and send it to the downstream server. Store the response in the Owin environment
    /// so that other middleware can change it
    /// </summary>
    public class ReverseProxyDispatchComponent
    {
        readonly Func<IDictionary<string, object>, Task> _next;
        private readonly string _bodyEnvironmentKey;
        private readonly HttpMessageHandler _messageHandler;
        private readonly string _providerRootUrl = "http://dummy:1234/";

        public ReverseProxyDispatchComponent(Func<IDictionary<string, object>, Task> next, string bodyEnvironmentKey, HttpMessageHandler messageHandler)
        {
            _next = next;
            _bodyEnvironmentKey = bodyEnvironmentKey;
            _messageHandler = messageHandler;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var request = env.GetHttpRequestMessage(_providerRootUrl);

            var requestJsonBody = await env.GetRequestBodyAsJsonAsync();
            var outStream = new MemoryStream();
            if (requestJsonBody != null)
            {
                var serializer = new JsonSerializer(); // TODO Make serializer configurable
                using (var writer = new JsonTextWriter(new StreamWriter(outStream)))
                {
                    serializer.Serialize(writer, requestJsonBody);
                }
            }

            using (var client = new HttpClient(_messageHandler))
            {
                if (outStream?.Length > 0)
                {
                    request.Content = new StreamContent(outStream);
                }

                // Wait for the response
                var response = await client.SendAsync(request);
                var responseStream = await response.Content.ReadAsStreamAsync();
                env[_bodyEnvironmentKey] = await responseStream.ReadAsJsonAsync();
                /*proxyContext.ResponseStatusCode = (int)response.StatusCode;
                proxyContext.ResponseReasonPhrase = response.ReasonPhrase;

                var content = response.Content;
                if (content != null)
                {
                    var contentStream = await content.ReadAsStreamAsync();
                    if (contentStream != null)
                    {
                        // Parse the body as Json. Maybe we could support other formats as well?
                        using (var reader = new StreamReader(contentStream))
                        using (var jsonReader = new JsonTextReader(reader))
                        {
                            var responseJsonBody = (JObject)JToken.ReadFrom(jsonReader);

                            // Store the parsed body in the context for other middleware to pick up
                            proxyContext.ResponseBodyJson = responseJsonBody;
                        }
                    }
                    proxyContext.ResponseContentType = response.Content.Headers.ContentType.ToString();
                }
                */
            }

            await _next.Invoke(env);
        }
    }
}