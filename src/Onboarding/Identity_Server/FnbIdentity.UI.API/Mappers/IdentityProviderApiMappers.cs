using AutoMapper;


namespace FnbIdentity.UI.API.Mappers
{
    public static class IdentityProviderApiMappers 
    {
        static IdentityProviderApiMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<IdentityProviderApiMapperProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }
        public static T ToIdentityProviderApiModel<T>(this object source)
        {
            return Mapper.Map<T>(source);
        }

    }
}
