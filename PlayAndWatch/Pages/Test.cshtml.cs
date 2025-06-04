using Microsoft.AspNetCore.Mvc.RazorPages;
using PlayAndWatch.Data;
using PlayAndWatch.Models;

namespace PlayAndWatch.Pages
{
    public class TestModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public List<Content> Contents { get; set; }

        public TestModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            Contents = _context.Contents.ToList();
        }
    }
}