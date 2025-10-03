using Microsoft.AspNetCore.Mvc;
using MoneyTrack.Application.Interfaces;
using MoneyTrack.Domain.Exceptions;
using MoneyTrack.Domain.Models.DTOs;

namespace MoneyTrack.Web.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ILogger<WalletsController> _logger;

        public WalletsController(IWalletRepository walletRepository, ILogger<WalletsController> logger)
        {
            _walletRepository = walletRepository;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateWalletAsync([FromBody] CreateWalletDTO walletDTO)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when creating wallet");
                return BadRequest(new { message = "Couldn't create wallet, please input correct data" });
            }

            try
            {
                _logger.LogInformation($"Creating new wallet: {walletDTO.WalletName}");
                var wallet = await _walletRepository.CreateWalletAsync(walletDTO);
                _logger.LogInformation($"Wallet {wallet.WalletName} created successfully with Id: {wallet.Id}");
                return Ok(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wallet");
                return Problem(
                    statusCode: 500,
                    title: "Internal server error",
                    detail: "Error occurred while trying to create wallet. Please try again later"
                );
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWalletAsync(Guid id)
        {
            try
            {
                _logger.LogInformation($"Fetching wallet with Id: {id}");
                var wallet = await _walletRepository.GetWalletByIdAsync(id);
                return Ok(wallet);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Wallet with id {id} not found");
                return NotFound(new { message = $"Wallet with id: {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching wallet with id: {id}");
                return Problem(
                    statusCode: 500,
                    title: "Internal server error",
                    detail: "Error occurred while fetching wallet"
                );
            }
        }

        [HttpPost("transaction/create")]
        public async Task<IActionResult> CreateTransactionAsync([FromBody] CreateTransactionDTO transactionDTO)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when creating transaction");
                return BadRequest(new { message = "Couldn't make transaction, please input correct data" });
            }

            try
            {
                _logger.LogInformation($"Creating transaction from {transactionDTO.SenderWalletId} to {transactionDTO.ReceiverWalletId}");

                await _walletRepository.CreateTransactionAsync(transactionDTO);

                _logger.LogInformation("Transaction created successfully");
                return Ok(new { message = "Transaction created successfully" });
            }
            catch (LowerBalanceException lb_ex)
            {
                _logger.LogError(lb_ex, "Error creating transaction");
                return BadRequest(new { message = "You don't have enough funds" });
            }
            catch (InvalidOperationException inv_ex)
            {
                _logger.LogError(inv_ex, "Error creating transaction");
                return BadRequest(new { message = "Couldn't make transaction, please input correct data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction");
                return Problem(
                    statusCode: 500,
                    title: "Internal server error",
                    detail: "Error occurred while trying to make transaction. Please try again later"
                );
            }
        }

        [HttpGet("{id}/transactions/{year}/{month}")]
        public async Task<IActionResult> GetMonthlyWalletTransactionsInfoAsync(Guid id, int year, int month)
        {
            try
            {
                _logger.LogInformation($"Fetching transactions for wallet {id} for {year}-{month}");
                var transactions = await _walletRepository.GetWalletTransactionsInfoAsync(id, year, month);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching transactions for wallet {id}");
                return Problem(
                    statusCode: 500,
                    title: "Internal server error",
                    detail: "Error occurred while trying to get wallet transactions info"
                );
            }
        }

        [HttpGet("get/all/{year}/{month}/{transactions_count}")]
        public async Task<IActionResult> GetMonthlyWalletsInfoAsync(int year, int month, int transactions_count)
        {
            try
            {
                _logger.LogInformation($"Fetching all wallets info for {year}-{month} with {transactions_count} transactions");
                var wallets = await _walletRepository.GetWalletsWithTransactionsAsync(transactions_count, year, month);
                return Ok(wallets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching wallets info for {year}-{month}");
                return Problem(
                    statusCode: 500,
                    title: "Internal server error",
                    detail: "Error occurred while trying to get wallets info"
                );
            }
        }
    }
}
