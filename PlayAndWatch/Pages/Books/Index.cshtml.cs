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
            // ����� ����� ������ ��������� ������������ �� ���� ������ ��� API
            // ���� ���������� �������� ������
            Books = new List<Recommendation>
            {
                new Recommendation
                {
                    Id = 1,
                    Title = "�������� ����",
                    Description = "������������� ������� ��� ����� � �����, ������ ������������, ����� � ����.",
                    ImageUrl = "https://example.com/inception.jpg",
                    Rating = 4.8m,
                    Genres = new[] { "�������", "�����" },
                    ReleaseDate = new DateTime(2008, 8, 12)
                },
                new Recommendation
                {
                    Id = 2,
                    Title = "���������� �����",
                    Description = "�������������� ����� ���������� �������� �� �� ����.",
                    ImageUrl = "https://example.com/shawshank.jpg",
                    Rating = 5.0m,
                    Genres = new[] { "������� ����������", "�����" },
                    ReleaseDate = new DateTime(1990, 4, 15)
                }
            };
        }
    }
}