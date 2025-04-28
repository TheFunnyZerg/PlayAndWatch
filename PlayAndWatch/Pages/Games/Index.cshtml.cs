using Microsoft.AspNetCore.Mvc.RazorPages;
using RecommendationSystem.Models;
using System.Collections.Generic;

namespace RecommendationSystem.Pages.Games
{
    public class IndexModel : PageModel
    {
        public List<Recommendation> Games { get; set; }

        public void OnGet()
        {
            // Здесь будет логика получения рекомендаций из базы данных или API
            // Пока используем тестовые данные
            Games = new List<Recommendation>
            {
                new Recommendation
                {
                    Id = 1,
                    Title = "Age of Wonders 4",
                    Description = "Продолжение легендарной серии глобальных фентезийных стратегий.",
                    ImageUrl = "https://example.com/inception.jpg",
                    Rating = 5.0m,
                    Genres = new[] { "4x стратегия", "Глобальная стратегия", "Фэнтези" },
                    ReleaseDate = new DateTime(2023, 5, 5)
                },
                new Recommendation
                {
                    Id = 2,
                    Title = "Halo 3",
                    Description = "Третья часть приключений воина спартанца Мастера Чифа.",
                    ImageUrl = "https://example.com/shawshank.jpg",
                    Rating = 4.5m,
                    Genres = new[] { "Фантастика", "FPS", "Шутер" },
                    ReleaseDate = new DateTime(2007, 10, 7)
                }
            };
        }
    }
}