using Duende.IdentityServer.EntityFramework.DbContexts;
using FnbIdentity.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace FnbIdentity.Infrastructure.DbContexts
{
    public class IdentityServerPersistedGrantDbContext : PersistedGrantDbContext<IdentityServerPersistedGrantDbContext>,
     IAdminPersistedGrantDbContext

    {
        public IdentityServerPersistedGrantDbContext(DbContextOptions<IdentityServerPersistedGrantDbContext> options)
            : base(options) 
        { }


    }
}
