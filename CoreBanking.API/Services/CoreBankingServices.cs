using CoreBanking.Infrastructure.Data;

namespace CoreBanking.API.Services
{
    public class CoreBankingServices(ILogger<CoreBankingServices> logger, CoreBankingDbContext dbContext)
    {
        public CoreBankingDbContext DbContext { get; } = dbContext;

        public ILogger<CoreBankingServices> Logger => logger;
    }
}
