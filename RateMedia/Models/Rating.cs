namespace RateMedia.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int Value { get; set; } // 1..10 for example
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
