using System.ComponentModel.DataAnnotations;

namespace PlayAndWatch.Models
{
    public class Task
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }

        public User? User { get; set; }

        public ICollection<Task> Tasks { get; set; }
    }
}
