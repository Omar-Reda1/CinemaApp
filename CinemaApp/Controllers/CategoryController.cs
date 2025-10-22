
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controllers
{
    public class CategoryController : Controller
    {
        ApplicationDbContext _context = new();
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            _context.Categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category is null)
                return NotFound();
            
            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                var categoryInDb = _context.Categories.FirstOrDefault(c => c.Id == category.Id);
                if (categoryInDb == null)
                    return NotFound();

                categoryInDb.Name = category.Name;
                _context.Categories.Update(categoryInDb);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id ==id);
            if (category is null) 
                return NotFound();

            _context.Categories.Remove(category);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        

    }
}
