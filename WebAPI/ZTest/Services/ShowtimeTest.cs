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
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ZTest.Services
{
    public class ShowtimeTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ShowtimeService _service;
        public ShowtimeTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _service = new ShowtimeService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateShowtimeAsync_Should_Return_BadRequest_When_Overlapping()
        {
            var movie = new Movie { Id = Guid.NewGuid(), Duration = 120 };
            var room = new CinemaRoom { Id = Guid.NewGuid(), Name = "Room A" };
            var request = new ShowtimeResquest
            {
                StartTime = new DateTime(2025, 7, 3, 14, 0, 0),
                Date = DateOnly.FromDateTime(new DateTime(2025, 7, 3))
            };

            var existingShowtime = new Showtime
            {
                StartTime = new DateTime(2025, 7, 3, 13, 0, 0),
                EndTime = new DateTime(2025, 7, 3, 15, 0, 0),
                CinemaRoomId = room.Id,
                Date = request.Date
            };

            _mapperMock.Setup(m => m.Map<Showtime>(request)).Returns(new Showtime { StartTime = request.StartTime, Date = request.Date });
            _unitOfWorkMock.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>())).ReturnsAsync(movie);
            _unitOfWorkMock.Setup(u => u.CinemaRoomRepo.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>())).ReturnsAsync(room);
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAllAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync(new List<Showtime> { existingShowtime });

            var result = await _service.CreateShowtimeAsync(request, movie.Id, room.Id);

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteShowtimeAsync_Should_Mark_As_Deleted()
        {
            var showtime = new Showtime { Id = Guid.NewGuid() };

            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync(showtime);

            var result = await _service.DeleteShowtimeAsync(showtime.Id);

            Assert.True(showtime.IsDeleted);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetAllShowtimesAsync_Should_Return_Showtimes()
        {
            var showtimes = new List<Showtime> { new Showtime { Id = Guid.NewGuid() } };

            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAllAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync(showtimes);
            _mapperMock.Setup(m => m.Map<List<ShowtimeResponse>>(showtimes)).Returns(new List<ShowtimeResponse> { new ShowtimeResponse { Id = showtimes[0].Id } });

            var result = await _service.GetAllShowtimesAsync();

            Assert.True(result.IsSuccess);
            Assert.Single((List<ShowtimeResponse>)result.Result);
        }

        [Fact]
        public async Task GetShowtimeByIdAsync_Should_Return_Showtime_When_Exists()
        {
            var showtime = new Showtime { Id = Guid.NewGuid(), CinemaRoomId = Guid.NewGuid() };
            var room = new CinemaRoom { Id = showtime.CinemaRoomId, Name = "Room 1" };

            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync(showtime);
            _unitOfWorkMock.Setup(u => u.CinemaRoomRepo.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>())).ReturnsAsync(room);
            _mapperMock.Setup(m => m.Map<RoomShowtimeResponse>(showtime)).Returns(new RoomShowtimeResponse { RoomName = room.Name });

            var result = await _service.GetShowtimeByIdAsync(showtime.Id);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateShowtimeAsync_Should_Update_And_Save()
        {
            var id = Guid.NewGuid();
            var showtime = new Showtime { Id = id, StartTime = DateTime.Now, Date = DateOnly.FromDateTime(DateTime.Today) };
            var request = new ShowtimeUpdateRequest { StartTime = DateTime.Now.AddHours(1), Date = DateOnly.FromDateTime(DateTime.Today), CinemaRoomId = Guid.NewGuid(), MovieId = Guid.NewGuid() };

            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync(showtime);
            _unitOfWorkMock.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>())).ReturnsAsync(new Movie { Duration = 120 });
            _unitOfWorkMock.Setup(u => u.CinemaRoomRepo.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>())).ReturnsAsync(new CinemaRoom());
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAllAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync(new List<Showtime>());

            _mapperMock.Setup(m => m.Map(request, showtime));

            var result = await _service.UpdateShowtimeAsync(id, request);

            Assert.True(result.IsSuccess);
            Assert.Equal("Updated successfully!", result.Result);
        }

        [Fact]
        public async Task GetShowtimeByMovieIdAsync_Should_Return_Showtimes()
        {
            var movieId = Guid.NewGuid();
            var showtimes = new List<Showtime> { new Showtime { Id = Guid.NewGuid(), CinemaRoomId = Guid.NewGuid() } };
            var cinemaRoom = new CinemaRoom { Id = showtimes[0].CinemaRoomId, Name = "Room A" };

            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAllAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync(showtimes);
            _unitOfWorkMock.Setup(u => u.CinemaRoomRepo.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>())).ReturnsAsync(cinemaRoom);
            _mapperMock.Setup(m => m.Map<List<MovieTimeResponse>>(showtimes)).Returns(new List<MovieTimeResponse> { new MovieTimeResponse { CinemaRoomId = cinemaRoom.Id } });

            var result = await _service.GetShowtimeByMovieIdAsync(movieId);

            Assert.True(result.IsSuccess);
        }
        [Fact]
        public async Task CreateShowtimeAsync_Should_Return_BadRequest_On_Exception()
        {
            var request = new ShowtimeResquest { StartTime = DateTime.Now, Date = DateOnly.FromDateTime(DateTime.Today) };

            _mapperMock.Setup(m => m.Map<Showtime>(request)).Throws(new Exception("Mapping failed"));

            var result = await _service.CreateShowtimeAsync(request, Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }
        [Fact]
        public async Task DeleteShowtimeAsync_Should_Return_BadRequest_On_Exception()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>()))
                .Throws(new Exception("DB error"));

            var result = await _service.DeleteShowtimeAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }
        [Fact]
        public async Task GetAllShowtimesAsync_Should_Return_BadRequest_On_Exception()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAllAsync(It.IsAny<Expression<Func<Showtime, bool>>>()))
                .Throws(new Exception("GetAll error"));

            var result = await _service.GetAllShowtimesAsync();

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }
        [Fact]
        public async Task GetShowtimeByIdAsync_Should_Return_BadRequest_On_Exception()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>()))
                .Throws(new Exception("Get by ID error"));

            var result = await _service.GetShowtimeByIdAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateShowtimeAsync_Should_Return_BadRequest_On_Exception()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>()))
                .Throws(new Exception("Update error"));

            var result = await _service.UpdateShowtimeAsync(Guid.NewGuid(), new ShowtimeUpdateRequest());

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }
        [Fact]
        public async Task GetShowtimeByMovieIdAsync_Should_Return_BadRequest_On_Exception()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAllAsync(It.IsAny<Expression<Func<Showtime, bool>>>()))
                .Throws(new Exception("MovieId error"));

            var result = await _service.GetShowtimeByMovieIdAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateShowtimeAsync_Should_Return_NotFound_When_Showtime_Not_Found()
        {
            // Arrange
            var showtimeId = Guid.NewGuid();
            var showtimeUpdateRequest = new ShowtimeUpdateRequest { MovieId = Guid.NewGuid(), CinemaRoomId = Guid.NewGuid() };

            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>()))
                .ReturnsAsync((Showtime)null); // Simulate showtime not found

            // Act
            var result = await _service.UpdateShowtimeAsync(showtimeId, showtimeUpdateRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task GetShowtimeByIdAsync_Should_Return_NotFound_When_Showtime_Not_Found()
        {
            // Arrange
            var showtimeId = Guid.NewGuid();

            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>()))
                .ReturnsAsync((Showtime)null); // Simulate showtime not found

            // Act
            var result = await _service.GetShowtimeByIdAsync(showtimeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteShowtimeAsync_Should_Return_NotFound_When_Showtime_Not_Found()
        {
            // Arrange
            var showtimeId = Guid.NewGuid();

            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>()))
                .ReturnsAsync((Showtime)null); // Simulate showtime not found

            // Act
            var result = await _service.DeleteShowtimeAsync(showtimeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task GetShowtimeByMovieIdAsync_Should_Return_NotFound_When_No_Showtimes_For_Movie()
        {
            // Arrange
            var movieId = Guid.NewGuid();

            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAllAsync(It.IsAny<Expression<Func<Showtime, bool>>>()))
                .ReturnsAsync(new List<Showtime>()); // Simulate no showtimes found

            // Act
            var result = await _service.GetShowtimeByMovieIdAsync(movieId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }
    }
}
