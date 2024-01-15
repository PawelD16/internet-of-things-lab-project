using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RemoteLight.Models
{
    public class Access
    {
        public int Id { get; set; } // Read-only ID
        public DateTime GivenAt { get; set; } = DateTime.Now;
        public string RoomId { get; set; } // Read-only ID
        public string RFIDId { get; set; } // Read-only ID

        // Relationships
        [ValidateNever]
        public virtual Room Room { get; set; }
        [ValidateNever]
        public virtual RFIDCard RFIDCard { get; set; }
    }
}
