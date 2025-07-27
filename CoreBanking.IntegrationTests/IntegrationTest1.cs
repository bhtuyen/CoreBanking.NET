using CoreBanking.Infrastructure.Entity;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CoreBanking.IntegrationTests.Tests
{
    public class IntegrationTest1
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        [Fact]
        public async Task GetWebResourceRootReturnsOkStatusCode()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CoreBanking_AppHost_AppHost>(cancellationToken);
            appHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                // Override the logging filters from the app's configuration
                logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
                logging.AddFilter("Aspire.", LogLevel.Debug);
                // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
            });
            appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
            await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

            // Act
            var httpClient = app.CreateHttpClient("corebanking-api");

            await app.ResourceNotifications.WaitForResourceHealthyAsync("corebanking-api", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

            // Assert
            var customer1 = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Address = "123 Main St",
                Accounts = []
            };
            var response1 = await httpClient.PostAsJsonAsync("api/v1/corebanking/customers", customer1, cancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            // Assert
            var customer2 = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Jane Smith",
                Address = "456 Elm St",
                Accounts = []
            };

            var response2 = await httpClient.PostAsJsonAsync("api/v1/corebanking/customers", customer2, cancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);


            var account1 = new Account
            {
                Id = Guid.NewGuid(),
                CustomerId = customer1.Id,
            };

            response1 = await httpClient.PostAsJsonAsync("api/v1/corebanking/accounts", account1, cancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            var account2 = new Account
            {
                Id = Guid.NewGuid(),
                CustomerId = customer2.Id,
            };

            response2 = await httpClient.PostAsJsonAsync("api/v1/corebanking/accounts", account2, cancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        }
    }
}
