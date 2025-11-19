namespace CinemaApp.Models
{
    public class Cart
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; }


        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public decimal Price { get; set; }
        public int Count { get; set; }
    }
}
