using Coast.Core.Barrier;
using Coast.Core.MigrationManager;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coast.PostgreSql
{
    internal class CoastDBContesxt : DbContext, ICoastDBContext
    {
        internal CoastDBContesxt(DbContextOptions options)
        : base(options)
        {
        }

        public DbSet<Barrier> Barriers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Barrier>();
        }
    }
}
