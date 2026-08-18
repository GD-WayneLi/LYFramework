using System;
using System.Threading.Tasks;
using LYFramework.Log;

namespace LYFramework.TaskEx
{
    public static class TaskExtension
    {
        public static void Forget(this Task task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            Observe(task);
        }

        public static void Forget(this ValueTask task)
        {
            Observe(task);
        }

        public static void Forget<T>(this ValueTask<T> task)
        {
            Observe(task);
        }

        private static async void Observe(Task task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException) when (task.IsCanceled)
            {
            }
            catch (Exception exception)
            {
                LYLogger.Error(exception.ToString());
            }
        }

        private static async void Observe(ValueTask task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                LYLogger.Error(exception.ToString());
            }
        }

        private static async void Observe<T>(ValueTask<T> task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                LYLogger.Error(exception.ToString());
            }
        }
    }
}
