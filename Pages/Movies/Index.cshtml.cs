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
            // ����� ����� ������ ��������� ������������ �� ���� ������ ��� API
            // ���� ���������� �������� ������
            Movies = new List<Recommendation>
            {
                new Recommendation
                {
                    Id = 1,
                    Title = "������",
                    Description = "������������ ������� � ������������� � ���.",
                    ImageUrl = "https://example.com/inception.jpg",
                    Rating = 4.5m,
                    Genres = new[] { "����������", "�������", "�����" },
                    ReleaseDate = new DateTime(2010, 7, 16)
                },
                new Recommendation
                {
                    Id = 2,
                    Title = "����� �� ��������",
                    Description = "����� � ������� � �������.",
                    ImageUrl = "https://example.com/shawshank.jpg",
                    Rating = 4.8m,
                    Genres = new[] { "�����" },
                    ReleaseDate = new DateTime(1994, 9, 23)
                }
            };
        }
    }
}