namespace RateMedia.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public string ListType { get; set; } // "watchlist","favorites","to-watch"
    }
}
