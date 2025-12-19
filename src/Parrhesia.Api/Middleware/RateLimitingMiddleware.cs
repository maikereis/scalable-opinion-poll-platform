using System.Collections.Concurrent;
using System.Net;

namespace Parrhesia.Api.Middleware;

/// <summary>
/// Simple in-memory rate limiting middleware.
/// For production, use a distributed cache like Redis.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    
    // In-memory storage: IP -> (Timestamp, Count)
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _requests = new();
    
    // Configuration
    private const int MaxRequestsPerMinute = 100;
    private const int VotingMaxRequestsPerMinute = 500; // Adjust for Prodution
    private const int WindowSeconds = 60;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        // Cleanup task to prevent memory leak
        _ = Task.Run(CleanupOldEntries);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        
        // Determine rate limit based on endpoint
        var limit = DetermineRateLimit(path);
        
        var rateLimitInfo = _requests.GetOrAdd(clientId, _ => new RateLimitInfo());

        bool shouldBlock = false;
        int retryAfter = 0;
        int remaining = 0;
        DateTime resetTime = DateTime.UtcNow;

        lock (rateLimitInfo)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddSeconds(-WindowSeconds);

            // Remove old requests outside the window
            rateLimitInfo.Requests.RemoveAll(t => t < windowStart);

            // Check if limit exceeded
            if (rateLimitInfo.Requests.Count >= limit)
            {
                shouldBlock = true;
                var oldestRequest = rateLimitInfo.Requests.Min();
                retryAfter = (int)(WindowSeconds - (now - oldestRequest).TotalSeconds);
                resetTime = oldestRequest.AddSeconds(WindowSeconds);
            }
            else
            {
                // Add current request
                rateLimitInfo.Requests.Add(now);
                remaining = limit - rateLimitInfo.Requests.Count;
                resetTime = rateLimitInfo.Requests.Min().AddSeconds(WindowSeconds);
            }
        }

        // Handle blocking outside of lock
        if (shouldBlock)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = retryAfter.ToString();
            context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = 
                new DateTimeOffset(resetTime).ToUnixTimeSeconds().ToString();

            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "RATE_LIMIT_EXCEEDED",
                    message = "Too many requests. Please try again later.",
                    retryAfter = retryAfter,
                    limit = limit,
                    window = $"{WindowSeconds} seconds"
                }
            });

            _logger.LogWarning(
                "Rate limit exceeded for {ClientId} on {Path}. Limit: {Limit}/min",
                clientId, path, limit);

            return;
        }

        // Set rate limit headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
            context.Response.Headers["X-RateLimit-Reset"] = 
                new DateTimeOffset(resetTime).ToUnixTimeSeconds().ToString();
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from header (if authenticated)
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private static int DetermineRateLimit(string path)
    {
        // Voting endpoints have stricter limits
        if (path.Contains("/votes") && !path.Contains("/count") && !path.Contains("/check"))
        {
            return VotingMaxRequestsPerMinute;
        }

        // Default limit for all other endpoints
        return MaxRequestsPerMinute;
    }

    private static async Task CleanupOldEntries()
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromMinutes(5));

            var now = DateTime.UtcNow;
            var expiredKeys = _requests
                .Where(kvp => kvp.Value.Requests.Count == 0 || 
                             kvp.Value.Requests.Max() < now.AddMinutes(-5))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _requests.TryRemove(key, out _);
            }
        }
    }
}

internal class RateLimitInfo
{
    public List<DateTime> Requests { get; } = new();
}