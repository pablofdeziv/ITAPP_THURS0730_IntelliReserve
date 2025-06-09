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
        public DbSet<ServiceSchedule> ServiceSchedules { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CONFIGURACIONES DE RELACIONES IMPORTANTES ---

            // 1. Configuración de la relación uno a uno entre ServiceSchedule y Appointment
            // Esto asegura que si un ServiceSchedule se elimina, su Appointment asociado también lo hará.
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.ServiceSchedule)       // Un Appointment tiene UN ServiceSchedule
                .WithOne(ss => ss.Appointment)        // Y ese ServiceSchedule tiene UN Appointment
                .HasForeignKey<Appointment>(a => a.ServiceScheduleId) // La clave foránea está en la entidad Appointment
                .IsRequired()                         // Un Appointment SIEMPRE debe tener un ServiceSchedule
                .OnDelete(DeleteBehavior.Cascade);    // Si se elimina ServiceSchedule, elimina también el Appointment

            // 2. Relación de uno a muchos: Service tiene muchos ServiceSchedules
            // Si un Servicio se elimina, sus ServiceSchedules (y por tanto sus Appointments) también se eliminan.
            modelBuilder.Entity<Service>()
                .HasMany(s => s.Schedules) // Asumiendo que 'Schedules' es la colección en tu modelo Service
                .WithOne(ss => ss.Service)
                .HasForeignKey(ss => ss.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Relación de uno a muchos: Service tiene muchos ServiceAvailabilities
            // Si un Servicio se elimina, sus ServiceAvailabilities también se eliminan.
            modelBuilder.Entity<Service>()
                .HasMany(s => s.AvailableDays) // Asumiendo que 'AvailableDays' es la colección en tu modelo Service
                .WithOne(sa => sa.Service)
                .HasForeignKey(sa => sa.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Relación de uno a muchos: User tiene muchos Appointments (un usuario reserva muchas citas)
            // Aquí usamos RESTRICT para no borrar usuarios si tienen citas, o SET NULL para dejar la FK en null.
            modelBuilder.Entity<User>()
                .HasMany(u => u.Appointments)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .IsRequired(false) // UserId puede ser null si la cita está pendiente/cancelada y no asignada
                .OnDelete(DeleteBehavior.Restrict); // O .OnDelete(DeleteBehavior.SetNull); si prefieres.

            // 5. Relación de uno a muchos: Business tiene muchos Services
            // Si un Negocio se elimina, sus Servicios asociados también se eliminan.
            modelBuilder.Entity<Business>()
                .HasMany(b => b.Services)
                .WithOne(s => s.Business)
                .HasForeignKey(s => s.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- FIN DE LAS CONFIGURACIONES DE RELACIONES ---


            // Relaciones uno a muchos, índices, restricciones... puedes añadir aquí si necesitas
        }
    }
}
