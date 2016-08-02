using System;
using System.Collections.Generic;
using System.Linq;

namespace Enrichable
{
    public class HalResourceEnricherRegistry
    {
        readonly Dictionary<string, List<Type>> _enrichers = new Dictionary<string, List<Type>>();

        /// <summary>
        /// Register a new enricher
        /// </summary>
        /// <typeparam name="TEnricher">Type of the enricher</typeparam>
        /// <param name="profile">Profile that this enricher should be applied to. The defaults to all profiles.</param>
        public void RegisterEnricher<TEnricher>(string profile = "") where TEnricher : IHalResourceEnricher
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
            if (_enrichers.TryGetValue(profile, out enrichers))
            {
                foreach (var enricher in enrichers)
                {
                    yield return enricher;
                }
            }
            if (_enrichers.TryGetValue(string.Empty, out enrichers))
            {
                foreach (var enricher in enrichers)
                {
                    yield return enricher;
                }
            }
        }
    }
}