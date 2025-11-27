using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace RateMedia.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public int Year { get; set; }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public string Director { get; set; }
        public string Actors { get; set; }
        public ICollection<Genre> Genres { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
