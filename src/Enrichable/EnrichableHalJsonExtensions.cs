using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Enrichable
{
    public class EmbeddedResource
    {
        public EmbeddedResource(JObject resource, string rel)
        {
            Resource = resource;
            Rel = rel;
        }

        public JObject Resource { get; }
        public string Rel { get; }
    }

    public class ResourceLink
    {
        public ResourceLink(string href, string rel)
        {
            Href = href;
            Rel = rel;
        }

        public string Href { get; }
        public string Rel { get; }
    }

    public static class EnrichableHalJsonExtensions
    {
        public static IEnumerable<ResourceLink> GetLinks(this JObject resource)
        {
            var links = resource["_links"] as JObject;
            if (links == null)
                yield break;

            foreach (var property in links.Properties())
            {
                foreach (var resourceLink in GetLinksFromLinkProperty(property))
                {
                    yield return resourceLink;
                }
            }
        }

        public static IEnumerable<ResourceLink> GetLinks(this JObject resource, string rel)
        {
            var links = resource["_links"] as JObject;
            var property = links?.Property(rel);
            if (property == null)
                yield break;

            foreach (var resourceLink in GetLinksFromLinkProperty(property))
            {
                yield return resourceLink;
            }
        }

        private static IEnumerable<ResourceLink> GetLinksFromLinkProperty(JProperty property)
        {
            var objs = property.Value as JArray;
            if (objs == null)
            {
                // Only a single entry
                yield return new ResourceLink(property.Value.SelectToken("href").Value<string>(), property.Name);
            }
            else
            {
                foreach (JObject obj in objs)
                {
                    yield return new ResourceLink(obj.SelectToken("href").Value<string>(), property.Name);
                }
            }
        }

        public static IEnumerable<EmbeddedResource> GetEmbedded(this JObject resource)
        {
            var embedded = resource["_embedded"] as JObject;
            if (embedded == null)
                yield break;
            foreach (var property in embedded.Properties())
            {
                foreach (var embeddedResource in GetEmbeddedFromEmbeddedProperty(property))
                {
                    yield return embeddedResource;
                }
            }
        }

        public static IEnumerable<EmbeddedResource> GetEmbedded(this JObject resource, string rel)
        {
            var embedded = resource["_embedded"] as JObject;

            var property = embedded?.Property(rel);
            if (property == null)
                yield break;

            foreach (var embeddedResource in GetEmbeddedFromEmbeddedProperty(property))
            {
                yield return embeddedResource;
            }
        }

        private static IEnumerable<EmbeddedResource> GetEmbeddedFromEmbeddedProperty(JProperty rel)
        {
            var objs = rel.Value as JArray;
            if (objs == null)
            {
                yield return new EmbeddedResource((JObject)rel.Value, rel.Name);
            }
            else
            {
                foreach (var obj in objs)
                {
                    yield return new EmbeddedResource((JObject)obj, rel.Name);
                }
            }
        }
    }
}