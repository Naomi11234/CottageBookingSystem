using AutoMapper;
using CottageBookingSystem.Core.DTOs;
using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Repositories;
using CottageBookingSystem.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CottageBookingSystem.Tests;

public sealed class BookingServiceTests
{
    [Fact]
    public async Task TryCreateBookingAsync_WhenCottageIsAvailable_ReturnsMappedBooking()
    {
        var repository = new Mock<IBookingRepository>(MockBehavior.Strict);
        var mapper = new Mock<IMapper>(MockBehavior.Strict);
        var logger = new Mock<ILogger<BookingService>>();
        var request = new CreateBookingDto
        {
            CottageId = 12,
            StartDate = DateTime.UtcNow.Date.AddDays(10),
            EndDate = DateTime.UtcNow.Date.AddDays(12),
            GuestsCount = 2,                        
            Notes = "Late arrival"
        };
        var entity = new CottageBooking
        {
            Id = 35,
            CottageId = request.CottageId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            GuestsCount = request.GuestsCount,
            Notes = request.Notes
        };
        var expected = new BookingResponseDto
        {
            Id = entity.Id,
            CottageId = entity.CottageId,
            UserId = 7,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            GuestsCount = entity.GuestsCount,
            Notes = entity.Notes,
            Status = "Pending"
        };
        const int userId = 7;
        repository
            .Setup(item => item.TryCreateIfAvailableAsync(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mapper.Setup(item => item.Map<CottageBooking>(request)).Returns(entity);
        mapper.Setup(item => item.Map<BookingResponseDto>(entity)).Returns(expected);

        var service = new BookingService(repository.Object, mapper.Object, logger.Object);

        var result = await service.TryCreateBookingAsync(request, userId);

        Assert.NotNull(result);
        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.CottageId, result.CottageId);
        Assert.Equal(expected.UserId, result.UserId);
        Assert.Equal(expected.StartDate, result.StartDate);
        Assert.Equal(expected.EndDate, result.EndDate);
        Assert.Equal(expected.GuestsCount, result.GuestsCount);
        Assert.Equal(expected.Notes, result.Notes);
        Assert.Equal("Pending", entity.Status);
        Assert.Equal(userId, entity.UserId);
        mapper.Verify(item => item.Map<CottageBooking>(request), Times.Once);
        repository.Verify(item => item.TryCreateIfAvailableAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        mapper.Verify(item => item.Map<BookingResponseDto>(entity), Times.Once);
        repository.VerifyNoOtherCalls();
        mapper.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task TryCreateBookingAsync_WhenRowVersionConflicts_ThrowsBookingConflictAndLogsWarning()
    {
        var repository = new Mock<IBookingRepository>(MockBehavior.Strict);
        var mapper = new Mock<IMapper>(MockBehavior.Strict);
        var logger = new Mock<ILogger<BookingService>>();
        var request = new CreateBookingDto
        {
            CottageId = 12,
            StartDate = DateTime.UtcNow.Date.AddDays(10),
            EndDate = DateTime.UtcNow.Date.AddDays(12),
            GuestsCount = 2,
            Notes = "Late arrival"
        };
        var entity = new CottageBooking
        {
            CottageId = request.CottageId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            GuestsCount = request.GuestsCount,
            Notes = request.Notes
        };
        var concurrencyException = new DbUpdateConcurrencyException("RowVersion mismatch.");
        mapper.Setup(item => item.Map<CottageBooking>(request)).Returns(entity);
        repository
            .Setup(item => item.TryCreateIfAvailableAsync(entity, It.IsAny<CancellationToken>()))
            .ThrowsAsync(concurrencyException);

        var service = new BookingService(repository.Object, mapper.Object, logger.Object);

        var exception = await Assert.ThrowsAsync<BookingConflictException>(
            () => service.TryCreateBookingAsync(request, userId: 7));

        Assert.Equal(BookingConflictException.ConflictMessage, exception.Message);
        Assert.Same(concurrencyException, exception.InnerException);
        mapper.Verify(item => item.Map<CottageBooking>(request), Times.Once);
        repository.Verify(item => item.TryCreateIfAvailableAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        logger.Verify(
            item => item.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("Concurrent booking attempt")),
                concurrencyException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        mapper.Verify(item => item.Map<BookingResponseDto>(It.IsAny<CottageBooking>()), Times.Never);
        repository.VerifyNoOtherCalls();
    }
}
