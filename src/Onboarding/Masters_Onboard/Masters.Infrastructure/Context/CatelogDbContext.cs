using Masters.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Context
{
    public class CatelogDbContext : DbContext
    {
        public CatelogDbContext(DbContextOptions<CatelogDbContext> option) : base(option)
        {
            // SeedData();
        }
        public virtual DbSet<Instrument> Instruments { get; set; }
        public virtual DbSet<Exchange> Exchanges { get; set; }
        public virtual DbSet<IncomeDetail> IncomeDetails { get; set; }
        public virtual DbSet<SourceOfWealth> SourceOfWealths { get; set; }
        public virtual DbSet<Occupation> Occupations { get; set; }
        public virtual DbSet<TaxStatus> TaxStatuses { get; set; }
        public virtual DbSet<UBOCode> UBOCodes { get; set; }
        public virtual DbSet<PincodeMaster> PincodeMasters { get; set; }
        public virtual DbSet<DistrictMaster> DistrictMasters { get; set; }
        public virtual DbSet<StateMaster> StateMasters { get; set; }
        public virtual DbSet<CountryCode> CountryCodes { get; set; }

        //private void SeedData()
        //{
        //    var instruments = new List<Instrument>()
        //    {
        //        new Instrument() { InstId = 1, InstrumentName ="PPF", InstrumentType="Fixed Maturity", InsrumentIssuer ="EPFO" , Description = "Public Provident Fund", IsActive = false, Risk ="Liquidity Risk", CreatedBy ="", CreatedDate = DateTime.Now },
        //        new Instrument() { InstId = 2, InstrumentName = "Post Office Deposit", InstrumentType = "Fixed Maturity", InsrumentIssuer = "India Post", Description = "Public Provident Fund", IsActive = false, Risk = "Liquidity Risk", CreatedBy = "", CreatedDate = DateTime.Now },
        //        new Instrument() { InstId = 3, InstrumentName = "Company Deposit", InstrumentType = "Fixed Maturity", InsrumentIssuer = "Public Limited Company", Description = "Limited Comapny", IsActive = false, Risk = "Liquidity Risk", CreatedBy = "", CreatedDate = DateTime.Now },
        //        new Instrument() { InstId = 4, InstrumentName = "Bonds", InstrumentType = "Fixed Maturity &  Secondory Market ", InsrumentIssuer = "Govt.and  Companies", Description = "Market Linked Funds", IsActive = true, Risk = "Credit Risk", CreatedBy = "", CreatedDate = DateTime.Now }
        //    };
        //    Instruments.AddRange(instruments);
        //    SaveChanges();
        //}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Instrument>()
                .HasOne(x => x.Exchange)
                .WithMany(i => i.Instrument)
                .HasForeignKey(e => e.ExchangeId);

            builder.Entity<StateMaster>()
                .HasOne(x => x.CountryCode)
                .WithMany(i => i.StateMaster)
                .HasForeignKey(e => e.CountryId);

            builder.Entity<DistrictMaster>()
                .HasOne(x => x.StateMaster)
                .WithMany(i => i.DistrictMaster)
                .HasForeignKey(e => e.StateMId);

            builder.Entity<PincodeMaster>()
                .HasOne(x => x.DistrictMaster)
                .WithMany(i => i.PincodeMaster)
                .HasForeignKey(e => e.DistrictId);
        }
    }
}
