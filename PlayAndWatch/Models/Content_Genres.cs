namespace PlayAndWatch.Models
{
    public class Content_Genres
    {
        public int Id { get; set; }
        public int ContentId { get; set; }
        public Content? Content { get; set; }

        public int GenreId { get; set; }
        public Genre? Genre { get; set; }
    }
}
