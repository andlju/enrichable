using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Enrichable
{

    public class Enricher
    {
        private readonly HalResourceEnricherRegistry _registry;
        private readonly Func<Type, object> _enricherFactory;

        private readonly Dictionary<string, List<IHalResourceEnricher>> _enrichers;

        public Enricher(HalResourceEnricherRegistry registry, Func<Type,object> enricherFactory)
        {
            _registry = registry;
            _enricherFactory = enricherFactory;
            _enrichers = new Dictionary<string, List<IHalResourceEnricher>>();
        }

        public void Enrich(JObject root)
        {
            // Recursively analyze the root resource and all embedded resources
            Analyze(root, "root");
            // Tell all enrichers that were engaged to commit their changes
            Commit();
        }

        private void Commit()
        {
            foreach (var enricher in GetAllEnrichers())
            {
                enricher.Commit();
            }
        }

        public void Analyze(JObject resource, string rel)
        {
            // Get the profile href of this object if any
            var profile = resource.GetLinks("profile")?.FirstOrDefault()?.Value["href"].Value<string>();
            var enrichers = GetEnrichersForProfile(profile);

            foreach (var enricher in enrichers)
            {
                // Let each enricher prepare the resource
                enricher.Analyze(resource, rel);
            }
            var embedded = resource.GetEmbedded();
            foreach (var embeddedResource in embedded)
            {
                Analyze(embeddedResource.Value, embeddedResource.Rel);
            }
        }

        private IEnumerable<IHalResourceEnricher> GetEnrichersForProfile(string profile)
        {
            if (string.IsNullOrEmpty(profile))
                return Enumerable.Empty<IHalResourceEnricher>();

            List<IHalResourceEnricher> profileEnrichers;
            if (!_enrichers.TryGetValue(profile, out profileEnrichers))
            {
                profileEnrichers = BuildEnrichers(profile).ToList();
                _enrichers.Add(profile, profileEnrichers);
            }
            return profileEnrichers;
        }

        private IEnumerable<IHalResourceEnricher> GetAllEnrichers()
        {
            return _enrichers.Values.SelectMany(d => d);
        }

        private IEnumerable<IHalResourceEnricher> BuildEnrichers(string profile)
        {
            foreach (var type in _registry.GetEnricherImplementations(profile))
            {
                yield return (IHalResourceEnricher) _enricherFactory(type);
            }
        }

    }
}