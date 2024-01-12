using System;

namespace IoT_Project_Light_System.Models
{
    public class Access
    {
        public int Id { get; set; } // Read-only ID
        public DateTime GivenAt { get; set; } = DateTime.Now;
        public string RoomId { get; set; } // Read-only ID
        public string RFIDId { get; set; } // Read-only ID

        // Relationships
        public virtual Room Room { get; set; }
        public virtual RFIDCard RFIDCard { get; set; }
    }
}
