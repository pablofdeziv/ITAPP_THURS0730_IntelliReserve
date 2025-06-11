using Microsoft.AspNetCore.Mvc;

namespace IntelliReserve.Models.ViewModels
{
    public class BusinessAppointmentViewModel
    {
        public string ServiceName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string CustomerName { get; set; }
        public int AppointmentId { get; set; }

    }
}
