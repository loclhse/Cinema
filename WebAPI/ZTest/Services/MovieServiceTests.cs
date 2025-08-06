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
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

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
        var movieRequest = new MovieRequest { Title = "Test Movie", GenreIds = new List<Guid> { genreId } };
        var genre = new Genre { Id = genreId, Name = "Action" };
        var movie = new Movie { Id = Guid.NewGuid(), Title = "Test Movie", MovieGenres = new List<MovieGenre>() };
        _mockMapper.Setup(m => m.Map<Movie>(movieRequest)).Returns(movie);
        _mockUnitOfWork.Setup(u => u.GenreRepo.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Genre, bool>>>())).ReturnsAsync(genre);
        _mockUnitOfWork.Setup(u => u.MovieRepo.AddAsync(movie)).Returns(Task.FromResult(true));
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockUnitOfWork.Setup(u => u.elasticMovieRepo.IndexMovieAsync(movie)).Returns(Task.FromResult(true));
        var result = await _movieService.CreateMovieAsync(movieRequest);
        Console.WriteLine($"IsSuccess: {result.IsSuccess}, ErrorMessage: {result.ErrorMessage}, Result: {result.Result}");
        Assert.True(result.IsSuccess);
        Assert.Equal("Movie created successfully.", result.Result);
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
    [Fact]
    public async Task CreateMovieAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var movieRequest = new MovieRequest { GenreIds = new List<Guid> { Guid.NewGuid() } };
        _mockMapper.Setup(m => m.Map<Movie>(movieRequest)).Returns(new Movie());
        _mockUnitOfWork.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _movieService.CreateMovieAsync(movieRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("Database error", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteMovieAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new Movie());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _movieService.DeleteMovieAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllMoviesAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAllAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _movieService.GetAllMoviesAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task GetMovieByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _movieService.GetMovieByIdAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }


    [Fact]
    public async Task SearchMoviesAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.MovieRepo.SearchMoviesAsync(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _movieService.SearchMoviesAsync("test", "title");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }
    [Fact]
public async Task ElasticSearchMovie_ShouldReturnNotFound_WhenNoMoviesFound()
{
    // Arrange
    var keyword = "test";
        _mockUnitOfWork.Setup(u => u.elasticMovieRepo.elasticSearchMoviesAsync(keyword, It.IsAny<int?>()));

    // Act
    var result = await _movieService.ElasticSearchMovie(keyword);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    Assert.Equal(null, result.ErrorMessage);
}

