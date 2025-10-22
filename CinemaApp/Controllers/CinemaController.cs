using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controllers
{
    public class CinemaController : Controller
    {
        ApplicationDbContext _context=new ApplicationDbContext();
        public IActionResult Index()
        {
            var cinemas=_context.Cinemas.ToList();
            return View(cinemas);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Cinema cinema,IFormFile Image)
        {
            if(Image is not null && Image.Length > 0)
            {
                var fileName=Guid.NewGuid().ToString()+Path.GetExtension(Image.FileName);
                var filePath=Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Cinemas", fileName);
                using(var stream = System.IO.File.Create(filePath))
                {
                    Image.CopyTo(stream);
                }
                cinema.Image=fileName;
            }
            _context.Cinemas.Add(cinema);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cinema= _context.Cinemas.FirstOrDefault(c => c.Id==id);
            if(cinema is null)
                return NotFound();

            return View(cinema);
        }
        [HttpPost]
        public IActionResult Edit(Cinema cinema , IFormFile Image)
        {
            var cinemaInDb = _context.Cinemas.FirstOrDefault(c => c.Id == cinema.Id);
            if( cinemaInDb is null )
                return NotFound();

            cinemaInDb.Name=cinema.Name;

            if(Image is not null && Image.Length > 0)
            {
                //Delete OldImage
                if (!string.IsNullOrEmpty(cinemaInDb.Image))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Cinemas", cinemaInDb.Image);
                    if(System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                //Create NewImage
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var filePath=Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Cinemas",fileName);
                using(var stream=System.IO.File.Create(filePath))
                {
                    Image.CopyTo(stream);
                }
                cinemaInDb.Image = fileName;
            }
            _context.Cinemas.Update(cinemaInDb);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var cinema = _context.Cinemas.FirstOrDefault(c => c.Id == id);
           if(cinema is not null)
            {
                if (!string.IsNullOrEmpty(cinema.Image))
                {
                    var path=Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Cinemas",cinema.Image);
                    if(System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                   
                }
                _context.Cinemas.Remove(cinema);
                _context.SaveChanges();
            }
           return RedirectToAction(nameof(Index));
          
        }

    }
}
