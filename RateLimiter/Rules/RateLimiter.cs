using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;

namespace RateLimiter.Rules
{
    public class RateLimiter
    {
        private readonly Dictionary<string, List<IRateLimitRule>> resourceRules = new();
        private readonly Dictionary<string, string> clientRegions = new(); // Map client tokens to regions
        private readonly Dictionary<string, List<IRateLimitRule>> regionRules = new(); // Map regions to rules

        public void AddRule(string resource, IRateLimitRule rule)
        {
            if (!resourceRules.ContainsKey(resource))
            {
                resourceRules[resource] = new List<IRateLimitRule>();
            }
            resourceRules[resource].Add(rule);
        }

        public void AddRegionRule(string region, IRateLimitRule rule)
        {
            if (!regionRules.ContainsKey(region))
            {
                regionRules[region] = new List<IRateLimitRule>();
            }
            regionRules[region].Add(rule);
        }

        public void RegisterClientRegion(string clientToken, string region)
        {
            clientRegions[clientToken] = region;
        }

        public bool CanProceed(string clientToken, string resource, DateTime requestTime)
        {
            // Validate that clientToken exists and has a region
            if (!clientRegions.TryGetValue(clientToken, out var region) || string.IsNullOrWhiteSpace(region))
            {
                throw new InvalidOperationException($"Client token '{clientToken}' does not have a valid region assigned.");
            }

            // Check if the region-specific rules exist and validate them
            if (regionRules.TryGetValue(region, out var rules))
            {
                foreach (var rule in rules)
                {
                    if (!rule.CanProceed(clientToken, resource, requestTime))
                    {
                        return false; // Region-specific rule failed
                    }
                }
            }

            // Check resource-specific rules
            if (resourceRules.TryGetValue(resource, out var resourceSpecificRules))
            {
                foreach (var rule in resourceSpecificRules)
                {
                    if (!rule.CanProceed(clientToken, resource, requestTime))
                    {
                        return false; // Resource-specific rule failed
                    }
                }
            }

            return true; // All rules passed
        }
    }
}