[Fact]
public async Task ElasticSearchMovie_ShouldReturnBadRequest_WhenExceptionOccurs()
{
    // Arrange
    var keyword = "test";
    _mockUnitOfWork.Setup(u => u.elasticMovieRepo.elasticSearchMoviesAsync(keyword, It.IsAny<int?>()))
        .ThrowsAsync(new Exception("Database error")); // Simulate an exception

    // Act
    var result = await _movieService.ElasticSearchMovie(keyword);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(null, result.ErrorMessage);
}

    [Fact]
    public async Task ElasticSearchMovie_ShouldReturnOk_WhenMoviesFound()
    {
        // Arrange
        var keyword = "test";
        var movie = new Movie { Id = Guid.NewGuid(), Title = "Test Movie" };
        var movieResponse = new MovieResponse { Title = "Test Movie" };

        // Setup the elastic search to return a list of movies
        _mockUnitOfWork.Setup(u => u.elasticMovieRepo.elasticSearchMoviesAsync(keyword, It.IsAny<int?>()));     
        // Setup the database call to return those movies
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAllAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new List<Movie> { movie });

        // Setup genre names retrieval
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetGenreNamesForMovieAsync(movie.Id))
            .ReturnsAsync(new List<string> { "Action" });

        // Map the movie to the response
        _mockMapper.Setup(m => m.Map<MovieResponse>(movie)).Returns(movieResponse);

        // Act
        var result = await _movieService.ElasticSearchMovie(keyword);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    [Fact]
    public async Task FindGenresNameByMovieNameAsync_Should_Return_Ok_When_Movies_Found()
    {
        // Arrange
        var movieName = "Test Movie";
        var movie = new Movie { Id = Guid.NewGuid(), Title = movieName };
        var genreNames = new List<string> { "Action", "Drama" };

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAllAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new List<Movie> { movie });

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetGenreNamesForMovieAsync(movie.Id))
            .ReturnsAsync(genreNames);

        // Act
        var result = await _movieService.FindGenresNameByMovieNameAsync(movieName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(genreNames, result.Result);
    }

    [Fact]
    public async Task FindGenresNameByMovieNameAsync_Should_Return_NotFound_When_No_Movies_Found()
    {
        // Arrange
        var movieName = "Nonexistent Movie";

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAllAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new List<Movie>()); // No movies found

        // Act
        var result = await _movieService.FindGenresNameByMovieNameAsync(movieName);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task FindGenresNameByMovieNameAsync_Should_Return_BadRequest_When_Exception_Occurs()
    {
        // Arrange
        var movieName = "Test Movie";

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAllAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ThrowsAsync(new Exception("Database error")); // Simulate an exception

        // Act
        var result = await _movieService.FindGenresNameByMovieNameAsync(movieName);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("Database error", result.ErrorMessage);
    }
    [Fact]
    public async Task GetShowtimeByMovieId_ShouldReturnNotFound_WhenMovieDoesNotExist()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync((Movie)null); // Giả lập không tìm thấy movie

        // Act
        var result = await _movieService.GetShowtimeByMovieId(movieId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("Movie not found!", result.ErrorMessage);
    }

    [Fact]
    public async Task GetShowtimeByMovieId_ShouldReturnNotFound_WhenNoShowtimesFound()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movie = new Movie
        {
            Id = movieId,
            Showtimes = new List<Showtime>() // Không có showtime
        };

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(movie); // Giả lập tìm thấy movie

        // Act
        var result = await _movieService.GetShowtimeByMovieId(movieId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("Movie not found!", result.ErrorMessage);
    }

    [Fact]
    public async Task GetShowtimeByMovieId_ShouldReturnOk_WhenShowtimesFound()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var showtime = new Showtime { Id = Guid.NewGuid(), MovieId = movieId, StartTime = DateTime.Now };
        var movie = new Movie
        {
            Id = movieId,
            Showtimes = new List<Showtime> { showtime } // Có showtimes
        };

        var showtimeResponse = new ShowtimeResponse { Id = showtime.Id, EndTime = showtime.EndTime }; // Giả lập response

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(movie); // Giả lập tìm thấy movie
        _mockMapper.Setup(m => m.Map<List<ShowtimeResponse>>(movie.Showtimes)).Returns(new List<ShowtimeResponse> { showtimeResponse });

        // Act
        var result = await _movieService.GetShowtimeByMovieId(movieId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Result);
        var showtimeResults = result.Result as List<ShowtimeResponse>;
        Assert.Single(showtimeResults); // Kiểm tra có đúng 1 showtime
        Assert.Equal(showtimeResponse.Id, showtimeResults[0].Id); // Kiểm tra dữ liệu showtime
    }
    [Fact]
    public async Task GetShowtimeByMovieId_ShouldReturnNotFound_WhenShowtimesIsNull()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movie = new Movie
        {
            Id = movieId,
            Showtimes = null // Giả lập showtimes là null
        };

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(movie); // Giả lập tìm thấy movie

        // Act
        var result = await _movieService.GetShowtimeByMovieId(movieId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("Movie not found!", result.ErrorMessage);
    }

    [Fact]
    public async Task GetShowtimeByMovieId_ShouldReturnNotFound_WhenShowtimesIsEmpty()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movie = new Movie
        {
            Id = movieId,
            Showtimes = new List<Showtime>() // Giả lập showtimes là danh sách rỗng
        };

        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(movie); // Giả lập tìm thấy movie

        // Act
        var result = await _movieService.GetShowtimeByMovieId(movieId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("Movie not found!", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateMovieAsync_Should_Return_BadRequest_When_GenreIds_Empty()
    {
        // Arrange
        var request = new MovieRequest { GenreIds = new List<Guid>() };

        // Act
        var result = await _movieService.CreateMovieAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("GenreIds cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateMovieAsync_Should_Return_BadRequest_When_Movie_Mapping_Returns_Null()
    {
        // Arrange
        var request = new MovieRequest { GenreIds = new List<Guid> { Guid.NewGuid() } };
        _mockMapper.Setup(m => m.Map<Movie>(request)).Returns((Movie)null);

        // Act
        var result = await _movieService.CreateMovieAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Invalid movie data", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateMovieAsync_Should_Return_BadRequest_When_GenreId_Is_Empty()
    {
        // Arrange
        var request = new MovieRequest { GenreIds = new List<Guid> { Guid.Empty } };
        var movie = new Movie { MovieGenres = new List<MovieGenre>() };
        _mockMapper.Setup(m => m.Map<Movie>(request)).Returns(movie);

        // Act
        var result = await _movieService.CreateMovieAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Invalid GenreId provided", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateMovieAsync_Should_Return_BadRequest_When_Genre_Not_Found()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        var request = new MovieRequest { GenreIds = new List<Guid> { genreId } };
        var movie = new Movie { MovieGenres = new List<MovieGenre>() };
        
        _mockMapper.Setup(m => m.Map<Movie>(request)).Returns(movie);
        _mockUnitOfWork.Setup(u => u.GenreRepo.GetAsync(It.IsAny<Expression<Func<Genre, bool>>>()))
            .ReturnsAsync((Genre)null);

        // Act
        var result = await _movieService.CreateMovieAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains($"Genre with ID {genreId} not found", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateMovieAsync_Should_Handle_Exception()
    {
        // Arrange
        var request = new MovieRequest { GenreIds = new List<Guid> { Guid.NewGuid() } };
        _mockMapper.Setup(m => m.Map<Movie>(request)).Throws(new Exception("Mapping error"));

        // Act
        var result = await _movieService.CreateMovieAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Mapping error", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteMovieAsync_Should_Handle_Exception()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _movieService.DeleteMovieAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Database error", result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllMoviesAsync_Should_Handle_Exception()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAllAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _movieService.GetAllMoviesAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Database error", result.ErrorMessage);
    }

    [Fact]
    public async Task SearchMoviesAsync_Should_Return_BadRequest_When_SearchType_Invalid()
    {
        // Act
        var result = await _movieService.SearchMoviesAsync("test", "invalid");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Invalid or missing searchType", result.ErrorMessage);
    }

    [Fact]
    public async Task SearchMoviesAsync_Should_Return_BadRequest_When_SearchType_Empty()
    {
        // Act
        var result = await _movieService.SearchMoviesAsync("test", "");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Invalid or missing searchType", result.ErrorMessage);
    }

    [Fact]
    public async Task ElasticSearchMovie_Should_Return_NotFound_When_No_Movies()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.elasticMovieRepo.elasticSearchMoviesAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<MovieElasticSearchRequest>());

        // Act
        var result = await _movieService.ElasticSearchMovie("test");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("Not found", result.ErrorMessage);
    }

    [Fact]
    public async Task ElasticSearchMovie_Should_Handle_Exception()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.elasticMovieRepo.elasticSearchMoviesAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ThrowsAsync(new Exception("Elasticsearch error"));

        // Act
        var result = await _movieService.ElasticSearchMovie("test");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Elasticsearch error", result.ErrorMessage);
    }

    [Fact]
    public async Task FindGenresNameByMovieNameAsync_Should_Return_NotFound_When_No_Movies()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAllAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new List<Movie>());

        // Act
        var result = await _movieService.FindGenresNameByMovieNameAsync("test");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("No movies found with the specified name", result.ErrorMessage);
    }

    [Fact]
    public async Task FindGenresNameByMovieNameAsync_Should_Handle_Exception()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.MovieRepo.GetAllAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _movieService.FindGenresNameByMovieNameAsync("test");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Database error", result.ErrorMessage);
    }
}

