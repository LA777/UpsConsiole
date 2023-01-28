using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.Versioning;
using UpsConsole.Services;

namespace UpsConsole
{
    [SupportedOSPlatform("windows")]
    public class EventsWorker : BackgroundService
    {
        private readonly IConsoleSpinner _consoleSpinner;
        private readonly ILogger<EventsWorker> _logger;
        private readonly ITaskService _taskService;

        public EventsWorker(ILogger<EventsWorker> logger, ITaskService taskService, IConsoleSpinner consoleSpinner)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _consoleSpinner = consoleSpinner ?? throw new ArgumentNullException(nameof(consoleSpinner));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("EventsWorker ExecuteAsync started");
            _logger.LogInformation($"Executing as {(WindowsServiceHelpers.IsWindowsService() ? "windows service" : "console application")}. Working directory is {Directory.GetCurrentDirectory()}");

            Subscribe();

            await Task.Delay(0, cancellationToken);
        }

        public void Subscribe()
        {
            //APC UPS Service
            EventLogWatcher eventLogWatcher174 = null!;
            EventLogWatcher eventLogWatcher61455 = null!;
            try
            {
                //var query = "<QueryList>" +
                //            "<Query Id=\"\"0\"\" Path=\"\"Application\"\">" +
                //            "   <Select Path=\"\"Application\"\">*[System[(Source &lt;= \"SecurityCenter\") and TimeCreated[timediff(@SystemTime) &lt;= 86400000]]]</Select>      </Query></QueryList>";
                //"*[Application/EventID=9027]"

                //EventId - 174 - Battery backup transferred to battery power due to a blackout.
                //EventId - 61455 - Battery backup transferred to AC utility power.

                var logName = "Application";

                var subscriptionQuery174 = new EventLogQuery(logName, PathType.LogName, "*[System/EventID=174]");
                var subscriptionQuery61455 = new EventLogQuery(logName, PathType.LogName, "*[System/EventID=61455]");

                eventLogWatcher174 = new EventLogWatcher(subscriptionQuery174);
                eventLogWatcher61455 = new EventLogWatcher(subscriptionQuery61455);

                eventLogWatcher174.EventRecordWritten += EventLogEventRead174!;
                eventLogWatcher61455.EventRecordWritten += EventLogEventRead61455!;

                eventLogWatcher174.Enabled = true;
                eventLogWatcher61455.Enabled = true;

                _logger.LogInformation("Listen to events...");

                while (true)
                {
                    Thread.Sleep(Utilities.GetMillisecondsFromSeconds(1));
                    _consoleSpinner.Turn();
                }
            }
            catch (EventLogReadingException e)
            {
                _logger.LogInformation("Error reading the log: {0}", e.Message);
            }
            finally
            {
                // Stop listening to events
                eventLogWatcher174.Enabled = false;
                eventLogWatcher61455.Enabled = false;
                eventLogWatcher174.Dispose();
                eventLogWatcher61455.Dispose();
            }

            Console.ReadKey();
        }

        public void EventLogEventRead174(object obj, EventRecordWrittenEventArgs arg)
        {
            _logger.LogInformation(arg is { EventRecord: { } } ? $"Description: {arg.EventRecord.FormatDescription()}" : "The event instance was null.");
            _taskService.StartTask();
        }

        public void EventLogEventRead61455(object obj, EventRecordWrittenEventArgs arg)
        {
            _logger.LogInformation(arg is { EventRecord: { } } ? $"Description: {arg.EventRecord.FormatDescription()}" : "The event instance was null.");
            _taskService.StopTask();
        }
    }
}