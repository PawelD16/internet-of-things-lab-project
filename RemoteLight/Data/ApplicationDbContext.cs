using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RemoteLight.Models;

namespace RemoteLight.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CardOwner> CardOwners { get; set; }
        public DbSet<RFIDCard> RFIDCards { get; set; }
        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<Access> Accesses { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Broker> Brokers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User to RFIDCard relationship
            builder.Entity<CardOwner>()
                .HasOne(rc => rc.RFIDCard)
                .WithOne(u => u.CardOwner)
                .HasForeignKey<RFIDCard>(rc => rc.FkCardOwnerId);

            /*
            // RFIDCard to AccessLog relationship
            builder.Entity<RFIDCard>()
                .HasMany(rf => rf.AccessLogs)
                .WithOne(al => al.RFIDCard)
                .HasForeignKey(al => al.FkRFIDCardId);
            */

            // RFIDCard to Access relationship
            builder.Entity<RFIDCard>()
                .HasMany(rf => rf.Accesses)
                .WithOne(a => a.RFIDCard)
                .HasForeignKey(a => a.FkRFIDCardId);

            // Access to Room relationship
            builder.Entity<Access>()
                .HasOne(a => a.Room)
                .WithMany(r => r.Accesses)
                .HasForeignKey(a => a.FkRoomId);

            // Room to Broker relationship
            builder.Entity<Room>()
                .HasOne(r => r.Broker)
                .WithMany(b => b.Rooms)
                .HasForeignKey(r => r.FkBrokerId);

            // Broker unique index
            builder.Entity<Broker>()
                .HasIndex(b => b.IPAddress)
                .IsUnique(true);

			// Access unique for given room and card
			builder.Entity<Access>()
				.HasIndex(a => new { a.FkRoomId, a.FkRFIDCardId })
				.IsUnique();

			builder.SeedDatabase();
        }
    }
}