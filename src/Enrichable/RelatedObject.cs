using Newtonsoft.Json.Linq;

namespace Enrichable
{
    /// <summary>
    /// Response object containing a resource/link/form and it's relation
    /// </summary>
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
}