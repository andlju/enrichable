using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Enrichable
{
    public class Enrichment
    {
        private readonly EnricherRegistry _registry;

        public Enrichment()
        {
            _registry = new EnricherRegistry();
        }

        public void Enrich(JObject root, IDictionary<string, object> owinEnvironment)
        {
            var enricher = new EnrichmentRunner(_registry, owinEnvironment);
            enricher.Enrich(root);
        }

        /// <summary>
        /// Register a new enricher
        /// </summary>
        /// <param name="enricherFactory">Factory method for creating the enricher given an owin environment</param>
        /// <param name="profile">Profile that this enricher should be applied to. Defaults to all profiles.</param>
        public void RegisterEnricher(Func<IDictionary<string, object>, IResourceEnricher> enricherFactory,
            string profile = null)
        {
            _registry.RegisterEnricher(enricherFactory, profile);
        }

    }
}