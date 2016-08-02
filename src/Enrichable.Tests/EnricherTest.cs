using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Enrichable.Tests
{
    public class GlobalEnricher : IHalResourceEnricher
    {
        List<JObject> _resources = new List<JObject>();

        public void Analyze(JObject resource, string rel)
        {
            _resources.Add(resource);
        }

        public void Commit()
        {
            var itemNumber = 1;
            foreach (var resource in _resources)
            {
                resource.Add("global", itemNumber++);
            }
        }
    }

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
            registry.RegisterEnricher<GlobalEnricher>();

            var target = new Enricher(registry, Activator.CreateInstance);
            target.Enrich(root);
            
            Assert.Equal("test", root.SelectToken("_embedded.order.test"));
            Assert.Equal(1, root.SelectToken("_embedded.order.global"));
            Assert.Equal(2, root.SelectToken("_embedded.order._embedded.item[0].global"));
        }
    }
}