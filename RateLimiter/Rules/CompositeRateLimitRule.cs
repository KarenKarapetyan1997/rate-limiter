using RateLimiter.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Rules;

public class CompositeRateLimitRule : IRateLimitRule
{
    public IEnumerable<IRateLimitRule> Rules { get; }

    public CompositeRateLimitRule(IEnumerable<IRateLimitRule> rules)
    {
        Rules = rules;
    }

    public bool CanProceed(string clientToken, string resource, DateTime requestTime)
    {
        return Rules.All(rule => rule.CanProceed(clientToken, resource, requestTime));
    }
}