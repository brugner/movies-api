using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.API.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public const string Name = "Database";

    public DatabaseHealthCheck(IDbConnectionFactory connectionFactory, ILogger<DatabaseHealthCheck> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await _connectionFactory.CreateConnectionAsync(cancellationToken);

            return HealthCheckResult.Healthy("Database is healthy");
        }
        catch (Exception ex)
        {
            const string error = "Database is unhealthy";
            _logger.LogError(error, ex);

            return HealthCheckResult.Unhealthy(error, ex);
        }
    }
}
