using Microsoft.AspNetCore.Mvc.RazorPages;
using RecommendationSystem.Models;
using System.Collections.Generic;

namespace RecommendationSystem.Pages.Books
{
    public class IndexModel : PageModel
    {
        public List<Recommendation> Books { get; set; }

        public void OnGet()
        {
            // Здесь будет логика получения рекомендаций из базы данных или API
            // Пока используем тестовые данные
            Books = new List<Recommendation>
            {
                new Recommendation
                {
                    Id = 1,
                    Title = "Рождение орды",
                    Description = "Захватывающая история про орков и людей, полная предательств, крови и битв.",
                    ImageUrl = "https://example.com/inception.jpg",
                    Rating = 4.8m,
                    Genres = new[] { "Фэнтези", "Роман" },
                    ReleaseDate = new DateTime(2008, 8, 12)
                },
                new Recommendation
                {
                    Id = 2,
                    Title = "Блуждающая Земля",
                    Description = "Фантастический роман китайского писателя Лю Ци Синя.",
                    ImageUrl = "https://example.com/shawshank.jpg",
                    Rating = 5.0m,
                    Genres = new[] { "Научная фантастика", "Драма" },
                    ReleaseDate = new DateTime(1990, 4, 15)
                }
            };
        }
    }
}