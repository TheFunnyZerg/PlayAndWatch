using Microsoft.AspNetCore.Mvc.RazorPages;
using RecommendationSystem.Models;
using System.Collections.Generic;

namespace RecommendationSystem.Pages.Serials
{
    public class IndexModel : PageModel
    {
        public List<Recommendation> Serials { get; set; }

        public void OnGet()
        {
            // Здесь будет логика получения рекомендаций из базы данных или API
            // Пока используем тестовые данные
            Serials = new List<Recommendation>
            {
                new Recommendation
                {
                    Id = 1,
                    Title = "Универ",
                    Description = "Коммендийный сериал про студентов, живущих в обычной московской общаге.",
                    ImageUrl = "https://example.com/inception.jpg",
                    Rating = 4.4m,
                    Genres = new[] { "Комедия" },
                    ReleaseDate = new DateTime(2008, 9, 1)
                },
                new Recommendation
                {
                    Id = 2,
                    Title = "Задача трёх тел",
                    Description = "Экранизация романа китайского писателя-фантаста Лю Ци Синя.",
                    ImageUrl = "https://example.com/shawshank.jpg",
                    Rating = 4.9m,
                    Genres = new[] { "Научная фантастика", "Триллер", "Драма" },
                    ReleaseDate = new DateTime(2024, 2, 21)
                }
            };
        }
    }
}