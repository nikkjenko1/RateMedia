using RateMedia.Models;

namespace RateMedia.Services
{
    public interface IJwtService
    {
        string CreateToken(ApplicationUser user, IList<string> roles = null);
    }
}
