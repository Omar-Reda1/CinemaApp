using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Models
{
    [Index(nameof(Code), IsUnique = true)]
    public class Promotion
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        [ValidateNever]
        public Movie Movie { get; set; }

        //عشان نعرف مين اللى عمل الكود
        //public string ApplicationUserId { get; set; }
        //public ApplicationUser ApplicationUser { get; set; }

        public DateTime PublishAt { get; set; } = DateTime.Now;
        public DateTime ValidTo { get; set; }
        public bool IsValid { get; set; } 

        public string Code { get; set; }
        public decimal Discount { get; set; }
    }
}
