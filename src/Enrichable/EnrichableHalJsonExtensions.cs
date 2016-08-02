using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Enrichable
{
    public class RelatedObject
    {
        public RelatedObject(JObject value, string rel)
        {
            Value = value;
            Rel = rel;
        }

        public JObject Value { get; }
        public string Rel { get; }
    }


    public static class EnrichableHalJsonExtensions
    {
        public static IEnumerable<RelatedObject> GetLinks(this JObject resource)
        {
            var links = resource["_links"] as JObject;
            if (links == null)
                yield break;

            foreach (var property in links.Properties())
            {
                foreach (var resourceLink in GetRelatedObjectsFromProperty(property))
                {
                    yield return resourceLink;
                }
            }
        }

        public static IEnumerable<RelatedObject> GetLinks(this JObject resource, string rel)
        {
            var links = resource["_links"] as JObject;
            var property = links?.Property(rel);
            if (property == null)
                yield break;

            foreach (var resourceLink in GetRelatedObjectsFromProperty(property))
            {
                yield return resourceLink;
            }
        }


        public static IEnumerable<RelatedObject> GetEmbedded(this JObject resource)
        {
            var embedded = resource["_embedded"] as JObject;
            if (embedded == null)
                yield break;
            foreach (var property in embedded.Properties())
            {
                foreach (var embeddedResource in GetRelatedObjectsFromProperty(property))
                {
                    yield return embeddedResource;
                }
            }
        }

        public static IEnumerable<RelatedObject> GetEmbedded(this JObject resource, string rel)
        {
            var embedded = resource["_embedded"] as JObject;

            var property = embedded?.Property(rel);
            if (property == null)
                yield break;

            foreach (var embeddedResource in GetRelatedObjectsFromProperty(property))
            {
                yield return embeddedResource;
            }
        }

        private static IEnumerable<RelatedObject> GetRelatedObjectsFromProperty(JProperty rel)
        {
            var objs = rel.Value as JArray;
            if (objs == null)
            {
                yield return new RelatedObject((JObject)rel.Value, rel.Name);
            }
            else
            {
                foreach (var obj in objs)
                {
                    yield return new RelatedObject((JObject)obj, rel.Name);
                }
            }
        }
    }
}