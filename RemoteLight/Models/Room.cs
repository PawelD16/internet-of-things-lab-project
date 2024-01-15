using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace RemoteLight.Models
{
    public class Room
    {
        public string Id { get; set; } // Read-only ID
        public string AdditionalInformation { get; set; }
        public int IdBroker { get; set; }
        public string TopicName { get; set; }

        // Relationships
        [ValidateNever]
        public virtual Broker Broker { get; set; }
        [ValidateNever]
        public virtual ICollection<Access> Accesses { get; set; }
    }
}
