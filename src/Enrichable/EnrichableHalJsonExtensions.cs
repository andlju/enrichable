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

        public static void AddLink(this JObject resource, string rel, string href)
        {
            var linksObj = resource["_links"] as JObject ?? (JObject)(resource["_links"] = new JObject());
            var linkToAdd = new JObject()
            {
                new JProperty("href", href)
            };
            if (href.Contains("{"))
            {
                // Add the templated flag
                linkToAdd.Add(new JProperty("templated", true));
            }

            // Do we already have an array?
            var relArray = linksObj[rel] as JArray;
            if (relArray == null)
            {
                // No array, do we have a single object?
                var relObj = linksObj[rel] as JObject;
                if (relObj != null)
                {
                    // Found single object, let's convert it to an array
                    // and add the single object
                    linksObj[rel] = relArray = new JArray(relObj);
                }
            }
            if (relArray != null)
            {
                // We have an array one way or another, let's add to it
                relArray.Add(linkToAdd);
            }
            else
            {
                // Still no array. Add the single object
                linksObj[rel] = linkToAdd;
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