using Microsoft.Extensions.Logging;
using UpsConsole.Services;

namespace UnitTests
{
    [CollectionDefinition(nameof(TaskServiceTests), DisableParallelization = true)]
    public class TaskServiceTests
    {
        private static readonly Mock<ILogger<EventsWorker>> LoggerMock = new();
        private readonly TaskService _sutTaskService = new(LoggerMock.Object);

        public TaskServiceTests()
        {
            _sutTaskService.TaskDelay = Utilities.GetMillisecondsFromSeconds(10);
        }

        [Fact]
        public void Test1()
        {
            _sutTaskService.StartTask();
            Thread.Sleep(Utilities.GetMillisecondsFromSeconds(3));
            _sutTaskService.StartTask();
            Thread.Sleep(Utilities.GetMillisecondsFromSeconds(3));
            _sutTaskService.StartTask();
            Thread.Sleep(Utilities.GetMillisecondsFromSeconds(12));


            LoggerMock.VerifyLog(logger => logger.LogInformation("StartTask"), Times.Exactly(3));
            LoggerMock.VerifyLog(logger => logger.LogInformation("StopTask"), Times.Never);
            LoggerMock.VerifyLog(logger => logger.LogInformation("RunTask"), Times.Once);
            LoggerMock.VerifyLog(logger => logger.LogInformation("TaskTodo"), Times.Once);
        }

        [Fact]
        public void Test2()
        {
            _sutTaskService.StartTask();
            Thread.Sleep(Utilities.GetMillisecondsFromSeconds(3));
            _sutTaskService.StopTask();
            Thread.Sleep(Utilities.GetMillisecondsFromSeconds(12));


            LoggerMock.VerifyLog(logger => logger.LogInformation("StartTask"), Times.Once);
            LoggerMock.VerifyLog(logger => logger.LogInformation("StopTask"), Times.Once);
            LoggerMock.VerifyLog(logger => logger.LogInformation("RunTask"), Times.Once);
            LoggerMock.VerifyLog(logger => logger.LogInformation("TaskTodo"), Times.Never);
        }

        [Fact]
        public void Test3()
        {
            _sutTaskService.StartTask();
            Thread.Sleep(Utilities.GetMillisecondsFromSeconds(3));
            _sutTaskService.StopTask();
            Thread.Sleep(Utilities.GetMillisecondsFromSeconds(2));
            _sutTaskService.StartTask();
            Thread.Sleep(Utilities.GetMillisecondsFromSeconds(12));

            LoggerMock.VerifyLog(logger => logger.LogInformation("StartTask"), Times.Exactly(2));
            LoggerMock.VerifyLog(logger => logger.LogInformation("StopTask"), Times.Once);
            LoggerMock.VerifyLog(logger => logger.LogInformation("RunTask"), Times.Exactly(2));
            LoggerMock.VerifyLog(logger => logger.LogInformation("TaskTodo"), Times.Once);
        }

        [Fact]
        public void Test4()
        {
            _sutTaskService.StartTask();
            Thread.Sleep(Utilities.GetMillisecondsFromSeconds(12));
            _sutTaskService.StartTask();
            Thread.Sleep(Utilities.GetMillisecondsFromSeconds(12));

            LoggerMock.VerifyLog(logger => logger.LogInformation("StartTask"), Times.Exactly(2));
            LoggerMock.VerifyLog(logger => logger.LogInformation("StopTask"), Times.Never);
            LoggerMock.VerifyLog(logger => logger.LogInformation("RunTask"), Times.Exactly(2));
            LoggerMock.VerifyLog(logger => logger.LogInformation("TaskTodo"), Times.Exactly(2));
        }
    }
}