using System;
using System.Collections.Generic;
using System.Linq;

namespace Enrichable
{
    class EnricherRegistry
    {
        readonly Dictionary<string, List<Func<IDictionary<string, object>, IResourceEnricher>>> _enricherFactories = new Dictionary<string, List<Func<IDictionary<string, object>, IResourceEnricher>>>();

        /// <summary>
        /// Register a new enricher
        /// </summary>
        /// <param name="enricherFactory">Factory method for creating the enricher given an owin environment</param>
        /// <param name="profile">Profile that this enricher should be applied to. Defaults to all profiles.</param>
        public void RegisterEnricher(Func<IDictionary<string,object>, IResourceEnricher> enricherFactory, string profile = null)
        {
            if (profile == string.Empty)
                throw new ArgumentException("Profile name should not be empty", nameof(profile));
            if (profile == null)
                profile = string.Empty;

            List<Func<IDictionary<string, object>, IResourceEnricher>> enrichers;
            if (!_enricherFactories.TryGetValue(profile, out enrichers))
            {
                enrichers = new List<Func<IDictionary<string, object>, IResourceEnricher>>();
                _enricherFactories.Add(profile, enrichers);
            }
            enrichers.Add(enricherFactory);
        }

        /// <summary>
        /// Return a new set of enrichers for the specified profile
        /// </summary>
        /// <param name="owinEnvironment"></param>
        /// <param name="profile"></param>
        /// <returns>An enumeration of enrichers</returns>
        public IEnumerable<IResourceEnricher> BuildEnrichers(IDictionary<string,object> owinEnvironment, string profile = null)
        {
            if (profile == string.Empty)
                throw new ArgumentException("Profile name should not be empty", nameof(profile));

            List<Func<IDictionary<string, object>, IResourceEnricher>> enrichers;
            if (profile != null)
            {
                // We have specified a profile name. Let's first build any enrichers that match that name
                if (_enricherFactories.TryGetValue(profile, out enrichers))
                {
                    foreach (var enricher in enrichers)
                    {
                        yield return enricher(owinEnvironment);
                    }
                }
            }
            if (_enricherFactories.TryGetValue(string.Empty, out enrichers))
            {
                // Also include any "global" enrichers (i.e. enrichers with no profile name specified)
                foreach (var enricher in enrichers)
                {
                    yield return enricher(owinEnvironment);
                }
            }
        }
    }
}