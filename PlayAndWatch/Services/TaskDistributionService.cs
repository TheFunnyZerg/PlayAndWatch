using PlayAndWatch.Contollers;
using PlayAndWatch.Data;

namespace PlayAndWatch.Services
{
    public class TaskDistributionService : ITaskDistributionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserActivityTracker _activityTracker;

        public TaskDistributionService(ApplicationDbContext context, IUserActivityTracker activityTracker)
        {
            _context = context;
            _activityTracker = activityTracker;
        }

        public async Task<bool> CheckTaskForUpdate(UserTask userTask)
        {
            // Здесь можно добавить логику проверки необходимости обновления задания
            return false; // По умолчанию возвращаем false
        }

        public async Task<Models.Task> GetUpdatedTask(int taskId)
        {
            return await _context.Tasks.FindAsync(taskId);
        }

        public async Task<List<Models.Task>> GetNewTasksForUser(string userId)
        {
            // Получаем уже назначенные задания пользователя
            var assignedTaskIds = await _context.UserTasks
                .Where(ut => ut.UserId == userId)
                .Select(ut => ut.TaskId)
                .ToListAsync();

            // Получаем статистику пользователя
            var userStats = await _activityTracker.GetUserStats(userId);

            // Определяем, какие задания подходят пользователю
            var availableTasks = await _context.Tasks
                .Where(t => !assignedTaskIds.Contains(t.Id)) // Еще не назначены
                .Where(t => t.MinLevel <= userStats.Level) // Подходит по уровню
                .Where(t => t.IsActive) // Активные задания
                .Take(3) // Ограничиваем количество новых заданий
                .ToListAsync();

            return availableTasks;
        }

        public async Task<List<Models.Task>> CheckForUnlockedTasks(string userId)
        {
            var userStats = await _activityTracker.GetUserStats(userId);
            var completedTasks = await _context.UserTasks
                .Where(ut => ut.UserId == userId && ut.Status == TaskStatus.Completed)
                .Select(ut => ut.TaskId)
                .ToListAsync();

            var allTasks = await _context.Tasks.ToListAsync();
            var unlockedTasks = new List<Models.Task>();

            foreach (var task in allTasks)
            {
                // Проверяем условия разблокировки задания
                if (!completedTasks.Contains(task.Id) &&
                    task.UnlockCondition != null &&
                    MeetsUnlockCondition(task.UnlockCondition, userStats))
                {
                    unlockedTasks.Add(task);
                }
            }

            return unlockedTasks;
        }

        private bool MeetsUnlockCondition(string condition, UserStats stats)
        {
            // Реализуйте логику проверки условий разблокировки
            return true; // Упрощенная реализация
        }
    }
}
