using BoardcampApiCS.Resourses.Games;
using BoardcampApiCS.Resourses.Games.Models;
using BoardcampApiCS.Resourses.Games.Interfaces;
using Moq;
using BoardcampApiCS.Errors;

namespace BoardcampApiCSTest.Unitaries;

public class GamesServiceUnitTest
{
  private readonly GamesService _gamesService;
  private readonly Mock<IGamesRepository> _gamesRepositoryMock;
  private readonly List<Game> _games = new()
  {
    new Game { Id = 1, Name = "chess", Image = "chess.jpg", PricePerDay = 3.99M, StockTotal = 10 },
    new Game { Id = 2, Name = "poker", Image = "poker.jpg", PricePerDay = 4.99M, StockTotal = 15 },
    new Game { Id = 3, Name = "uno", Image = "uno.jpg", PricePerDay = 2.99M, StockTotal = 5 }
  };

  public GamesServiceUnitTest()
  {
    _gamesRepositoryMock = new Mock<IGamesRepository>();

    _gamesRepositoryMock.Setup(x => x.GetGameByName(It.IsAny<string>()))
      .ReturnsAsync((string name) => _games.FirstOrDefault((game) => game.Name == name));

    _gamesService = new GamesService(_gamesRepositoryMock.Object);
  }

  [Fact(DisplayName = "Create Game - It should return ConflictError if name already in use")]
  public async Task CreateGameTest()
  {
    var game = new Game { Name = "uno", Image = "uno.jpg", PricePerDay = 2.99M, StockTotal = 5 };
    await Assert.ThrowsAsync<ConflictError>(async () => await _gamesService.CreateGame(game));
  }
}