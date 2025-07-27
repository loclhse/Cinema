using Application.Services;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Application.IServices;
using AutoMapper;
using Moq;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Common;
using Domain.Entities;
using Application.ViewModel;
using System.Linq.Expressions;
using Application;
using System.Net;

namespace ZTest.Services
{
    public class GenreServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IMapper> _mapper;
        private readonly GenreService _sut;

        public GenreServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _mapper = new Mock<IMapper>();
            _sut = new GenreService(_uow.Object, _mapper.Object);
        }

        /* Test CreateGenreAsync */
        [Fact]
        public async Task CreateGenreAsync_NewGenre_ShouldAddAndReturnResponse()
        {
            // Arrange
            var genreRequest = new GenreRequest { Name = "Action" };
            var genre = new Genre { Name = "Action" };
            var response = new ApiResp();

            _mapper.Setup(m => m.Map<Genre>(genreRequest)).Returns(genre);
            _uow.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>())).ReturnsAsync((Genre)null); // No genre found
            _uow.Setup(u => u.GenreRepo.AddAsync(It.IsAny<Genre>())).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateGenreAsync(genreRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorMessage);
            _uow.Verify(u => u.GenreRepo.AddAsync(genre), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateGenreAsync_ShouldReturnBadRequest_WhenGenreIsNull()
        {
            _mapper.Setup(m => m.Map<Genre>(It.IsAny<GenreRequest>())).Returns((Genre)null);
            var result = await _sut.CreateGenreAsync(new GenreRequest { Name = "Test" });
            Assert.False(result.IsSuccess);
            Assert.Equal("Enter Data please!!!", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateGenreAsync_ShouldReturnBadRequest_WhenGenreAlreadyExists()
        {
            var genreRequest = new GenreRequest { Name = "Action" };
            var genre = new Genre { Name = "Action" };
            _mapper.Setup(m => m.Map<Genre>(genreRequest)).Returns(genre);
            _uow.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>())).ReturnsAsync(genre);
            var result = await _sut.CreateGenreAsync(genreRequest);
            Assert.False(result.IsSuccess);
            Assert.Equal("Genre already exists!!!", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateGenreAsync_ShouldReturnBadRequest_OnException()
        {
            _mapper.Setup(m => m.Map<Genre>(It.IsAny<GenreRequest>())).Throws(new Exception("fail"));
            var result = await _sut.CreateGenreAsync(new GenreRequest { Name = "Test" });
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        /* Test DeleteGenreAsync */
        [Fact]
        public async Task DeleteGenreAsync_GenreExists_ShouldDelete()
        {
            // Arrange
            var genre = new Genre { Id = Guid.NewGuid(), Name = "Action", IsDeleted = false };
            _uow.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>())).ReturnsAsync(genre);

            // Act
            var result = await _sut.DeleteGenreAsync(genre.Id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorMessage);
            Assert.True(genre.IsDeleted);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteGenreAsync_ShouldReturnNotFound_WhenGenreNotFound()
        {
            _uow.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>())).ReturnsAsync((Genre)null);
            var result = await _sut.DeleteGenreAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("Genre not found!!!", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteGenreAsync_ShouldReturnBadRequest_OnException()
        {
            _uow.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>())).Throws(new Exception("fail"));
            var result = await _sut.DeleteGenreAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        /* Test GetAllGenresAsync */
        [Fact]
        public async Task GetAllGenresAsync_GenresExist_ShouldReturnGenres()
        {
            // Arrange
            var genres = new List<Genre> { new Genre { Id = Guid.NewGuid(), Name = "Action", IsDeleted = false } };
            var response = new List<GenreResponse> { new GenreResponse { Id = genres[0].Id, Name = "Action" } };

            _uow.Setup(u => u.GenreRepo.GetAllAsync(It.IsAny<Expression<Func<Genre, bool>>>())).ReturnsAsync(genres);
            _mapper.Setup(m => m.Map<List<GenreResponse>>(genres)).Returns(response);

            // Act
            var result = await _sut.GetAllGenresAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(response, result.Result);
        }

        [Fact]
        public async Task GetAllGenresAsync_ShouldReturnNotFound_WhenEmpty()
        {
            _uow.Setup(u => u.GenreRepo.GetAllAsync(It.IsAny<Expression<Func<Genre, bool>>>())).ReturnsAsync(new List<Genre>());
            _mapper.Setup(m => m.Map<List<GenreResponse>>(It.IsAny<List<Genre>>())).Returns(new List<GenreResponse>());
            var result = await _sut.GetAllGenresAsync();
            Assert.False(result.IsSuccess);
            Assert.Equal("No genres found.", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllGenresAsync_ShouldReturnBadRequest_OnException()
        {
            _uow.Setup(u => u.GenreRepo.GetAllAsync(It.IsAny<Expression<Func<Genre, bool>>>())).Throws(new Exception("fail"));
            var result = await _sut.GetAllGenresAsync();
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        /* Test UpdateGenreAsync */
        [Fact]
        public async Task UpdateGenreAsync_ExistingGenreUniqueName_ShouldUpdateSuccessfully()
        {
            // Arrange
            var genreId = Guid.NewGuid();
            var existingGenre = new Genre { Id = genreId, Name = "Action", IsDeleted = false };
            var updateRequest = new GenreRequest { Name = "Adventure" };

            _uow.SetupSequence(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>()))
                .ReturnsAsync(existingGenre)
                .ReturnsAsync((Genre)null);

            _mapper.Setup(m => m.Map(updateRequest, existingGenre)).Verifiable();

            // Act
            var resp = await _sut.UpdateGenreAsync(genreId, updateRequest);

            // Assert
            Assert.True(resp.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.ErrorMessage);

            _mapper.Verify(m => m.Map(updateRequest, existingGenre), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateGenreAsync_GenreNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var genreId = Guid.NewGuid();
            var updateRequest = new GenreRequest { Name = "Comedy" };

            _uow.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>()))
                .ReturnsAsync((Genre)null);

            // Act
            var resp = await _sut.UpdateGenreAsync(genreId, updateRequest);

            // Assert
            Assert.False(resp.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
            Assert.Equal("Genre does not exist!!", resp.ErrorMessage);

            _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateGenreAsync_ShouldReturnBadRequest_WhenNameExists()
        {
            var genreId = Guid.NewGuid();
            var existingGenre = new Genre { Id = genreId, Name = "Action", IsDeleted = false };
            var updateRequest = new GenreRequest { Name = "Adventure" };
            _uow.SetupSequence(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>()))
                .ReturnsAsync(existingGenre)
                .ReturnsAsync(new Genre { Name = "Adventure" });
            var resp = await _sut.UpdateGenreAsync(genreId, updateRequest);
            Assert.False(resp.IsSuccess);
            Assert.Equal("Genre already exists!!!", resp.ErrorMessage);
        }

        [Fact]
        public async Task UpdateGenreAsync_ShouldReturnBadRequest_OnException()
        {
            var genreId = Guid.NewGuid();
            _uow.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>())).Throws(new Exception("fail"));
            var resp = await _sut.UpdateGenreAsync(genreId, new GenreRequest { Name = "Test" });
            Assert.False(resp.IsSuccess);
            Assert.Equal("fail", resp.ErrorMessage);
        }

        /* Test GetGenresAsync */
        [Fact]
        public async Task GetGenresAsync_Should_Return_NotFound_When_Genre_Does_Not_Exist()
        {
            // Arrange
            var genreId = Guid.NewGuid();

            _uow.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>()))
                .ReturnsAsync((Genre)null);

            // Act
            var result = await _sut.GetGenresAsync(genreId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Genre does not exist!!", result.ErrorMessage);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task GetGenresAsync_Should_Return_BadRequest_When_Exception_Occurs()
        {
            // Arrange
            var genreId = Guid.NewGuid();
            _uow.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.GetGenresAsync(genreId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Database error", result.ErrorMessage);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetGenresAsync_Should_Return_Ok_When_Genre_Exists()
        {
            // Arrange
            var genreId = Guid.NewGuid();
            var genre = new Genre { Id = genreId, Name = "Action" };
            var genreResponse = new GenreResponse { Id = genreId, Name = "Action" };

            _uow.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>()))
                .ReturnsAsync(genre);
            _mapper.Setup(m => m.Map<GenreResponse>(genre)).Returns(genreResponse);

            // Act
            var result = await _sut.GetGenresAsync(genreId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
            Assert.Equal(genreResponse, result.Result);
        }
    }
}
