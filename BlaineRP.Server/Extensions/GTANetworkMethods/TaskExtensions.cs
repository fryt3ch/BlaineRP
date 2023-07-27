using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlaineRP.Server.Extensions.GTANetworkMethods
{
    public static class TaskExtensions
    {
        public static async Task<T> RunAsync<T>(this global::GTANetworkMethods.Task task, Func<T> func, long delay = 0)
        {
            if (delay <= 0 && Thread.CurrentThread.ManagedThreadId == global::GTANetworkAPI.NAPI.MainThreadId)
            {
                return func.Invoke();
            }

            var taskCompletionSource = new TaskCompletionSource<T>();

            task.Run(() => taskCompletionSource.SetResult(func.Invoke()), delay);

            return await taskCompletionSource.Task;
        }

        public static async Task RunAsync(this global::GTANetworkMethods.Task task, Action action, long delay = 0)
        {
            if (delay <= 0 && Thread.CurrentThread.ManagedThreadId == global::GTANetworkAPI.NAPI.MainThreadId)
            {
                action.Invoke();

                return;
            }

            var taskCompletionSource = new TaskCompletionSource<object>();

            task.Run(() => { action.Invoke(); taskCompletionSource.SetResult(null); }, delay);

            await taskCompletionSource.Task;
        }

        public static void RunSafe(this global::GTANetworkMethods.Task task, Action action, long delay = 0)
        {
            if (delay <= 0 && Thread.CurrentThread.ManagedThreadId == global::GTANetworkAPI.NAPI.MainThreadId)
            {
                action.Invoke();

                return;
            }

            task.Run(action, delay);
        }
    }
}