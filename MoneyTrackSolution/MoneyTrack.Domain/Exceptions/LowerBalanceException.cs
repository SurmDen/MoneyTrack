using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrack.Domain.Exceptions
{
    public class LowerBalanceException : InvalidOperationException
    {
        public Guid WalletId { get; }
        public decimal CurrentBalance { get; }
        public decimal RequiredAmount { get; }

        public LowerBalanceException(Guid walletId, decimal currentBalance, decimal requiredAmount)
            : base($"Lower balance: Wallet {walletId}, Current: {currentBalance}, Required: {requiredAmount}")
        {
            WalletId = walletId;
            CurrentBalance = currentBalance;
            RequiredAmount = requiredAmount;
        }

        public LowerBalanceException(string message) : base(message)
        {
        }

        public LowerBalanceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
