using RateMedia.Models;

namespace RateMedia.Services
{
    public interface ITmdbService
    {
        Task<List<Movie>> SearchMoviesAsync(string query);
        Task<Movie?> GetMovieDetailsAsync(int tmdbId);
    }
}
