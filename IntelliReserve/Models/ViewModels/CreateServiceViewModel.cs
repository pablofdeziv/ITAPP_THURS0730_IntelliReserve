using System.ComponentModel.DataAnnotations;

namespace IntelliReserve.Models.ViewModels
{
    public class CreateServiceViewModel
    {
        [Required(ErrorMessage = "Service name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes.")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be at least 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        public TimeSpan AvailableFrom { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public TimeSpan AvailableTo { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Please select at least one day.")]
        public List<DayOfWeek> AvailableDays { get; set; } = new();

        // Schedules generados dinámicamente en la vista
        public List<ServiceScheduleViewModel> Schedules { get; set; } = new();

        // Se asignará automáticamente en el controlador
        public int BusinessId { get; set; }
    }

    public class ServiceScheduleViewModel
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
