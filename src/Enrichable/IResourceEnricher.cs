using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Enrichable
{
    public interface IResourceEnricher
    {
        void Analyze(JObject resource, string rel);
        void Commit();
    }
}