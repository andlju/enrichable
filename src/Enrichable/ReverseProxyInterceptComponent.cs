using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enrichable.LibOwin;

namespace Enrichable
{
    /// <summary>
    /// Intercept all query parameters, parse the json body (if any) and store in the Owin Environment
    /// for use by other Middleware
    /// </summary>
    public class ReverseProxyInterceptComponent
    {
        readonly Func<IDictionary<string, object>, Task> _next;

        public ReverseProxyInterceptComponent(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);

            // var req = context.GetHttpRequestMessage("http://dummy:1234/");

            await _next.Invoke(env);
        }
    }
}