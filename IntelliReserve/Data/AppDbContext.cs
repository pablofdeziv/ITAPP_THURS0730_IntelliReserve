using Microsoft.EntityFrameworkCore;
using IntelliReserve.Models;

namespace IntelliReserve.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ServiceAvailability> ServiceAvailabilities { get; set; }

        public DbSet<AppointmentHistory> AppointmentHistories { get; set; }
        public DbSet<ServiceSchedule> ServiceSchedules { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relaciones uno a muchos, índices, restricciones... puedes añadir aquí si necesitas
        }
    }
}
