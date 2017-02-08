using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Enrichable
{
    public static class EnrichableHalJsonExtensions
    {
        /// <summary>
        /// Get all links and their relations from this resource. 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns>A list of RelatedObjects</returns>
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

        /// <summary>
        /// Get all links for a specific relation from this resource. 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns>A list of RelatedObjects</returns>
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

        /// <summary>
        /// Get all forms and their relations from this resource. 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns>A list of RelatedObjects</returns>
        public static IEnumerable<RelatedObject> GetForms(this JObject resource)
        {
            var forms = resource["_forms"] as JObject;
            if (forms == null)
                yield break;

            foreach (var property in forms.Properties())
            {
                foreach (var resourceForm in GetRelatedObjectsFromProperty(property))
                {
                    yield return resourceForm;
                }
            }
        }

        /// <summary>
        /// Get all forms for a specific relation from this resource. 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns>A list of RelatedObjects</returns>
        public static IEnumerable<RelatedObject> GetForms(this JObject resource, string rel)
        {
            var forms = resource["_forms"] as JObject;
            var property = forms?.Property(rel);
            if (property == null)
                yield break;

            foreach (var resourceForm in GetRelatedObjectsFromProperty(property))
            {
                yield return resourceForm;
            }
        }


        /// <summary>
        /// Add to a HAL-type property
        /// </summary>
        /// <param name="resource">The resource to add a rel property to</param>
        /// <param name="propertyName">The name of the rel property</param>
        /// <param name="rel">Relation</param>
        /// <param name="objectToAdd">The object to add</param>
        private static void AddRelObject(this JObject resource, string propertyName, string rel, bool forceArray, JObject objectToAdd)
        {
            var linksObj = resource[propertyName] as JObject ?? (JObject)(resource[propertyName] = new JObject());
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
                relArray.Add(objectToAdd);
            }
            else
            {
                // Still no array. Add the single object or as an array if forced
                if (forceArray)
                {
                    linksObj[rel] = new JArray(objectToAdd);
                }
                else
                {
                    linksObj[rel] = objectToAdd;
                }
            }
        }

        public static void AddLink(this JObject resource, string rel, string href, string prompt = null)
        {
            resource.AddLink(rel, href, false, prompt);
        }

        public static void AddLink(this JObject resource, string rel, string href, bool forceArray, string prompt = null)
        {
            var linkToAdd = new JObject()
            {
                new JProperty("href", href)
            };
            if (prompt != null)
            {
                linkToAdd.Add(new JProperty("prompt", prompt));
            }
            if (href.Contains("{"))
            {
                // Add the templated flag
                linkToAdd.Add(new JProperty("templated", true));
            }
            resource.AddRelObject("_links", rel, forceArray, linkToAdd);
        }

        public static void AddEmbedded(this JObject resource, string rel, JObject embeddedObjectToAdd)
        {
            resource.AddRelObject("_embedded", rel, false, embeddedObjectToAdd);
        }

        public static void AddEmbedded(this JObject resource, string rel, bool forceArray, JObject embeddedObjectToAdd)
        {
            resource.AddRelObject("_embedded", rel, forceArray, embeddedObjectToAdd);
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