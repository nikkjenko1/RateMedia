using RateMedia.Models;

namespace RateMedia.Services
{
    public interface IRecommendationService
    {
        Task<List<Movie>> GetRecommendationsForUserAsync(string userId, int count = 10);

    }
}
