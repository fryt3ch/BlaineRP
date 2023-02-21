using System;
using System.Threading;

namespace BCRPClient
{
    public class AsyncTask
    {
        /*
            Async Tasks Manager by frytech
         */

        private CancellationTokenSource CancellationTokenSource { get; set; }

        private Action Action { get; set; }
        private Func<bool> Func { get; set; }

        private int Delay { get; set; }
        private bool Loop { get; set; }

        private int DelayToStart { get; set; }

        /// <summary>Завершено ли задание?</summary>
        public bool IsFinished { get; private set; }

        public bool IsCancelled => CancellationTokenSource?.IsCancellationRequested != false;

        /// <summary>Новое асинхронное задание с возвращаемым значением</summary>
        /// <param name="Action">Действие</param>
        /// <param name="Delay">Задержка в мс.</param>
        /// <param name="Loop">Зациклить?</param>
        /// <param name="DelayToStart">Задержка перед началом (срабатывает только если Loop = true)</param>
        public AsyncTask(Action Action, int Delay = 0, bool Loop = false, int DelayToStart = 0)
        {
            this.Func = null;
            this.Action = Action;

            this.Delay = Delay;
            this.Loop = Loop;

            this.DelayToStart = DelayToStart;
        }

        /// <summary>Новое асинхронное задание без возвращаемого значения</summary>
        /// <param name="Func">Действие</param>
        /// <param name="Delay">Задержка в мс.</param>
        /// <param name="Loop">Зациклить?</param>
        /// <param name="DelayToStart">Задержка перед началом (срабатывает только если Loop = true)</param>
        public AsyncTask(Func<bool> Func, int Delay = 0, bool Loop = false, int DelayToStart = 0)
        {
            this.Action = null;
            this.Func = Func;

            this.Delay = Delay;
            this.Loop = Loop;

            this.DelayToStart = DelayToStart;
        }

        /// <summary>Запустить задание</summary>
        public void Run()
        {
            if (Action == null && Func == null)
                return;

            IsFinished = false;

            CancellationTokenSource = new CancellationTokenSource();

            if (Action != null)
            {
                if (Loop)
                    ExecuteLoopAction(CancellationTokenSource.Token);
                else
                    ExecuteOnceAction(CancellationTokenSource.Token);
            }
            else
            {
                if (Loop)
                    ExecuteLoopFunc(CancellationTokenSource.Token);
                else
                    ExecuteOnceFunc(CancellationTokenSource.Token);
            }
        }

        public static void RunSlim(Action action, int timeout) => ExecuteOnceAction(action, timeout);

        /// <summary>Отменить задание</summary>
        public void Cancel()
        {
            if (CancellationTokenSource == null)
                return;

            CancellationTokenSource.Cancel();
        }

        #region Sub Methods
        private static async void ExecuteOnceAction(Action action, int timeout)
        {
            await RAGE.Game.Invoker.WaitAsync(timeout);

            action.Invoke();
        }

        private async System.Threading.Tasks.Task ExecuteOnceAction(CancellationToken ct)
        {
            await RAGE.Game.Invoker.WaitAsync(Delay);

            if (!ct.IsCancellationRequested)
                Action.Invoke();

            IsFinished = true;
        }

        private async System.Threading.Tasks.Task ExecuteLoopAction(CancellationToken ct)
        {
            await RAGE.Game.Invoker.WaitAsync(DelayToStart);

            while (!ct.IsCancellationRequested)
            {
                Action.Invoke();

                await RAGE.Game.Invoker.WaitAsync(Delay);
            }

            IsFinished = true;
        }

        private async System.Threading.Tasks.Task ExecuteOnceFunc(CancellationToken ct)
        {
            await RAGE.Game.Invoker.WaitAsync(Delay);

            if (!ct.IsCancellationRequested)
                Func.Invoke();

            IsFinished = true;
        }

        // Return FALSE in delegate to continue loop, return TRUE - to stop it
        private async System.Threading.Tasks.Task ExecuteLoopFunc(CancellationToken ct)
        {
            await RAGE.Game.Invoker.WaitAsync(DelayToStart);

            while (!ct.IsCancellationRequested)
            {
                if (Func.Invoke())
                    return;

                await RAGE.Game.Invoker.WaitAsync(Delay);
            }

            IsFinished = true;
        }
        #endregion
    }
}
