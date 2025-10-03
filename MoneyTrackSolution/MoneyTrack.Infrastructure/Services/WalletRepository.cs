using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoneyTrack.Application.Interfaces;
using MoneyTrack.Domain.Entities;
using MoneyTrack.Domain.Exceptions;
using MoneyTrack.Domain.Models.DTOs;
using MoneyTrack.Domain.Models.Responses;
using MoneyTrack.Infrastructure.Data;

namespace MoneyTrack.Infrastructure.Services
{
    public class WalletRepository : IWalletRepository
    {
        public WalletRepository(ApplicationDbContext dbContext, ICurrencyApiService currencyApiService, ILogger<WalletRepository> logger)
        {
            _dbContext = dbContext;
            _currencyService = currencyApiService;
            _logger = logger;
        }

        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrencyApiService _currencyService;
        private readonly ILogger<WalletRepository> _logger;

        public async Task CreateTransactionAsync(CreateTransactionDTO transactionDTO)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                Wallet? senderWallet = await _dbContext.Wallets
                    .Include(x => x.Transactions)
                    .FirstOrDefaultAsync(x => x.Id == transactionDTO.SenderWalletId);

                if (senderWallet == null)
                {
                    _logger.LogError($"sender wallet with id {transactionDTO.SenderWalletId} not found");
                    throw new InvalidOperationException($"sender wallet with id {transactionDTO.SenderWalletId} not found");
                }

                Wallet? receiverWallet = await _dbContext.Wallets
                    .FirstOrDefaultAsync(x => x.Id == transactionDTO.ReceiverWalletId);

                if (receiverWallet == null)
                {
                    _logger.LogError($"receiver wallet with id {transactionDTO.ReceiverWalletId} not found");
                    throw new InvalidOperationException($"receiver wallet with id {transactionDTO.ReceiverWalletId} not found");
                }

                senderWallet.CurrentBalance = senderWallet.InitialBalance;

                decimal currency = 1;

                try
                {
                    currency = await _currencyService.ConvertAsync(senderWallet.Currency, receiverWallet.Currency);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Currency conversion from {senderWallet.Currency} to {receiverWallet.Currency} failed");

                    throw new InvalidOperationException(
                        $"Currency conversion from {senderWallet.Currency} to {receiverWallet.Currency} failed", ex);
                }

                if (senderWallet.Transactions != null)
                {
                    if (senderWallet.Transactions.Count > 0)
                    {
                        foreach (var transactionModel in senderWallet.Transactions)
                        {
                            if (transactionModel.TransactionType == TransactionType.Income)
                            {
                                senderWallet.CurrentBalance += transactionModel.Amount;
                            }
                            else
                            {
                                senderWallet.CurrentBalance -= transactionModel.Amount;
                            }
                        }
                    }
                }

                if (senderWallet.CurrentBalance < transactionDTO.Amount)
                {
                    _logger.LogError($"sender with id {senderWallet.Id} has lower balance, than transacted sum");

                    throw new LowerBalanceException($"sender with id {senderWallet.Id} has lower balance, than transacted sum");
                }

                Transaction senderTransaction = new Transaction()
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.UtcNow,
                    Amount = transactionDTO.Amount,
                    TransactionType = TransactionType.Expense,
                    WalletId = senderWallet.Id,
                    Description = transactionDTO.Description
                };

