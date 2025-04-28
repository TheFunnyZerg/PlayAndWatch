using Microsoft.AspNetCore.Mvc.RazorPages;
using RecommendationSystem.Models;
using System.Collections.Generic;

namespace RecommendationSystem.Pages.Movies
{
    public class IndexModel : PageModel
    {
        public List<Recommendation> Movies { get; set; }

        public void OnGet()
        {
            // Здесь будет логика получения рекомендаций из базы данных или API
            // Пока используем тестовые данные
            Movies = new List<Recommendation>
            {
                new Recommendation
                {
                    Id = 1,
                    Title = "Начало",
                    Description = "Криминальный триллер о проникновении в сны.",
                    ImageUrl = "https://example.com/inception.jpg",
                    Rating = 4.5m,
                    Genres = new[] { "Фантастика", "Триллер", "Драма" },
                    ReleaseDate = new DateTime(2010, 7, 16)
                },
                new Recommendation
                {
                    Id = 2,
                    Title = "Побег из Шоушенка",
                    Description = "Драма о надежде и свободе.",
                    ImageUrl = "https://example.com/shawshank.jpg",
                    Rating = 4.8m,
                    Genres = new[] { "Драма" },
                    ReleaseDate = new DateTime(1994, 9, 23)
                }
            };
        }
    }
}