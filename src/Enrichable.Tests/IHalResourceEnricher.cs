using Newtonsoft.Json.Linq;

namespace Enrichable.Tests
{
    public interface IHalResourceEnricher
    {
        void Analyze(JObject resource, string rel);
        void Commit();
    }
}