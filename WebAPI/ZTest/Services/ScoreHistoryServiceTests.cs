using Application;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ZTest.Services
{
    public class ScoreHistoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ScoreHistoryService _service;

        public ScoreHistoryServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _service = new ScoreHistoryService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task DeleteHistory_Should_Return_Ok_When_ScoreLog_Exists()
        {
            // Arrange
            var scoreLogId = Guid.NewGuid();
            var scoreLog = new ScoreLog { Id = scoreLogId, IsDeleted = false };

            _mockUnitOfWork.Setup(u => u.ScoreLogRepo.GetAsync(It.IsAny<Expression<Func<ScoreLog, bool>>>()))
                .ReturnsAsync(scoreLog);

            // Act
            var result = await _service.DeleteHistory(Guid.NewGuid(), scoreLogId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Deleted", result.Result);
            Assert.True(scoreLog.IsDeleted);
        }

        [Fact]
        public async Task DeleteHistory_Should_Return_NotFound_When_ScoreLog_Does_Not_Exist()
        {
            // Arrange
            var scoreLogId = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.ScoreLogRepo.GetAsync(It.IsAny<Expression<Func<ScoreLog, bool>>>()))
                .ReturnsAsync((ScoreLog)null); // No score log found

            // Act
            var result = await _service.DeleteHistory(Guid.NewGuid(), scoreLogId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteHistory_Should_Return_BadRequest_When_Exception_Occurs()
        {
            // Arrange
            var scoreLogId = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.ScoreLogRepo.GetAsync(It.IsAny<Expression<Func<ScoreLog, bool>>>()))
                .ThrowsAsync(new Exception("Database error")); // Simulate exception

            // Act
            var result = await _service.DeleteHistory(Guid.NewGuid(), scoreLogId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ViewHistory_Should_Return_Ok_When_ScoreLogs_Exist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var scoreLogs = new List<ScoreLog> { new ScoreLog { UserId = userId } };

            _mockUnitOfWork.Setup(u => u.ScoreLogRepo.GetAllAsync(It.IsAny<Expression<Func<ScoreLog, bool>>>()))
                .ReturnsAsync(scoreLogs);

            // Act
            var result = await _service.ViewHistory(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(scoreLogs, result.Result);
        }

        [Fact]
        public async Task ViewHistory_Should_Return_NotFound_When_No_ScoreLogs_Exist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.ScoreLogRepo.GetAllAsync(It.IsAny<Expression<Func<ScoreLog, bool>>>()))
                .ReturnsAsync(new List<ScoreLog>()); // No score logs found

            // Act
            var result = await _service.ViewHistory(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task ViewHistory_Should_Return_BadRequest_When_Exception_Occurs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.ScoreLogRepo.GetAllAsync(It.IsAny<Expression<Func<ScoreLog, bool>>>()))
                .ThrowsAsync(new Exception("Database error")); // Simulate exception

            // Act
            var result = await _service.ViewHistory(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}
