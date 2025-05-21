using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlayAndWatch.DBModels
{
    public class Content
    {
        public int Id { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public string content_type { get; set; } // "movie", "series", "book", "game"

        public DateTime release_date { get; set; }

        // Навигационные свойства
        public ICollection<Content_Genres> Content_Genres { get; set; }
        public ICollection<Rating> Ratings { get; set; }
    }
}
