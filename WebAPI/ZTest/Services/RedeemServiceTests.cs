using Application;
using Application.Services;
using Application.ViewModel.Request;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace ZTest.Services
{
    public class RedeemServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly RedeemService _redeemService;

        public RedeemServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _redeemService = new RedeemService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task CreateRedeemAsync_ReturnsOk_WhenValidRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var scoreItemId = Guid.NewGuid();
            var scoreItem = new ScoreItem
            {
                Id = scoreItemId,
                Score = 10,
                Quantity = 100
            };

            _unitOfWorkMock.Setup(x => x.ScoreItemRepo.GetByIdAsync(scoreItemId))
                .ReturnsAsync(scoreItem);

            _unitOfWorkMock.Setup(x => x.redeemRepo.AddAsync(It.IsAny<Redeem>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            var requests = new List<RedeemRequest>
        {
            new RedeemRequest { ScoreItemId = scoreItemId, Quantity = 2 }
        };

            // Act
            var result = await _redeemService.CreateRedeemAsync(userId, requests);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Redeem create successfully!", result.Result);
        }

        [Fact]
        public async Task CreateRedeemAsync_ReturnsNotFound_WhenScoreItemIsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invalidScoreItemId = Guid.NewGuid();

            _unitOfWorkMock.Setup(x => x.ScoreItemRepo.GetByIdAsync(invalidScoreItemId))
                .ReturnsAsync((ScoreItem)null);

            var requests = new List<RedeemRequest>
        {
            new RedeemRequest { ScoreItemId = invalidScoreItemId, Quantity = 1 }
        };

            // Act
            var result = await _redeemService.CreateRedeemAsync(userId, requests);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task CreateRedeemAsync_ReturnsBadRequest_WhenQuantityTooLarge()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var scoreItemId = Guid.NewGuid();
            var scoreItem = new ScoreItem
            {
                Id = scoreItemId,
                Score = 5,
                Quantity = 1
            };

            _unitOfWorkMock.Setup(x => x.ScoreItemRepo.GetByIdAsync(scoreItemId))
                .ReturnsAsync(scoreItem);

            var requests = new List<RedeemRequest>
        {
            new RedeemRequest { ScoreItemId = scoreItemId, Quantity = 10 }
        };

            // Act
            var result = await _redeemService.CreateRedeemAsync(userId, requests);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task CreateRedeemAsync_ReturnsBadRequest_OnException()
        {
            _unitOfWorkMock.Setup(x => x.ScoreItemRepo.GetByIdAsync(It.IsAny<Guid>())).Throws(new Exception("fail"));
            var requests = new List<RedeemRequest> { new RedeemRequest { ScoreItemId = Guid.NewGuid(), Quantity = 1 } };
            var result = await _redeemService.CreateRedeemAsync(Guid.NewGuid(), requests);
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task GetRedeemAsync_ReturnsNotFound_WhenNotFound()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync((Redeem)null);
            var result = await _redeemService.GetRedeemAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("Redeem not found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetRedeemAsync_ReturnsBadRequest_OnException()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).Throws(new Exception("fail"));
            var result = await _redeemService.GetRedeemAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task GetPendingRedeemsByAccountAsync_ReturnsNotFound_WhenNotFound()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(new List<Redeem>());
            var result = await _redeemService.GetPendingRedeemsByAccountAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("No redeems found for this account", result.ErrorMessage);
        }

        [Fact]
        public async Task GetPendingRedeemsByAccountAsync_ReturnsBadRequest_OnException()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).Throws(new Exception("fail"));
            var result = await _redeemService.GetPendingRedeemsByAccountAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task GetPaidRedeemsByAccountAsync_ReturnsNotFound_WhenNotFound()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(new List<Redeem>());
            var result = await _redeemService.GetPaidRedeemsByAccountAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("No redeems found for this account", result.ErrorMessage);
        }

        [Fact]
        public async Task GetPaidRedeemsByAccountAsync_ReturnsBadRequest_OnException()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).Throws(new Exception("fail"));
            var result = await _redeemService.GetPaidRedeemsByAccountAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllRedeemsAsync_ReturnsNotFound_WhenNotFound()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(new List<Redeem>());
            var result = await _redeemService.GetAllRedeemsAsync();
            Assert.False(result.IsSuccess);
            Assert.Equal("No redeems found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllRedeemsAsync_ReturnsBadRequest_OnException()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAllAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).Throws(new Exception("fail"));
            var result = await _redeemService.GetAllRedeemsAsync();
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task CancelRedeemAsync_ReturnsNotFound_WhenNotFound()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync((Redeem)null);
            var result = await _redeemService.CancelRedeemAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("Redeem not found", result.ErrorMessage);
        }

        [Fact]
        public async Task CancelRedeemAsync_ReturnsBadRequest_OnException()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).Throws(new Exception("fail"));
            var result = await _redeemService.CancelRedeemAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task updateRedeemAsync_ReturnsNotFound_WhenNotFound()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync((Redeem)null);
            var result = await _redeemService.updateRedeemAsync(Guid.NewGuid(), new List<RedeemRequest>());
            Assert.False(result.IsSuccess);
            Assert.Equal("Redeem not found", result.ErrorMessage);
        }

        [Fact]
        public async Task updateRedeemAsync_ReturnsNotFound_WhenScoreItemNotFound()
        {
            var redeem = new Redeem { Id = Guid.NewGuid(), ScoreOrders = new List<ScoreOrder>() };
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(redeem);
            _unitOfWorkMock.Setup(x => x.ScoreItemRepo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ScoreItem)null);
            var requests = new List<RedeemRequest> { new RedeemRequest { ScoreItemId = Guid.NewGuid(), Quantity = 1 } };
            var result = await _redeemService.updateRedeemAsync(redeem.Id, requests);
            Assert.False(result.IsSuccess);
            Assert.Equal("Score item not found", result.ErrorMessage);
        }

        [Fact]
        public async Task updateRedeemAsync_ReturnsBadRequest_WhenQuantityTooLarge()
        {
            var redeem = new Redeem { Id = Guid.NewGuid(), ScoreOrders = new List<ScoreOrder>() };
            var scoreItem = new ScoreItem { Id = Guid.NewGuid(), Score = 5, Quantity = 1 };
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(redeem);
            _unitOfWorkMock.Setup(x => x.ScoreItemRepo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(scoreItem);
            var requests = new List<RedeemRequest> { new RedeemRequest { ScoreItemId = scoreItem.Id, Quantity = 10 } };
            var result = await _redeemService.updateRedeemAsync(redeem.Id, requests);
            Assert.False(result.IsSuccess);
            Assert.Equal("The items are not enough for you to exchange!!!", result.ErrorMessage);
        }

        [Fact]
        public async Task updateRedeemAsync_ReturnsBadRequest_OnException()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).Throws(new Exception("fail"));
            var result = await _redeemService.updateRedeemAsync(Guid.NewGuid(), new List<RedeemRequest>());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task redeemItem_ReturnsNotFound_WhenNotFound()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync((Redeem)null);
            var result = await _redeemService.redeemItem(Guid.NewGuid(), Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("Redeem not found or already processed", result.ErrorMessage);
        }

        [Fact]
        public async Task redeemItem_ReturnsBadRequest_WhenNotEnoughScore()
        {
            var redeem = new Redeem { Id = Guid.NewGuid(), TotalScore = 100, ScoreOrders = new List<ScoreOrder>() };
            var user = new AppUser { Id = Guid.NewGuid(), Score = 50 };
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(redeem);
            _unitOfWorkMock.Setup(x => x.UserRepo.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>())).ReturnsAsync(user);
            var result = await _redeemService.redeemItem(redeem.Id, user.Id);
            Assert.False(result.IsSuccess);
            Assert.Equal("Your score is not enough!", result.ErrorMessage);
        }

        [Fact]
        public async Task redeemItem_ReturnsNotFound_WhenScoreItemNotFound()
        {
            var redeem = new Redeem { Id = Guid.NewGuid(), TotalScore = 10, ScoreOrders = new List<ScoreOrder> { new ScoreOrder { ScoreItemId = Guid.NewGuid(), Quantity = 1 } } };
            var user = new AppUser { Id = Guid.NewGuid(), Score = 100 };
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).ReturnsAsync(redeem);
            _unitOfWorkMock.Setup(x => x.UserRepo.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.ScoreItemRepo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ScoreItem)null);
            var result = await _redeemService.redeemItem(redeem.Id, user.Id);
            Assert.False(result.IsSuccess);
            Assert.Equal("Score item not found", result.ErrorMessage);
        }

        [Fact]
        public async Task redeemItem_ReturnsBadRequest_OnException()
        {
            _unitOfWorkMock.Setup(x => x.redeemRepo.GetAsync(It.IsAny<Expression<Func<Redeem, bool>>>())).Throws(new Exception("fail"));
            var result = await _redeemService.redeemItem(Guid.NewGuid(), Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }
    }
}
