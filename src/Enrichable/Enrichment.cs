using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Enrichable
{
    public class Enrichment
    {
        private readonly Func<IDictionary<string, object>, Type, IResourceEnricher> _defaultEnricherFactory;
        private readonly EnricherRegistry _registry;

        public Enrichment()
        {
            _registry = new EnricherRegistry();
        }

        public Enrichment(Func<IDictionary<string,object>, Type, IResourceEnricher> defaultEnricherFactory)
        {
            _defaultEnricherFactory = defaultEnricherFactory;
            _registry = new EnricherRegistry();
        }

        public void Enrich(JObject root, IDictionary<string, object> owinEnvironment)
        {
            var enricher = new EnrichmentRunner(_registry, owinEnvironment);
            enricher.Enrich(root);
        }

        /// <summary>
        /// Register a new enricher using a specific factory method
        /// </summary>
        /// <param name="enricherFactory">Factory method for creating the enricher given an owin environment</param>
        /// <param name="profile">Profile that this enricher should be applied to. Defaults to all profiles.</param>
        public void RegisterEnricher(Func<IDictionary<string, object>, IResourceEnricher> enricherFactory,
            string profile = null)
        {
            _registry.RegisterEnricher(enricherFactory, profile);
        }

        /// <summary>
        /// Register a new enricher using the default enricher factory
        /// </summary>
        /// <typeparam name="TEnricher">Type of enricher to register</typeparam>
        /// <param name="profile">Profile that this enricher should be applied to. Defaults to all profiles.</param>
        public void RegisterEnricher<TEnricher>(string profile = null)
        {
            Func<IDictionary<string, object>, IResourceEnricher> enricherFactory = (env) => _defaultEnricherFactory(env, typeof(TEnricher));
            _registry.RegisterEnricher(enricherFactory, profile);
        }

    }
}