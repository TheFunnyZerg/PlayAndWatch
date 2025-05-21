namespace PlayAndWatch.DBModels
{
    public class Genre
    {
        public int Id { get; set; }
        public string name { get; set; }

        // Навигационное свойство
        public ICollection<Content_Genres> Content_Genres { get; set; }
    }
}