namespace CoreBanking.API.Apis;

public static class CoreBankingApi
{
    public static IEndpointRouteBuilder MapCoreBankingApi(this IEndpointRouteBuilder builder)
    {
        var vApi = builder.NewVersionedApi("CoreBanking");
        var v1 = vApi.MapGroup("api/v{version:apiVersion}/corebanking").HasApiVersion(1, 0);

        v1.MapGet("/customers", GetCustomers);
        v1.MapPost("/customers", CreateCustomer);

        v1.MapGet("/accounts", GetAccounts);
        v1.MapPost("/accounts", CreateAccount);
        v1.MapPut("/accounts/{id:guid}/deposit", Deposit);
        v1.MapPut("/accounts/{id:guid}/withdraw", Withdraw);
        v1.MapPut("/accounts/{id:guid}/transfer", Transfer);
        return builder;
    }

    #region Transfer API
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static async Task<Results<Ok<Account>, BadRequest>> Transfer(Guid id, [AsParameters] CoreBankingServices services, TransferRequest transfer)
    {
        if (id == Guid.Empty)
        {
            services.Logger.LogError("AccountId can't be empty!");
            return TypedResults.BadRequest();
        }
        if (string.IsNullOrEmpty(transfer.DestinationAccountNumber))
        {
            services.Logger.LogError("DestinationAccountId can't be empty!");
            return TypedResults.BadRequest();
        }

        if (transfer.Amount <= 0)
        {
            services.Logger.LogError("Amount must be greater then zero!");
            return TypedResults.BadRequest();
        }

        var sourceAccount = await services.DbContext.Accounts.FindAsync(id);

        if (sourceAccount == null)
        {
            services.Logger.LogError("Source account not found");
            return TypedResults.BadRequest();
        }

        if (sourceAccount.Balance < transfer.Amount)
        {
            services.Logger.LogError("Insufficient funds in source account");
            return TypedResults.BadRequest();
        }

        var destinationAccount = await services.DbContext.Accounts.FirstOrDefaultAsync(x => x.Number == transfer.DestinationAccountNumber);

        if (destinationAccount == null)
        {
            services.Logger.LogError("Destination account not found");
            return TypedResults.BadRequest();
        }

        if (sourceAccount.Id == destinationAccount.Id)
        {
            services.Logger.LogError("Source and Destination accounts can't be the same");
            return TypedResults.BadRequest();
        }

        sourceAccount.Balance -= transfer.Amount;
        destinationAccount.Balance += transfer.Amount;

        try
        {
            var now = DateTime.UtcNow;

            services.DbContext.Transactions.Add(new Transaction
            {
                Id = Guid.CreateVersion7(),
                Type = TransactionTypes.Withdraw,
                AccountId = id,
                Amount = transfer.Amount,
                DateUtc = now,
            });

            services.DbContext.Transactions.Add(new Transaction
            {
                Id = Guid.CreateVersion7(),
                Type = TransactionTypes.Deposit,
                AccountId = destinationAccount.Id,
                Amount = transfer.Amount,
                DateUtc = now,
            });

            services.DbContext.Accounts.Update(sourceAccount);
            services.DbContext.Accounts.Update(destinationAccount);
            await services.DbContext.SaveChangesAsync();
            services.Logger.LogInformation("Funds transferred successfully from {SourceAccount} to {DestinationAccount}", sourceAccount.Number, destinationAccount.Number);
        }
        catch (Exception ex)
        {
            services.Logger.LogError(ex, "Error while transferring funds between accounts");
            return TypedResults.BadRequest();
        }
        return TypedResults.Ok(sourceAccount);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="services"></param>
    /// <param name="withdrawal"></param>
    /// <returns></returns>
    public static async Task<Results<Ok<Account>, BadRequest>> Withdraw(Guid id, [AsParameters] CoreBankingServices services, WithdrawalRequest withdrawal)
    {
        if (id == Guid.Empty)
        {
            services.Logger.LogError("AccountId can't be empty!");
            return TypedResults.BadRequest();
        }

        if (withdrawal.Amount <= 0)
        {
            services.Logger.LogError("Amount must be greater then zero!");
            return TypedResults.BadRequest();
        }

        var account = await services.DbContext.Accounts.FindAsync(id);

        if (account == null)
        {
            services.Logger.LogError("Account not found");
            return TypedResults.BadRequest();
        }

        if (account.Balance < withdrawal.Amount)
        {
            services.Logger.LogError("Insufficient funds");
            return TypedResults.BadRequest();
        }

        account.Balance -= withdrawal.Amount;

        try
        {
            services.DbContext.Transactions.Add(new Transaction
            {
                Id = Guid.CreateVersion7(),
                Type = TransactionTypes.Withdraw,
                AccountId = id,
                Amount = withdrawal.Amount,
                DateUtc = DateTime.UtcNow
            });
            services.DbContext.Accounts.Update(account);
            await services.DbContext.SaveChangesAsync();
            services.Logger.LogInformation("Account balance updated successfully");
        }
        catch (Exception ex)
        {
            services.Logger.LogError(ex, "Error while updating account balance");
            return TypedResults.BadRequest();
        }
        return TypedResults.Ok(account);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="services"></param>
    /// <param name="deposition"></param>
    /// <returns></returns>
    public static async Task<Results<Ok<Account>, BadRequest>> Deposit(Guid id, [AsParameters] CoreBankingServices services, DepositionRequest deposition)
    {
        if (id == Guid.Empty)
        {
            services.Logger.LogError("AccountId can't be empty!");
            return TypedResults.BadRequest();
        }

        if (deposition.Amount <= 0)
        {
            services.Logger.LogError("Amount must be greater then zero!");
            return TypedResults.BadRequest();
        }

        var account = await services.DbContext.Accounts.FindAsync(id);

        if (account == null)
        {
            services.Logger.LogError("Account not found");
            return TypedResults.BadRequest();
        }

        account.Balance += deposition.Amount;

        try
        {
            services.DbContext.Transactions.Add(new Transaction
            {
                Id = Guid.CreateVersion7(),
                Type = TransactionTypes.Deposit,
                AccountId = id,
                Amount = deposition.Amount,
                DateUtc = DateTime.UtcNow
            });

            services.DbContext.Accounts.Update(account);

            await services.DbContext.SaveChangesAsync();

            services.Logger.LogInformation("Account balance updated successfully");
        }
        catch (Exception ex)
        {
            services.Logger.LogError(ex, "Error while updating account balance");
            return TypedResults.BadRequest();
        }
        return TypedResults.Ok(account);
    }
    #endregion

    #region Account API
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="account"></param>
    /// <returns></returns>
    public static async Task<Results<Ok<Account>, BadRequest>> CreateAccount([AsParameters] CoreBankingServices services, Account account)
    {
        if (account.CustomerId == Guid.Empty)
        {
            services.Logger.LogError("CustomerId can't be empty!");
            return TypedResults.BadRequest();
        }

        account.Id = Guid.CreateVersion7();
        account.Balance = 0;
        account.Number = GenerateAccountNumber();

        services.DbContext.Accounts.Add(account);

        await services.DbContext.SaveChangesAsync();

        services.Logger.LogInformation("Account created!");

        return TypedResults.Ok(account);
    }

    public static string GenerateAccountNumber()
    {
        return DateTime.UtcNow.Ticks.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pagination"></param>
    /// <param name="services"></param>
    /// <param name="customerId"></param>
    /// <returns></returns>
    public static async Task<Ok<PaginationResponse<Account>>> GetAccounts([AsParameters] PaginationRequest pagination, [AsParameters] CoreBankingServices services, Guid? customerId = null)
    {
        IQueryable<Account> account = services.DbContext.Accounts;

        if (customerId.HasValue)
        {
            account = account.Where(x => x.CustomerId == customerId);
        }

        var list = await account.OrderBy(x => x.Number).Skip(pagination.PageIndex * pagination.PageSize).Take(pagination.PageSize).ToListAsync();
        var count = await account.CountAsync(x => x.CustomerId == customerId);

        return TypedResults.Ok(new PaginationResponse<Account>(pagination.PageIndex, pagination.PageSize, count, list));
    }
    #endregion

    #region Cusomer API
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="customer"></param>
    /// <returns></returns>
    public static async Task<Results<Ok<Customer>, BadRequest>> CreateCustomer([AsParameters] CoreBankingServices services, Customer customer)
    {
        if (string.IsNullOrEmpty(customer.Name))
        {
            services.Logger.LogError("Customer name can't be empty!");
            return TypedResults.BadRequest();
        }

        customer.Address ??= string.Empty;

        if (customer.Id == Guid.Empty)
        {
            customer.Id = Guid.CreateVersion7();
        }

        services.DbContext.Customers.Add(customer);

        await services.DbContext.SaveChangesAsync();

        services.Logger.LogInformation("Customer created!");

        return TypedResults.Ok(customer);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pagination"></param>
    /// <param name="services"></param>
    /// <returns></returns>
    public static async Task<Ok<PaginationResponse<Customer>>> GetCustomers([AsParameters] PaginationRequest pagination, [AsParameters] CoreBankingServices services)
    {
        var customers = services.DbContext.Customers;
        var list = await customers.OrderBy(x => x.Name).Skip(pagination.PageIndex * pagination.PageSize).Take(pagination.PageSize).ToListAsync();
        var count = await customers.CountAsync();

        return TypedResults.Ok(new PaginationResponse<Customer>(pagination.PageIndex, pagination.PageSize, count, list));
    }
    #endregion
}

public class DepositionRequest
{
    public decimal Amount { get; set; }
}

public class WithdrawalRequest
{
    public decimal Amount { get; set; }
}

public class TransferRequest
{
    public string DestinationAccountNumber { get; set; } = default!;
    public decimal Amount { get; set; }
}