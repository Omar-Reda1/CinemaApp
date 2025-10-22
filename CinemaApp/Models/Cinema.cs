namespace CinemaApp.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Image { get; set; }

        public ICollection<Movie>? Movies { get; set; }
    }
}
