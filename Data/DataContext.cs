namespace ClimateTrackr_Server.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<TempAndHum> TempAndHums => Set<TempAndHum>();
        public DbSet<User> Users => Set<User>();
        public DbSet<RoomConfig> RoomConfigs => Set<RoomConfig>();
        public DbSet<NotificationSettings> NotificationSettings => Set<NotificationSettings>();
        public DbSet<UserRoom> UserRooms => Set<UserRoom>();
        public DbSet<Smtp> SmtpSettings => Set<Smtp>();
        public DbSet<History> History => Set<History>();
        public DbSet<Report> Reports => Set<Report>();
    }
}