using Microsoft.AspNetCore.Identity;

namespace RateMedia.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
