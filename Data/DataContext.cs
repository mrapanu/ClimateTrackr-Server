namespace ClimateTrackr_Server.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }
        public DbSet<TempAndHum> TempAndHums => Set<TempAndHum>();
        public DbSet<User> Users => Set<User>();
    }
}