using System;
using System.Collections.Generic;
using System.Linq;

namespace Enrichable
{
    public class HalResourceEnricherRegistry
    {
        readonly Dictionary<string, List<Type>> _enrichers = new Dictionary<string, List<Type>>();

        public void RegisterEnricher<TEnricher>(string profile) where TEnricher : IHalResourceEnricher
        {
            List<Type> enrichers;
            if (!_enrichers.TryGetValue(profile, out enrichers))
            {
                enrichers = new List<Type>();
                _enrichers.Add(profile, enrichers);
            }
            enrichers.Add(typeof(TEnricher));
        }

        public IEnumerable<Type> GetEnricherImplementations(string profile)
        {
            List<Type> enrichers;
            if (!_enrichers.TryGetValue(profile, out enrichers))
                return Enumerable.Empty<Type>();

            return enrichers;
        }
    }
}