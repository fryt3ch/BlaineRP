﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace BlaineRP.Client
{
    public class AsyncTask
    {
        private CancellationTokenSource _cancellationTokenSource;

        private readonly Action _action;
        private readonly Func<bool> _func;

        private readonly int _delay;
        private readonly bool _loop;

        private readonly int _delayToStart;

        /// <summary>Завершено ли задание?</summary>
        public bool IsFinished { get; private set; }

        public bool IsCancelled => _cancellationTokenSource?.IsCancellationRequested != false;

        /// <summary>Новое асинхронное задание с возвращаемым значением</summary>
        /// <param name="action">Действие</param>
        /// <param name="delay">Задержка в мс.</param>
        /// <param name="loop">Зациклить?</param>
        /// <param name="delayToStart">Задержка перед началом (срабатывает только если Loop = true)</param>
        public AsyncTask(Action action, int delay = 0, bool loop = false, int delayToStart = 0)
        {
            this._func = null;
            this._action = action;

            this._delay = delay;
            this._loop = loop;

            this._delayToStart = delayToStart;
        }

        /// <summary>Новое асинхронное задание без возвращаемого значения</summary>
        /// <param name="func">Действие</param>
        /// <param name="delay">Задержка в мс.</param>
        /// <param name="loop">Зациклить?</param>
        /// <param name="delayToStart">Задержка перед началом (срабатывает только если Loop = true)</param>
        public AsyncTask(Func<bool> func, int delay = 0, bool loop = false, int delayToStart = 0)
        {
            this._action = null;
            this._func = func;

            this._delay = delay;
            this._loop = loop;

            this._delayToStart = delayToStart;
        }

        public void Run()
        {
            if (_action == null && _func == null)
                return;

            IsFinished = false;

            _cancellationTokenSource = new CancellationTokenSource();

            if (_action != null)
            {
                if (_loop)
                    ExecuteLoopAction(_cancellationTokenSource.Token);
                else
                    ExecuteOnceAction(_cancellationTokenSource.Token);
            }
            else
            {
                if (_loop)
                    ExecuteLoopFunc(_cancellationTokenSource.Token);
                else
                    ExecuteOnceFunc(_cancellationTokenSource.Token);
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        private async System.Threading.Tasks.Task ExecuteOnceAction(CancellationToken ct)
        {
            await RAGE.Game.Invoker.WaitAsync(_delay);

            if (!ct.IsCancellationRequested)
                _action.Invoke();

            IsFinished = true;
        }

        private async System.Threading.Tasks.Task ExecuteLoopAction(CancellationToken ct)
        {
            if (_delayToStart > 0)
                await RAGE.Game.Invoker.WaitAsync(_delayToStart);

            while (!ct.IsCancellationRequested)
            {
                _action.Invoke();

                await RAGE.Game.Invoker.WaitAsync(_delay);
            }

            IsFinished = true;
        }

        private async System.Threading.Tasks.Task ExecuteOnceFunc(CancellationToken ct)
        {
            await RAGE.Game.Invoker.WaitAsync(_delay);

            if (!ct.IsCancellationRequested)
                _func.Invoke();

            IsFinished = true;
        }

        // Return FALSE in delegate to continue loop, return TRUE - to stop it
        private async System.Threading.Tasks.Task ExecuteLoopFunc(CancellationToken ct)
        {
            if (_delayToStart > 0)
                await RAGE.Game.Invoker.WaitAsync(_delayToStart);

            while (!ct.IsCancellationRequested)
            {
                if (_func.Invoke())
                    return;

                await RAGE.Game.Invoker.WaitAsync(_delay);
            }

            IsFinished = true;
        }

        public static class Methods
        {
            private static readonly Dictionary<string, AsyncTask> PendingTasksDict = new Dictionary<string, AsyncTask>();

            public static void SetAsPending(AsyncTask asyncTask, string key)
            {
                if (!PendingTasksDict.TryAdd(key, asyncTask))
                {
                    PendingTasksDict[key] = asyncTask;
                }

                asyncTask.Run();
            }

            public static bool CancelPendingTask(string key)
            {
                if (!PendingTasksDict.Remove(key, out var aTask)) return false;

                aTask.Cancel();

                return true;
            }

            public static bool IsTaskStillPending(string key, AsyncTask aTask)
            {
                var task = PendingTasksDict.GetValueOrDefault(key);

                if (task?.IsCancelled != false)
                    return false;

                return aTask == null || task == aTask;
            }

            public static void Run(Action action, int timeout) => ExecuteOnceAction(action, timeout);

            private static async void ExecuteOnceAction(Action action, int timeout)
            {
                await RAGE.Game.Invoker.WaitAsync(timeout);

                action.Invoke();
            }
        }
    }
}