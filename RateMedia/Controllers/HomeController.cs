using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateMedia.Data;
using RateMedia.Models;
using RateMedia.Services;
using System.Security.Claims;

namespace RateMedia.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRecommendationService _recommendationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            IRecommendationService recommendationService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _recommendationService = recommendationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var topMovies = await _context.Movies
                .Include(m => m.Ratings)
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
                .OrderByDescending(m => m.Ratings.Average(r => (double?)r.Value) ?? 0)
                .Take(6)
                .ToListAsync();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                ViewBag.Recommendations = await _recommendationService.GetRecommendationsForUserAsync(userId!, 6);
            }

            var newMovies = await _context.Movies
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
                .OrderByDescending(m => m.AddedAt)
                .Take(6)
                .ToListAsync();

            ViewBag.NewMovies = newMovies;

            return View(topMovies);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
