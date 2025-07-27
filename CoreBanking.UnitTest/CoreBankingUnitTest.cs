using CoreBanking.API.Apis;
using CoreBanking.API.Services;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Infrastructure.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;

namespace CoreBanking.UnitTest
{
    public class CoreBankingUnitTest
    {
        private SqliteConnection _sqliteConnection = default!;
        private DbContextOptions<CoreBankingDbContext> _dbContextOptions = default!;

        [Fact]
        public async Task Create_Customer_UnitTest()
        {
            // Arrange
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");

            _dbContextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_sqliteConnection)
                .Options;

            await _sqliteConnection.OpenAsync();

            using var context = new CoreBankingDbContext(_dbContextOptions);
            context.Database.EnsureCreated();

            var services = new CoreBankingServices(NullLogger<CoreBankingServices>.Instance, context);

            // Act
            var customer = new Customer
            {
                Id = Guid.CreateVersion7(),
                Name = "John Doe",
                Address = "123 Main St, Anytown, USA",
                Accounts = []
            };

            var result = await CoreBankingApi.CreateCustomer(services, customer);

            // Assert
            var createdCustomer = context.Customers.Find(customer.Id);
            Assert.NotNull(createdCustomer);
            Assert.Equal("John Doe", createdCustomer.Name);
            Assert.Equal("123 Main St, Anytown, USA", createdCustomer.Address);
            Assert.Empty(createdCustomer.Accounts);
        }

        [Fact]
        public async Task Create_Account_UnitTest()
        {
            // Arrange
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");
            _dbContextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_sqliteConnection)
                .Options;
            await _sqliteConnection.OpenAsync();
            using var context = new CoreBankingDbContext(_dbContextOptions);
            context.Database.EnsureCreated();
            var services = new CoreBankingServices(NullLogger<CoreBankingServices>.Instance, context);
            // Act
            var customer = new Customer
            {
                Id = Guid.CreateVersion7(),
                Name = "Jane Doe",
                Address = "456 Elm St, Othertown, USA",
                Accounts = []
            };

            var customerResult = await CoreBankingApi.CreateCustomer(services, customer);
            var account = new Account
            {
                Id = Guid.CreateVersion7(),
                CustomerId = customer.Id,
                Customer = customer,
                Number = "1234567890",
            };
            var result = await CoreBankingApi.CreateAccount(services, account);
            // Assert
            var createdAccount = context.Accounts.Find(account.Id);
            Assert.NotNull(createdAccount);
            Assert.Equal(customer.Id, createdAccount.CustomerId);
            Assert.Equal(0, createdAccount.Balance);
            // Bằng đoạn mã sau để kiểm tra giá trị Number đúng cách:
            if (result.Result is Ok<Account> okResult && okResult.Value is Account accountValue)
            {
                Assert.Equal(accountValue.Number, createdAccount.Number);
            }
        }
    }
}
