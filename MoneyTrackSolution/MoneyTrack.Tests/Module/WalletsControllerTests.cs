using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyTrack.Application.Interfaces;
using MoneyTrack.Domain.Exceptions;
using MoneyTrack.Domain.Models.DTOs;
using MoneyTrack.Web.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrack.Tests.Module
{
    // wrote a couple of tests for an example
    public class WalletsControllerTests
    {
        private readonly Mock<IWalletRepository> _mockRepo;
        private readonly WalletsController _controller;

        public WalletsControllerTests()
        {
            _mockRepo = new Mock<IWalletRepository>();
            var mockLogger = new Mock<ILogger<WalletsController>>();
            _controller = new WalletsController(_mockRepo.Object, mockLogger.Object);
        }

        [Fact]
        public async Task CreateTransactionAsync_ValidData_ReturnsOk()
        {
            var dto = new CreateTransactionDTO
            {
                SenderWalletId = Guid.NewGuid(),
                ReceiverWalletId = Guid.NewGuid(),
                Amount = 100,
                Description = "Transaction to my future job"
            };

            _mockRepo.Setup(x => x.CreateTransactionAsync(dto)).Returns(Task.CompletedTask);

            var result = await _controller.CreateTransactionAsync(dto);

            Assert.IsType<OkObjectResult>(result);

            _mockRepo.Verify(x => x.CreateTransactionAsync(dto), Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_LowBalance_ReturnsBadRequest()
        {
            var dto = new CreateTransactionDTO
            {
                SenderWalletId = Guid.NewGuid(),
                ReceiverWalletId = Guid.NewGuid(),
                Amount = 1000,
                Description = "Transaction to my awful military past"
            };

            _mockRepo.Setup(x => x.CreateTransactionAsync(dto))
                .ThrowsAsync(new LowerBalanceException("Low balance, man"));

            var result = await _controller.CreateTransactionAsync(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
