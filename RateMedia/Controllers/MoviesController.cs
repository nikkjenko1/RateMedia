using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateMedia.Data;
using RateMedia.Models;
using RateMedia.Services;

public class MoviesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITmdbService _tmdbService;
    private readonly IRecommendationService _recommendationService;

    public MoviesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ITmdbService tmdbService,
        IRecommendationService recommendationService)
    {
        _context = context;
        _userManager = userManager;
        _tmdbService = tmdbService;
        _recommendationService = recommendationService;
    }

    public async Task<IActionResult> Index(string? searchString, int? genreId, int? year, int page = 1)
    {
        const int pageSize = 12;

        var movies = _context.Movies
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Include(m => m.Ratings)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            movies = movies.Where(m => m.Title.Contains(searchString) ||
                                      (m.Description != null && m.Description.Contains(searchString)));
        }

        if (genreId.HasValue && genreId.Value > 0)
        {
            movies = movies.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId.Value));
        }

        if (year.HasValue && year.Value > 0)
        {
            movies = movies.Where(m => m.Year == year.Value);
        }

        var totalMovies = await movies.CountAsync();
        var totalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

        var movieList = await movies
            .OrderByDescending(m => m.AddedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Genres = await _context.Genres.ToListAsync();
        ViewBag.SearchString = searchString;
        ViewBag.SelectedGenreId = genreId;
        ViewBag.SelectedYear = year;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(movieList);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movies
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Include(m => m.Ratings)
            .ThenInclude(r => r.User)
            .Include(m => m.Comments.OrderByDescending(c => c.CreatedAt))
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.UserRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.MovieId == id && r.UserId == userId);

            ViewBag.InWatchlist = await _context.Favorites
                .AnyAsync(f => f.MovieId == id && f.UserId == userId && f.ListType == "watchlist");
            ViewBag.InFavorites = await _context.Favorites
                .AnyAsync(f => f.MovieId == id && f.UserId == userId && f.ListType == "favorites");
            ViewBag.InToWatch = await _context.Favorites
                .AnyAsync(f => f.MovieId == id && f.UserId == userId && f.ListType == "to-watch");
        }

        return View(movie);
    }

    [Authorize]
    public async Task<IActionResult> Create()
    {
        ViewBag.Genres = await _context.Genres.ToListAsync();
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,Year,Director,Actors,PosterUrl")] Movie movie, int[] selectedGenres)
    {
        if (ModelState.IsValid)
        {
            _context.Add(movie);
            await _context.SaveChangesAsync();

            if (selectedGenres != null && selectedGenres.Length > 0)
            {
                foreach (var genreId in selectedGenres)
                {
                    _context.MovieGenres.Add(new MovieGenre
                    {
                        MovieId = movie.Id,
                        GenreId = genreId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = movie.Id });
        }

        ViewBag.Genres = await _context.Genres.ToListAsync();
        return View(movie);
    }

    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movies
            .Include(m => m.MovieGenres)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        ViewBag.Genres = await _context.Genres.ToListAsync();
        ViewBag.SelectedGenres = movie.MovieGenres.Select(mg => mg.GenreId).ToArray();
        return View(movie);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Year,Director,Actors,PosterUrl,TmdbId")] Movie movie, int[] selectedGenres)
    {
        if (id != movie.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(movie);

                // Odstrani stare žanre
                var oldGenres = await _context.MovieGenres.Where(mg => mg.MovieId == id).ToListAsync();
                _context.MovieGenres.RemoveRange(oldGenres);

                // Dodaj nove žanre
                if (selectedGenres != null && selectedGenres.Length > 0)
                {
                    foreach (var genreId in selectedGenres)
                    {
                        _context.MovieGenres.Add(new MovieGenre
                        {
                            MovieId = movie.Id,
                            GenreId = genreId
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(movie.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Details), new { id = movie.Id });
        }

        ViewBag.Genres = await _context.Genres.ToListAsync();
        return View(movie);
    }

    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movies
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie != null)
        {
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int movieId, int value)
    {
        if (value < 1 || value > 10)
        {
            TempData["Error"] = "Ocena mora biti med 1 in 10.";
            return RedirectToAction(nameof(Details), new { id = movieId });
        }

        var userId = _userManager.GetUserId(User);
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);

        if (existingRating != null)
        {
            existingRating.Value = value;
            existingRating.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            var rating = new Rating
            {
                MovieId = movieId,
                UserId = userId!,
                Value = value
            };
            _context.Ratings.Add(rating);
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Ocena uspešno dodana!";
        return RedirectToAction(nameof(Details), new { id = movieId });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comment(int movieId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["Error"] = "Komentar ne sme biti prazen.";
            return RedirectToAction(nameof(Details), new { id = movieId });
        }

        var userId = _userManager.GetUserId(User);
        var comment = new Comment
        {
            MovieId = movieId,
            UserId = userId!,
            Content = content
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Komentar uspešno dodan!";
        return RedirectToAction(nameof(Details), new { id = movieId });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int commentId, int movieId)
    {
        var userId = _userManager.GetUserId(User);
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

        if (comment != null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Komentar uspešno izbrisan!";
        }

        return RedirectToAction(nameof(Details), new { id = movieId });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToList(int movieId, string listType)
    {
        if (listType != "watchlist" && listType != "favorites" && listType != "to-watch")
        {
            TempData["Error"] = "Neveljaven tip seznama.";
            return RedirectToAction(nameof(Details), new { id = movieId });
        }

        var userId = _userManager.GetUserId(User);
        var existing = await _context.Favorites
            .FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == userId && f.ListType == listType);

        if (existing == null)
        {
            var favorite = new Favorite
            {
                MovieId = movieId,
                UserId = userId!,
                ListType = listType
            };
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Film dodan na seznam!";
        }
        else
        {
            TempData["Info"] = "Film je že na tem seznamu.";
        }

        return RedirectToAction(nameof(Details), new { id = movieId });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromList(int movieId, string listType)
    {
        var userId = _userManager.GetUserId(User);
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == userId && f.ListType == listType);

        if (favorite != null)
        {
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Film odstranjen s seznama!";
        }

        return RedirectToAction(nameof(Details), new { id = movieId });
    }

    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return View(new List<Movie>());
        }

        var movies = await _tmdbService.SearchMoviesAsync(query);
        ViewBag.Query = query;
        return View(movies);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromTmdb(int tmdbId)
    {
        var existingMovie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (existingMovie != null)
        {
            TempData["Info"] = "Ta film že obstaja v bazi.";
            return RedirectToAction(nameof(Details), new { id = existingMovie.Id });
        }

        var movie = await _tmdbService.GetMovieDetailsAsync(tmdbId);
        if (movie == null)
        {
            TempData["Error"] = "Napaka pri pridobivanju podatkov o filmu.";
            return RedirectToAction(nameof(Search));
        }

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Film uspešno uvožen!";
        return RedirectToAction(nameof(Details), new { id = movie.Id });
    }

    [Authorize]
    public async Task<IActionResult> Recommendations()
    {
        var userId = _userManager.GetUserId(User);
        var recommendations = await _recommendationService.GetRecommendationsForUserAsync(userId!, 12);
        return View(recommendations);
    }

    private bool MovieExists(int id)
    {
        return _context.Movies.Any(e => e.Id == id);
    }
}