using Microsoft.EntityFrameworkCore;
using RealState.Models;

namespace RealState.Data
{
    public class RealStateContext : DbContext
    {
        public RealStateContext(DbContextOptions<RealStateContext> options) : base(options) { }

        public DbSet<Property> Properties { get; set; }
    }
}
