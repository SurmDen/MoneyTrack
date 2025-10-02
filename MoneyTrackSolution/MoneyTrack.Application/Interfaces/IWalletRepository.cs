using MoneyTrack.Domain.Entities;
using MoneyTrack.Domain.Models.DTOs;
using MoneyTrack.Domain.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrack.Application.Interfaces
{
    public interface IWalletRepository
    {
        public Task<Wallet> CreateWalletAsync(CreateWalletDTO walletDTO);

        public Task CreateTransactionAsyncAsync(CreateTransactionDTO transactionDTO);

        public Task<List<Wallet>> GetWalletsWithTransactionsAsync();

        public Task<Wallet> GetWalletByIdAsync(Guid walletId);

        public Task<WalletInfoResponse> GetWalletTransactionsInfoAsync(Guid walletId, int year, int month);
    }
}
