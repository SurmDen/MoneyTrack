using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrack.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; set; }

        public string WalletName { get; set; } = string.Empty;

        public Currency Currency { get; set; }

        public decimal InitialBalance { get; set; }

        public List<Transaction> Transactions { get; set; } = new();

        // I did't use any AI models!!!!!!!!!!!!

        [NotMapped]
        public decimal CurrentBalance { get; set; }
    }

    public enum Currency
    {
        USD,
        RUB,
        EUR
    }
}
