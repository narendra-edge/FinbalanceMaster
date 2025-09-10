using AutoMapper;


namespace FnbIdentity.UI.API.Mappers
{
    public static class ApiScopeApiMappers
    {
        static ApiScopeApiMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ApiScopeApiMapperProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        public static T ToApiScopeApiModel<T>(this object source)
        {
            return Mapper.Map<T>(source);
        }
    }
}
