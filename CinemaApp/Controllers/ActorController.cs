using CinemaApp.DataAccess;
using CinemaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Controllers
{
    public class ActorController : Controller
    {
        ApplicationDbContext _context = new();
        public IActionResult Index()
        {
            var actor = _context.Actors.ToList();

            return View(actor);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Actor actor,IFormFile Img)
        {
            if (Img is not null && Img.Length > 0)
            {
                var fileName=Guid.NewGuid().ToString()+Path.GetExtension(Img.FileName);
                var filePath=Path.Combine(Directory.GetCurrentDirectory(),"wwwroot\\images\\Actors",fileName);

                using(var stream = System.IO.File.Create(filePath))
                {
                    Img.CopyTo(stream);
                }
                actor.Image = fileName;
            }
            _context.Actors.Add(actor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var actor= _context.Actors.FirstOrDefault(x=>x.Id==id);
            if (actor is null)
            {
                return NotFound();
            }

            return View(actor);
        }
        [HttpPost]
        public IActionResult Edit(Actor actor , IFormFile Img)
        {
           var actorInDb= _context.Actors.FirstOrDefault(x=> x.Id==actor.Id);
            if (actorInDb is null) 
                return NotFound();
            actorInDb.Name = actor.Name;

            if(Img is not null && Img.Length > 0)
            {
                //delete oldImg
                if (!String.IsNullOrEmpty(actorInDb.Image))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Actors", actorInDb.Image);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                var fileName=Guid.NewGuid().ToString()+Path.GetExtension(Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot\\images\\Actors", fileName);
                using(var stream=System.IO.File.Create(filePath))
                {
                    Img.CopyTo(stream);
                }
                
                actorInDb.Image = fileName;
            }
            _context.Actors.Update(actorInDb);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Delete(int id)
        {
            var actor = _context.Actors.FirstOrDefault(a => a.Id==id);
            if (actor is not null)
            {
                if (!string.IsNullOrEmpty(actor.Image))
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Actors", actor.Image);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                _context.Actors.Remove(actor);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
