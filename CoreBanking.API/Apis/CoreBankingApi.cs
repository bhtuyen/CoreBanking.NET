
using CoreBanking.API.Models;
using CoreBanking.API.Services;
using CoreBanking.Infrastructure.Entity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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

    private static async Task Transfer(Guid id)
    {
        throw new NotImplementedException();
    }

    private static async Task Withdraw(Guid id)
    {
        throw new NotImplementedException();
    }

    private static async Task Deposit(Guid id)
    {
        throw new NotImplementedException();
    }

    private static async Task CreateAccount()
    {
        throw new NotImplementedException();
    }

    private static async Task GetAccounts()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="customer"></param>
    /// <returns></returns>
    private static async Task<Results<Ok<Customer>, BadRequest>> CreateCustomer([AsParameters] CoreBankingServices services, Customer customer)
    {
        if (string.IsNullOrEmpty(customer.Name))
        {
            services.Logger.LogError("Customer name can't be empty");
            return TypedResults.BadRequest();
        }

        customer.Address ??= string.Empty;

        if(customer.Id == Guid.Empty)
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
    private static async Task<Ok<PaginationResponse<Customer>>> GetCustomers([AsParameters] PaginationRequest pagination, [AsParameters] CoreBankingServices services)
    {
        var list = await services.DbContext.Customers.OrderBy(x => x.Name).Skip(pagination.PageIndex * pagination.PageSize).Take(pagination.PageSize).ToListAsync();
        var count = await services.DbContext.Customers.CountAsync();

        return TypedResults.Ok(new PaginationResponse<Customer>(pagination.PageIndex, pagination.PageSize, count, list));

    }
}
