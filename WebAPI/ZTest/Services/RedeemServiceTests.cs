using Application;
using Application.IRepos;
using Application.Services;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ZTest.Services
{
    public class RedeemServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IRedeemRepo> _mockRedeemRepo;
        private readonly Mock<IScoreItemRepo> _mockScoreItemRepo;
        private readonly Mock<IUserRepo> _mockUserRepo;
        private readonly RedeemService _service;

        public RedeemServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _mockRedeemRepo = new Mock<IRedeemRepo>();
            _mockScoreItemRepo = new Mock<IScoreItemRepo>();
            _mockUserRepo = new Mock<IUserRepo>();

            _unitOfWorkMock.Setup(u => u.redeemRepo).Returns(_mockRedeemRepo.Object);
            _unitOfWorkMock.Setup(u => u.ScoreItemRepo).Returns(_mockScoreItemRepo.Object);
            _unitOfWorkMock.Setup(u => u.UserRepo).Returns(_mockUserRepo.Object);

            _service = new RedeemService(_unitOfWorkMock.Object, _mapperMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task CreateRedeemAsync_ReturnsOk_WhenValidRequest()
        {
            var userId = Guid.NewGuid();
            var scoreItemId = Guid.NewGuid();
            var scoreItem = new ScoreItem { Id = scoreItemId, Score = 10, Quantity = 100 };

            _mockScoreItemRepo.Setup(x => x.GetByIdAsync(scoreItemId)).ReturnsAsync(scoreItem);
            _mockRedeemRepo.Setup(x => x.AddAsync(It.IsAny<Redeem>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            var requests = new List<RedeemRequest> { new RedeemRequest { ScoreItemId = scoreItemId, Quantity = 2 } };

            var result = await _service.CreateRedeemAsync(userId, requests);

            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Redeem create successfully!", result.Result);
        }

        [Fact]
        public async Task CreateRedeemAsync_ReturnsNotFound_WhenScoreItemIsNull()
        {
            var userId = Guid.NewGuid();
            var invalidScoreItemId = Guid.NewGuid();
            _mockScoreItemRepo.Setup(x => x.GetByIdAsync(invalidScoreItemId)).ReturnsAsync((ScoreItem)null);

            var requests = new List<RedeemRequest> { new RedeemRequest { ScoreItemId = invalidScoreItemId, Quantity = 1 } };

            var result = await _service.CreateRedeemAsync(userId, requests);

            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal("Score item not found", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateRedeemAsync_ReturnsBadRequest_WhenQuantityTooLarge()
        {
            var userId = Guid.NewGuid();
            var scoreItemId = Guid.NewGuid();
            var scoreItem = new ScoreItem { Id = scoreItemId, Score = 5, Quantity = 1 };

            _mockScoreItemRepo.Setup(x => x.GetByIdAsync(scoreItemId)).ReturnsAsync(scoreItem);

            var requests = new List<RedeemRequest> { new RedeemRequest { ScoreItemId = scoreItemId, Quantity = 10 } };

            var result = await _service.CreateRedeemAsync(userId, requests);

            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("The items are not enought for you to exchange!!!", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateRedeemAsync_ReturnsBadRequest_OnException()
        {
            var userId = Guid.NewGuid();
            var scoreItemId = Guid.NewGuid();
            var requests = new List<RedeemRequest> { new RedeemRequest { ScoreItemId = scoreItemId, Quantity = 1 } };

            _mockScoreItemRepo.Setup(x => x.GetByIdAsync(scoreItemId)).ThrowsAsync(new Exception("Database error"));

            var result = await _service.CreateRedeemAsync(userId, requests);

            Assert.False(result.IsSuccess);
            Assert.Equal("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task GetRedeemAsync_ReturnsNotFound_WhenRedeemDoesNotExist()
        {
            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync((Redeem)null);

            var result = await _service.GetRedeemAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal("Redeem not found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetRedeemAsync_ReturnsBadRequest_OnException()
        {
            var redeemId = Guid.NewGuid();
            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>()))
                           .ThrowsAsync(new Exception("Database error"));

            var result = await _service.GetRedeemAsync(redeemId);

            Assert.False(result.IsSuccess);
            Assert.Equal("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task GetPendingRedeemsByAccountAsync_ReturnsNotFound_WhenNoRedeems()
        {
            _mockRedeemRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(new List<Redeem>());

            var result = await _service.GetPendingRedeemsByAccountAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal("No redeems found for this account", result.ErrorMessage);
        }

        [Fact]
        public async Task GetPendingRedeemsByAccountAsync_ReturnsBadRequest_OnException()
        {
            var accountId = Guid.NewGuid();
            _mockRedeemRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>()))
                           .ThrowsAsync(new Exception("Database error"));

            var result = await _service.GetPendingRedeemsByAccountAsync(accountId);

            Assert.False(result.IsSuccess);
            Assert.Equal("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task GetPaidRedeemsByAccountAsync_ReturnsOk_WhenRedeemsExist()
        {
            var userId = Guid.NewGuid();
            var redeemId = Guid.NewGuid();

            var redeemList = new List<Redeem>
            {
                new Redeem
                {
                    Id = redeemId,
                    UserId = userId,
                    IsDeleted = false,
                    status = ScoreStatus.paid
                }
            };

            _mockRedeemRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>()))
                .ReturnsAsync(redeemList);

            _mockRedeemRepo.Setup(r => r.GetItemNamesByRedeemId(redeemId))
                .ReturnsAsync(new List<string> { "Item1" });

            var result = await _service.GetPaidRedeemsByAccountAsync(userId);

            Assert.True(result.IsSuccess);
            var data = Assert.IsType<List<RedeemResponse>>(result.Result);
            Assert.Single(data);
            Assert.Equal("Item1", data[0].ItemNames[0]);
        }

        [Fact]
        public async Task GetPaidRedeemsByAccountAsync_ReturnsNotFound_WhenNoRedeemsFound()
        {
            var accountId = Guid.NewGuid();
            _mockRedeemRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>()))
                .ReturnsAsync(new List<Redeem>());

            var result = await _service.GetPaidRedeemsByAccountAsync(accountId);

            Assert.False(result.IsSuccess);
            Assert.Equal("No redeems found for this account", result.ErrorMessage);
        }

        [Fact]
        public async Task GetPaidRedeemsByAccountAsync_ReturnsBadRequest_OnException()
        {
            var accountId = Guid.NewGuid();
            _mockRedeemRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>()))
                .ThrowsAsync(new Exception("Database error"));

            var result = await _service.GetPaidRedeemsByAccountAsync(accountId);

            Assert.False(result.IsSuccess);
            Assert.Equal("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllRedeemsAsync_ReturnsOk_WhenRedeemsExist()
        {
            var redeemId = Guid.NewGuid();
            var redeemList = new List<Redeem>
            {
                new Redeem
                {
                    Id = redeemId,
                    IsDeleted = false
                }
            };

            _mockRedeemRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>()))
                .ReturnsAsync(redeemList);

            _mockRedeemRepo.Setup(r => r.GetItemNamesByRedeemId(redeemId))
                .ReturnsAsync(new List<string> { "Item1" });

            var result = await _service.GetAllRedeemsAsync();

            Assert.True(result.IsSuccess);
            var data = Assert.IsType<List<RedeemResponse>>(result.Result);
            Assert.Single(data);
            Assert.Equal("Item1", data[0].ItemNames[0]);
        }

        [Fact]
        public async Task GetAllRedeemsAsync_ReturnsNotFound_WhenNoRedeemsFound()
        {
            _mockRedeemRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>()))
                .ReturnsAsync(new List<Redeem>());

            var result = await _service.GetAllRedeemsAsync();

            Assert.False(result.IsSuccess);
            Assert.Equal("No redeems found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllRedeemsAsync_ReturnsBadRequest_OnException()
        {
            _mockRedeemRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>()))
                .ThrowsAsync(new Exception("Database error"));

            var result = await _service.GetAllRedeemsAsync();

            Assert.False(result.IsSuccess);
            Assert.Equal("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task CancelRedeemAsync_ReturnsOk_WhenRedeemFound()
        {
            var redeem = new Redeem { Id = Guid.NewGuid(), status = ScoreStatus.pending };
            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(redeem);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.CancelRedeemAsync(redeem.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal("Redeem cancelled successfully!", result.Result);
        }

        [Fact]
        public async Task CancelRedeemAsync_ReturnsNotFound_WhenRedeemNotFound()
        {
            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync((Redeem)null);

            var result = await _service.CancelRedeemAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal("Redeem not found", result.ErrorMessage);
        }

        [Fact]
        public async Task CancelRedeemAsync_ReturnsBadRequest_OnException()
        {
            var redeemId = Guid.NewGuid();
            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>()))
                           .ThrowsAsync(new Exception("Database error"));

            var result = await _service.CancelRedeemAsync(redeemId);

            Assert.False(result.IsSuccess);
            Assert.Equal("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateRedeemAsync_ReturnsNotFound_WhenRedeemNotFound()
        {
            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync((Redeem)null);

            var result = await _service.updateRedeemAsync(Guid.NewGuid(), new List<RedeemRequest>());

            Assert.False(result.IsSuccess);
            Assert.Equal("Redeem not found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateRedeemAsync_ReturnsNotFound_WhenScoreItemNotFound()
        {
            var redeem = new Redeem { Id = Guid.NewGuid(), ScoreOrders = new List<ScoreOrder>() };
            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(redeem);
            _mockScoreItemRepo.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ScoreItem)null);

            var requests = new List<RedeemRequest> { new RedeemRequest { ScoreItemId = Guid.NewGuid(), Quantity = 1 } };
            var result = await _service.updateRedeemAsync(redeem.Id, requests);

            Assert.False(result.IsSuccess);
            Assert.Equal("Score item not found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateRedeemAsync_ReturnsBadRequest_WhenQuantityTooLarge()
        {
            var redeem = new Redeem { Id = Guid.NewGuid(), ScoreOrders = new List<ScoreOrder>() };
            var scoreItem = new ScoreItem { Id = Guid.NewGuid(), Score = 5, Quantity = 1 };

            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(redeem);
            _mockScoreItemRepo.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(scoreItem);

            var requests = new List<RedeemRequest> { new RedeemRequest { ScoreItemId = scoreItem.Id, Quantity = 10 } };
            var result = await _service.updateRedeemAsync(redeem.Id, requests);

            Assert.False(result.IsSuccess);
            Assert.Equal("The items are not enough for you to exchange!!!", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateRedeemAsync_ReturnsBadRequest_OnException()
        {
            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>()))
                           .ThrowsAsync(new Exception("Database error"));

            var result = await _service.updateRedeemAsync(Guid.NewGuid(), new List<RedeemRequest>());

            Assert.False(result.IsSuccess);
            Assert.Equal("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task RedeemItem_ReturnsNotFound_WhenRedeemNotFound()
        {
            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync((Redeem)null);

            var result = await _service.redeemItem(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal("Redeem not found or already processed", result.ErrorMessage);
        }

        [Fact]
        public async Task RedeemItem_ReturnsNotFound_WhenUserNotFound()
        {
            var redeem = new Redeem
            {
                Id = Guid.NewGuid(),
                status = ScoreStatus.pending,
                TotalScore = 50,
                ScoreOrders = new List<ScoreOrder>()
            };

            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(redeem);
            _mockUserRepo.Setup(u => u.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>())).ReturnsAsync((AppUser)null);

            var result = await _service.redeemItem(redeem.Id, Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal("Redeem not found or already processed", result.ErrorMessage);
        }

        [Fact]
        public async Task RedeemItem_ReturnsBadRequest_WhenUserScoreIsNotEnough()
        {
            var redeem = new Redeem { Id = Guid.NewGuid(), status = ScoreStatus.pending, TotalScore = 100, ScoreOrders = new List<ScoreOrder>() };
            var user = new AppUser { Id = Guid.NewGuid(), Score = 50 };

            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(redeem);
            _mockUserRepo.Setup(u => u.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>())).ReturnsAsync(user);

            var result = await _service.redeemItem(redeem.Id, user.Id);

            Assert.False(result.IsSuccess);
            Assert.Equal("Your score is not enough!", result.ErrorMessage);
        }

        [Fact]
        public async Task RedeemItem_ReturnsNotFound_WhenScoreItemNotFound()
        {
            var scoreItemId = Guid.NewGuid();
            var redeem = new Redeem
            {
                Id = Guid.NewGuid(),
                status = ScoreStatus.pending,
                TotalScore = 50,
                ScoreOrders = new List<ScoreOrder> {
                    new ScoreOrder { ScoreItemId = scoreItemId, Quantity = 1 }
                }
            };
            var user = new AppUser { Id = Guid.NewGuid(), Score = 100 };

            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(redeem);
            _mockUserRepo.Setup(u => u.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>())).ReturnsAsync(user);
            _mockScoreItemRepo.Setup(s => s.GetByIdAsync(scoreItemId)).ReturnsAsync((ScoreItem)null);

            var result = await _service.redeemItem(redeem.Id, user.Id);

            Assert.False(result.IsSuccess);
            Assert.Equal("Score item not found", result.ErrorMessage);
        }

        [Fact]
        public async Task RedeemItem_ReturnsBadRequest_WhenScoreItemQuantityInsufficient()
        {
            var scoreItemId = Guid.NewGuid();
            var redeem = new Redeem
            {
                Id = Guid.NewGuid(),
                status = ScoreStatus.pending,
                TotalScore = 50,
                ScoreOrders = new List<ScoreOrder> {
                    new ScoreOrder { ScoreItemId = scoreItemId, Quantity = 5 }
                }
            };
            var user = new AppUser { Id = Guid.NewGuid(), Score = 100 };
            var scoreItem = new ScoreItem { Id = scoreItemId, Quantity = 2 };

            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(redeem);
            _mockUserRepo.Setup(u => u.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>())).ReturnsAsync(user);
            _mockScoreItemRepo.Setup(s => s.GetByIdAsync(scoreItemId)).ReturnsAsync(scoreItem);

            var result = await _service.redeemItem(redeem.Id, user.Id);

            Assert.False(result.IsSuccess);
            Assert.Equal("The items are not enough for you to exchange!!!", result.ErrorMessage);
        }

        [Fact]
        public async Task RedeemItem_ReturnsBadRequest_OnException()
        {
            _mockRedeemRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>()))
                           .ThrowsAsync(new Exception("Database error"));

            var result = await _service.redeemItem(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal("Database error", result.ErrorMessage);
        }
    }
}
