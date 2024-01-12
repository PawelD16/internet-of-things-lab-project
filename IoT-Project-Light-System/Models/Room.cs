using System.Collections.Generic;

namespace IoT_Project_Light_System.Models
{
    public class Room
    {
        public string Id { get; set; } // Read-only ID
        public string AdditionalInformation { get; set; }
        public int IdBroker { get; set; }
        public string TopicName { get; set; }

        // Relationships
        public virtual Broker Broker { get; set; }
        public virtual ICollection<Access> Accesses { get; set; }
    }
}
