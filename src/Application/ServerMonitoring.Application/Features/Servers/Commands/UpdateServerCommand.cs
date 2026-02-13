using MediatR;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Common;

namespace ServerMonitoring.Application.Features.Servers.Commands;

public class UpdateServerCommand : IRequest<Result<ServerDto>>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string OperatingSystem { get; set; } = string.Empty;
}
