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

            var allMovies = await _context.Movies.ToListAsync();
            var allSerias = await _context.Serias.ToListAsync();
            var allBooks = await _context.Books.ToListAsync();
            var allGames = await _context.Games.ToListAsync();

            result.MovieRecommendations = await GenerateContentRecommendations(
                allMovies, history.WatchedMovies, preferences);

            result.BookRecommendations = await GenerateContentRecommendations(
                allBooks, history.ReadBooks, preferences);

            result.GameRecommendations = await GenerateContentRecommendations(
                allGames, history.PlayedGames, preferences);

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
            var targetItem = await GetContentItem(contentId, contentType);
            if (targetItem == null) return new List<RecommendationItem>();

            var allItems = await GetAllContentItems(contentType);

            allItems = allItems.Where(x => x.Id != contentId).ToList();

            var similarityScores = new List<(ContentItem Item, double Score)>();

            foreach (var item in allItems)
            {
                var score = await CalculateSimilarityScore(targetItem, item, contentType);
                similarityScores.Add((item, score));
            }

            return similarityScores
                .Where(x => x.Score > 0.2) 
                .OrderByDescending(x => x.Score)
                .Take(10) 
                .Select(x => new RecommendationItem
                {
                    Id = x.Item.Id,
                    Title = x.Item.Title,
                    Description = x.Item.Description,
                    ImageUrl = x.Item.ImageUrl,
                    MatchScore = (float)x.Score,
                    Reasons = GetSimilarityReasons(targetItem, x.Item, contentType)
                })
                .ToList();
        }

        private async Task<ContentItem> GetContentItem(int contentId, ContentType contentType)
        {
            return contentType switch
            {
                ContentType.Movie => await _context.Movies
                    .Include(m => m.Genres)
                    .Include(m => m.Director)
                    .FirstOrDefaultAsync(m => m.Id == contentId),
                ContentType.Book => await _context.Books
                    .Include(b => b.Genres)
                    .Include(b => b.Author)
                    .FirstOrDefaultAsync(b => b.Id == contentId),
                ContentType.Game => await _context.Games
                    .Include(g => g.Genres)
                    .Include(g => g.Platforms)
                    .FirstOrDefaultAsync(g => g.Id == contentId),
                _ => null
            };
        }

        private async Task<List<ContentItem>> GetAllContentItems(ContentType contentType)
        {
            return contentType switch
            {
                ContentType.Movie => await _context.Movies
                    .Include(m => m.Genres)
                    .Include(m => m.Director)
                    .ToListAsync<ContentItem>(),
                ContentType.Book => await _context.Books
                    .Include(b => b.Genres)
                    .Include(b => b.Author)
                    .ToListAsync<ContentItem>(),
                ContentType.Game => await _context.Games
                    .Include(g => g.Genres)
                    .Include(g => g.Platforms)
                    .ToListAsync<ContentItem>(),
                _ => new List<ContentItem>()
            };
        }

        private async Task<double> CalculateSimilarityScore(
            ContentItem target,
            ContentItem candidate,
            ContentType contentType)
        {
            double score = 0;
            double totalWeight = 0;

            if (target.Genres != null && candidate.Genres != null)
            {
                var commonGenres = target.Genres.Select(g => g.Name)
                    .Intersect(candidate.Genres.Select(g => g.Name))
                    .Count();

                var genreScore = (double)commonGenres / target.Genres.Count;
                score += genreScore * 0.4; 
                totalWeight += 0.4;
            }

            switch (contentType)
            {
                case ContentType.Movie:
                    score += CalculateMovieSimilarity(target as Movie, candidate as Movie);
                    totalWeight += 0.6;
                    break;

                case ContentType.Book:
                    score += CalculateBookSimilarity(target as Book, candidate as Book);
                    totalWeight += 0.6;
                    break;

                case ContentType.Game:
                    score += CalculateGameSimilarity(target as Game, candidate as Game);
                    totalWeight += 0.6;
                    break;
            }

            return totalWeight > 0 ? score / totalWeight : 0;
        }

        private double CalculateMovieSimilarity(Movie target, Movie candidate)
        {
            double score = 0;

            if (target.Director != null && candidate.Director != null &&
                target.Director.Id == candidate.Director.Id)
            {
                score += 0.25;
            }

            if (target.ReleaseYear.HasValue && candidate.ReleaseYear.HasValue)
            {
                var yearDiff = Math.Abs(target.ReleaseYear.Value - candidate.ReleaseYear.Value);
                var yearScore = 1 - Math.Min(1, yearDiff / 20.0); 
                score += yearScore * 0.15;
            }

            if (target.AverageRating.HasValue && candidate.AverageRating.HasValue)
            {
                var ratingDiff = Math.Abs(target.AverageRating.Value - candidate.AverageRating.Value);
                var ratingScore = 1 - Math.Min(1, ratingDiff / 2.0); 
                score += ratingScore * 0.1;
            }

            if (!string.IsNullOrEmpty(target.Actors) && !string.IsNullOrEmpty(candidate.Actors))
            {
                var targetActors = target.Actors.Split(',').Select(a => a.Trim());
                var candidateActors = candidate.Actors.Split(',').Select(a => a.Trim());
                var commonActors = targetActors.Intersect(candidateActors).Count();

                var actorsScore = (double)commonActors / Math.Max(1, targetActors.Count());
                score += actorsScore * 0.1;
            }

            return score;
        }

        private double CalculateBookSimilarity(Book target, Book candidate)
        {
            double score = 0;

            if (target.Author != null && candidate.Author != null &&
                target.Author.Id == candidate.Author.Id)
            {
                score += 0.3;
            }

            if (target.PublicationYear.HasValue && candidate.PublicationYear.HasValue)
            {
                var yearDiff = Math.Abs(target.PublicationYear.Value - candidate.PublicationYear.Value);
                var yearScore = 1 - Math.Min(1, yearDiff / 15.0); 
                score += yearScore * 0.1;
            }

            if (!string.IsNullOrEmpty(target.OriginalLanguage) &&
                target.OriginalLanguage == candidate.OriginalLanguage)
            {
                score += 0.1;
            }

            if (target.PageCount > 0 && candidate.PageCount > 0)
            {
                var lengthRatio = (double)Math.Min(target.PageCount, candidate.PageCount) /
                                 Math.Max(target.PageCount, candidate.PageCount);
                score += lengthRatio * 0.1;
            }

            return score;
        }

        private double CalculateGameSimilarity(Game target, Game candidate)
        {
            double score = 0;

            if (target.Platforms != null && candidate.Platforms != null)
            {
                var commonPlatforms = target.Platforms.Select(p => p.Id)
                    .Intersect(candidate.Platforms.Select(p => p.Id))
                    .Count();

                var platformScore = (double)commonPlatforms /
                                  Math.Max(1, target.Platforms.Count);
                score += platformScore * 0.2;
            }

            if (target.GameplayType == candidate.GameplayType)
            {
                score += 0.2;
            }

            if (!string.IsNullOrEmpty(target.Developer) &&
                target.Developer == candidate.Developer)
            {
                score += 0.15;
            }

            if (target.DifficultyLevel.HasValue && candidate.DifficultyLevel.HasValue)
            {
                var diffDiff = Math.Abs(target.DifficultyLevel.Value - candidate.DifficultyLevel.Value);
                var diffScore = 1 - Math.Min(1, diffDiff / 3.0); 
                score += diffScore * 0.05;
            }

            return score;
        }

        private string[] GetSimilarityReasons(ContentItem target, ContentItem candidate, ContentType contentType)
        {
            var reasons = new List<string>();

            if (target.Genres != null && candidate.Genres != null)
            {
                var commonGenres = target.Genres.Select(g => g.Name)
                    .Intersect(candidate.Genres.Select(g => g.Name))
                    .ToList();

                if (commonGenres.Any())
                {
                    reasons.Add($"Совпадающие жанры: {string.Join(", ", commonGenres)}");
                }
            }

            switch (contentType)
            {
                case ContentType.Movie:
                    var movieTarget = target as Movie;
                    var movieCandidate = candidate as Movie;

                    if (movieTarget.Director != null && movieCandidate.Director != null &&
                        movieTarget.Director.Id == movieCandidate.Director.Id)
                    {
                        reasons.Add($"Один режиссер: {movieTarget.Director.FullName}");
                    }
                    break;

                case ContentType.Book:
                    var bookTarget = target as Book;
                    var bookCandidate = candidate as Book;

                    if (bookTarget.Author != null && bookCandidate.Author != null &&
                        bookTarget.Author.Id == bookCandidate.Author.Id)
                    {
                        reasons.Add($"Один автор: {bookTarget.Author.FullName}");
                    }
                    break;

                case ContentType.Game:
                    var gameTarget = target as Game;
                    var gameCandidate = candidate as Game;

                    if (!string.IsNullOrEmpty(gameTarget.Developer) &&
                        gameTarget.Developer == gameCandidate.Developer)
                    {
                        reasons.Add($"Один разработчик: {gameTarget.Developer}");
                    }
                    break;
            }

            return reasons.ToArray();
        }
    }
}
