using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enrichable.LibOwin;

namespace Enrichable
{
    /// <summary>
    ///  Pick up the response parameters and response body from the Owin environment and send it back to the client
    /// </summary>
    public class ReverseProxyResponseComponent
    {
        readonly Func<IDictionary<string, object>, Task> _next;

        public ReverseProxyResponseComponent(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);

            /*
            // Set response status and message
            context.Response.StatusCode = proxyContext.ResponseStatusCode;
            context.Response.ReasonPhrase = proxyContext.ResponseReasonPhrase;
            context.Response.ContentType = proxyContext.ResponseContentType;

            var responseJsonBody = proxyContext.ResponseBodyJson;
            if (responseJsonBody != null)
            {
                // Write the body to the output stream
                using (var writer = new StreamWriter(context.Response.Body))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, responseJsonBody);
                }
            }*/

            await _next.Invoke(env);
        }
    }
}