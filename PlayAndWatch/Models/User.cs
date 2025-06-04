using System.ComponentModel.DataAnnotations;

namespace PlayAndWatch.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string username { get; set; }

        [Required]
        public string email { get; set; }

        [Required]
        public string password_hash { get; set; }

        public int experience { get; set; } = 0;

        public DateTime created_at { get; set; }

        public ICollection<Rating> Ratings { get; set; }

        public ICollection<Task> Tasks { get; set; }
    }
}
