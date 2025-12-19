using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parrhesia.Infrastructure.Persistence;

namespace Parrhesia.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    private readonly ParrhesiaDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        ParrhesiaDbContext dbContext,
        ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint - Síncrono para evitar CS1998
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        return Ok(health);
    }

    /// <summary>
    /// Detailed health check with dependency checks
    /// </summary>
    [HttpGet("detailed")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDetailed()
    {
        var checks = new Dictionary<string, object>();
        var overallHealthy = true;

        // Check database connectivity
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            checks["database"] = new
            {
                status = canConnect ? "healthy" : "unhealthy",
                responseTime = await MeasureDatabaseResponseTime()
            };

            if (!canConnect)
                overallHealthy = false;
        }
        catch (Exception ex)
        {
            checks["database"] = new
            {
                status = "unhealthy",
                error = ex.Message
            };
            overallHealthy = false;
            _logger.LogError(ex, "Database health check failed");
        }

        // Check memory usage
        var memoryInfo = GC.GetGCMemoryInfo();
        var memoryUsedMB = memoryInfo.HeapSizeBytes / (1024.0 * 1024.0);
        checks["memory"] = new
        {
            status = memoryUsedMB < 1024 ? "healthy" : "warning",
            usedMB = Math.Round(memoryUsedMB, 2),
            totalMB = Math.Round(memoryInfo.TotalAvailableMemoryBytes / (1024.0 * 1024.0), 2)
        };

        var response = new
        {
            status = overallHealthy ? "healthy" : "unhealthy",
            timestamp = DateTime.UtcNow,
            checks = checks
        };

        return overallHealthy 
            ? Ok(response) 
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    /// <summary>
    /// Readiness probe - checks if the application is ready to serve traffic
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "not_ready",
                    reason = "Database connection failed"
                });
            }

            return Ok(new { status = "ready" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "not_ready",
                reason = ex.Message
            });
        }
    }

    /// <summary>
    /// Liveness probe - checks if the application is alive
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Live()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    private async Task<double> MeasureDatabaseResponseTime()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
            sw.Stop();
            return Math.Round(sw.Elapsed.TotalMilliseconds, 2);
        }
        catch
        {
            return -1;
        }
    }
}