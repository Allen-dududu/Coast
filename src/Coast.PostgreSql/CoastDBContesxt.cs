//using Coast.Core.Barrier;
//using Coast.Core.MigrationManager;

//namespace Coast.PostgreSql
//{
//    internal class CoastDBContesxt : DbContext, ICoastDBContext
//    {
//        internal CoastDBContesxt(DbContextOptions options)
//        : base(options)
//        {
//        }

//        public DbSet<Barrier> Barriers { get; set; }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.Entity<Barrier>();
//        }
//    }
//}
