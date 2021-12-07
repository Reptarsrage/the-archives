
using AutoMapper;

using DTO = TheArchives.Server.Models.Dto;
using ES = TheArchives.Server.Models.Elastic;
using SHARE = TheArchives.Shared;

namespace TheArchives.Server.MappingProfiles
{
    public class AllMappings : Profile
    {
        public AllMappings()
        {
            // Map from DTO => SHARE
            CreateMap<DTO.Content, SHARE.Content>();
            CreateMap<DTO.Tag, SHARE.Tag>();

            // Map from ES => SHARE
            CreateMap<ES.Content, SHARE.Content>();

            // Map from DTO => ES
            CreateMap<DTO.Content, ES.Content>()
                .ForMember(dest => dest.Tags,
                    opt => opt.MapFrom(src => src.Tags!.Select(tag => tag.Label)))
                .ForMember(dest => dest.Keywords,
                    opt => opt.MapFrom(src =>
                        src.Keywords!.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)));
        }
    }
}