using BoardcampApiCS.Errors;
using BoardcampApiCS.Resourses.Customers;
using BoardcampApiCS.Resourses.Customers.Interfaces;
using BoardcampApiCS.Resourses.Customers.Models;
using Moq;

namespace BoardcampApiCSTest.Unitaries;

public class CustomersServiceUnitTest
{

  private readonly CustomersService _customersService;
  private readonly Mock<ICustomersRepository> _customersRepositoryMock;
  private readonly List<Customer> _customers = new List<Customer>{
    new Customer { Id = 1, Name = "Jeff", Cpf = "00011122233", Phone = "11987659876", Birthday = new DateTime(1996, 06, 19) },
    new Customer { Id = 2, Name = "Rai", Cpf = "11122233300", Phone = "11987659876", Birthday = new DateTime(1996, 06, 19) },
    new Customer { Id = 3, Name = "Margaret", Cpf = "22233344400", Phone = "11987659876", Birthday = new DateTime(1996, 06, 19) },
    new Customer { Id = 4, Name = "João", Cpf = "33344455500", Phone = "11987659876", Birthday = new DateTime(1996, 06, 19) }
  };

  public CustomersServiceUnitTest()
  {
    _customersRepositoryMock = new Mock<ICustomersRepository>();
    _customersRepositoryMock.Setup(x => x.GetCustomerById(It.IsAny<int>()))
      .ReturnsAsync((int id) => _customers.FirstOrDefault(c => c.Id == id));

    _customersRepositoryMock.Setup(x => x.GetByCpf(It.IsAny<string>()))
      .ReturnsAsync((string cpf) => _customers.FirstOrDefault(c => c.Cpf == cpf));

    _customersService = new CustomersService(_customersRepositoryMock.Object);
  }

  [Theory(DisplayName = "Get Customer by Id - It should return NotFoundError if not existing.")]
  [InlineData(-1)]
  [InlineData(0)]
  [InlineData(9999)]
  public async Task GetCustomerByIdNotFoundTest(int id)
  {
    await Assert.ThrowsAsync<NotFoundError>(async () => await _customersService.GetCustomerById(id));
  }

  [Fact(DisplayName = "Update Customer - It should return NotFoundError if not existing customer with id")]
  public async Task UpdateCustomerIdNotExistingTest()
  {
    var customer = new Customer
    { Id = 90, Cpf = "99988877700", Phone = "11987659876", Name = "Arthur", Birthday = new DateTime(1996, 6, 19) };
    await Assert.ThrowsAsync<NotFoundError>(async () => await _customersService.UpdateCustomer(90, customer));
  }

  [Fact(DisplayName = "Update Customer - It should return ConflictError if cpf already in use by another customer")]
  public async Task UpdateCustomerCpfAlreadyInUseTest()
  {
    var customer = new Customer
    { Id = 1, Cpf = "11122233300", Phone = "11987659876", Name = "Arthur", Birthday = new DateTime(1996, 6, 19) };
    await Assert.ThrowsAsync<ConflictError>(async () => await _customersService.UpdateCustomer(1, customer));
  }

  [Fact(DisplayName = "Update Customer - It should not return ConflictError if cpf in use by the updated user")]
  public async Task UpdateCustomerCpfAlreadyInUseByTheUpdatedUserTest()
  {
    var customer = new Customer
    { Id = 1, Cpf = "00011122233", Phone = "11987659876", Name = "Arthur", Birthday = new DateTime(1996, 6, 19) };

    await _customersService.UpdateCustomer(1, customer);
  }

  [Fact(DisplayName = "Create Customer - It should return ConflicError if cpf already in use")]
  public async Task CreateCustomerCpfAlreadyInUseTest()
  {
    var customer = _customers[0];
    await Assert.ThrowsAsync<ConflictError>(async () => await _customersService.CreateCustomer(customer));
  }
}