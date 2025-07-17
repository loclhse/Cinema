using Application;
using Application.Services;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Application.IRepos;

namespace ZTest.Services
{
    public class ScoreItemServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IScoreItemRepo> _mockScoreItemRepo;
        private readonly IMapper _mapper;
        private readonly ScoreItemService _service;

        public ScoreItemServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockScoreItemRepo = new Mock<IScoreItemRepo>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ItemRequest, ScoreItem>();
                cfg.CreateMap<ScoreItem, ItemResponse>();
            });
            _mapper = config.CreateMapper();

            _mockUnitOfWork.Setup(u => u.ScoreItemRepo).Returns(_mockScoreItemRepo.Object);
            _service = new ScoreItemService(_mockUnitOfWork.Object, _mapper);
        }

        [Fact]
        public async Task CreateNewItemAsync_ReturnsBadRequest_WhenQuantityIsZero()
        {
            var request = new ItemRequest { Quantity = 0 };
            var result = await _service.CreateNewItemAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal("Quantity must be greater than 0", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateNewItemAsync_ReturnsOk_WhenValidRequest()
        {
            var request = new ItemRequest { Name = "Test", Quantity = 5 };
            _mockScoreItemRepo.Setup(r => r.AddAsync(It.IsAny<ScoreItem>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.CreateNewItemAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task DeleteItemAsync_ReturnsNotFound_WhenItemNotExist()
        {
            _mockScoreItemRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ScoreItem, bool>>>()))
                              .ReturnsAsync((ScoreItem)null);

            var result = await _service.DeleteItemAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal("Item not found", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteItemAsync_ReturnsOk_WhenItemExists()
        {
            var item = new ScoreItem { Id = Guid.NewGuid(), IsDeleted = false };
            _mockScoreItemRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ScoreItem, bool>>>()))
                              .ReturnsAsync(item);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.DeleteItemAsync(item.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task GetAllItemsAsync_ReturnsNotFound_WhenEmpty()
        {
            _mockScoreItemRepo.Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<ScoreItem, bool>>>(), null, 1, 5))
                .ReturnsAsync(new List<ScoreItem>());

            var result = await _service.GetAllItemsAsync(1, 5);

            Assert.False(result.IsSuccess);
            Assert.Equal("No items found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllItemsAsync_ReturnsOk_WhenHasData()
        {
            var items = new List<ScoreItem> { new ScoreItem { Id = Guid.NewGuid(), Name = "Item1", Quantity = 10 } };

            _mockScoreItemRepo.Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<ScoreItem, bool>>>(), null, 1, 5))
                .ReturnsAsync(items);

            var result = await _service.GetAllItemsAsync(1, 5);

            Assert.True(result.IsSuccess);
            var data = Assert.IsType<List<ItemResponse>>(result.Result);
            Assert.Single(data);
        }

        [Fact]
        public async Task GetItemByIdAsync_ReturnsNotFound_WhenNotExist()
        {
            _mockScoreItemRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ScoreItem, bool>>>()))
                .ReturnsAsync((ScoreItem)null);

            var result = await _service.GetItemByIdAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal("Item not found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetItemByIdAsync_ReturnsOk_WhenExists()
        {
            var item = new ScoreItem { Id = Guid.NewGuid(), Name = "Sample" };
            _mockScoreItemRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ScoreItem, bool>>>()))
                .ReturnsAsync(item);

            var result = await _service.GetItemByIdAsync(item.Id);

            Assert.True(result.IsSuccess);
            var data = Assert.IsType<ItemResponse>(result.Result);
            Assert.Equal("Sample", data.Name);
        }

        [Fact]
        public async Task UpdateItemAsync_ReturnsNotFound_WhenNotExist()
        {
            _mockScoreItemRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ScoreItem, bool>>>()))
                .ReturnsAsync((ScoreItem)null);

            var result = await _service.UpdateItemAsync(Guid.NewGuid(), new ItemRequest());

            Assert.False(result.IsSuccess);
            Assert.Equal("Item not found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateItemAsync_ReturnsBadRequest_WhenQuantityIsZero()
        {
            var item = new ScoreItem { Id = Guid.NewGuid() };
            _mockScoreItemRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ScoreItem, bool>>>()))
                .ReturnsAsync(item);

            var result = await _service.UpdateItemAsync(item.Id, new ItemRequest { Quantity = 0 });

            Assert.False(result.IsSuccess);
            Assert.Equal("Quantity must be greater than 0", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateItemAsync_ReturnsOk_WhenValid()
        {
            var item = new ScoreItem { Id = Guid.NewGuid(), Name = "OldName", Quantity = 10 };
            _mockScoreItemRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ScoreItem, bool>>>()))
                .ReturnsAsync(item);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var request = new ItemRequest { Name = "UpdatedName", Quantity = 5 };

            var result = await _service.UpdateItemAsync(item.Id, request);

            Assert.True(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }
    }
}
