namespace CinemaApp.Models
{
    public class Movie
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public bool Status { get; set; }

        public DateTime DateTime { get; set; }

        public string? MainImage { get; set; }

        public List<string>? SubImages { get; set; }


        // Relations
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int CinemaId { get; set; }
        public Cinema? Cinema { get; set; }

        public ICollection<Actor>? Actors { get; set; }
        public ICollection<MovieActor>? MovieActors { get; set; }

    }
}
