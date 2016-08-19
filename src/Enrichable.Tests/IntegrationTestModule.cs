using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Nancy;
using Nancy.Conventions;
using Nancy.Owin;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enrichable.Tests
{
    public class NancyTestBootstrapper : DefaultNancyBootstrapper
    {
        private readonly HttpMessageHandler _proxyMessageHandler;

        public NancyTestBootstrapper(HttpMessageHandler proxyMessageHandler)
        {
            _proxyMessageHandler = proxyMessageHandler;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register<HttpMessageHandler>(_proxyMessageHandler);
        }
    }

    public class IntegrationTestModule : NancyModule
    {
        public IntegrationTestModule(HttpMessageHandler httpMessageHandler)
        {
            var enricher = new Enrichment();
            enricher.RegisterEnricher((env) => new TestEnricher(), "order");

            Get["/proxy/{url*}", true] = async (pars, token) =>
            {
                using (var client = new HttpClient(httpMessageHandler))
                {
                    var req = new HttpRequestMessage(HttpMethod.Get,
                        "http://localhost/server/" + (string) pars.url + ".json");
                    var resp = await client.SendAsync(req);
                    using (var bodyStream = await resp.Content.ReadAsStreamAsync())
                    {
                        var jsonBody = await bodyStream.ReadAsJsonAsync();
                        enricher.Enrich(jsonBody, Context.GetOwinEnvironment());

                        var response = (Response)jsonBody.ToString();
                        response.ContentType = "application/json";
                        return response;
                    }
                }
            };
        }
    }
}