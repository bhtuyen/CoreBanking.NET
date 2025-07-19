namespace CoreBanking.MigrationService;

public class Worker(ILogger<Worker> logger, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}
