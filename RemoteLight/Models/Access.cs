using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RemoteLight.Models
{
    public class Access
    {
        public int Id { get; set; }
        public DateTime GivenAt { get; set; } = DateTime.Now;

        [Display(Name = "Room")]
        public string FkRoomId { get; set; } = string.Empty;

        [Display(Name = "RFID Card")]
        public string FkRFIDId { get; set; } = string.Empty;

        // Relationships
        [ValidateNever]
        public virtual Room Room { get; set; }
        [ValidateNever]
        public virtual RFIDCard RFIDCard { get; set; }
    }
}
