using System.Collections.Concurrent;
using System.Collections.Generic;
using System;

using RateLimiter.Interfaces;

namespace RateLimiter.Rules;

public class FixedWindowRateLimitRule(int maxRequests, TimeSpan timeSpan) : IRateLimitRule
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _requestLogs = new();

    public bool CanProceed(string clientToken, string resource, DateTime requestTime)
    {
        string key = GenerateKey(clientToken, resource);
        _requestLogs.TryAdd(key, new List<DateTime>());

        var log = _requestLogs[key];

        lock (log)
        {
            log.RemoveAll(timestamp => timestamp < requestTime - timeSpan);
            if (log.Count < maxRequests)
            {
                log.Add(requestTime);
                return true;
            }

            return false;
        }
    }

    private string GenerateKey(string clientToken, string resource)
    {
        return $"{clientToken}:{resource}";
    }
}