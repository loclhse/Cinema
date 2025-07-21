using Application;
using Application.IRepos;
using Application.Services;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using System.Net;
using Xunit;

public class MovieServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ILogger<MovieService> _logger;
    private readonly MovieService _movieService;

    public MovieServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _logger = new LoggerFactory().CreateLogger<MovieService>();
        _movieService = new MovieService(_logger, _mockUnitOfWork.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CreateMovieAsync_ShouldReturnBadRequest_IfGenreIdsIsNull()
    {
        var request = new MovieRequest { GenreIds = null };

        var result = await _movieService.CreateMovieAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateMovieAsync_ShouldReturnOk_IfMovieCreated()
    {
        var genreId = Guid.NewGuid();
        var request = new MovieRequest
        {
            Title = "Sample Movie",
            GenreIds = new List<Guid> { genreId }
        };

        var movie = new Movie();
        _mockMapper.Setup(m => m.Map<Movie>(request)).Returns(movie);
        _mockUnitOfWork.Setup(u => u.GenreRepo.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Genre, bool>>>())).ReturnsAsync(new Genre());
        _mockUnitOfWork.Setup(u => u.MovieRepo.AddAsync(It.IsAny<Movie>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _movieService.CreateMovieAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task DeleteMovieAsync_ShouldReturnNotFound_IfMovieNotExist()
    {
        var id = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>())).ReturnsAsync((Movie)null);

        var result = await _movieService.DeleteMovieAsync(id);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteMovieAsync_ShouldReturnOk_IfDeleted()
    {
        var id = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new Movie());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _movieService.DeleteMovieAsync(id);

        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetAllMoviesAsync_ShouldReturnNotFound_WhenNoMovies()
    {
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAllAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new List<Movie>());

        var result = await _movieService.GetAllMoviesAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetAllMoviesAsync_ShouldReturnOk_WhenFound()
    {
        var movie = new Movie { Id = Guid.NewGuid(), Title = "Movie A" };
        var movieResp = new MovieResponse { Title = "Movie A" };

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAllAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new List<Movie> { movie });
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetGenreNamesForMovieAsync(movie.Id))
            .ReturnsAsync(new List<string> { "Action" });
        _mockMapper.Setup(m => m.Map<MovieResponse>(movie)).Returns(movieResp);

        var result = await _movieService.GetAllMoviesAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetMovieByIdAsync_ShouldReturnNotFound_IfNotFound()
    {
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync((Movie)null);

        var result = await _movieService.GetMovieByIdAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task GetMovieByIdAsync_ShouldReturnOk_IfFound()
    {
        var movie = new Movie { Id = Guid.NewGuid(), Title = "Test" };
        var movieResp = new MovieResponse { Title = "Test" };

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(movie);
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetGenreNamesForMovieAsync(movie.Id))
            .ReturnsAsync(new List<string> { "Drama" });
        _mockMapper.Setup(m => m.Map<MovieResponse>(movie)).Returns(movieResp);

        var result = await _movieService.GetMovieByIdAsync(movie.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task SearchMoviesAsync_ShouldReturnBadRequest_IfSearchTypeInvalid()
    {
        var result = await _movieService.SearchMoviesAsync("test", "invalid");

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task SearchMoviesAsync_ShouldReturnOk_IfFound()
    {
        var movie = new Movie { Id = Guid.NewGuid(), Title = "Avengers" };
        var movieResp = new MovieResponse { Title = "Avengers" };

        _mockUnitOfWork.Setup(u => u.MovieRepo.SearchMoviesAsync(
                It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<Movie> { movie });

        _mockMapper.Setup(m => m.Map<List<MovieResponse>>(It.IsAny<IEnumerable<Movie>>()))
            .Returns(new List<MovieResponse> { movieResp });

        var result = await _movieService.SearchMoviesAsync("Avengers", "title");

        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    [Fact]
    public async Task UpdateMovieAsync_ShouldReturnNotFound_WhenMovieDoesNotExist()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movieRequest = new MovieRequest { GenreIds = new List<Guid> { Guid.NewGuid() } };

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync((Movie)null); // Giả lập không tìm thấy movie

        // Act
        var result = await _movieService.UpdateMovieAsync(movieId, movieRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateMovieAsync_ShouldReturnBadRequest_WhenGenreIdsAreEmpty()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movieRequest = new MovieRequest { GenreIds = new List<Guid>() };

        // Giả lập movie tồn tại
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new Movie { Id = movieId });

        // Act
        var result = await _movieService.UpdateMovieAsync(movieId, movieRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateMovieAsync_ShouldReturnOk_WhenMovieUpdatedSuccessfully()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movieRequest = new MovieRequest { GenreIds = new List<Guid> { Guid.NewGuid() } };
        var movie = new Movie { Id = movieId };

        // Giả lập movie tồn tại
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(movie);

        // Giả lập genre tồn tại
        _mockUnitOfWork.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>()))
            .ReturnsAsync(new Genre { Id = movieRequest.GenreIds[0] });

        // Giả lập phương thức thêm genre
        _mockUnitOfWork.Setup(u => u.MovieGenreRepo.AddAsync(It.IsAny<MovieGenre>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _movieService.UpdateMovieAsync(movieId, movieRequest);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("Movie updated successfully.", result.Result);
    }
    [Fact]
    public async Task UpdateMovieAsync_ShouldReturnBadRequest_WhenInvalidGenreIdProvided()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movieRequest = new MovieRequest { GenreIds = new List<Guid> { Guid.Empty } }; // GenreId không hợp lệ

        // Giả lập movie tồn tại
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new Movie { Id = movieId });

        // Act
        var result = await _movieService.UpdateMovieAsync(movieId, movieRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateMovieAsync_ShouldReturnBadRequest_WhenGenreNotFound()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var genreId = Guid.NewGuid();
        var movieRequest = new MovieRequest { GenreIds = new List<Guid> { genreId } };
        var movie = new Movie { Id = movieId };

        // Giả lập movie tồn tại
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(movie);

        // Giả lập không tìm thấy genre
        _mockUnitOfWork.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>()))
            .ReturnsAsync((Genre)null);

        // Act
        var result = await _movieService.UpdateMovieAsync(movieId, movieRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateMovieAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movieRequest = new MovieRequest { GenreIds = new List<Guid> { Guid.NewGuid() } };
        var movie = new Movie { Id = movieId };

        // Giả lập movie tồn tại
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(movie);

        // Giả lập gây ra ngoại lệ khi xóa genre
        _mockUnitOfWork.Setup(u => u.MovieGenreRepo.RemoveByIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _movieService.UpdateMovieAsync(movieId, movieRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }
    [Fact]
    public async Task UpdateMovieAsync_ShouldReturnBadRequest_WhenGenreIdsAreNullOrEmpty()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movieRequest = new MovieRequest { GenreIds = null }; 

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new Movie { Id = movieId }); 

        // Act
        var result = await _movieService.UpdateMovieAsync(movieId, movieRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }
    [Fact]
    public async Task UpdateMovieAsync_ShouldReturnBadRequest_WhenGenreIdsAreNull()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movieRequest = new MovieRequest { GenreIds = null }; // GenreIds là null

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new Movie { Id = movieId }); // Giả lập movie tồn tại

        // Act
        var result = await _movieService.UpdateMovieAsync(movieId, movieRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }

}

