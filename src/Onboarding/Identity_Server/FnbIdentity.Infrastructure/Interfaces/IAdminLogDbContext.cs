using FnbIdentity.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;


namespace FnbIdentity.Infrastructure.Interfaces
{
    public interface IAdminLogDbContext
    {
        DbSet<Log> Logs { get; set; }
    }
}
