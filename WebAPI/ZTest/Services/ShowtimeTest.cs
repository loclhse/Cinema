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
        public async Task CreateShowtimeAsync_Should_Return_BadRequest_When_Exception()
        {
            var request = new ShowtimeResquest();
            _mapperMock.Setup(m => m.Map<Showtime>(request)).Throws(new Exception("fail"));
            var result = await _service.CreateShowtimeAsync(request, Guid.NewGuid(), Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateShowtimeAsync_Should_Return_BadRequest_When_InvalidShowtime()
        {
            var request = new ShowtimeResquest();
            _mapperMock.Setup(m => m.Map<Showtime>(request)).Returns((Showtime)null);
            var result = await _service.CreateShowtimeAsync(request, Guid.NewGuid(), Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid showtime data.", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateShowtimeAsync_Should_Return_NotFound_When_NoSeats()
        {
            var movie = new Movie { Id = Guid.NewGuid(), Duration = 100 };
            var room = new CinemaRoom { Id = Guid.NewGuid(), Name = "Room A" };
            var request = new ShowtimeResquest { StartTime = DateTime.Now, Date = DateOnly.FromDateTime(DateTime.Today) };
            _mapperMock.Setup(m => m.Map<Showtime>(request)).Returns(new Showtime { StartTime = request.StartTime, Date = request.Date });
            _unitOfWorkMock.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>())).ReturnsAsync(movie);
            _unitOfWorkMock.Setup(u => u.CinemaRoomRepo.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>())).ReturnsAsync(room);
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAllAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync(new List<Showtime>());
            _unitOfWorkMock.Setup(u => u.SeatRepo.GetAllAsync(It.IsAny<Expression<Func<Seat, bool>>>())).ReturnsAsync(new List<Seat>());
            var result = await _service.CreateShowtimeAsync(request, movie.Id, room.Id);
            Assert.False(result.IsSuccess);
            Assert.Contains("No seats found", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteShowtimeAsync_Should_Return_NotFound_When_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync((Showtime)null);
            var result = await _service.DeleteShowtimeAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Contains("Showtime not found", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteShowtimeAsync_Should_Return_BadRequest_OnException()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).Throws(new Exception("fail"));
            var result = await _service.DeleteShowtimeAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllShowtimesAsync_Should_Return_NotFound_When_Empty()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAllAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync(new List<Showtime>());
            var result = await _service.GetAllShowtimesAsync();
            Assert.False(result.IsSuccess);
            Assert.Contains("No showtimes found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllShowtimesAsync_Should_Return_BadRequest_OnException()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAllAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).Throws(new Exception("fail"));
            var result = await _service.GetAllShowtimesAsync();
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task GetShowtimeByIdAsync_Should_Return_NotFound_When_Showtime_NotFound()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync((Showtime)null);
            var result = await _service.GetShowtimeByIdAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Contains("Showtime not found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetShowtimeByIdAsync_Should_Return_NotFound_When_Room_NotFound()
        {
            var showtime = new Showtime { Id = Guid.NewGuid(), CinemaRoomId = Guid.NewGuid() };
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).ReturnsAsync(showtime);
            _unitOfWorkMock.Setup(u => u.CinemaRoomRepo.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>())).ReturnsAsync((CinemaRoom)null);
            var result = await _service.GetShowtimeByIdAsync(showtime.Id);
            Assert.False(result.IsSuccess);
            Assert.Contains("Cinema room not found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetShowtimeByIdAsync_Should_Return_BadRequest_OnException()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).Throws(new Exception("fail"));
            var result = await _service.GetShowtimeByIdAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateShowtimeAsync_Should_Return_BadRequest_OnException()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).Throws(new Exception("fail"));
            var result = await _service.UpdateShowtimeAsync(Guid.NewGuid(), new ShowtimeUpdateRequest());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task GetShowtimeByMovieIdAsync_Should_Return_BadRequest_OnException()
        {
            _unitOfWorkMock.Setup(u => u.ShowtimeRepo.GetAllAsync(It.IsAny<Expression<Func<Showtime, bool>>>())).Throws(new Exception("fail"));
            var result = await _service.GetShowtimeByMovieIdAsync(Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }
    }
}
