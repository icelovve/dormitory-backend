using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {}

        public DbSet<Model.Admin> Admins { get; set; }
        public DbSet<Model.Room> Rooms { get; set; }
        public DbSet<Model.Contract> Contracts { get; set; }
        public DbSet<Model.Payment> Payments { get; set; }
        public DbSet<Model.RepairRequest> RepairRequests { get;set; }
        public DbSet<Model.Tenant> Tenants { get; set; }
    }
}
