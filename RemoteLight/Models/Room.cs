using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RemoteLight.Models
{
    public class Room
    {
        [ValidateNever]
        public string Id { get; set; }

        [Display(Name = "Additional information")]
        public string AdditionalInformation { get; set; } = string.Empty;

        [Display(Name = "Room broker")]
        public int FkIdBroker { get; set; }

        [Display(Name = "MQTT topic")]
        public string TopicName { get; set; }

        // Relationships
        [ValidateNever]
        public virtual Broker Broker { get; set; }
        [ValidateNever]
        public virtual ICollection<Access> Accesses { get; set; }
    }
}
