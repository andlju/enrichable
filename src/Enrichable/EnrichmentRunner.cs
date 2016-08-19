using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Enrichable
{
    class EnrichmentRunner
    {
        private readonly EnricherRegistry _registry;
        private readonly IDictionary<string, object> _owinEnvironment;
        private readonly Dictionary<string, List<IResourceEnricher>> _enricherInstances;

        public EnrichmentRunner(EnricherRegistry registry, IDictionary<string, object> owinEnvironment)
        {
            _registry = registry;
            _owinEnvironment = owinEnvironment;
            _enricherInstances = new Dictionary<string, List<IResourceEnricher>>();
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

        private void Analyze(JObject resource, string rel)
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

        private IEnumerable<IResourceEnricher> GetEnrichersForProfile(string profile)
        {
            List<IResourceEnricher> profileEnrichers;
            if (!_enricherInstances.TryGetValue(profile ?? string.Empty, out profileEnrichers))
            {
                profileEnrichers = BuildEnrichers(profile).ToList();
                _enricherInstances.Add(profile ?? string.Empty, profileEnrichers);
            }
            return profileEnrichers;
        }

        private IEnumerable<IResourceEnricher> GetAllEnrichers()
        {
            return _enricherInstances.Values.SelectMany(d => d);
        }

        private IEnumerable<IResourceEnricher> BuildEnrichers(string profile)
        {
            return _registry.BuildEnrichers(_owinEnvironment, profile);
        }

    }
}