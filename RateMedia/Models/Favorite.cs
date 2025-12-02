using System.ComponentModel.DataAnnotations;

namespace RateMedia.Models
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ListType { get; set; } = string.Empty; // "watchlist", "favorites", "to-watch"

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
    }
}
