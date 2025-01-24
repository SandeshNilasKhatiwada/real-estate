using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealState.Models;

namespace RealState.Data
{
    public class RealStateContext : IdentityDbContext
    {
        public RealStateContext(DbContextOptions<RealStateContext> options) : base(options) { }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Bid> Bids { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Property)
                .WithMany(p => p.Bids)
                .HasForeignKey(b => b.PropertyId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
