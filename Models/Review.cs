using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proiect.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        [Required(ErrorMessage = "Campul nu poate fi lasat necompletat!")]
        public string Content { get; set; }
        public DateTime Date { get; set; }
        //[Required]
        public int Rating { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }

        public virtual Product Product { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}