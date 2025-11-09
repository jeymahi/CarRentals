using CarRentals.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentals.API.Data
{
    public class CarRentalContext : DbContext
    {
        public CarRentalContext(DbContextOptions<CarRentalContext> options)
            : base(options)
        {
        }

        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<CarInventoryEntity> CarInventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reservation>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<CarInventoryEntity>()
                .HasKey(ci => ci.CarType);
        }
    }
}
