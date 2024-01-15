using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteLight.Models
{
    public class Broker
    {
        public int BrokerId { get; set; } // Read-only ID
        public string IPAddress { get; set; }

        // Relationships
        [ValidateNever]
        public virtual ICollection<Room> Rooms { get; set; }
    }
}
