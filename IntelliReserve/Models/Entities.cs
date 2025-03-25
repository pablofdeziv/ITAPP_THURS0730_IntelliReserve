using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntelliReserve.Models
{
    // 🟢 Usuario (User)
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public UserRole Role { get; set; } = UserRole.Customer;

        public ICollection<Business> Businesses { get; set; } = new List<Business>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }

    public enum UserRole { Customer, BusinessAdmin }

    // 🟢 Negocio (Business)
    public class Business
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public Guid OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        public string Address { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }

    // 🟢 Empleado (Employee)
    public class Employee
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessId { get; set; }

        [ForeignKey("BusinessId")]
        public Business Business { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Role { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }

    // 🟢 Servicio (Service)
    public class Service
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessId { get; set; }

        [ForeignKey("BusinessId")]
        public Business Business { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public int Duration { get; set; } // Duration in minutes
        public decimal Price { get; set; }

        public ICollection<ServiceSchedule> ServiceSchedules { get; set; } = new List<ServiceSchedule>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }

    // 🟢 Horario de Servicio (ServiceSchedule)
    public class ServiceSchedule
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public Service Service { get; set; }

        public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
    }

    // 🟢 Cita (Appointment)
    public class Appointment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public Guid ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public Service Service { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public ICollection<AppointmentHistory> AppointmentHistories { get; set; } = new List<AppointmentHistory>();
        public Payment Payment { get; set; }
    }

    public enum AppointmentStatus { Pending, Confirmed, Canceled }

    // 🟢 Historial de Cita (AppointmentHistory)
    public class AppointmentHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        public HistoryChangeType ChangeType { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    public enum HistoryChangeType { Created, Modified, Canceled }

    // 🟢 Horario de Negocio (Schedule)
    public class Schedule
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessId { get; set; }

        [ForeignKey("BusinessId")]
        public Business Business { get; set; }

        public int DayOfWeek { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
    }

    // 🟢 Pago (Payment)
    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime PaymentDate { get; set; }
    }

    public enum PaymentStatus { Pending, Paid, Refunded }

    // 🟢 Notificación (Notification)
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required, MaxLength(500)]
        public string Message { get; set; }

        public DateTime SentAt { get; set; }
    }

    // 🟢 Reseña (Review)
    public class Review
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public Guid BusinessId { get; set; }

        [ForeignKey("BusinessId")]
        public Business Business { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
