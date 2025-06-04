using System.ComponentModel.DataAnnotations;

namespace PlayAndWatch.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        public int task_id { get; set; }
        public Task? task { get; set; }

        [Required]
        public bool is_completed { get; set; }

        public ICollection<Quiz> Quizes { get; set; }
    }
}
