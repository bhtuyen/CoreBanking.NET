using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoreBanking.Infrastructure.Entity
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Address { get; set; } = default!;
        [JsonIgnore]
        public ICollection<Account> Accounts { get; set; } = [];
    }
}
