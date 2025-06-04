using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PlayAndWatch.Data;

namespace RecommendationSystem.Pages.Movies
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ViewModel> Movies { get; set; } = new();

        public async Task OnGetAsync()
        {
            Movies = await _context.Contents
                .Where(c => c.content_type == "movie")
                .Include(c => c.Content_Genres)
                    .ThenInclude(cg => cg.Genre)
                .Include(c => c.Ratings)
                .Select(c => new ViewModel
                {
                    Id = c.Id,
                    Title = c.title,
                    Description = c.description,
                    Image_url = c.image_url,
                    Rating = c.Ratings.Any() ? c.Ratings.Average(r => r.rating_value) : 0,
                    ReleaseDate = c.release_date,
                    Genres = c.Content_Genres.Select(cg => cg.Genre.name).ToList()
                })
                .AsNoTracking()
                .ToListAsync();
        }
    }

    public class ViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Image_url { get; set; }
        public double Rating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<string> Genres { get; set; } = new();
    }
}