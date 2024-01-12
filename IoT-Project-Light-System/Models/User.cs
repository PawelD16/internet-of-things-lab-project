using System;
using System.Collections.Generic;

namespace IoT_Project_Light_System.Models
{
    public class User
    {
        public int Id { get; set; } // Read-only ID
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Name { get; set; }

        // Relationships
        public virtual ICollection<RFIDCard> RFIDCards { get; set; }
    }
}
