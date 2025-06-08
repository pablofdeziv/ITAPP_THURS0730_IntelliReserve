using IntelliReserve.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntelliReserve.Models.ViewModels
{
    public class ServiceCalendarViewModel
    {
            public int ServiceId { get; set; }
            public string ServiceName { get; set; }
            public int ServiceDurationMinutes { get; set; }
    }


}

