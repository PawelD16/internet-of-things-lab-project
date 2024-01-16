using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace RemoteLight.Models
{
    public class AccessLog
    {
        public long Id { get; set; }

        [Display(Name = "Created at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Information")]
        public string Data { get; set; } = string.Empty;

        public string FkRFIDCardId { get; set; }
        [ValidateNever]
        public virtual RFIDCard RFIDCard { get; set; }
    }
}
