namespace CinemaApp.ViewModels
{
    public class MoviesVM
    {
        public Movie? Movie { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Cinema> Cinemas { get; set; }
    }
}
