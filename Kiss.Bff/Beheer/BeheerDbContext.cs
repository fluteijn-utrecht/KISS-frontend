using Kiss.Bff.Beheer.Links.Data.Entities;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.EntityFrameworkCore;


namespace Kiss.Bff.NieuwsEnWerkinstructies.Data
{
    public class BeheerDbContext : DbContext
    {

        public BeheerDbContext(DbContextOptions<BeheerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bericht>(b => 
            {
                b.Property(x => x.Type).HasMaxLength(200).IsRequired();
                b.HasIndex(x => x.Type);

                b.Property(x => x.Inhoud).IsRequired();
                b.Property(x => x.Titel).IsRequired();
            });

            modelBuilder.Entity<BerichtGelezen>(g =>
            {
                g.HasKey(x => new { x.UserId, x.BerichtId });
            });
        }

        public DbSet<Bericht> Berichten { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<BerichtGelezen> Gelezen { get; set; } = null!;
        public DbSet<Link> Links { get; set; } = null!;
    }
}
