using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;


namespace Kiss.Bff.NieuwsEnWerkinstructies.Data
{
    public class NieuwsEnWerkinstructiesDbContext : DbContext
    {

        public NieuwsEnWerkinstructiesDbContext(DbContextOptions<NieuwsEnWerkinstructiesDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }


        public DbSet<Bericht> Berichten { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
    }
}
