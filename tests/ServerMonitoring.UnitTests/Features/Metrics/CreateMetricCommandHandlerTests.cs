using FluentAssertions;
using Moq;
using ServerMonitoring.Application.Features.Metrics.Commands;
using ServerMonitoring.Domain.Entities;
using ServerMonitoring.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ServerMonitoring.UnitTests.Features.Metrics;

public class CreateMetricCommandHandlerTests
{
    private readonly Mock<IMetricRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateMetricCommandHandler _handler;

    public CreateMetricCommandHandlerTests()
    {
        _mockRepository = new Mock<IMetricRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new CreateMetricCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidMetric_ShouldCreateMetric()
    {
        // Arrange
        var command = new CreateMetricCommand
        {
            ServerId = 1,
            CpuUsage = 45.5,
            MemoryUsage = 60.2,
            DiskUsage = 70.1,
            ResponseTime = 120
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Metric>()))
            .ReturnsAsync((Metric m) => m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ServerId.Should().Be(command.ServerId);
        result.CpuUsage.Should().Be(command.CpuUsage);
        
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Metric>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 50, 50, 100)]      // Zero CPU
    [InlineData(100, 50, 50, 100)]    // Max CPU
    [InlineData(50, 0, 50, 100)]      // Zero Memory
    [InlineData(50, 100, 50, 100)]    // Max Memory
    public async Task Handle_WithBoundaryValues_ShouldSucceed(
        double cpu, double memory, double disk, double responseTime)
    {
        // Arrange
        var command = new CreateMetricCommand
        {
            ServerId = 1,
            CpuUsage = cpu,
            MemoryUsage = memory,
            DiskUsage = disk,
            ResponseTime = responseTime
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Metric>()))
            .ReturnsAsync((Metric m) => m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Metric>()), Times.Once);
    }
}
