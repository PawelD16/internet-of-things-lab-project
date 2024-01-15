using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace RemoteLight.Models
{
    public class RFIDCard
    {
        public string Id { get; set; } // Read-only ID

        // Relationships
        public int CardOwnerId { get; set; }

        [ValidateNever]
        public virtual CardOwner CardOwner { get; set; }
        [ValidateNever]
        public virtual ICollection<AccessLog> AccessLogs { get; set; }
        [ValidateNever]
        public virtual ICollection<Access> Accesses { get; set; }
    }
}
