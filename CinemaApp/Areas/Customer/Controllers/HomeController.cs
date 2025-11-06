using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? categoryId, int? cinemaId, int page = 1)
        {
            int pageSize = 8;  

            var moviesQuery = _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .AsQueryable();

            var categories = _context.Categories
                .OrderBy(c => c.Name)
                .ToList();

            var cinemas = _context.Cinemas
                .OrderBy(c => c.Name)
                .ToList();
            //Filtering
            if (categoryId is not null)
            {
                moviesQuery = moviesQuery.Where(m => m.CategoryId == categoryId.Value);
                ViewBag.CategoryId = categoryId.Value;
            }

            if (cinemaId is not null)
            {
                moviesQuery = moviesQuery.Where(m => m.CinemaId == cinemaId.Value);
                ViewBag.CinemaId = cinemaId.Value;
            }
            //Pagination
            int totalMovies = moviesQuery.Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

            var movies = moviesQuery
                .OrderBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Categories = categories;
            ViewBag.Cinemas = cinemas;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(movies);
        }


        public IActionResult Details(int id)
        {
            var movie = _context.Movies.Include(m => m.Category).Include(m => m.Cinema).Include(m => m.MovieActors!)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

    }
}
