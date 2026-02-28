using API.Test.Helpers;
using Application.Commands;
using Application.Exceptions;
using Application.Services;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class RemoveEventFileCommandHandlerTests
{
    private readonly Mock<IEventRepository> _repoMock;
    private readonly Mock<IBlobStorageService> _blobMock;
    private readonly RemoveEventFileCommandHandler _handler;

    private const string ValidFileUrl = "https://thenorsoundstorage.blob.core.windows.net/event-files/abc/guid-report.pdf";

    public RemoveEventFileCommandHandlerTests()
    {
        _repoMock = new Mock<IEventRepository>();
        _blobMock = new Mock<IBlobStorageService>();
        _handler = new RemoveEventFileCommandHandler(_repoMock.Object, _blobMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_EventNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Event?)null);

        var command = new RemoveEventFileCommand { EventId = "nonexistent", FileUrl = ValidFileUrl };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_FileNotOnEvent()
    {
        var ev = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId)).ReturnsAsync(ev);

        var command = new RemoveEventFileCommand
        {
            EventId = TestDataFactory.ValidMongoId,
            FileUrl = "https://blob.example.com/other-file.pdf",
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_CallBlobDelete_When_FileExists()
    {
        var ev = TestDataFactory.ValidEventWithFile(ValidFileUrl);
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId)).ReturnsAsync(ev);
        _repoMock.Setup(r => r.RemoveFileAsync(TestDataFactory.ValidMongoId, ValidFileUrl)).ReturnsAsync(true);

        var command = new RemoveEventFileCommand { EventId = TestDataFactory.ValidMongoId, FileUrl = ValidFileUrl };

        await _handler.Handle(command, CancellationToken.None);

        _blobMock.Verify(b => b.DeleteAsync(ValidFileUrl), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_CallRemoveFileAsync_When_FileExists()
    {
        var ev = TestDataFactory.ValidEventWithFile(ValidFileUrl);
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId)).ReturnsAsync(ev);
        _repoMock.Setup(r => r.RemoveFileAsync(TestDataFactory.ValidMongoId, ValidFileUrl)).ReturnsAsync(true);

        var command = new RemoveEventFileCommand { EventId = TestDataFactory.ValidMongoId, FileUrl = ValidFileUrl };

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.RemoveFileAsync(TestDataFactory.ValidMongoId, ValidFileUrl), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnTrue_When_FileSuccessfullyRemoved()
    {
        var ev = TestDataFactory.ValidEventWithFile(ValidFileUrl);
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId)).ReturnsAsync(ev);
        _repoMock.Setup(r => r.RemoveFileAsync(TestDataFactory.ValidMongoId, ValidFileUrl)).ReturnsAsync(true);

        var command = new RemoveEventFileCommand { EventId = TestDataFactory.ValidMongoId, FileUrl = ValidFileUrl };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_NotCallBlobDelete_When_EventNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Event?)null);

        var command = new RemoveEventFileCommand { EventId = "nonexistent", FileUrl = ValidFileUrl };

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _blobMock.Verify(b => b.DeleteAsync(It.IsAny<string>()), Times.Never);
    }
}
