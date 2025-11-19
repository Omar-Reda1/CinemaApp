using CinemaApp.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]

    public class PromotionController : Controller
    {
        private readonly IRepository<Promotion> _promotionRepository;// = new();
        private readonly IRepository<Movie> _movieRepository;

        public PromotionController(IRepository<Promotion> promotionRepository, IRepository<Movie> movieRepository)
        {
            _promotionRepository = promotionRepository;
            _movieRepository = movieRepository;
        }


        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var promotions = await _promotionRepository.GetAsync(includes: [e => e.Movie], tracked: false, cancellationToken: cancellationToken);
            return View(promotions);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.movies = await _movieRepository.GetAsync(tracked: false);
            return View(new Promotion());
        }
        [HttpPost]
        public async Task<IActionResult> Create(Promotion promotion, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.movies = await _movieRepository.GetAsync(tracked: false);
                return View(promotion);
            }
            await _promotionRepository.AddAsync(promotion, cancellationToken);
            await _promotionRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add promotion Successfully";

            return RedirectToAction(nameof(Index));

        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var promotion = await _promotionRepository.GetOneAsync(e => e.Id == id);

            if (promotion is null)
                return RedirectToAction("NotFoundPage", "Home");

            ViewBag.movies = await _movieRepository.GetAsync();

            return View(promotion);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Promotion promotion)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.movies = await _movieRepository.GetAsync();

                ModelState.AddModelError(string.Empty, "Please fix validation errors");

                return View(promotion);
            }

            _promotionRepository.Update(promotion);
            await _promotionRepository.CommitAsync();
            TempData["success-notification"] = "Update promotion Successfully";
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var promotion = await _promotionRepository.GetOneAsync(
                e => e.Id == id,
                cancellationToken: cancellationToken
            );

            if (promotion is null)
                return RedirectToAction("NotFoundPage", "Home");

            _promotionRepository.Delete(promotion);
            await _promotionRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "deleted Promotion successfully";

            return RedirectToAction(nameof(Index));
        }
    }

}
