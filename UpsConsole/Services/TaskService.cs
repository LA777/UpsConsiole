using Microsoft.Extensions.Logging;

namespace UpsConsole.Services
{
    public class TaskService : ITaskService
    {
        private static bool _isTaskActive;
        private static bool _isTaskCanceled;
        private readonly ILogger<EventsWorker> _logger;
        private readonly ISshService _sshService;
        private readonly IWakeOnlineService _wakeOnlineService;

        public TaskService(ILogger<EventsWorker> logger, ISshService sshService, IWakeOnlineService wakeOnlineService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sshService = sshService ?? throw new ArgumentNullException(nameof(sshService));
            _wakeOnlineService = wakeOnlineService ?? throw new ArgumentNullException(nameof(wakeOnlineService));
        }

        public int TaskDelay { get; set; } = Utilities.GetMillisecondsFromMinutes(3);

        public void StartTask()
        {
            _logger.LogInformation("StartTask");
            if (_isTaskActive)
            {
                return;
            }

            _isTaskActive = true;
            Task.Run(RunTask);
        }

        public void StopTask()
        {
            _logger.LogInformation("StopTask");
            if (_isTaskActive)
            {
                _isTaskActive = false;
                _isTaskCanceled = true;
                _logger.LogInformation("Task Stopped");
            }
            else
            {
                // Wake PC
                _wakeOnlineService.WakeOnLan();
            }
        }

        public void RunTask()
        {
            _logger.LogInformation("RunTask");
            _isTaskActive = true;
            Thread.Sleep(TaskDelay);
            if (_isTaskCanceled)
            {
                _isTaskCanceled = false;
                return;
            }

            TaskTodo();
            _isTaskActive = false;
        }

        public void TaskTodo()
        {
            _logger.LogInformation("TaskTodo");
            _sshService.ShutdownDeviceSsh();
        }
    }
}