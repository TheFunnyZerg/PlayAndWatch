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
            // ����� ����� ������ ��������� ������������ �� ���� ������ ��� API
            // ���� ���������� �������� ������
            Games = new List<Recommendation>
            {
                new Recommendation
                {
                    Id = 1,
                    Title = "Age of Wonders 4",
                    Description = "����������� ����������� ����� ���������� ����������� ���������.",
                    ImageUrl = "https://example.com/inception.jpg",
                    Rating = 5.0m,
                    Genres = new[] { "4x ���������", "���������� ���������", "�������" },
                    ReleaseDate = new DateTime(2023, 5, 5)
                },
                new Recommendation
                {
                    Id = 2,
                    Title = "Halo 3",
                    Description = "������ ����� ����������� ����� ��������� ������� ����.",
                    ImageUrl = "https://example.com/shawshank.jpg",
                    Rating = 4.5m,
                    Genres = new[] { "����������", "FPS", "�����" },
                    ReleaseDate = new DateTime(2007, 10, 7)
                }
            };
        }
    }
}