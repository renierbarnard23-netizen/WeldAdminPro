using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WeldAdminPro.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WeldAdminDbContext>
    {
        public WeldAdminDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<WeldAdminDbContext>();
            builder.UseSqlite("Data Source=weldadmin.db");
            return new WeldAdminDbContext(builder.Options);
        }
    }
}
