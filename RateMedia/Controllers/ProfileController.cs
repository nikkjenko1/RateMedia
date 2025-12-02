using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateMedia.Data;
using RateMedia.Models;
using RateMedia.Services;
using System.Security.Claims;

public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize]
    public async Task<IActionResult> MyLists(string listType = "favorites")
    {
        var userId = _userManager.GetUserId(User);

        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId && f.ListType == listType)
            .Include(f => f.Movie)
            .ThenInclude(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Include(f => f.Movie.Ratings)
            .OrderByDescending(f => f.AddedAt)
            .ToListAsync();

        ViewBag.ListType = listType;
        return View(favorites);
    }

    [Authorize]
    public async Task<IActionResult> MyRatings()
    {
        var userId = _userManager.GetUserId(User);

        var ratings = await _context.Ratings
            .Where(r => r.UserId == userId)
            .Include(r => r.Movie)
            .ThenInclude(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(ratings);
    }

    [Authorize]
    public async Task<IActionResult> MyComments()
    {
        var userId = _userManager.GetUserId(User);

        var comments = await _context.Comments
            .Where(c => c.UserId == userId)
            .Include(c => c.Movie)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return View(comments);
    }
}

