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

        public async Task<Models.Task> GetUpdatedTask(int taskId)
        {
            return await _context.Tasks.FindAsync(taskId);
        }

        public async Task<List<Models.Task>> GetNewTasksForUser(string userId)
        {
            var assignedTaskIds = await _context.UserTasks
                .Where(ut => ut.UserId == userId)
                .Select(ut => ut.TaskId)
                .ToListAsync();

            var userStats = await _activityTracker.GetUserStats(userId);

            var availableTasks = await _context.Tasks
                .Where(t => !assignedTaskIds.Contains(t.Id))
                .Where(t => t.MinLevel <= userStats.Level) 
                .Where(t => t.IsActive)
                .Take(3)
                .ToListAsync();

            return availableTasks;
        }

        public async Task<bool> CheckTaskForUpdate(UserTask userTask)
        {
            var task = await _context.Tasks.FindAsync(userTask.TaskId);
            if (task == null) return false;

            if (task.TargetValue != userTask.Task?.TargetValue ||
                task.Reward != userTask.Task?.Reward)
            {
                return true;
            }

            if (task.ExpirationDate.HasValue &&
                task.ExpirationDate.Value < DateTime.UtcNow &&
                userTask.Status == TaskStatus.Active)
            {
                userTask.Status = TaskStatus.Failed;
                return true;
            }

            if (task.IsSeasonal)
            {
                var now = DateTime.UtcNow;
                var isSeasonMatch = now.Month switch
                {
                    12 or 1 or 2 when task.Season == Season.Winter => true,
                    3 or 4 or 5 when task.Season == Season.Spring => true,
                    6 or 7 or 8 when task.Season == Season.Summer => true,
                    9 or 10 or 11 when task.Season == Season.Autumn => true,
                    _ => false
                };

                if (!isSeasonMatch && userTask.Status == TaskStatus.Active)
                {
                    userTask.Status = TaskStatus.Failed;
                    return true;
                }
            }

            if (userTask.Progress >= task.TargetValue &&
                userTask.Status != TaskStatus.Completed)
            {
                userTask.Status = TaskStatus.Completed;
                userTask.CompletedDate = DateTime.UtcNow;
                return true;
            }

            if (!string.IsNullOrEmpty(task.AvailabilityCondition))
            {
                var meetsCondition = MeetsUnlockCondition(task.AvailabilityCondition,
                    await _activityTracker.GetUserStats(userTask.UserId));

                if (!meetsCondition && userTask.Status == TaskStatus.Active)
                {
                    userTask.Status = TaskStatus.Failed;
                    return true;
                }
            }

            return false;
        }

        private bool MeetsUnlockCondition(string condition, UserStats userStats)
        {
            try
            {
                var parts = condition.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 3) return false;

                string property = parts[0];
                string op = parts[1];
                string value = parts[2];

                decimal statValue = property switch
                {
                    "Level" => userStats.Level,
                    "TotalPoints" => userStats.TotalPoints,
                    "CompletedTasks" => userStats.CompletedTasks.Count,
                    "MovieTasksCompleted" => userStats.CompletedTasks.Count(t => t.Type == TaskType.Movie),
                    "BookTasksCompleted" => userStats.CompletedTasks.Count(t => t.Type == TaskType.Book),
                    "GameTasksCompleted" => userStats.CompletedTasks.Count(t => t.Type == TaskType.Game),
                    "LastActiveDays" => (decimal)(DateTime.UtcNow - userStats.LastActivityDate).TotalDays,
                    "AvgCompletionTime" => (decimal)userStats.CompletedTasks
                        .Where(t => t.CompletionTime.HasValue)
                        .Average(t => t.CompletionTime.Value.TotalHours),
                    _ => throw new ArgumentException($"Unknown property: {property}")
                };

                decimal targetValue = decimal.Parse(value);

                return op switch
                {
                    ">=" => statValue >= targetValue,
                    "<=" => statValue <= targetValue,
                    ">" => statValue > targetValue,
                    "<" => statValue < targetValue,
                    "==" => statValue == targetValue,
                    "!=" => statValue != targetValue,
                    _ => throw new ArgumentException($"Unknown operator: {op}")
                };
            }
            catch
            {
                return false;
            }
        }
    }
}
