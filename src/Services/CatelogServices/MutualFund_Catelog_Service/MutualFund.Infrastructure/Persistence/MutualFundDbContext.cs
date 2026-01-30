using Microsoft.EntityFrameworkCore;
using MutualFund.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Infrastructure.Persistence
{
    public class MutualFundDbContext :  DbContext
    {
        public MutualFundDbContext(DbContextOptions<MutualFundDbContext> options) : base(options) { }

        public DbSet<CamsRaw> CamsRaw { get; set; }
        public DbSet<KfinRaw> KfinRaw { get; set; }
        public DbSet<AmfiRaw> AmfiRaw { get; set; }
        public DbSet<AmfiScheme> AmfiScheme { get; set; }    
        public DbSet<SchemeMapping> SchemeMapping { get; set; }
        
        public DbSet<RtaSchemeData> RtaSchemeData { get; set; }
        public DbSet<SchemeMasterFinal> SchemeMasterFinal { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MutualFundDbContext).Assembly);
        }
    }
}
