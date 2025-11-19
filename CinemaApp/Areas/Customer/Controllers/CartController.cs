using CinemaApp.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Linq.Expressions;

namespace CinemaApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<Movie> _movieRepository;

        public CartController(UserManager<ApplicationUser> userManager,
            IRepository<Cart> cartRepository, IRepository<Promotion> promotionRepository,
            IRepository<Movie> movieRepository)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _promotionRepository = promotionRepository;
            _movieRepository = movieRepository;
        }

        public async Task<IActionResult> Index(string code)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();

            var cart = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id,
                 includes: [e => e.Movie, e => e.ApplicationUser]);

            var promotion = await _promotionRepository.GetOneAsync(e => e.Code == code && e.IsValid);

            if (promotion is not null)
            {
                //Movie id has promotion or not
                var result = cart.FirstOrDefault(e => e.MovieId == promotion.MovieId);

                if (result is not null)
                    result.Price -= result.Movie.Price * (promotion.Discount / 100);
                await _cartRepository.CommitAsync();
                TempData["success-notification"] = "Discount Applied";
            }

            return View(cart);
        }
        public async Task<IActionResult> AddToCart(int count, int movieId, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();

            var movieInDb = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id
            && e.MovieId == movieId);

            if (movieInDb is not null)
            {
                movieInDb.Count += count;
                await _cartRepository.CommitAsync(cancellationToken);
                TempData["success-notification"] = "Update Movie Count To Cart Successfully";

                return RedirectToAction("Index", "Home");
            }

            await _cartRepository.AddAsync(new()
            {
                MovieId = movieId,
                Count = count,
                ApplicationUserId = user.Id,
                Price = (await _movieRepository.GetOneAsync(e => e.Id == movieId)).Price

            }, cancellationToken: cancellationToken);

            await _cartRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add Movie To Cart Successfully";
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> DeleteMovie(int movieId, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();

            var movie = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id
            && e.MovieId == movieId);

            if (movie is null)
                return NotFound();

            _cartRepository.Delete(movie);
            await _cartRepository.CommitAsync(cancellationToken);
            TempData["success-notification"] = "Delete Movie From Cart Success";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cart = await _cartRepository.GetAsync(
                e => e.ApplicationUserId == user.Id,
                includes: [ e => e.Movie ]);

            if (cart == null || !cart.Any())
            {
                TempData["error-notification"] = "Cart Is Empty";
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = Url.Action("CheckoutResult", "Cart", new { area = "Customer", status = "success" }, Request.Scheme),
                CancelUrl = Url.Action("CheckoutResult", "Cart", new { area = "Customer", status = "fail" }, Request.Scheme),
            };

            foreach (var item in cart)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Movie.Name,
                            Description = item.Movie.Description,
                        },
                        UnitAmount = (long)item.Price * 100,
                    },
                    Quantity = 1,
                });
            }

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }
        public IActionResult CheckoutResult(string status)
        {
            if (status == "success")
            {
                TempData["success-notification"] = "Purchase Success!";
                
                // _cartRepository.ClearCart(userId);
            }
            else
            {
                TempData["error-notification"] = "Purchase Faild!";
            }

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }


    }
}
