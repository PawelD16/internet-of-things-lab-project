using System;

namespace IoT_Project_Light_System.Models
{
    public class AccessLog
    {
        public long Id { get; set; } // Read-only ID
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Data { get; set; }

        // Relationships
        public string RFIDCardId { get; set; }
        public virtual RFIDCard RFIDCard { get; set; }
    }
}
