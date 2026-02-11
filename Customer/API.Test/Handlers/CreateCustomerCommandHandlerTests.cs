using API.Test.Helpers;
using Application.Commands;
using Application.Exceptions;
using Domain.Enums;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

[Collection("Mapster")]
public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _repoMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _repoMock = new Mock<ICustomerRepository>();
        _handler = new CreateCustomerCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnCreatedCustomer_When_ValidCommand()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Test AB",
            OrgNumber = "123456-7890",
            Adress = "Testgatan 1",
            City = "Stockholm",
            Zip = "11122",
            Mail = "test@example.com",
            Phone = "+46701234567",
            CustomerType = CustomerType.Customer,
            Tariffs = new List<Tariff>(),
        };

        _repoMock.Setup(r => r.GetByOrgNumberAsync(command.OrgNumber))
            .ReturnsAsync((Customer?)null);

        var expected = TestDataFactory.ValidCustomer();
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowDuplicateException_When_OrgNumberAlreadyExists()
    {
        var command = new CreateCustomerCommand
        {
            Name = "Test AB",
            OrgNumber = "123456-7890",
            CustomerType = CustomerType.Customer,
        };

        _repoMock.Setup(r => r.GetByOrgNumberAsync(command.OrgNumber))
            .ReturnsAsync(TestDataFactory.ValidCustomer());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_When_NonCustomerHasTariffs()
    {
        var command = new CreateCustomerCommand
        {
            Name = "Crew AB",
            OrgNumber = "123456-7890",
            CustomerType = CustomerType.Crew,
            Tariffs = new List<Tariff> { TestDataFactory.ValidTariff() },
        };

        _repoMock.Setup(r => r.GetByOrgNumberAsync(command.OrgNumber))
            .ReturnsAsync((Customer?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_Should_AssignGuidToTariffs_When_TariffsProvided()
    {
        var tariff = new Tariff
        {
            Category = TariffCategory.Ljudtekniker,
            Skill = TariffSkill.A,
            TimeType = TariffTimeType.Heldag,
            TariffValue = 5000m,
        };

        var command = new CreateCustomerCommand
        {
            Name = "Test AB",
            OrgNumber = "123456-7890",
            CustomerType = CustomerType.Customer,
            Tariffs = new List<Tariff> { tariff },
        };

        _repoMock.Setup(r => r.GetByOrgNumberAsync(command.OrgNumber))
            .ReturnsAsync((Customer?)null);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(TestDataFactory.ValidCustomer());

        await _handler.Handle(command, CancellationToken.None);

        tariff.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(tariff.Id, out _).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Succeed_When_CrewWithoutTariffs()
    {
        var command = new CreateCustomerCommand
        {
            Name = "Crew AB",
            OrgNumber = "999999-0000",
            CustomerType = CustomerType.Crew,
            Contact = "Anna Andersson",
            Tariffs = new List<Tariff>(),
        };

        _repoMock.Setup(r => r.GetByOrgNumberAsync(command.OrgNumber))
            .ReturnsAsync((Customer?)null);

        var expected = TestDataFactory.ValidCustomer(CustomerType.Crew);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectData_When_ValidCommand()
    {
        var command = new CreateCustomerCommand
        {
            Name = "Test AB",
            OrgNumber = "123456-7890",
            CustomerType = CustomerType.Customer,
            Tariffs = new List<Tariff>(),
        };

        _repoMock.Setup(r => r.GetByOrgNumberAsync(command.OrgNumber))
            .ReturnsAsync((Customer?)null);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(TestDataFactory.ValidCustomer());

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.CreateAsync(
            It.Is<Customer>(c => c.Name == "Test AB" && c.OrgNumber == "123456-7890")),
            Times.Once);
    }
}
