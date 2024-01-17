

namespace FnbIdentity.Infrastructure.Configuration
{
    public class ConnectionStringsConfiguration
    {
        public string? ConfigurationDbConnection { get; set; }

        public string? PersistedGrantDbConnection { get; set; }

        public string? AdminLogDbConnection { get; set; }

        public string? IdentityDbConnection { get; set; }


        public string? DataProtectionDbConnection { get; set; }
        public void SetConnections(string commonConnectionString)
        {
            
            AdminLogDbConnection = commonConnectionString;
            ConfigurationDbConnection = commonConnectionString;
            DataProtectionDbConnection = commonConnectionString;
            IdentityDbConnection = commonConnectionString;
            PersistedGrantDbConnection = commonConnectionString;
        }

    }
}
