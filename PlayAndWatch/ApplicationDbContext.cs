using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Task = PlayAndWatch.Models.Task;
using PlayAndWatch.Models;

namespace PlayAndWatch.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users {  get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Content_Genres> Content_Genres { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Task> Tasks { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка таблиц (учитывая разницу в именах)
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Content>().ToTable("Contents");
            modelBuilder.Entity<Genre>().ToTable("Genres");
            modelBuilder.Entity<Rating>().ToTable("Ratings");
            modelBuilder.Entity<Content_Genres>().ToTable("Content_genres"); // Обратите внимание на нижнее подчёркивание и регистр
            modelBuilder.Entity<Task>().ToTable("Tasks");

            // Настройка первичных ключей для всех сущностей
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<Content>().HasKey(c => c.Id);
            modelBuilder.Entity<Genre>().HasKey(g => g.Id);
            modelBuilder.Entity<Rating>().HasKey(r => r.Id);
            modelBuilder.Entity<Content_Genres>().HasKey(cg => cg.Id);
            modelBuilder.Entity<Task>().HasKey(t => t.Id);

            // Настройка связей для Content_Genres (many-to-many)
            modelBuilder.Entity<Content_Genres>()
                .HasOne(cg => cg.Content)
                .WithMany(c => c.Content_Genres)
                .HasForeignKey(cg => cg.ContentId)
                .IsRequired();

            modelBuilder.Entity<Content_Genres>()
                .HasOne(cg => cg.Genre)
                .WithMany(g => g.Content_Genres)
                .HasForeignKey(cg => cg.GenreId)
                .IsRequired();

            // Настройка связи между User и Rating (1-to-many)
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .IsRequired();

            // Настройка связи между Content и Rating (1-to-many)
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Content)
                .WithMany(c => c.Ratings)
                .HasForeignKey(r => r.ContentId)
                .IsRequired();

            modelBuilder.Entity<Task>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId);
        }
    }
}
