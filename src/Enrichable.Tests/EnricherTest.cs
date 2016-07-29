using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

    public class EnricherTest
    {
        [Fact]
        public void Test()
        {
            var root = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("samples\\embedded-sample.json"));
            var registry = new HalResourceEnricherRegistry();
            registry.RegisterEnricher<TestEnricher>("order");
            var target = new Enricher(registry, Activator.CreateInstance);
            target.Enrich(root);

            Assert.Equal("test", root.SelectToken("_embedded.order.test"));
        }
    }
}