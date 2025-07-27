using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoreBanking.Infrastructure.Entity
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateUtc { get; set; }
        public TransactionTypes Type { get; set; }
        public Guid AccountId { get; set; }
        [JsonIgnore]
        public Account Account { get; set; } = default!;
    }

    public enum TransactionTypes
    {
        Deposit,
        Withdraw
    }
}
