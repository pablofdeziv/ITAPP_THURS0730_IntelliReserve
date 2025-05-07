using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IntelliReserve.Models;

namespace IntelliReserve.ViewModels
{
    public class ServiceWithSchedulesViewModel
    {
        public Service Service { get; set; } = new Service();

        [Display(Name = "Schedules")]
        public List<ServiceSchedule> ServiceSchedules { get; set; } = new List<ServiceSchedule>();
    }
}

