//using Aruba.CmpService.ResourceProvider.Common.Dtos.FullPayload;
//using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;
//using AutoMapper;
//using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;

//namespace Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
//public class ProvidersMapping :
//    Profile
//{
//    public ProvidersMapping()
//    {
//        CreateMap<ProjectResponseDto, ProjectDto>();
//        CreateMap<LocationResponseDto, LocationDto>();

//        CreateMap<Location, LocationDto>();
//        CreateMap<DataCenter, DataCenterDto>();
//        CreateMap<Typology, TypologyDto<TypologyDetailExtraInfo>>()
//            .ForMember(t => t.Parents, s => s.MapFrom(s1 => new List<ParentTypologyDto>() { new ParentTypologyDto()
//            {
//                Id = s1.CategoryId,
//                Name = s1.Category
//            } }));
//    }
//}
