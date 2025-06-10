using Microsoft.AspNetCore.Mvc;
using PlayAndWatch.Data;

namespace PlayAndWatch.Contollers
{
    public class TaskContoller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaskDistributionService _taskDistributionService;

        public TasksController(ApplicationDbContext context, ITaskDistributionService taskDistributionService)
        {
            _context = context;
            _taskDistributionService = taskDistributionService;
        }

        // Получить текущие задания пользователя
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTasks(string userId)
        {
            var userTasks = await _context.UserTasks
                .Include(ut => ut.Task)
                .Where(ut => ut.UserId == userId)
                .Select(ut => new UserTaskDto
                {
                    Id = ut.Id,
                    TaskId = ut.TaskId,
                    Title = ut.Task.Title,
                    Description = ut.Task.Description,
                    TaskType = ut.Task.TaskType,
                    Progress = ut.Progress,
                    TargetValue = ut.Task.TargetValue,
                    Reward = ut.Task.Reward,
                    Status = ut.Status,
                    AssignedDate = ut.AssignedDate,
                    CompletedDate = ut.CompletedDate
                })
                .ToListAsync();

            return Ok(userTasks);
        }

        // Распределить новые задания пользователю
        [HttpPost("assign/{userId}")]
        public async Task<IActionResult> AssignTasks(string userId)
        {
            // Проверяем существование пользователя
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound("User not found");
            }

            // Получаем текущие задания пользователя
            var currentTasks = await _context.UserTasks
                .Where(ut => ut.UserId == userId && ut.Status != TaskStatus.Completed)
                .ToListAsync();

            // Проверяем, не нужно ли обновить существующие задания
            foreach (var userTask in currentTasks)
            {
                var shouldUpdate = await _taskDistributionService.CheckTaskForUpdate(userTask);
                if (shouldUpdate)
                {
                    var updatedTask = await _taskDistributionService.GetUpdatedTask(userTask.TaskId);
                    userTask.Task = updatedTask;
                    userTask.Status = TaskStatus.Active;
                }
            }

            // Определяем новые задания для пользователя
            var newTasks = await _taskDistributionService.GetNewTasksForUser(userId);

            // Добавляем новые задания
            foreach (var task in newTasks)
            {
                var userTask = new UserTask
                {
                    UserId = userId,
                    TaskId = task.Id,
                    Progress = 0,
                    Status = TaskStatus.Active,
                    AssignedDate = DateTime.UtcNow
                };
                _context.UserTasks.Add(userTask);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Tasks assigned successfully", newTasks.Count });
        }

        // Обновить прогресс задания
        [HttpPost("progress/{userTaskId}")]
        public async Task<IActionResult> UpdateTaskProgress(int userTaskId, [FromBody] ProgressUpdateDto progressUpdate)
        {
            var userTask = await _context.UserTasks
                .Include(ut => ut.Task)
                .FirstOrDefaultAsync(ut => ut.Id == userTaskId);

            if (userTask == null)
            {
                return NotFound("Task not found");
            }

            userTask.Progress += progressUpdate.Increment;

            // Проверяем, выполнено ли задание
            if (userTask.Progress >= userTask.Task.TargetValue)
            {
                userTask.Status = TaskStatus.Completed;
                userTask.CompletedDate = DateTime.UtcNow;

                // Начисляем награду
                var user = await _context.Users.FindAsync(userTask.UserId);
                user.Points += userTask.Task.Reward;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                UpdatedProgress = userTask.Progress,
                IsCompleted = userTask.Status == TaskStatus.Completed,
                Reward = userTask.Status == TaskStatus.Completed ? userTask.Task.Reward : 0
            });
        }

        // Проверить и разблокировать новые задания
        [HttpPost("check-unlock/{userId}")]
        public async Task<IActionResult> CheckAndUnlockTasks(string userId)
        {
            var unlockedTasks = await _taskDistributionService.CheckForUnlockedTasks(userId);

            if (unlockedTasks.Any())
            {
                foreach (var task in unlockedTasks)
                {
                    if (!await _context.UserTasks.AnyAsync(ut => ut.UserId == userId && ut.TaskId == task.Id))
                    {
                        _context.UserTasks.Add(new UserTask
                        {
                            UserId = userId,
                            TaskId = task.Id,
                            Progress = 0,
                            Status = TaskStatus.Active,
                            AssignedDate = DateTime.UtcNow
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { UnlockedTasks = unlockedTasks.Select(t => t.Title) });
            }

            return Ok(new { Message = "No new tasks unlocked" });
        }
    }

    public class ProgressUpdateDto
    {
        public int Increment { get; set; } = 1;
    }

    public class UserTaskDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskType TaskType { get; set; }
        public int Progress { get; set; }
        public int TargetValue { get; set; }
        public int Reward { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }

    public interface ITaskDistributionService
    {
        Task<bool> CheckTaskForUpdate(UserTask userTask);
        Task<Models.Task> GetUpdatedTask(int taskId);
        Task<List<Models.Task>> GetNewTasksForUser(string userId);
        Task<List<Models.Task>> CheckForUnlockedTasks(string userId);
    }

    public enum TaskType
    {
        Movie,
        Book,
        Game,
        Mixed
    }

    public enum TaskStatus
    {
        Active,
        Completed,
        Failed,
        Pending
    }
}
