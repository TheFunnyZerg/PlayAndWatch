using Microsoft.AspNetCore.Mvc;
using PlayAndWatch.Data;

namespace PlayAndWatch.Contollers
{
    public class RecommendationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRecommendationService _recommendationService;
        private readonly IUserActivityTracker _activityTracker;

        public RecommendationsController(
            ApplicationDbContext context,
            IRecommendationService recommendationService,
            IUserActivityTracker activityTracker)
        {
            _context = context;
            _recommendationService = recommendationService;
            _activityTracker = activityTracker;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRecommendations(string userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound("User not found");
            }

            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var userHistory = await _activityTracker.GetUserActivityHistory(userId);

            var recommendations = await _recommendationService.GenerateRecommendations(
                userId,
                preferences,
                userHistory);

            return Ok(new
            {
                Movies = recommendations.MovieRecommendations,
                Books = recommendations.BookRecommendations,
                Games = recommendations.GameRecommendations
            });
        }

        [HttpPost("preferences/{userId}")]
        public async Task<IActionResult> UpdatePreferences(
            string userId,
            [FromBody] PreferenceUpdateDto preferenceUpdate)
        {
            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (preferences == null)
            {
                preferences = new UserPreferences
                {
                    UserId = userId
                };
                _context.UserPreferences.Add(preferences);
            }

            switch (preferenceUpdate.ContentType)
            {
                case ContentType.Movie:
                    UpdateMoviePreferences(preferences, preferenceUpdate);
                    break;
                case ContentType.Book:
                    UpdateBookPreferences(preferences, preferenceUpdate);
                    break;
                case ContentType.Game:
                    UpdateGamePreferences(preferences, preferenceUpdate);
                    break;
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Preferences updated successfully" });
        }

        private void UpdateMoviePreferences(UserPreferences preferences, PreferenceUpdateDto update)
        {
            var movie = _context.Movies
                .Include(m => m.Genres)
                .FirstOrDefault(m => m.Id == update.ContentId);

            if (movie == null) return;

            preferences.MovieGenreWeights ??= new Dictionary<string, double>();
            preferences.MovieDirectorWeights ??= new Dictionary<string, double>();
            preferences.MovieActorWeights ??= new Dictionary<string, double>();

            double delta = update.IsLiked ? 0.2 : -0.1;
            if (update.Rating.HasValue)
            {
                delta = (update.Rating.Value - 5) / 10.0;
            }

            foreach (var genre in movie.Genres)
            {
                if (preferences.MovieGenreWeights.ContainsKey(genre.Name))
                {
                    preferences.MovieGenreWeights[genre.Name] += delta;
                    preferences.MovieGenreWeights[genre.Name] =
                        Math.Max(-1, Math.Min(1, preferences.MovieGenreWeights[genre.Name]));
                }
                else
                {
                    preferences.MovieGenreWeights[genre.Name] = delta;
                }
            }

            if (!string.IsNullOrEmpty(movie.Director))
            {
                if (preferences.MovieDirectorWeights.ContainsKey(movie.Director))
                {
                    preferences.MovieDirectorWeights[movie.Director] += delta * 1.5;
                }
                else
                {
                    preferences.MovieDirectorWeights[movie.Director] = delta * 1.5;
                }
            }

            if (movie.ReleaseYear.HasValue)
            {
                preferences.AveragePreferredMovieYear = preferences.AveragePreferredMovieYear.HasValue
                    ? (preferences.AveragePreferredMovieYear * 0.9 + movie.ReleaseYear.Value * 0.1)
                    : movie.ReleaseYear.Value;
            }
        }

        private void UpdateBookPreferences(UserPreferences preferences, PreferenceUpdateDto update)
        {
            var book = _context.Books
                .Include(b => b.Genres)
                .Include(b => b.Author)
                .FirstOrDefault(b => b.Id == update.ContentId);

            if (book == null) return;

            preferences.BookGenreWeights ??= new Dictionary<string, double>();
            preferences.AuthorWeights ??= new Dictionary<string, double>();
            preferences.BookLengthPreference ??= 0;

            double delta = update.IsLiked ? 0.15 : -0.08;
            if (update.Rating.HasValue)
            {
                delta = (update.Rating.Value - 3) / 7.0;
            }

            foreach (var genre in book.Genres)
            {
                if (preferences.BookGenreWeights.ContainsKey(genre.Name))
                {
                    preferences.BookGenreWeights[genre.Name] += delta;
                }
                else
                {
                    preferences.BookGenreWeights[genre.Name] = delta;
                }
            }
        }

        private void UpdateGamePreferences(UserPreferences preferences, PreferenceUpdateDto update)
        {
            var game = _context.Games
                .Include(g => g.Genres)
                .Include(g => g.Platforms)
                .FirstOrDefault(g => g.Id == update.ContentId);

            if (game == null) return;

            preferences.GameGenreWeights ??= new Dictionary<string, double>();
            preferences.GamePlatformWeights ??= new Dictionary<string, double>();
            preferences.GameplayTypePreference ??= new Dictionary<GameplayType, double>();

            double delta = update.IsLiked ? 0.25 : -0.15;
            if (update.Rating.HasValue)
            {
                delta = (update.Rating.Value - 5) / 10.0;
            }

            foreach (var genre in game.Genres)
            {
                if (preferences.GameGenreWeights.ContainsKey(genre.Name))
                {
                    preferences.GameGenreWeights[genre.Name] += delta;
                }
                else
                {
                    preferences.GameGenreWeights[genre.Name] = delta;
                }
            }
        }

        [HttpGet("similar/{contentId}")]
        public async Task<IActionResult> GetSimilarRecommendations(
            int contentId,
            [FromQuery] ContentType contentType)
        {
            var similarItems = await _recommendationService
                .GetSimilarItems(contentId, contentType);

            return Ok(similarItems);
        }

        [HttpPost("view/{userId}/{contentId}")]
        public async Task<IActionResult> TrackRecommendationView(
            string userId,
            int contentId,
            [FromQuery] ContentType contentType)
        {
            await _activityTracker.TrackContentViewed(userId, contentId, contentType);
            return Ok(new { Message = "View tracked successfully" });
        }
    }

    public class PreferenceUpdateDto
    {
        public ContentType ContentType { get; set; }
        public int ContentId { get; set; }
        public bool IsLiked { get; set; }
        public int? Rating { get; set; }
    }

    public enum ContentType
    {
        Movie,
        Book,
        Game
    }

    public interface IRecommendationService
    {
        Task<RecommendationResult> GenerateRecommendations(
            string userId,
            UserPreferences preferences,
            UserActivityHistory history);

        Task<List<RecommendationItem>> GetSimilarItems(
            int contentId,
            ContentType contentType);
    }

    public class RecommendationResult
    {
        public List<RecommendationItem> MovieRecommendations { get; set; }
        public List<RecommendationItem> BookRecommendations { get; set; }
        public List<RecommendationItem> GameRecommendations { get; set; }
    }

    public class RecommendationItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public float MatchScore { get; set; }
        public string[] Reasons { get; set; }
    }
}
