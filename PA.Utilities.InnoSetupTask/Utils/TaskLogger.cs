using Microsoft.Build.Framework;

namespace PA.Utilities.InnoSetupTask
{

    class TaskLogger
    {
        ITask task;

        public TaskLogger(ITask task)
        {
            this.task = task;
        }

        public void LogWarning(string message)
        {
            task.BuildEngine.LogWarningEvent(new BuildWarningEventArgs(string.Empty, string.Empty, null, 0, 0, 0, 0, message, string.Empty, "InnoSetupTask"));
        }

        public void LogInfo(string message)
        {
            task.BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message, string.Empty, "InnoSetupTask", MessageImportance.Normal));
        }

        public void LogError(string message, string file = null)
        {
            task.BuildEngine.LogErrorEvent(new BuildErrorEventArgs(string.Empty, string.Empty, file, 0, 0, 0, 0, message, string.Empty, "InnoSetupTask"));
        }
    }
}