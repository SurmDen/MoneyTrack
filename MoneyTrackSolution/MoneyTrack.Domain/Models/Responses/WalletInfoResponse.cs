using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MoneyTrack.Domain.Models.Responses
{
    public class WalletInfoResponse
    {
        public List<TransactionGroup> IncomeTransactions { get; set; } = new();

        public List<TransactionGroup> ExpenseTransactions { get; set; } = new();

        public decimal TotalIncome { get; set; }

        public decimal TotalExpense { get; set; }

    }

    public class TransactionGroup
    {
        public decimal GroupTotal { get; set; }

        public List<Transaction> Transactions { get; set; } = new();
    }
}
