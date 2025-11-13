using CinemaApp.Repositories.IRepositories;
using CinemaApp.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]
    public class CinemaController : Controller
    {
        private readonly IRepository<Cinema> _cinemaRepository;

        public CinemaController(IRepository<Cinema> cinemaRepository)
        {
            _cinemaRepository = cinemaRepository;
        }

        public async Task<IActionResult> Index()
        {
            var cinemas = await _cinemaRepository.GetAsync();
            return View(cinemas);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Cinema());
        }
        [HttpPost]
        public async Task<IActionResult> Create(Cinema cinema, [Required] IFormFile Image)
        {
            if (!ModelState.IsValid)
                return View(cinema);

            if (Image != null && Image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Cinemas", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await Image.CopyToAsync(stream);
                }

                cinema.Image = fileName;
            }

            await _cinemaRepository.AddAsync(cinema);
            await _cinemaRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(e => e.Id == id);
            if (cinema == null)
                return NotFound();

            return View(cinema);
        }
        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(Cinema cinema, IFormFile Image)
        {
            var existingCinema = await _cinemaRepository.GetOneAsync(c => c.Id == cinema.Id);
            if (existingCinema == null)
                return NotFound();

            existingCinema.Name = cinema.Name;

            if (Image != null && Image.Length > 0)
            {
                // حذف الصورة القديمة لو موجودة
                if (!string.IsNullOrEmpty(existingCinema.Image))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Cinemas", existingCinema.Image);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // حفظ الصورة الجديدة
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Cinemas", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await Image.CopyToAsync(stream);
                }

                existingCinema.Image = fileName;
            }

            _cinemaRepository.Update(existingCinema);
            await _cinemaRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);
            if (cinema != null)
            {
                if (!string.IsNullOrEmpty(cinema.Image))
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Cinemas", cinema.Image);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                _cinemaRepository.Delete(cinema);
                await _cinemaRepository.CommitAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
