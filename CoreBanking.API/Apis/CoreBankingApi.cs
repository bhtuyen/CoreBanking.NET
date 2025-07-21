
using CoreBanking.API.Models;

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

    private static async Task CreateCustomer()
    {
        throw new NotImplementedException();
    }

    private static async Task GetCustomers([AsParameters] PaginationRequest paginationRequest)
    {
        throw new NotImplementedException();
    }
}
