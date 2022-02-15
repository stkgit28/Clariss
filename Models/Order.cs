using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proiect.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
     //   [Required(ErrorMessage = "Adresa este obligatorie!")]
      //  public string Address { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}