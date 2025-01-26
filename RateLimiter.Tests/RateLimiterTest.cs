using NUnit.Framework;
using RateLimiter.Rules;
using System;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RateLimiterTest
    {
        private RateLimiter.Rules.RateLimiter _rateLimiter;

        [SetUp]
        public void SetUp()
        {
            _rateLimiter = new RateLimiter.Rules.RateLimiter();

            // Add sample rules for testing
            _rateLimiter.AddRegionRule("US", new FixedWindowRateLimitRule(5, TimeSpan.FromMinutes(1))); // 5 requests per minute
            _rateLimiter.AddRegionRule("EU", new SlidingWindowRateLimitRule(TimeSpan.FromSeconds(10))); // 1 request every 10 seconds
            _rateLimiter.AddRule("api/resource1", new FixedWindowRateLimitRule(3, TimeSpan.FromMinutes(1))); // 3 requests per minute
        }

        [Test]
        public void CanProceed_WithinLimits_AllowsRequest()
        {
            // Assign client to a region
            string clientToken = "client1";
            string region = "US";
            _rateLimiter.RegisterClientRegion(clientToken, region);

            // Make 5 requests within limit
            for (int i = 0; i < 3; i++)
            {
                var result = _rateLimiter.CanProceed(clientToken, "api/resource1", DateTime.UtcNow.AddSeconds(i));
                Assert.IsTrue(result, $"Request {i + 1} should be allowed.");
            }
        }

        [Test]
        public void CanProceed_ExceedsRegionLimit_BlocksRequest()
        {
            // Assign client to a region
            string clientToken = "client2";
            string region = "US";
            _rateLimiter.RegisterClientRegion(clientToken, region);

            // Exceed limit (6th request should fail)
            for (int i = 0; i < 6; i++)
            {
                var result = _rateLimiter.CanProceed(clientToken, "api/resource1", DateTime.UtcNow.AddSeconds(i));
                if (i < 3)
                {
                    Assert.IsTrue(result, $"Request {i + 1} should be allowed.");
                }
                else
                {
                    Assert.IsFalse(result, "6th request should be blocked due to region limit.");
                }
            }
        }

        [Test]
        public void CanProceed_ExceedsResourceLimit_BlocksRequest()
        {
            // Assign client to a region
            string clientToken = "client3";
            string region = "EU";
            _rateLimiter.RegisterClientRegion(clientToken, region);

            // Exceed limit (4th request should fail)
            for (int i = 0; i < 4; i++)
            {
                var result = _rateLimiter.CanProceed(clientToken, "api/resource1", DateTime.UtcNow.AddSeconds(i));
                if (i < 1)
                {
                    Assert.IsTrue(result, $"Request {i + 1} should be allowed.");
                }
                else
                {
                    Assert.IsFalse(result, "4th request should be blocked due to resource limit.");
                }
            }
        }

        [Test]
        public void CanProceed_InvalidClientToken_ThrowsException()
        {
            // Try a client token not assigned to a region
            string clientToken = "unknownClient";
            Assert.Throws<InvalidOperationException>(() =>
            {
                _rateLimiter.CanProceed(clientToken, "api/resource1", DateTime.UtcNow);
            });
        }

        [Test]
        public void CanProceed_ValidClientWithSlidingWindowRule_RespectsSlidingWindow()
        {
            // Assign client to EU region with sliding window rule
            string clientToken = "client4";
            string region = "EU";
            _rateLimiter.RegisterClientRegion(clientToken, region);

            // First request should pass
            var firstResult = _rateLimiter.CanProceed(clientToken, "api/resource1", DateTime.UtcNow);
            Assert.IsTrue(firstResult, "First request should be allowed.");

            // Second request within 10 seconds should fail
            var secondResult = _rateLimiter.CanProceed(clientToken, "api/resource1", DateTime.UtcNow.AddSeconds(5));
            Assert.IsFalse(secondResult, "Second request within 10 seconds should be blocked.");
        }
    }
}