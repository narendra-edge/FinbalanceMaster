using AutoMapper;

namespace Masters.Api.Helper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Infrastructure.Entities.CountryCode, Core.Models.CountryCode>().ReverseMap();
            CreateMap<Infrastructure.Entities.DistrictMaster, Core.Models.DistrictMaster>().ReverseMap();
            CreateMap<Infrastructure.Entities.Exchange, Core.Models.Exchange>().ReverseMap();
            CreateMap<Infrastructure.Entities.IncomeDetail, Core.Models.IncomeDetail>().ReverseMap();
            CreateMap<Infrastructure.Entities.Instrument, Core.Models.Instrument>().ReverseMap();
            CreateMap<Infrastructure.Entities.Occupation, Core.Models.Occupation>().ReverseMap();
            CreateMap<Infrastructure.Entities.PincodeMaster, Core.Models.PincodeMaster>().ReverseMap();
            CreateMap<Infrastructure.Entities.StateMaster, Core.Models.StateMaster>().ReverseMap();
            CreateMap<Infrastructure.Entities.SourceOfWealth,Core.Models.SourceOfWealth>().ReverseMap();          
            CreateMap<Infrastructure.Entities.TaxStatus, Core.Models.TaxStatus>().ReverseMap();
            CreateMap<Infrastructure.Entities.UBOCode, Core.Models.UBOCode>().ReverseMap();
            
            
            
        }
    }
}
