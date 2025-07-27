using CoreBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreBanking.MigrationService;

public class Worker(IHostApplicationLifetime hostApplicationLifetime, IServiceProvider serviceProvider, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Migrating the database...");

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CoreBankingDbContext>();

            logger.LogInformation("Ensuring the database exists and is up to date...");
            await EnsureDatabaseAsync(dbContext, stoppingToken);

            logger.LogInformation("Running migration...");
            await RunMigrationAsync(dbContext, stoppingToken);
            // await SeedDataAsync(dbContext, stoppingToken);

            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private async Task EnsureDatabaseAsync(CoreBankingDbContext dbContext, CancellationToken cancellationToken)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();


        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Create the database if it does not exist.
            // Do this first so there is then a database to start a transaction against.
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });
    }

    private async Task RunMigrationAsync(CoreBankingDbContext dbContext, CancellationToken cancellationToken)
    {
        // Kiểm tra các migrations có sẵn
        //var allMigrations = dbContext.Database.GetMigrations().ToList();
        //logger.LogInformation($"Tất cả migrations có sẵn: {string.Join(", ", allMigrations)}");

        //var appliedMigrations = dbContext.Database.GetAppliedMigrations().ToList();
        //logger.LogInformation($"Migrations đã áp dụng: {string.Join(", ", appliedMigrations)}");

        //var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
        //logger.LogInformation($"Migrations đang chờ: {string.Join(", ", pendingMigrations)}");

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            //await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.Database.MigrateAsync(cancellationToken);
            //await transaction.CommitAsync(cancellationToken);
        });
    }

    private async Task SeedDataAsync(CoreBankingDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
