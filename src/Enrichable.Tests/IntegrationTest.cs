using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Owin;
using Xunit;

namespace Enrichable.Tests
{
    public class TestEnricher : IHalResourceEnricher
    {
        List<JObject> _resources = new List<JObject>();

        public void Analyze(JObject resource, string rel)
        {
            _resources.Add(resource);
        }

        public void Commit()
        {
            foreach (var resource in _resources)
            {
                resource.Add("test", "test");
            }
        }
    }

    public class EnrichTest
    {
        [Fact]
        public void Test()
        {
            var root = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("samples\\embedded-sample.json"));
            var registry = new HalResourceEnricherRegistry();
            registry.RegisterEnricher<TestEnricher>("order");
            var target = new Enrichable(registry, Activator.CreateInstance);
            target.Enrich(root);

            Assert.Equal("test", root.SelectToken("_embedded.order.test"));
        }
    }

    public class IntegrationTest
    {
        [Fact]
        public async void Test()
        {
            using (var server = TestServer.Create(app =>
            {
                app.Run(async ow =>
                {
                    var req = ow.Environment.GetHttpRequestMessage("http://dummy:1234/");

                    using (var httpClient = new HttpClient(new TestHttpMessageHandler()))
                    {
                        var resp = await httpClient.SendAsync(req);
                        ow.Environment.SetHttpResponse(resp);
                        var responseStream = await resp.Content.ReadAsStreamAsync();
                        if (responseStream != null)
                        {
                            await responseStream.CopyToAsync(ow.Response.Body);
                        }
                    }
                });
                /*app.Use<ReverseProxyInterceptComponent>();
                app.Use<ReverseProxyDispatchComponent>(new TestHttpMessageHandler());
                app.Use<ReverseProxyResponseComponent>();*/
            }))
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync("/test");
                
                var responseText = await response.Content.ReadAsStringAsync();
                // TODO: Validate response
                Assert.Equal("{}", responseText);
            }
        }
    }
}