namespace WebAppAccVerify.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder
                .UseSqlServer("Server=.\\SQLEXPRESS;Database=UserDB;Trusted_Connection=True;");
        }

        public DbSet<User> Users => Set<User>();
    }
}
