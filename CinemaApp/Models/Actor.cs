namespace CinemaApp.Models
{
    public class Actor
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Image { get; set; } 

        public ICollection<Movie>? Movies { get; set; }
        public ICollection<MovieActor>? MovieActors { get; set; }

    }

}
