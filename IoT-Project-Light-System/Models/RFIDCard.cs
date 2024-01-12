using System;
using System.Collections.Generic;

namespace IoT_Project_Light_System.Models
{
    public class RFIDCard
    {
        public string Id { get; set; } // Read-only ID

        // Relationships
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<AccessLog> AccessLogs { get; set; }
        public virtual ICollection<Access> Accesses { get; set; }
    }
}
