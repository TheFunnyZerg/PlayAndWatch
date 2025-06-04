using System.ComponentModel.DataAnnotations;

namespace PlayAndWatch.Models
{
    public class Quize_Questions
    {
        public int id { get; set; }

        public int quiz_id { get; set; }
        public Quiz? Quiz {  get; set; }

        [Required]
        public string question {  get; set; }

        [Required]
        public string answer_1 { get; set; }

        [Required]
        public string answer_2 { get; set; }

        [Required]
        public string answer_3 { get; set; }

        [Required]
        public int correct_answer { get; set; }
    }
}
