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

        // Получить рекомендации для пользователя
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRecommendations(string userId)
        {
            // Проверяем существование пользователя
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound("User not found");
            }

            // Получаем предпочтения пользователя
            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // Получаем историю действий пользователя
            var userHistory = await _activityTracker.GetUserActivityHistory(userId);

            // Генерируем рекомендации
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

        // Обновить пользовательские предпочтения (лайки/дизлайки)
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

            // Обновляем предпочтения в зависимости от типа контента
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
            // Здесь можно добавить логику обновления предпочтений для фильмов
            // Например, учитывать жанры, рейтинги и т.д.
        }

        private void UpdateBookPreferences(UserPreferences preferences, PreferenceUpdateDto update)
        {
            // Логика для книг
        }

        private void UpdateGamePreferences(UserPreferences preferences, PreferenceUpdateDto update)
        {
            // Логика для игр
        }

        // Получить похожие рекомендации на основе конкретного элемента
        [HttpGet("similar/{contentId}")]
        public async Task<IActionResult> GetSimilarRecommendations(
            int contentId,
            [FromQuery] ContentType contentType)
        {
            var similarItems = await _recommendationService
                .GetSimilarItems(contentId, contentType);

            return Ok(similarItems);
        }

        // Отметить рекомендацию как просмотренную
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
