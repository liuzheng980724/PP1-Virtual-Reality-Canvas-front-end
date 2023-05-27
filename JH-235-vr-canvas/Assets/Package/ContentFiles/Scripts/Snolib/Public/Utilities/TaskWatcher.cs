using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snobal.Library;

namespace Snobal.Utilities
{
    public class TaskWatcher : IUpdatable
    {
        List<WatchableTask> tasksToWatchQueue = new List<WatchableTask>();
        List<WatchableTask> tasksToWatch;
        Action<Library.SnobalException> exceptionHandler;
        bool isShuttingDown = false;
        public bool HasActiveTasks { get { return (tasksToWatch.Count > 0) || (tasksToWatchQueue.Count > 0); } }

        public TaskWatcher(Action<Library.SnobalException> _handler)
        {
            exceptionHandler = _handler;
            tasksToWatch = new List<WatchableTask>();
        }

        public void Shutdown()
        {
            isShuttingDown = true;
        }

        /// <summary>
        /// Call this in each frame of your update loop
        /// </summary>
        public void Update()
        {
            if (isShuttingDown)
                return;

            // Add
            foreach (var q in tasksToWatchQueue)
            {
                tasksToWatch.Add(q);
            }
            tasksToWatchQueue.Clear();


            List<WatchableTask> toRemove = new List<WatchableTask>();

            foreach (WatchableTask watchableTask in tasksToWatch)
            {
                if (watchableTask.task.IsFaulted)
                {
                    var innerException = watchableTask.task.Exception.Flatten();
                    Library.SnobalException snobalException = new Library.SnobalException(watchableTask.task.Exception.InnerException.Message, innerException);
                    exceptionHandler(snobalException);
                    toRemove.Add(watchableTask);
                    continue;
                }

                if (watchableTask.task.IsCompleted)
                {
                    watchableTask.action?.Invoke();         // This callback may result in a call to Watch - adding to tasks
                    toRemove.Add(watchableTask);
                }
            }

            foreach (var item in toRemove)
            {
                tasksToWatch.Remove(item);
            }
        }

        public void Watch(Task task, Action onComplete)
        {
            if (isShuttingDown)
            {
                throw new Library.SnobalException("Tried to watch a task but taskwatcher is shutting down.");
            }

            tasksToWatchQueue.Add(new WatchableTask { task = task, action = onComplete });
        }

        public void Watch(Task task)
        {
            Watch(task, null);
        }

        public static Task ToTask (Action action)
        {
            return Task.Run(action);
        }

        public void WatchAction(Action action)
        {
            Watch(ToTask(action));
        }

        /// <summary>
        /// Run the given list of tasks. Timeout & report any issues.
        /// </summary>
        public static bool RunTasks(IEnumerable<Task> tasks, int timeoutMilliSec)
        {
            return RunTasks(new List<Task>(tasks), timeoutMilliSec);
        }
        public static bool RunTasks(Task[] tasks, int timeoutMilliSec)
        {
            return RunTasks(new List<Task>(tasks), timeoutMilliSec);
        }
        public static bool RunTasks(List<Task> tasks, int timeoutMilliSec)
        {
            bool completedExecution = false;
            try
            {
                completedExecution = Task.WaitAll(tasks.ToArray(), timeoutMilliSec);
            }
            catch (AggregateException e)
            {
                string message = "One or more errors occured: ";
                foreach (var exception in e.InnerExceptions)
                {
                    if (exception is SnobalException)
                    {
                        message += exception.Message + "\n";
                    }
                }
                throw new SnobalException(message, SnobalException.MSG_UnexpectedError);
            }
            return completedExecution;
        }
    }

    struct WatchableTask
    {
        public Task task;
        public Action action;
    }

    
}
