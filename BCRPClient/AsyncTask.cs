﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPClient
{
    public class AsyncTask
    {
        /*
            Async Tasks Manager by frytech
         */

        private Task Task { get; set; }
        private CancellationTokenSource CancellationTokenSource { get; set; }

        private Action Action { get; set; }
        private Func<bool> Func { get; set; }

        private int Delay { get; set; }
        private bool Loop { get; set; }

        private int DelayToStart { get; set; }

        /// <summary>Завершено ли задание?</summary>
        public bool IsFinished { get => Task?.IsCompleted != false; }

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
            if ((Action == null && Func == null) || Task?.IsCompleted == false)
                return;

            CancellationTokenSource = new CancellationTokenSource();

            if (Action != null)
                Task = Loop ? ExecuteLoopAction(CancellationTokenSource.Token) : ExecuteOnceAction(CancellationTokenSource.Token);
            else
                Task = Loop ? ExecuteLoopFunc(CancellationTokenSource.Token) : ExecuteOnceFunc(CancellationTokenSource.Token);
        }

        public static void RunSlim(Action action, int timeout) => ExecuteOnceAction(action, timeout);

        /// <summary>Отменить задание</summary>
        public void Cancel()
        {
            if (CancellationTokenSource == null)
                return;

            CancellationTokenSource.Cancel();
            //Task = null;
            CancellationTokenSource = null;
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
        }

        private async System.Threading.Tasks.Task ExecuteLoopAction(CancellationToken ct)
        {
            await RAGE.Game.Invoker.WaitAsync(DelayToStart);

            while (!ct.IsCancellationRequested)
            {
                Action.Invoke();

                await RAGE.Game.Invoker.WaitAsync(Delay);
            }
        }

        private async System.Threading.Tasks.Task ExecuteOnceFunc(CancellationToken ct)
        {
            await RAGE.Game.Invoker.WaitAsync(Delay);

            if (!ct.IsCancellationRequested)
                Func.Invoke();
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
        }
        #endregion
    }
}
