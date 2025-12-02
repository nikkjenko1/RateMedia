using System.ComponentModel.DataAnnotations;

namespace RateMedia.Models
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }

        [Range(1, 10)]
        public int Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
    }
}
