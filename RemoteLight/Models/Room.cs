using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteLight.Models
{
    public class Room
    {
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int Id { get; set; }

		[Display(Name = "Additional information")]
        public string AdditionalInformation { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Room broker")]
        public int FkBrokerId { get; set; }

        // Relationships
        [ValidateNever]
        public virtual Broker Broker { get; set; }
        [ValidateNever]
        public virtual ICollection<Access> Accesses { get; set; }
    }
}
