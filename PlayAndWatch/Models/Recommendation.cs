namespace RecommendationSystem.Models
{
    public class Recommendation
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Rating { get; set; }
        public string Type { get; set; }
        public string[] Genres { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
