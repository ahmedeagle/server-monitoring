using FluentValidation;
using ServerMonitoring.Application.Features.Servers.Commands;

namespace ServerMonitoring.Application.Validators;

public class CreateServerCommandValidator : AbstractValidator<CreateServerCommand>
{
    public CreateServerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Server name is required")
            .MaximumLength(200).WithMessage("Server name must not exceed 200 characters");

        RuleFor(x => x.Hostname)
            .NotEmpty().WithMessage("Hostname is required")
            .MaximumLength(200).WithMessage("Hostname must not exceed 200 characters");

        RuleFor(x => x.IPAddress)
            .NotEmpty().WithMessage("IP Address is required")
            .Matches(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$").WithMessage("Invalid IP Address format");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535");

        RuleFor(x => x.OperatingSystem)
            .NotEmpty().WithMessage("Operating System is required");
    }
}

