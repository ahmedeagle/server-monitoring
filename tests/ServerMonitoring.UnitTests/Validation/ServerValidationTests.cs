using FluentAssertions;
using FluentValidation.TestHelper;
using ServerMonitoring.Application.Features.Servers.Commands;
using ServerMonitoring.Application.Validators;
using Xunit;

namespace ServerMonitoring.UnitTests.Validation;

public class ServerValidationTests
{
    private readonly CreateServerCommandValidator _createValidator;

    public ServerValidationTests()
    {
        _createValidator = new CreateServerCommandValidator();
    }

    [Fact]
    public void CreateServerCommand_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new CreateServerCommand
        {
            Name = "Test Server",
            Hostname = "testhost.com",
            IPAddress = "192.168.1.100",
            Port = 22,
            OperatingSystem = "Ubuntu 22.04"
        };

        // Act
        var result = _createValidator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreateServerCommand_WithInvalidName_ShouldFail(string name)
    {
        // Arrange
        var command = new CreateServerCommand
        {
            Name = name,
            Hostname = "testhost.com",
            IPAddress = "192.168.1.100",
            Port = 22,
            OperatingSystem = "Ubuntu 22.04"
        };

        // Act
        var result = _createValidator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("192.168.1.100")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    public void CreateServerCommand_WithValidIPAddress_ShouldPass(string ipAddress)
    {
        // Arrange
        var command = new CreateServerCommand
        {
            Name = "Test Server",
            Hostname = "testhost.com",
            IPAddress = ipAddress,
            Port = 22,
            OperatingSystem = "Ubuntu 22.04"
        };

        // Act
        var result = _createValidator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(80)]
    [InlineData(443)]
    [InlineData(8080)]
    [InlineData(65535)]
    public void CreateServerCommand_WithValidPort_ShouldPass(int port)
    {
        // Arrange
        var command = new CreateServerCommand
        {
            Name = "Test Server",
            Hostname = "testhost.com",
            IPAddress = "192.168.1.100",
            Port = port,
            OperatingSystem = "Ubuntu 22.04"
        };

        // Act
        var result = _createValidator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(100000)]
    public void CreateServerCommand_WithInvalidPort_ShouldFail(int port)
    {
        // Arrange
        var command = new CreateServerCommand
        {
            Name = "Test Server",
            Hostname = "testhost.com",
            IPAddress = "192.168.1.100",
            Port = port,
            OperatingSystem = "Ubuntu 22.04"
        };

        // Act
        var result = _createValidator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Port);
    }
}
