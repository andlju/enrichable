using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
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

    public class IntegrationTestBase : IDisposable
    {
        // Serving static files
        protected TestServer BackendServer;

        public IntegrationTestBase()
        {
            var ctp = new FileExtensionContentTypeProvider();
            ctp.Mappings.Add(".json", "application/json");
            BackendServer = TestServer.Create(backendapp =>
            {
                // Setup the "backend" server to serve static files from the samples folder
                backendapp.UseStaticFiles(new StaticFileOptions()
                {
                    RequestPath = new PathString("/server"),
                    ContentTypeProvider = ctp,
                    FileSystem = new PhysicalFileSystem(@".\samples")
                });
            });
        }

        public void Dispose()
        {
            BackendServer.Dispose();
        }
    }

    public class IntegrationTest :IntegrationTestBase
    {
        public IntegrationTest()
        {
        }

        [Fact]
        public async void Test()
        {
            using (var server = TestServer.Create(app =>
            {
                app.Map("/proxy", a => a.Run(
                    async ow =>
                    {
                        var req = ow.Environment.GetHttpRequestMessage("http://" + ow.Request.Host + "/server");

                        using (var httpClient = BackendServer.HttpClient)
                        {
                            var resp = await httpClient.SendAsync(req);
                            ow.Environment.SetHttpResponse(resp);
                            var responseStream = await resp.Content.ReadAsStreamAsync();
                            if (responseStream != null)
                            {
                                await responseStream.CopyToAsync(ow.Response.Body);
                            }
                        }
                    }));

                /*app.Use<ReverseProxyInterceptComponent>();
                app.Use<ReverseProxyDispatchComponent>(new TestHttpMessageHandler());
                app.Use<ReverseProxyResponseComponent>();*/
            }))
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync("/proxy/embedded-sample.json");

                var responseText = await response.Content.ReadAsStringAsync();
                var responseObj = JsonConvert.DeserializeObject<JObject>(responseText);

                // TODO: Validate response
                Assert.Equal(30, responseObj.SelectToken("_embedded.order.total").Value<decimal>());
            }
        }
    }
}