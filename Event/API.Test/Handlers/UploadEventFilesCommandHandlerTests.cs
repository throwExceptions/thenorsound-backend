using API.Test.Helpers;
using Application.Commands;
using Application.Exceptions;
using Application.Services;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class UploadEventFilesCommandHandlerTests
{
    private readonly Mock<IEventRepository> _repoMock;
    private readonly Mock<IBlobStorageService> _blobMock;
    private readonly UploadEventFilesCommandHandler _handler;

    public UploadEventFilesCommandHandlerTests()
    {
        _repoMock = new Mock<IEventRepository>();
        _blobMock = new Mock<IBlobStorageService>();
        _handler = new UploadEventFilesCommandHandler(_repoMock.Object, _blobMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_EventNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Event?)null);

        var command = new UploadEventFilesCommand { EventId = "nonexistent" };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_CallBlobUpload_Once_Per_File()
    {
        var ev = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId)).ReturnsAsync(ev);
        _repoMock.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);
        _blobMock.Setup(b => b.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("https://blob.example.com/file");

        var command = new UploadEventFilesCommand
        {
            EventId = TestDataFactory.ValidMongoId,
            Files =
            [
                new UploadFileItem { FileName = "a.pdf", ContentType = "application/pdf", Content = Stream.Null },
                new UploadFileItem { FileName = "b.pdf", ContentType = "application/pdf", Content = Stream.Null },
            ],
        };

        await _handler.Handle(command, CancellationToken.None);

        _blobMock.Verify(b => b.UploadAsync(
            ev.Id, It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_Should_CallAddFilesAsync_With_CorrectFileNames()
    {
        var ev = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId)).ReturnsAsync(ev);
        _repoMock.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);
        _blobMock.Setup(b => b.UploadAsync(It.IsAny<string>(), "report.pdf", It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("https://blob.example.com/report.pdf");

        var command = new UploadEventFilesCommand
        {
            EventId = TestDataFactory.ValidMongoId,
            Files = [new UploadFileItem { FileName = "report.pdf", ContentType = "application/pdf", Content = Stream.Null }],
        };

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.AddFilesAsync(
            ev.Id,
            It.Is<List<EventFile>>(files =>
                files.Count == 1 &&
                files[0].Name == "report.pdf" &&
                files[0].Url == "https://blob.example.com/report.pdf")),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnUpdatedEventFiles()
    {
        var ev = TestDataFactory.ValidEvent();
        var updatedEv = TestDataFactory.ValidEvent();
        updatedEv.Files = [new EventFile { Name = "report.pdf", Url = "https://blob.example.com/report.pdf" }];

        _repoMock.SetupSequence(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(ev)
            .ReturnsAsync(updatedEv);
        _blobMock.Setup(b => b.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("https://blob.example.com/report.pdf");

        var command = new UploadEventFilesCommand
        {
            EventId = TestDataFactory.ValidMongoId,
            Files = [new UploadFileItem { FileName = "report.pdf", ContentType = "application/pdf", Content = Stream.Null }],
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("report.pdf");
        result[0].Url.Should().Be("https://blob.example.com/report.pdf");
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_When_TooManyFiles()
    {
        var ev = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId)).ReturnsAsync(ev);

        var command = new UploadEventFilesCommand
        {
            EventId = TestDataFactory.ValidMongoId,
            Files = Enumerable.Range(0, 11)
                .Select(_ => new UploadFileItem { FileName = "f.pdf", ContentType = "application/pdf", Length = 1024 })
                .ToList(),
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*10*");
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_When_FileTooLarge()
    {
        var ev = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId)).ReturnsAsync(ev);

        var command = new UploadEventFilesCommand
        {
            EventId = TestDataFactory.ValidMongoId,
            Files = [new UploadFileItem { FileName = "big.pdf", ContentType = "application/pdf", Length = 4 * 1024 * 1024 }],
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*big.pdf*");
    }

    [Theory]
    [InlineData("virus.exe", "application/octet-stream")]
    [InlineData("script.js", "text/javascript")]
    [InlineData("movie.mp4", "video/mp4")]
    [InlineData("archive.zip", "application/zip")]
    public async Task Handle_Should_ThrowBadRequestException_When_FileTypeNotAllowed(string fileName, string contentType)
    {
        var ev = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId)).ReturnsAsync(ev);

        var command = new UploadEventFilesCommand
        {
            EventId = TestDataFactory.ValidMongoId,
            Files = [new UploadFileItem { FileName = fileName, ContentType = contentType, Length = 1024 }],
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Theory]
    [InlineData("doc.pdf", "application/pdf")]
    [InlineData("doc.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.png", "image/png")]
    public async Task Handle_Should_AcceptFile_When_TypeIsAllowed(string fileName, string contentType)
    {
        var ev = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(ev);
        _blobMock.Setup(b => b.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("https://blob.example.com/file");

        var command = new UploadEventFilesCommand
        {
            EventId = TestDataFactory.ValidMongoId,
            Files = [new UploadFileItem { FileName = fileName, ContentType = contentType, Length = 1024 }],
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoFilesProvided()
    {
        var ev = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId)).ReturnsAsync(ev);

        var command = new UploadEventFilesCommand
        {
            EventId = TestDataFactory.ValidMongoId,
            Files = [],
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeEmpty();
        _blobMock.Verify(b => b.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        _repoMock.Verify(r => r.AddFilesAsync(It.IsAny<string>(), It.IsAny<List<EventFile>>()), Times.Never);
    }
}
