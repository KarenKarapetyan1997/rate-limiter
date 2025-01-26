using RateLimiter.Interfaces;
using System;
using System.Collections.Concurrent;

namespace RateLimiter.Rules
{
	public class SlidingWindowRateLimitRule(TimeSpan minimumInterval) : IRateLimitRule
	{
		private readonly ConcurrentDictionary<string, DateTime> _lastRequestTime = new();

		public bool CanProceed(string clientToken, string resource, DateTime requestTime)
		{
			string key = GenerateKey(clientToken, resource);

			if (_lastRequestTime.TryGetValue(key, out DateTime lastTime))
			{
				if (requestTime - lastTime < minimumInterval)
				{
					return false;
				}
			}

			_lastRequestTime[key] = requestTime;
			return true;
		}

		private string GenerateKey(string clientToken, string resource)
		{
			return $"{clientToken}:{resource}";
		}
	}
}
