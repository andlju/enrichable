using System.Collections.Generic;
using System.IO;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Sdk;

namespace Enrichable.Tests
{
    public abstract class with_owin_context
    {
        protected IOwinAsserts Asserts;

        protected readonly OwinContext Context;

        protected with_owin_context(string method = "GET",
            string host = "dummy:1234",
            string path = "/",
            string accept = "application/json")
        {
            Context = CreateOwinContext(method, host, path, accept);
            var body = RequestBody;
            if (body != null)
            {
                Context.Request.Body = GetJsonStream(JObject.FromObject(body));
            }
        }

        protected abstract object RequestBody { get; }

        protected OwinContext CreateOwinContext(
            string method, 
            string host,
            string path,
            string accept)
        {
            var ctxt = new OwinContext();
            ctxt.Request.Method = method;
            ctxt.Request.Host = new HostString("dummy:8080");
            ctxt.Request.Accept = accept;
            ctxt.Request.Path = new PathString(path);

            return ctxt;
        }

        protected static MemoryStream GetJsonStream(JObject obj)
        {
            var bodyStream = new MemoryStream();
            var streamWriter = new StreamWriter(bodyStream);
            var writer = new JsonTextWriter(streamWriter);
            var serializer = new JsonSerializer();
            serializer.Serialize(writer, obj);
            writer.Flush();
            bodyStream.Position = 0;
            return bodyStream;
        }
    }

    public interface IOwinAsserts
    {

    }

    public static class OwinAssertExtensions
    {
        public static void DictionaryMatch<TKey, TValue>(this IOwinAsserts assert, TKey key, TValue expectedValue,
            IDictionary<TKey, TValue> dictionary)
        {
            TValue actualValue;
            if (!dictionary.TryGetValue(key, out actualValue))
            {
                throw new AssertActualExpectedException(key, null, "Key not found in dictionary", "Expected key");
            }
            if (!actualValue.Equals(expectedValue))
            {
                throw new AssertActualExpectedException(expectedValue, actualValue, $"Value of key {key} didn't match.");
            }
        }
    }

}