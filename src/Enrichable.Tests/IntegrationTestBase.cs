using System;
using System.Collections.Generic;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Microsoft.Owin.Testing;
using Owin;

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
}