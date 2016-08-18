using System.Net.Http;
using Microsoft.Owin.Testing;
using Nancy.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Owin;
using Xunit;

namespace Enrichable.Tests
{
    public class when_running_nancy_based_proxy : IntegrationTestBase
    {
        private readonly JObject _response;

        public when_running_nancy_based_proxy()
        {
            var enricherRegistry = new HalResourceEnricherRegistry();
            enricherRegistry.RegisterEnricher(() => new TestEnricher(), "order");

            using (var server = TestServer.Create(app =>
            {
                app.UseNancy(new NancyOptions() { Bootstrapper = new NancyTestBootstrapper(BackendServer.Handler, enricherRegistry) });
            }))
            {
                HttpResponseMessage response = server.HttpClient.GetAsync("/proxy/embedded-sample").Result;

                var responseText = response.Content.ReadAsStringAsync().Result;
                _response = JsonConvert.DeserializeObject<JObject>(responseText);
            }
        }

        [Fact]
        public async void then_json_is_returned()
        {
            Assert.NotNull(_response);
        }

        [Fact]
        public void then_TestEnricher_has_been_run()
        {
            Assert.Equal("test", _response.SelectToken("_embedded.order.test").Value<string>());
        }
    }
}