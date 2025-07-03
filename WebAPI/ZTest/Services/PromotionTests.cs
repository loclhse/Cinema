using Application;
using Application.IServices;
using Application.Services;
using Application.ViewModel;
using Application.ViewModel.Request;
using AutoMapper;
using Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ZTest.Services
{
    public class PromotionTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly PromotionService _promotionService;

        public PromotionTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _promotionService = new PromotionService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task AddPromotion_Success_ReturnsOk()
        {
            // Arrange
            var request = new EditPromotionRequest
            {
                Title = "Test Promo",
                Description = "Test Description",
                DiscountPercent = 10.0m,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7))
            };

            var promotion = new Promotion { Id = Guid.NewGuid() };
            _mockMapper.Setup(m => m.Map<Promotion>(request)).Returns(promotion);
            _mockUow.Setup(u => u.PromotionRepo.AddAsync(promotion)).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _promotionService.AddPromotion(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.ErrorMessage);
            Assert.Equal("Successfully updated", result.Result);
        }

        [Fact]
        public async Task AddPromotion_FailedSave_ReturnsBadRequest()
        {
            // Arrange
            var request = new EditPromotionRequest();
            var promotion = new Promotion();
            _mockMapper.Setup(m => m.Map<Promotion>(request)).Returns(promotion);
            _mockUow.Setup(u => u.PromotionRepo.AddAsync(promotion)).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _promotionService.AddPromotion(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Null(result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task AddPromotion_ThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var request = new EditPromotionRequest();
            _mockMapper.Setup(m => m.Map<Promotion>(request)).Throws(new Exception("Mapping error"));

            // Act
            var result = await _promotionService.AddPromotion(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Mapping error", result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task DeletePromotion_PromotionNotFound_ReturnsNotFound()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            _mockUow.Setup(u => u.PromotionRepo.GetPromotionById(promotionId)).ReturnsAsync((Promotion)null);

            // Act
            var result = await _promotionService.DeletePromotion(promotionId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal("Not found this Promotion", result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task DeletePromotion_Success_ReturnsOk()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var promotion = new Promotion { Id = promotionId, IsDeleted = false };
            _mockUow.Setup(u => u.PromotionRepo.GetPromotionById(promotionId)).ReturnsAsync(promotion);
            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _promotionService.DeletePromotion(promotionId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.ErrorMessage);
            Assert.Equal("Delete Successfully", result.Result);
            Assert.True(promotion.IsDeleted);
        }

        [Fact]
        public async Task EditPromotion_PromotionNotFound_ReturnsNotFound()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var request = new EditPromotionRequest();
            _mockUow.Setup(u => u.PromotionRepo.GetPromotionById(promotionId)).ReturnsAsync((Promotion)null);

            // Act
            var result = await _promotionService.EditPromotion(promotionId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal("Promotion not found", result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task EditPromotion_Success_ReturnsOk()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var request = new EditPromotionRequest { Title = "Updated Promo" };
            var promotion = new Promotion { Id = promotionId };
            _mockUow.Setup(u => u.PromotionRepo.GetPromotionById(promotionId)).ReturnsAsync(promotion);
            _mockMapper.Setup(m => m.Map(request, promotion)).Returns(promotion);
            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _promotionService.EditPromotion(promotionId, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(promotion, result.Result);
        }

        [Fact]
        public async Task GetAllPromotion_NoPromotions_ReturnsNotFound()
        {
            // Arrange
            _mockUow.Setup(u => u.PromotionRepo.GetAllPromotion()).ReturnsAsync(new List<Promotion>());

            // Act
            var result = await _promotionService.GetAllPromotion();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal("Not found any Promotion", result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task GetAllPromotion_HasPromotions_ReturnsOk()
        {
            // Arrange
            var promotions = new List<Promotion> { new Promotion { Id = Guid.NewGuid() } };
            _mockUow.Setup(u => u.PromotionRepo.GetAllPromotion()).ReturnsAsync(promotions);

            // Act
            var result = await _promotionService.GetAllPromotion();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(promotions, result.Result);
        }

        [Fact]
        public async Task GetPromotionById_PromotionNotFound_ReturnsNotFound()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            _mockUow.Setup(u => u.PromotionRepo.GetPromotionById(promotionId)).ReturnsAsync((Promotion)null);

            // Act
            var result = await _promotionService.GetPromotionById(promotionId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal("Not found this Promotion", result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task GetPromotionById_Success_ReturnsOk()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var promotion = new Promotion { Id = promotionId };
            _mockUow.Setup(u => u.PromotionRepo.GetPromotionById(promotionId)).ReturnsAsync(promotion);

            // Act
            var result = await _promotionService.GetPromotionById(promotionId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(promotion, result.Result);
        }
    }
}