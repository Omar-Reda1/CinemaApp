using CinemaApp.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAsync();
            return View(categories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            var existing = await _categoryRepository.GetOneAsync(c => c.Id == category.Id);
            if (existing == null)
                return NotFound();

            existing.Name = category.Name;

            _categoryRepository.Update(existing);
            await _categoryRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category != null)
            {
                _categoryRepository.Delete(category);
                await _categoryRepository.CommitAsync();
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
