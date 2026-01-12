using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WeldAdminPro.Data
{
    public class DesignTimeDbContextFactory
        : IDesignTimeDbContextFactory<WeldAdminDbContext>
    {
        public WeldAdminDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<WeldAdminDbContext>()
                .UseSqlite(
                    @"Data Source=C:\Users\renie\Documents\WeldAdminPro\weldadmin.db"
                )
                .Options;

            return new WeldAdminDbContext(options);
        }
    }
}
