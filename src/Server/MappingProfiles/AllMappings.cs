
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
            CreateMap<ES.Tag, SHARE.Tag>();

            // Map from DTO => ES
            CreateMap<DTO.Content, ES.Content>();
            CreateMap<DTO.Tag, ES.Tag>();
        }
    }
}
