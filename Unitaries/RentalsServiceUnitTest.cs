using BoardcampApiCS.Errors;
using BoardcampApiCS.Resourses.Customers.Interfaces;
using BoardcampApiCS.Resourses.Customers.Models;
using BoardcampApiCS.Resourses.Games.Interfaces;
using BoardcampApiCS.Resourses.Games.Models;
using BoardcampApiCS.Resourses.Rentals;
using BoardcampApiCS.Resourses.Rentals.Interfaces;
using BoardcampApiCS.Resourses.Rentals.Models;
using Moq;

namespace BoardcampApiCSTest.Unitaries;

public class RentalsServiceUnitTest
{
  private readonly RentalsService _rentalsService;
  private readonly Mock<IRentalsRepository> _rentalsRepository;
  private readonly Mock<IGamesRepository> _gamesRepository;
  private readonly Mock<ICustomersRepository> _customersRepository;
  private List<Game> _games = new() {
    new Game { Id = 1, StockTotal = 5},
    new Game { Id = 2, StockTotal = 2},
  };
  private List<Customer> _customers = new() {
    new Customer { Id = 1},
    new Customer { Id = 2},
  };
  private List<Rental> _rentals = new() {
    new Rental {
      Id = 1,
      CustomerId = 1, 
      GameId = 1, 
      ReturnDate = null, 
      RentDate = DateTime.Now, 
      DaysRented = 3, 
      Game = new Game{PricePerDay = 1}
    },
    new Rental {
      Id = 2,
      CustomerId = 1, 
      GameId = 1, 
      ReturnDate = DateTime.Now, 
      RentDate = DateTime.Now, 
      DaysRented = 3, 
      Game = new Game{PricePerDay = 1}
    },
    new Rental {Id = 3, CustomerId = 2, GameId = 1, ReturnDate = null},
    new Rental {Id = 4, CustomerId = 2, GameId = 2, ReturnDate = null},
    new Rental {Id = 5, CustomerId = 2, GameId = 2, ReturnDate = null},
  };

  public RentalsServiceUnitTest()
  {
    _rentalsRepository = new Mock<IRentalsRepository>();
    _gamesRepository = new Mock<IGamesRepository>();
    _customersRepository = new Mock<ICustomersRepository>();

    _rentalsRepository.Setup(x => x.GetRentalByIdAsync(It.IsAny<int>()))
      .ReturnsAsync((int id) => _rentals.FirstOrDefault(r => r.Id == id));
    _rentalsRepository.Setup(x => x.GetRentalsByGameIdWhereReturnNullAsync(It.IsAny<int>()))
      .ReturnsAsync((int id) => _rentals.Where(r => r.GameId == id && r.ReturnDate is null).ToList());

    _gamesRepository.Setup(x => x.GetGameById(It.IsAny<int>()))
      .ReturnsAsync((int id) => _games.FirstOrDefault(g => g.Id == id));

    _customersRepository.Setup(x => x.GetCustomerById(It.IsAny<int>()))
      .ReturnsAsync((int id) => _customers.FirstOrDefault(c => c.Id == id));

    _rentalsService = new RentalsService(_rentalsRepository.Object, _gamesRepository.Object, _customersRepository.Object);
  }

  [Fact(DisplayName =
    "Get Rental by Id - It should return NotFoundError if rental id does not existing")]
  public async Task GetRentalByIdAsync()
  {
    await Assert.ThrowsAsync<NotFoundError>
      (async () => await _rentalsService.GetRentalByIdAsync(9));
  }

  [Fact(DisplayName =
    "Create Rental - It should return BadRequestError if game id does not existing")]
  public async Task CreateRentalInvalidGameId()
  {
    var rental = new Rental { GameId = 10, CustomerId = 1 };
    await Assert.ThrowsAsync<BadRequestError>
      (async () => await _rentalsService.CreateRental(rental));
  }

  [Fact(DisplayName =
    "Create Rental - It should return BadRequestError if customer id does not existing")]
  public async Task CreateRentalInvalidCustomerId()
  {
    var rental = new Rental { GameId = 1, CustomerId = 10 };
    await Assert.ThrowsAsync<BadRequestError>
      (async () => await _rentalsService.CreateRental(rental));
  }

  [Fact(DisplayName =
    "Create Rental - It should return BadRequestError if insufficient stock")]
  public async Task CreateRentalInsufficientStock()
  {
    var rental = new Rental { GameId = 2, CustomerId = 1 };
    await Assert.ThrowsAsync<BadRequestError>
      (async () => await _rentalsService.CreateRental(rental));
  }

  [Fact(DisplayName =
    "Return Rental - It should return NotFoundError if rental id does not existing")]
  public async Task ReturnRentalInvalidRentalId()
  {
    await Assert.ThrowsAsync<NotFoundError>
      (async () => await _rentalsService.ReturnRentalAsync(9));
  }

  [Fact(DisplayName =
    "Return Rental - It should return BadRequestError if rental already returned")]
  public async Task ReturnRentalAlreadyReturned()
  {
    await Assert.ThrowsAsync<BadRequestError>
      (async () => await _rentalsService.ReturnRentalAsync(2));
  }

  [Fact(DisplayName =
    "Delete Rental - It should return NotFoundError if rental id does not existing")]
  public async Task DeleteRentalInvalidRentalId()
  {
    await Assert.ThrowsAsync<NotFoundError>
      (async () => await _rentalsService.DeleteRentalAsync(9));
  }

  [Fact(DisplayName =
    "Delete Rental - It should return BadRequestError if rental is opened")]
  public async Task DeleteRentalRentalIsOpened()
  {
    await Assert.ThrowsAsync<BadRequestError>
      (async () => await _rentalsService.DeleteRentalAsync(1));
  }
}