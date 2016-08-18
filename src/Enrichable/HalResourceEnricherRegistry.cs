using System;
using System.Collections.Generic;
using System.Linq;

namespace Enrichable
{
    public class HalResourceEnricherRegistry
    {
        readonly Dictionary<string, List<Func<IHalResourceEnricher>>> _enricherFactories = new Dictionary<string, List<Func<IHalResourceEnricher>>>();

        /// <summary>
        /// Register a new enricher
        /// </summary>
        /// <param name="enricherFactory">Factory method for creating the enricher</param>
        /// <param name="profile">Profile that this enricher should be applied to. The defaults to all profiles.</param>
        public void RegisterEnricher(Func<IHalResourceEnricher> enricherFactory, string profile = "")
        {
            List<Func<IHalResourceEnricher>> enrichers;
            if (!_enricherFactories.TryGetValue(profile, out enrichers))
            {
                enrichers = new List<Func<IHalResourceEnricher>>();
                _enricherFactories.Add(profile, enrichers);
            }
            enrichers.Add(enricherFactory);
        }

        public IEnumerable<IHalResourceEnricher> GetEnrichers(string profile)
        {
            List<Func<IHalResourceEnricher>> enrichers;
            if (!string.IsNullOrEmpty(profile))
            {
                if (_enricherFactories.TryGetValue(profile, out enrichers))
                {
                    foreach (var enricher in enrichers)
                    {
                        yield return enricher();
                    }
                }
            }
            if (_enricherFactories.TryGetValue(string.Empty, out enrichers))
            {
                foreach (var enricher in enrichers)
                {
                    yield return enricher();
                }
            }
        }
    }
}