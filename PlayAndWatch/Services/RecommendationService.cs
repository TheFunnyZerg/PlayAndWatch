using PlayAndWatch.Contollers;
using PlayAndWatch.Data;

namespace PlayAndWatch.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IContentSimilarityCalculator _similarityCalculator;

        public RecommendationService(
            ApplicationDbContext context,
            IContentSimilarityCalculator similarityCalculator)
        {
            _context = context;
            _similarityCalculator = similarityCalculator;
        }

        public async Task<RecommendationResult> GenerateRecommendations(
            string userId,
            UserPreferences preferences,
            UserActivityHistory history)
        {
            var result = new RecommendationResult();

            // Получаем все доступные элементы каждого типа
            var allMovies = await _context.Movies.ToListAsync();
            var allBooks = await _context.Books.ToListAsync();
            var allGames = await _context.Games.ToListAsync();

            // Генерируем рекомендации для каждого типа контента
            result.MovieRecommendations = await GenerateContentRecommendations(
                allMovies, history.WatchedMovies, preferences);

            result.BookRecommendations = await GenerateContentRecommendations(
                allBooks, history.ReadBooks, preferences);

            result.GameRecommendations = await GenerateContentRecommendations(
                allGames, history.PlayedGames, preferences);

            // Сортируем по релевантности
            result.MovieRecommendations = result.MovieRecommendations
                .OrderByDescending(r => r.MatchScore)
                .Take(10)
                .ToList();

            result.BookRecommendations = result.BookRecommendations
                .OrderByDescending(r => r.MatchScore)
                .Take(10)
                .ToList();

            result.GameRecommendations = result.GameRecommendations
                .OrderByDescending(r => r.MatchScore)
                .Take(10)
                .ToList();

            return result;
        }

        private async Task<List<RecommendationItem>> GenerateContentRecommendations<T>(
            List<T> allItems,
            List<int> viewedItemIds,
            UserPreferences preferences) where T : ContentItem
        {
            var recommendations = new List<RecommendationItem>();

            foreach (var item in allItems)
            {
                if (!viewedItemIds.Contains(item.Id))
                {
                    var score = await _similarityCalculator.CalculateMatchScore(item, preferences);
                    var reasons = await _similarityCalculator.GetMatchReasons(item, preferences);

                    recommendations.Add(new RecommendationItem
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Description = item.Description,
                        ImageUrl = item.ImageUrl,
                        MatchScore = score,
                        Reasons = reasons
                    });
                }
            }

            return recommendations;
        }

        public async Task<List<RecommendationItem>> GetSimilarItems(
            int contentId,
            ContentType contentType)
        {
            // Реализация поиска похожих элементов
            // Можно использовать алгоритмы коллаборативной фильтрации
            // или content-based подход
            return new List<RecommendationItem>();
        }
    }
}
