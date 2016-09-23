using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Enrichable.Tests
{
    public class HalJsonExtensionTests
    {
        private readonly JObject _sampleJson;

        public HalJsonExtensionTests()
        {
            _sampleJson = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("samples\\embedded-sample.json"));
        }

        [Fact]
        public void GetEmbedded_returns_single_embedded_item()
        {
            var embeddedOrder = _sampleJson.GetEmbedded();
            Assert.Equal(1, embeddedOrder.Count());
        }

        [Fact]
        public void GetEmbedded_returns_multiple_items()
        {
            var embeddedOrders = _sampleJson.GetEmbedded();
            var embeddedItems = embeddedOrders.First().Value.GetEmbedded();
            Assert.Equal(2, embeddedItems.Count());
        }

        [Fact]
        public void AddLink_adds_single_link()
        {
            var embeddedOrder = _sampleJson.GetEmbedded().First().Value;

            embeddedOrder.AddLink("test-link", "http://my.test.int");

            Assert.Equal("http://my.test.int", embeddedOrder.SelectToken("_links['test-link']['href']"));
            Assert.Equal(null, embeddedOrder.SelectToken("_links['test-link']['templated']"));
        }

        [Fact]
        public void AddLink_with_template_sets_templated_flat()
        {
            var embeddedOrder = _sampleJson.GetEmbedded().First().Value;

            embeddedOrder.AddLink("test-link", "http://my.test.int/{?Test}");

            Assert.Equal(true, embeddedOrder.SelectToken("_links['test-link']['templated']"));
        }

        [Fact]
        public void AddLink_adds_multiple_links()
        {
            var embeddedOrder = _sampleJson.GetEmbedded().First().Value;

            embeddedOrder.AddLink("test-link", "http://my.test.int");
            embeddedOrder.AddLink("test-link", "http://second.test.int");

            Assert.Equal("http://second.test.int", embeddedOrder.SelectToken("_links['test-link'][1]['href']"));
        }

        [Fact]
        public void AddLink_adds_to_existing_single_link()
        {
            var embeddedOrder = _sampleJson.GetEmbedded().First().Value;

            embeddedOrder.AddLink("profile", "http://my.test.int");

            Assert.Equal("http://my.test.int", embeddedOrder.SelectToken("_links['profile'][1]['href']"));
        }


    }
}