using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace RemoteLight.Models
{
    public class Broker
    {
        public int BrokerId { get; set; }

        [Display(Name = "Broker IP address")]
        [RegularExpression(@"^([0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Invalid IP Address")]
        public string IPAddress { get; set; } = string.Empty;

        // Relationships
        [ValidateNever]
        public virtual ICollection<Room> Rooms { get; set; }
    }
}
