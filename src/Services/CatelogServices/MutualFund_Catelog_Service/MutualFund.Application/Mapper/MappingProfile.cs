using AutoMapper;
using MutualFund.Application.DTOS;
using MutualFund.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Application.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AmfiScheme, AmfiSchemeDto>();
            CreateMap<RtaSchemeData, RtaSchemeDataDto>();         
            CreateMap<SchemeMapping, SchemeMappingDto>();
            CreateMap<SchemeMasterFinal, SchemeMasterFinalDto>();

        }
    }
}
