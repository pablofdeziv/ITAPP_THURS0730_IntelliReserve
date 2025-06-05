using System.ComponentModel.DataAnnotations;

namespace IntelliReserve.Models.ViewModels
{
    public class BusinessWithServicesViewModel
    {
        public Business Business { get; set; }
        public List<Service> Services { get; set; }
    }
}
