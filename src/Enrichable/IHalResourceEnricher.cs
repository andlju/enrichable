using Newtonsoft.Json.Linq;

namespace Enrichable
{
    public interface IHalResourceEnricher
    {
        void Analyze(JObject resource, string rel);
        void Commit();
    }
}