                Transaction receiverTransaction = new Transaction()
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.UtcNow,
                    Amount = transactionDTO.Amount * currency,
                    TransactionType = TransactionType.Income,
                    WalletId = receiverWallet.Id,
                    Description = transactionDTO.Description
                };

                await _dbContext.Transactions.AddRangeAsync(senderTransaction, receiverTransaction);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unespected error occured");

                await transaction.RollbackAsync();

                throw;
            }
        }

        public async Task<Wallet> CreateWalletAsync(CreateWalletDTO walletDTO)
        {
            if (walletDTO == null)
            {
                throw new ArgumentNullException(nameof(walletDTO));
            }

            Wallet wallet = new Wallet()
            {
                Id = Guid.NewGuid(),
                WalletName = walletDTO.WalletName,
                InitialBalance = walletDTO.InitialBalance,
                Currency = walletDTO.Currency
            };

            await _dbContext.Wallets.AddAsync(wallet);

            await _dbContext.SaveChangesAsync();

            return wallet;
        }

        public async Task<Wallet> GetWalletByIdAsync(Guid walletId)
        {
            Wallet? wallet = await _dbContext.Wallets
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x => x.Id == walletId);

            if (wallet == null)
            {
                _logger.LogError($"Wallet with id {walletId} not found");

                throw new InvalidOperationException($"Wallet with id {walletId} not found");
            }

            wallet.CurrentBalance = wallet.InitialBalance;

            var transactions = wallet.Transactions;

            if (transactions != null)
            {
                if (transactions.Count > 0)
                {
                    foreach (var transaction in transactions)
                    {
                        if (transaction.TransactionType == TransactionType.Income)
                        {
                            wallet.CurrentBalance += transaction.Amount;
                        }
                        else
                        {
                            wallet.CurrentBalance -= transaction.Amount;
                        }
                    }
                }
            }

            return wallet;
        }

        public async Task<List<Wallet>> GetWalletsWithTransactionsAsync(int transactionsCount, int year, int month)
        {
            if (transactionsCount < 0)
            {
                throw new ArgumentOutOfRangeException("Transactions count cannot be negative");
            }

            int currentYear = DateTime.Now.Year;

            if (month < 1 || month > 12)
            {
                _logger.LogError($"Invalid month data: {month}");

                throw new ArgumentOutOfRangeException("Month must be between 1 and 12");
            }

            if (year < 2000 || year > currentYear)
            {
                _logger.LogError($"Invalid year data: {year}");

                throw new ArgumentOutOfRangeException($"Year must be between 2000 and {currentYear}");
            }

            if (year == currentYear && month > DateTime.Now.Month)
            {
                _logger.LogError($"Invalid year data: {year}");

                throw new ArgumentOutOfRangeException($"We cannot see future");
            }

            List<Wallet> wallets = await _dbContext.Wallets
                .AsNoTracking()
                .Include(x => x.Transactions)
                .ToListAsync();

            foreach (var wallet in wallets)
            {
                wallet.CurrentBalance = wallet.InitialBalance;

                if (wallet.Transactions.Count > 0)
                {
                    foreach (var transaction in wallet.Transactions)
                    {
                        if (transaction.TransactionType == TransactionType.Income)
                        {
                            wallet.CurrentBalance += transaction.Amount;
                        }
                        else
                        {
                            wallet.CurrentBalance -= transaction.Amount;
                        }

                        transaction.Wallet = null;
                    }

                    wallet.Transactions = wallet.Transactions
                        .Where(x => x.Date.Month == month && x.Date.Year == year && x.TransactionType == TransactionType.Expense)
                        .Take(transactionsCount)
                        .OrderByDescending(x => x.Amount)
                        .ToList();
                }
            }

            return wallets;
        }

        public async Task<WalletInfoResponse> GetWalletTransactionsInfoAsync(Guid walletId, int year, int month)
        {
            int currentYear = DateTime.Now.Year;

            if (month < 1 || month > 12)
            {
                _logger.LogError($"Invalid month data: {month}");

                throw new ArgumentOutOfRangeException("Month must be between 1 and 12");
            }

            if (year < 2000 || year > currentYear)
            {
                _logger.LogError($"Invalid year data: {year}");

                throw new ArgumentOutOfRangeException($"Year must be between 2000 and {currentYear}");
            }

            if (year == currentYear && month > DateTime.Now.Month)
            {
                _logger.LogError($"Invalid year data: {year}");

                throw new ArgumentOutOfRangeException($"We cannot see future");
            }

            Wallet? wallet = await _dbContext.Wallets
                .AsNoTracking()
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x => x.Id == walletId);

            if (wallet == null)
            {
                _logger.LogError($"Wallet with id {walletId} not found");

                throw new InvalidOperationException($"Wallet with id {walletId} not found");
            }

            wallet.Transactions = wallet.Transactions.Where(t => t.Date.Year == year && t.Date.Month == month).OrderBy(x => x.Date).ToList();

            WalletInfoResponse walletInfoResponse = new WalletInfoResponse();

            foreach (var transaction in wallet.Transactions)
            {
                transaction.Wallet = null;

                if (transaction.TransactionType == TransactionType.Income)
                {
                    walletInfoResponse.IncomeTransactionsGroup.Transactions.Add(transaction);
                    walletInfoResponse.IncomeTransactionsGroup.TotalSum += transaction.Amount;
                }
                else
                {
                    walletInfoResponse.ExpenseTransactionsGroup.Transactions.Add(transaction);
                    walletInfoResponse.ExpenseTransactionsGroup.TotalSum += transaction.Amount;
                }
            }

            return walletInfoResponse;
        }
    }
}
