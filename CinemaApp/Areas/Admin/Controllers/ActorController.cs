using CinemaApp.DataAccess;
using CinemaApp.Models;
using CinemaApp.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorController : Controller
    {
        private readonly IRepository<Actor> _actorRepository;

        public ActorController(IRepository<Actor> actorRepository)
        {
            _actorRepository = actorRepository;
        }

        public async Task<IActionResult> Index()
        {
            var actors = await _actorRepository.GetAsync();
            return View(actors);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Actor actor, IFormFile? Image)
        {
            if (!ModelState.IsValid)
                return View(actor);

            if (Image != null && Image.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(Image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Actors", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await Image.CopyToAsync(stream);
                }

                actor.Image = fileName;
            }

            await _actorRepository.AddAsync(actor);
            await _actorRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var actor = await _actorRepository.GetOneAsync(a => a.Id == id);
            if (actor == null)
                return NotFound();

            return View(actor);
        }
      

        [HttpPost]
        public async Task<IActionResult> Edit(Actor actor, IFormFile? Img)
        {
            var existing = await _actorRepository.GetOneAsync(a => a.Id == actor.Id);
            if (existing == null)
                return NotFound();

            existing.Name = actor.Name;

            if (Img != null && Img.Length > 0)
            {
                if (!string.IsNullOrEmpty(existing.Image))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Actors", existing.Image);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Actors", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await Img.CopyToAsync(stream);
                }

                existing.Image = fileName;
            }

                _actorRepository.Update(existing);
            await _actorRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var actor = await _actorRepository.GetOneAsync(a => a.Id == id);
            if (actor == null)
                return NotFound();

            if (!string.IsNullOrEmpty(actor.Image))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Actors", actor.Image);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

                _actorRepository.Delete(actor);
            await _actorRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
