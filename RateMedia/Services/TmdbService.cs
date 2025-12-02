using Microsoft.EntityFrameworkCore;
using RateMedia.Data;
using RateMedia.Models;
using System.Net.Http;
using System.Text.Json;

namespace RateMedia.Services
{   

    public class TmdbService : ITmdbService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TmdbService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<List<Movie>> SearchMoviesAsync(string query)
        {
            var client = _httpClientFactory.CreateClient("TMDb");
            var apiKey = _configuration["TMDb:ApiKey"];

            var response = await client.GetAsync($"search/movie?api_key={apiKey}&query={Uri.EscapeDataString(query)}");

            if (!response.IsSuccessStatusCode)
                return new List<Movie>();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbSearchResult>(content);

            return result?.Results?.Select(r => new Movie
            {
                Title = r.Title ?? "Unknown",
                Description = r.Overview,
                Year = r.ReleaseDate?.Year ?? 0,
                PosterUrl = !string.IsNullOrEmpty(r.PosterPath)
                    ? $"{_configuration["TMDb:BaseImageUrl"]}{r.PosterPath}"
                    : null,
                TmdbId = r.Id
            }).ToList() ?? new List<Movie>();
        }

        public async Task<Movie?> GetMovieDetailsAsync(int tmdbId)
        {
            var client = _httpClientFactory.CreateClient("TMDb");
            var apiKey = _configuration["TMDb:ApiKey"];

            var response = await client.GetAsync($"movie/{tmdbId}?api_key={apiKey}&append_to_response=credits");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var movieData = JsonSerializer.Deserialize<TmdbMovieDetails>(content);

            if (movieData == null)
                return null;

            return new Movie
            {
                Title = movieData.Title ?? "Unknown",
                Description = movieData.Overview,
                Year = movieData.ReleaseDate?.Year ?? 0,
                Director = movieData.Credits?.Crew?.FirstOrDefault(c => c.Job == "Director")?.Name,
                Actors = string.Join(", ", movieData.Credits?.Cast?.Take(5).Select(a => a.Name) ?? Array.Empty<string>()),
                PosterUrl = !string.IsNullOrEmpty(movieData.PosterPath)
                    ? $"{_configuration["TMDb:BaseImageUrl"]}{movieData.PosterPath}"
                    : null,
                TmdbId = tmdbId
            };
        }
    }


    public class TmdbSearchResult
    {
        public List<TmdbMovie>? Results { get; set; }
    }

    public class TmdbMovie
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Overview { get; set; }
        public string? PosterPath { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }

    public class TmdbMovieDetails
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Overview { get; set; }
        public string? PosterPath { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public TmdbCredits? Credits { get; set; }
    }

    public class TmdbCredits
    {
        public List<TmdbCast>? Cast { get; set; }
        public List<TmdbCrew>? Crew { get; set; }
    }

    public class TmdbCast
    {
        public string? Name { get; set; }
    }

    public class TmdbCrew
    {
        public string? Name { get; set; }
        public string? Job { get; set; }
    }
}