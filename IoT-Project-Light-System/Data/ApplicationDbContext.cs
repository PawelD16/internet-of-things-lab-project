using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IoT_Project_Light_System.Models;

namespace IoT_Project_Light_System.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RFIDCard> RFIDCards { get; set; }
        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<Access> Accesses { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Broker> Brokers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User to RFIDCard relationship
            builder.Entity<User>()
                .HasMany(u => u.RFIDCards)
                .WithOne(rc => rc.User)
                .HasForeignKey(rc => rc.UserId);

            // RFIDCard to AccessLog relationship
            builder.Entity<RFIDCard>()
                .HasMany(rf => rf.AccessLogs)
                .WithOne(al => al.RFIDCard)
                .HasForeignKey(al => al.RFIDCardId);

            // RFIDCard to Access relationship
            builder.Entity<RFIDCard>()
                .HasMany(rf => rf.Accesses)
                .WithOne(a => a.RFIDCard)
                .HasForeignKey(a => a.RFIDId);

            // Access to Room relationship
            builder.Entity<Access>()
                .HasOne(a => a.Room)
                .WithMany(r => r.Accesses)
                .HasForeignKey(a => a.RoomId);

            // Room to Broker relationship
            builder.Entity<Room>()
                .HasOne(r => r.Broker)
                .WithMany(b => b.Rooms)
                .HasForeignKey(r => r.IdBroker);
        }

    }
}
