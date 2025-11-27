using Microsoft.AspNetCore.Identity;

namespace RateMedia.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string DisplayName { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
    }
}
