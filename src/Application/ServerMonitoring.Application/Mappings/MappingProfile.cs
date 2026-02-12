using AutoMapper;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Application.Mappings;

/// <summary>
/// AutoMapper profile configuration for entity-to-DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Server mappings
        CreateMap<Server, ServerDto>();
        
        // Metric mappings
        CreateMap<Metric, MetricDto>();
        
        // Alert mappings
        CreateMap<Alert, AlertDto>()
            .ForMember(dest => dest.ServerName, opt => opt.MapFrom(src => src.Server != null ? src.Server.Name : string.Empty));
    }
}
