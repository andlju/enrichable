using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Nancy;
using Nancy.Conventions;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enrichable.Tests
{
    public class NancyTestBootstrapper : DefaultNancyBootstrapper
    {
        private readonly HttpMessageHandler _proxyMessageHandler;
        private readonly HalResourceEnricherRegistry _enricherRegistry;

        public NancyTestBootstrapper(HttpMessageHandler proxyMessageHandler, HalResourceEnricherRegistry enricherRegistry)
        {
            _proxyMessageHandler = proxyMessageHandler;
            _enricherRegistry = enricherRegistry;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register<HttpMessageHandler>(_proxyMessageHandler);
            container.Register<Enricher>((c, o) => new Enricher(c.Resolve<HalResourceEnricherRegistry>(), c.Resolve));
            container.Register<HalResourceEnricherRegistry>(_enricherRegistry);
        }
    }

    public class IntegrationTestModule : NancyModule
    {
        public IntegrationTestModule(HttpMessageHandler httpMessageHandler, Enricher enricher)
        {
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
                        enricher.Enrich(jsonBody);

                        var response = (Response)jsonBody.ToString();
                        response.ContentType = "application/json";
                        return response;
                    }
                }
            };
        }
    }
}