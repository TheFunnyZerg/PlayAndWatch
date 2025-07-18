﻿using System.ComponentModel.DataAnnotations;

namespace PlayAndWatch.Models
{
    public class Rating
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int ContentId { get; set; }
        public Content? Content { get; set; }

        [Required]
        public int rating_value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
