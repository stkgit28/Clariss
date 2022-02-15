using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proiect.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Denumirea este obligatorie")]
        [StringLength(20, ErrorMessage = "Titlul nu poate avea mai mult de 20 caractere")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Descrierea produsului este obligatorie")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Required(ErrorMessage = "Pretul este obligatoriu")]
        public int Price { get; set; }
        // [Required]
        public string Photo { get; set; }
        public string Status { get; set; }
        public decimal ProductRating { get; set; }


        /*public int Rating { get; set; }
        */
        [Required(ErrorMessage = "Categoria este obligatorie")]
        public int CategoryId { get; set; }
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Item> Items { get; set; }

        public IEnumerable<SelectListItem> Categ { get; set; }
    }
}