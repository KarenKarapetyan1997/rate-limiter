using System;

namespace RateLimiter.Interfaces
{
	public interface IRateLimitRule
	{
		bool CanProceed(string clientToken, string resource, DateTime requestTime);
	}
}
