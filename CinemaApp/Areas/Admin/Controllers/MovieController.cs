using CinemaApp.Models;
using CinemaApp.Repositories.IRepositories;
using CinemaApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;

namespace CinemaApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController : Controller
    {
        private readonly ILogger<MovieController> _logger;
        private readonly IRepository<Movie> _movieRepo;
        private readonly IRepository<MovieActor> _movieActorRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Cinema> _cinemaRepo;
        private readonly IRepository<Actor> _actorRepo;

        public MovieController(
            ILogger<MovieController> logger,
            IRepository<Movie> movieRepo,
            IRepository<MovieActor> movieActorRepo,
            IRepository<Category> categoryRepo,
            IRepository<Cinema> cinemaRepo,
            IRepository<Actor> actorRepo)
        {
            _logger = logger;
            _movieRepo = movieRepo;
            _movieActorRepo = movieActorRepo;
            _categoryRepo = categoryRepo;
            _cinemaRepo = cinemaRepo;
            _actorRepo = actorRepo;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _movieRepo.GetAsync(
                includes: new Expression<Func<Movie, object>>[]
                {
                    m => m.Category,
                    m => m.Cinema
                });

            return View(movies);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryRepo.GetAsync();
            ViewBag.Cinemas = await _cinemaRepo.GetAsync();
            ViewBag.ActorsList = (await _actorRepo.GetAsync())
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToList();

            return View(new Movie());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Movie movie, IFormFile MainImage, List<IFormFile> SubImages, List<int> Actors)
        {


            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepo.GetAsync();
                ViewBag.Cinemas = await _cinemaRepo.GetAsync();
                ViewBag.ActorsList = (await _actorRepo.GetAsync())
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    }).ToList();

                return View(movie);
            }

            // حفظ الصورة الأساسية
            if (MainImage != null && MainImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(MainImage.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await MainImage.CopyToAsync(stream);
                }

                movie.MainImage = fileName;
            }

            // حفظ الصور الفرعية
            if (SubImages != null && SubImages.Count > 0)
            {
                movie.SubImages = new List<string>();

                foreach (var subImg in SubImages)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(subImg.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/MovieSubImg", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await subImg.CopyToAsync(stream);
                    }

                    movie.SubImages.Add(fileName);
                }
            }

            await _movieRepo.AddAsync(movie);
            await _movieRepo.CommitAsync();

            // إضافة الممثلين للفيلم
            if (Actors != null && Actors.Count > 0)
            {
                foreach (var actorId in Actors)
                {
                    var movieActor = new MovieActor
                    {
                        MovieId = movie.Id,
                        ActorId = actorId
                    };
                    await _movieActorRepo.AddAsync(movieActor);
                }
                await _movieActorRepo.CommitAsync();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieRepo.GetOneAsync(
                m => m.Id == id,
                include: q => q
                    .Include(m => m.Category)
                    .Include(m => m.Cinema)
                    .Include(m => m.MovieActors)
                        .ThenInclude(ma => ma.Actor)
            );

            if (movie == null)
                return NotFound();

            TempData["success"] = "Saved successfully";
            return View(movie);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _movieRepo.GetOneAsync(m => m.Id == id);
            if (movie == null)
                return NotFound();

            ViewBag.Categories = await _categoryRepo.GetAsync();
            ViewBag.Cinemas = await _cinemaRepo.GetAsync();
            ViewBag.ActorsList = (await _actorRepo.GetAsync())
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToList();

            return View(movie);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Movie movie, IFormFile Img, List<IFormFile> SubImgs)
        {
            var existingMovie = await _movieRepo.GetOneAsync(m => m.Id == movie.Id);
            if (existingMovie == null)
                return NotFound();

            existingMovie.Name = movie.Name;
            existingMovie.Description = movie.Description;
            existingMovie.Price = movie.Price;
            existingMovie.CinemaId = movie.CinemaId;
            existingMovie.CategoryId = movie.CategoryId;

            // الصورة الرئيسية
            if (Img != null && Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await Img.CopyToAsync(stream);
                }

                existingMovie.MainImage = fileName;
            }

            // الصور الفرعية
            if (SubImgs != null && SubImgs.Count > 0)
            {
                existingMovie.SubImages = new List<string>();

                foreach (var subImg in SubImgs)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(subImg.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/MovieSubImg", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await subImg.CopyToAsync(stream);
                    }

                    existingMovie.SubImages.Add(fileName);
                }
            }

            _movieRepo.Update(existingMovie);
            await _movieRepo.CommitAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _movieRepo.GetOneAsync(m => m.Id == id);
            if (movie == null)
                return NotFound();

            _movieRepo.Delete(movie);
            await _movieRepo.CommitAsync();

            TempData["success"] = "Movie deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
