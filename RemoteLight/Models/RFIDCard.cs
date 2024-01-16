using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RemoteLight.Models
{
    public class RFIDCard
    {
        [ValidateNever]
        public string Id { get; set; }

        // Relationships
        [Display(Name = "Card owner")]
        public int FkCardOwnerId { get; set; }

        [ValidateNever]
        public virtual CardOwner CardOwner { get; set; }
        [ValidateNever]
        public virtual ICollection<AccessLog> AccessLogs { get; set; }
        [ValidateNever]
        public virtual ICollection<Access> Accesses { get; set; }
    }
}
