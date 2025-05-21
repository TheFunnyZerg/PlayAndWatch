using System.ComponentModel.DataAnnotations;

namespace PlayAndWatch.DBModels
{
    public class User
    {
        public int Id { get; set; }

        public string username { get; set; }

        public string email { get; set; }

        public string password_hash { get; set; }

        public int experience { get; set; }

        public DateTime created_at { get; set; }

        public ICollection<Rating> Ratings { get; set; }
    }
}
