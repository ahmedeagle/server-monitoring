using FluentAssertions;
using Moq;
using ServerMonitoring.Application.Features.Alerts.Commands;
using ServerMonitoring.Domain.Entities;
using ServerMonitoring.Domain.Enums;
using ServerMonitoring.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ServerMonitoring.UnitTests.Features.Alerts;

public class CreateAlertCommandHandlerTests
{
    private readonly Mock<IAlertRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateAlertCommandHandler _handler;

    public CreateAlertCommandHandlerTests()
    {
        _mockRepository = new Mock<IAlertRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new CreateAlertCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithCriticalAlert_ShouldCreateAlert()
    {
        // Arrange
        var command = new CreateAlertCommand
        {
            ServerId = 1,
            MetricType = "CPU",
            MetricValue = 95.5,
            Threshold = 90.0,
            Severity = AlertSeverity.Critical,
            Message = "CPU usage critical"
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Alert>()))
            .ReturnsAsync((Alert a) => a);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Severity.Should().Be(AlertSeverity.Critical);
        result.Status.Should().Be(AlertStatus.Triggered);
        
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Alert>()), Times.Once);
    }

    [Theory]
    [InlineData(AlertSeverity.Critical)]
    [InlineData(AlertSeverity.Warning)]
    [InlineData(AlertSeverity.Info)]
    public async Task Handle_WithDifferentSeverities_ShouldCreateCorrectAlert(AlertSeverity severity)
    {
        // Arrange
        var command = new CreateAlertCommand
        {
            ServerId = 1,
            MetricType = "Memory",
            MetricValue = 80.0,
            Threshold = 75.0,
            Severity = severity
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Alert>()))
            .ReturnsAsync((Alert a) => a);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Severity.Should().Be(severity);
    }
}
