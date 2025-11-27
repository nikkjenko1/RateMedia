namespace RateMedia.DTOs
{
    public class MovieDtos
    {
        public class MovieListDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int Year { get; set; }
            public string PosterUrl { get; set; }
            public double AverageRating { get; set; }
            public IEnumerable<string> Genres { get; set; }
        }

        public class MovieDetailDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int Year { get; set; }
            public string Description { get; set; }
            public string PosterUrl { get; set; }
            public IEnumerable<string> Genres { get; set; }
            public IEnumerable<object> Comments { get; set; } // simplified
        }
    }
}
