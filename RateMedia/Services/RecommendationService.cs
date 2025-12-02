using RateMedia.Data;
using RateMedia.Models;
using RateMedia.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;

public class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _context;

    public RecommendationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Movie>> GetRecommendationsForUserAsync(string userId, int count = 10)
    {
        var userRatings = await _context.Ratings
            .Where(r => r.UserId == userId)
            .Include(r => r.Movie)
            .ThenInclude(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .ToListAsync();

        if (!userRatings.Any())
        {
            return await _context.Movies
                .Include(m => m.Ratings)
                .OrderByDescending(m => m.Ratings.Average(r => (double?)r.Value) ?? 0)
                .Take(count)
                .ToListAsync();
        }

        var favoriteGenres = userRatings
            .Where(r => r.Value >= 7)
            .SelectMany(r => r.Movie.MovieGenres.Select(mg => mg.GenreId))
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        var ratedMovieIds = userRatings.Select(r => r.MovieId).ToList();

        var recommendations = await _context.Movies
            .Where(m => !ratedMovieIds.Contains(m.Id))
            .Where(m => m.MovieGenres.Any(mg => favoriteGenres.Contains(mg.GenreId)))
            .Include(m => m.Ratings)
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .OrderByDescending(m => m.Ratings.Average(r => (double?)r.Value) ?? 0)
            .Take(count)
            .ToListAsync();

        return recommendations;
    }
}