using System;
using System.Collections.Generic;
using BlaineRP.Client.Utils;

namespace BlaineRP.Client.Additional
{
    public class Scaleform
    {
        private static Dictionary<string, Scaleform> AllScaleforms { get; set; } = new Dictionary<string, Scaleform>();

        public static Scaleform Get(string id) => AllScaleforms.GetValueOrDefault(id);

        public enum CounterSoundTypes
        {
            None = 0,

            Default,
            Deep,
        }

        public string Id { get; private set; }

        public int Handle { get; private set; }

        public bool IsLoaded => RAGE.Game.Graphics.HasScaleformMovieLoaded(Handle);

        public bool Exists => Handle > 0;

        private Queue<(string FuncName, object[] Args)> FunctionsQueue { get; set; } = new Queue<(string, object[])>();

        public AsyncTask CurrentTask { get; set; }

        public Main.UpdateHandler OnRender { get; set; }

        public Scaleform(string Id, string ScaleformName)
        {
            Handle = RAGE.Game.Graphics.RequestScaleformMovie(ScaleformName);

            this.Id = Id;

            if (!AllScaleforms.TryAdd(Id, this))
            {
                AllScaleforms[Id].Destroy();

                AllScaleforms.Add(Id, this);
            }

            Main.Render -= OnRender;
            Main.Render += OnRender;
        }

        public static Scaleform CreateShard(string id, string title, string text, int duration = -1)
        {
            var sc = new Scaleform(id, "mp_big_message_freemode");

            sc.CallFunction("SHOW_SHARD_CENTERED_MP_MESSAGE", title, text);

            if (duration > 0)
            {
                sc.CurrentTask = new AsyncTask(() => sc.Destroy(), duration, false, 0);

                sc.CurrentTask.Run();
            }

            sc.AddOnRenderAction(sc.Render2D);

            return sc;
        }

        public static Scaleform CreateCounter(string id, string title, string text, int durationSec = 5, CounterSoundTypes soundType = CounterSoundTypes.None)
        {
            var sc = new Scaleform(id, "mp_big_message_freemode");

            AsyncTask task = null;

            task = new AsyncTask(async () =>
            {
                for (int i = durationSec; i > 0; i--)
                {
                    sc.CallFunction("SHOW_SHARD_CENTERED_MP_MESSAGE", title, string.Format(text, i));

                    if (soundType != CounterSoundTypes.None)
                    {
                        if (i > 1)
                        {
                            if (soundType == CounterSoundTypes.Default)
                            {
                                RAGE.Game.Audio.PlaySoundFrontend(-1, "5_SEC_WARNING", "HUD_MINI_GAME_SOUNDSET", true);
                            }
                            else if (soundType == CounterSoundTypes.Deep)
                            {
                                RAGE.Game.Audio.PlaySoundFrontend(-1, "3_2_1", "HUD_MINI_GAME_SOUNDSET", true);
                            }
                        }
                        else
                        {
                            if (soundType == CounterSoundTypes.Default)
                                RAGE.Game.Audio.PlaySoundFrontend(-1, "TIMER_STOP", "HUD_MINI_GAME_SOUNDSET", true);
                            else if (soundType == CounterSoundTypes.Deep)
                                RAGE.Game.Audio.PlaySoundFrontend(-1, "GO", "HUD_MINI_GAME_SOUNDSET", true);
                        }
                    }

                    await RAGE.Game.Invoker.WaitAsync(1000);

                    if (task.IsCancelled)
                        return;
                }

                sc.Destroy();
            }, 0, false, 0);

            sc.CurrentTask = task;

            sc.CurrentTask.Run();

            sc.AddOnRenderAction(sc.Render2D);

            return sc;
        }

        public void AddOnRenderAction(Action action)
        {
            Main.Render -= OnRender;

            OnRender -= action.Invoke;
            OnRender += action.Invoke;

            Main.Render += OnRender;
        }

        public void RemoveOnRenderAction(Action action)
        {
            Main.Render -= OnRender;

            OnRender -= action.Invoke;

            Main.Render += OnRender;
        }

        private void OnUpdate()
        {
            if (FunctionsQueue.Count > 0)
            {
                (string, object[]) nextFunc;

                while (FunctionsQueue.TryDequeue(out nextFunc))
                    CallFunction(nextFunc.Item1, nextFunc.Item2);
            }
        }

        public void Destroy()
        {
            if (!Exists)
                return;

            Main.Render -= OnRender;

            OnRender = null;

            if (CurrentTask != null)
            {
                CurrentTask.Cancel();

                CurrentTask = null;
            }

            var handle = Handle;

            RAGE.Game.Graphics.SetScaleformMovieAsNoLongerNeeded(ref handle);

            Handle = 0;

            AllScaleforms.Remove(Id);
        }

        public void CallFunction(string funcName, params object[] args)
        {
            if (!IsLoaded || !Exists)
            {
                FunctionsQueue.Enqueue((funcName, args));

                return;
            }

            RAGE.Game.Graphics.PushScaleformMovieFunction(Handle, funcName);

            foreach (var x in args)
            {
                if (x is string str)
                    RAGE.Game.Graphics.PushScaleformMovieFunctionParameterString(str);
                else if (x is bool b)
                    RAGE.Game.Graphics.PushScaleformMovieFunctionParameterBool(b);
                else if (x is float f)
                    RAGE.Game.Graphics.PushScaleformMovieFunctionParameterFloat(f);
                else if (x is int i)
                    RAGE.Game.Graphics.PushScaleformMovieFunctionParameterInt(i);
            }

            RAGE.Game.Graphics.PopScaleformMovieFunctionVoid();
        }

        public void Render2D()
        {
            if (!IsLoaded || !Exists)
                return;

            OnUpdate();

            RAGE.Game.Graphics.DrawScaleformMovieFullscreen(Handle, 255, 255, 255, 255, 0);
        }
    }
}
