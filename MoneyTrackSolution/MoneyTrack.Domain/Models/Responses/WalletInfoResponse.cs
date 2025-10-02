using MoneyTrack.Domain.Entities;

namespace MoneyTrack.Domain.Models.Responses
{
    public class WalletInfoResponse
    {
        public TransactionGroup IncomeTransactionsGroup { get; set; } = new();

        public TransactionGroup ExpenseTransactionsGroup { get; set; } = new();
    }

    public class TransactionGroup
    {
        public decimal TotalSum { get; set; }

        public List<Transaction> Transactions { get; set; } = new();
    }
}
