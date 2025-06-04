using System.ComponentModel.DataAnnotations;

namespace PlayAndWatch.Models
{
    public class Content
    {
        public int Id { get; set; }

        [Required]
        public string title { get; set; }

        public string description { get; set; }

        public string? image_url { get; set; }

        [Required]
        public string content_type { get; set; }

        public DateTime release_date { get; set; }

        public ICollection<Content_Genres> Content_Genres { get; set; }
        public ICollection<Rating> Ratings { get; set; }
    }
}
