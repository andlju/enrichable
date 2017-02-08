using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Enrichable.Tests
{
    public class GlobalEnricher : IResourceEnricher
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

    public class TestEnricher : IResourceEnricher
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

    public class DefaultFactoryEnricherTest : EnricherTestBase
    {
        public DefaultFactoryEnricherTest()
        {
            Target = new Enrichment();
            Target.RegisterEnricher((env) => new TestEnricher(), "order");
            Target.RegisterEnricher((env) => new GlobalEnricher());
        }
    }

    public class CustomFactoryEnricherTest : EnricherTestBase
    {
        public CustomFactoryEnricherTest()
        {
            Target = new Enrichment(EnricherFactory);
            Target.RegisterEnricher<TestEnricher>( "order");
            Target.RegisterEnricher<GlobalEnricher>();
        }

        private IResourceEnricher EnricherFactory(IDictionary<string, object> env, Type enricherType)
        {
            return (IResourceEnricher) Activator.CreateInstance(enricherType);
        }
    }

    public abstract class EnricherTestBase
    {
        protected Enrichment Target;

        [Fact]
        public void Test()
        {
            var root = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("samples\\embedded-sample.json"));
            var owinContext = new OwinContext();
            Target.Enrich(root, owinContext.Environment);
            
            Assert.Equal("test", root.SelectToken("_embedded.order.test"));
            Assert.Equal(1, root.SelectToken("_embedded.order.global"));
            Assert.Equal(2, root.SelectToken("_embedded.order._embedded.item[1].global"));
        }

        [Fact]
        public void Enrichment_with_factory()
        {
            
        }
    }
}