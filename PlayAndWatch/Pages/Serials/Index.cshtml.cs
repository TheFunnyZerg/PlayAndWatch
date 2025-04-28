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
            // ����� ����� ������ ��������� ������������ �� ���� ������ ��� API
            // ���� ���������� �������� ������
            Serials = new List<Recommendation>
            {
                new Recommendation
                {
                    Id = 1,
                    Title = "������",
                    Description = "������������ ������ ��� ���������, ������� � ������� ���������� ������.",
                    ImageUrl = "https://example.com/inception.jpg",
                    Rating = 4.4m,
                    Genres = new[] { "�������" },
                    ReleaseDate = new DateTime(2008, 9, 1)
                },
                new Recommendation
                {
                    Id = 2,
                    Title = "������ ��� ���",
                    Description = "����������� ������ ���������� ��������-�������� �� �� ����.",
                    ImageUrl = "https://example.com/shawshank.jpg",
                    Rating = 4.9m,
                    Genres = new[] { "������� ����������", "�������", "�����" },
                    ReleaseDate = new DateTime(2024, 2, 21)
                }
            };
        }
    }
}