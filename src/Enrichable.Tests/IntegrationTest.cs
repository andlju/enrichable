using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Microsoft.Owin.Testing;
using Nancy.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Owin;
using Xunit;

namespace Enrichable.Tests
{

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
                app.UseNancy(new NancyOptions() { Bootstrapper = new NancyTestBootstrapper(BackendServer.Handler)});
            }))
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync("/proxy/embedded-sample");

                var responseText = await response.Content.ReadAsStringAsync();
                var responseObj = JsonConvert.DeserializeObject<JObject>(responseText);

                // TODO: Validate response
                Assert.Equal(30, responseObj.SelectToken("_embedded.order.total").Value<decimal>());
            }
        }
    }
}