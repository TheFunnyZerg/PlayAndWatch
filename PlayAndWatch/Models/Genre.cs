using System.ComponentModel.DataAnnotations;

namespace PlayAndWatch.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [Required]
        public string name { get; set; }

        public ICollection<Content_Genres> Content_Genres { get; set; }
    }
}