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
    }
}
