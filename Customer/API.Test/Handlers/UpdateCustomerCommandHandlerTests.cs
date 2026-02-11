using API.Test.Helpers;
using Application.Commands;
using Application.Exceptions;
using Domain.Enums;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class UpdateCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _repoMock;
    private readonly UpdateCustomerCommandHandler _handler;

    public UpdateCustomerCommandHandlerTests()
    {
        _repoMock = new Mock<ICustomerRepository>();
        _handler = new UpdateCustomerCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_CustomerNotFound()
    {
        var command = new UpdateCustomerCommand { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync((Customer?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyProvidedField_When_SingleFieldGiven()
    {
        var existing = TestDataFactory.ValidCustomer();
        var originalMail = existing.Mail;

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<Customer>()))
            .ReturnsAsync(true);

        var command = new UpdateCustomerCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Name = "New Name",
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.Name.Should().Be("New Name");
        existing.Mail.Should().Be(originalMail);
    }

    [Fact]
    public async Task Handle_Should_UpdateAllFields_When_AllFieldsProvided()
    {
        var existing = TestDataFactory.ValidCustomer();

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<Customer>()))
            .ReturnsAsync(true);

        var command = new UpdateCustomerCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Name = "Updated AB",
            Adress = "Ny gatan 2",
            City = "Malmö",
            Zip = "22233",
            Mail = "new@example.com",
            Phone = "+46709999999",
            Contact = "Ny kontakt",
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.Name.Should().Be("Updated AB");
        existing.Adress.Should().Be("Ny gatan 2");
        existing.City.Should().Be("Malmö");
        existing.Zip.Should().Be("22233");
        existing.Mail.Should().Be("new@example.com");
        existing.Phone.Should().Be("+46709999999");
        existing.Contact.Should().Be("Ny kontakt");
    }

    [Fact]
    public async Task Handle_Should_NotOverwriteFields_When_FieldsAreNull()
    {
        var existing = TestDataFactory.ValidCustomer();
        var originalName = existing.Name;

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<Customer>()))
            .ReturnsAsync(true);

        var command = new UpdateCustomerCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Name = null,
            Mail = null,
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.Name.Should().Be(originalName);
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_When_CrewGivenTariffs()
    {
        var existing = TestDataFactory.ValidCustomer(CustomerType.Crew);

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);

        var command = new UpdateCustomerCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Tariffs = new List<Tariff> { TestDataFactory.ValidTariff() },
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_Should_AssignGuids_When_NewTariffsHaveNoId()
    {
        var existing = TestDataFactory.ValidCustomer();

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<Customer>()))
            .ReturnsAsync(true);

        var newTariff = new Tariff
        {
            Id = string.Empty,
            Category = TariffCategory.Ljudtekniker,
            Skill = TariffSkill.A,
            TimeType = TariffTimeType.Heldag,
            TariffValue = 5000m,
        };

        var command = new UpdateCustomerCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Tariffs = new List<Tariff> { newTariff },
        };

        await _handler.Handle(command, CancellationToken.None);

        newTariff.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(newTariff.Id, out _).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_PreserveExistingIds_When_TariffsAlreadyHaveIds()
    {
        var existing = TestDataFactory.ValidCustomer();
        var existingId = "existing-tariff-id-123";

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<Customer>()))
            .ReturnsAsync(true);

        var tariffWithId = new Tariff
        {
            Id = existingId,
            Category = TariffCategory.Ljudtekniker,
            Skill = TariffSkill.A,
            TimeType = TariffTimeType.Heldag,
            TariffValue = 5000m,
        };

        var command = new UpdateCustomerCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Tariffs = new List<Tariff> { tariffWithId },
        };

        await _handler.Handle(command, CancellationToken.None);

        tariffWithId.Id.Should().Be(existingId);
    }

    [Fact]
    public async Task Handle_Should_ReplaceAllTariffs_When_NewTariffListProvided()
    {
        var oldTariff = TestDataFactory.ValidTariff("old-id");
        var existing = TestDataFactory.ValidCustomer(tariffs: new List<Tariff> { oldTariff });

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<Customer>()))
            .ReturnsAsync(true);

        var newTariff = TestDataFactory.ValidTariff("new-id");
        var command = new UpdateCustomerCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Tariffs = new List<Tariff> { newTariff },
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.Tariffs.Should().HaveCount(1);
        existing.Tariffs[0].Id.Should().Be("new-id");
    }

    [Fact]
    public async Task Handle_Should_SetUpdatedAt_When_CustomerUpdated()
    {
        var existing = TestDataFactory.ValidCustomer();
        var before = DateTime.UtcNow;

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<Customer>()))
            .ReturnsAsync(true);

        var command = new UpdateCustomerCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Name = "Updated",
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.UpdatedAt.Should().NotBeNull();
        existing.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task Handle_Should_ClearTariffs_When_EmptyTariffListProvided()
    {
        var existing = TestDataFactory.ValidCustomer(
            tariffs: new List<Tariff> { TestDataFactory.ValidTariff() });

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<Customer>()))
            .ReturnsAsync(true);

        var command = new UpdateCustomerCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Tariffs = new List<Tariff>(),
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.Tariffs.Should().BeEmpty();
    }
}
