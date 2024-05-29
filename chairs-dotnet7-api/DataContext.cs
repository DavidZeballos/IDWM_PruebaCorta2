using Microsoft.EntityFrameworkCore;

namespace chairs_dotnet7_api
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options) { }

        public DbSet<Chair> Chairs { get; set; }
    }
}
