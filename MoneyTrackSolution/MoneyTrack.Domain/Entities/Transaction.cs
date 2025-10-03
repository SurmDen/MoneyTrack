using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrack.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public TransactionType TransactionType { get; set; }

        public string Description { get; set; } = string.Empty;

        public Wallet Wallet { get; set; }

        public Guid WalletId { get; set; }

    }

    public enum TransactionType
    {
        Income,
        Expense
    }
}
