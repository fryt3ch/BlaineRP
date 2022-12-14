using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BCRPClient.Additional
{
    class Scaleform : Events.Script
    {
        private static int CurrentHandle;

        private static AsyncTask CurrentTask { get; set; }

        public Scaleform()
        {
            CurrentHandle = -1;
        }

        public enum CounterSoundTypes
        {
            None = 0,

            Default,
            Deep,
        }

        public static async void ShowShard(string title, string text, int duration = -1)
        {
            int handle = RAGE.Game.Graphics.RequestScaleformMovie("mp_big_message_freemode");

            while (!RAGE.Game.Graphics.HasScaleformMovieLoaded(handle))
                await RAGE.Game.Invoker.WaitAsync(25);

            CurrentHandle = handle;

            RAGE.Game.Graphics.PushScaleformMovieFunction(handle, "SHOW_SHARD_CENTERED_MP_MESSAGE");

            RAGE.Game.Graphics.PushScaleformMovieFunctionParameterString(title);
            RAGE.Game.Graphics.PushScaleformMovieFunctionParameterString(text);

/*            RAGE.Game.Graphics.PushScaleformMovieFunctionParameterInt(titleColor);
            RAGE.Game.Graphics.PushScaleformMovieFunctionParameterInt(backgroundColor);*/

            RAGE.Game.Graphics.PopScaleformMovieFunctionVoid();

            GameEvents.Render -= Render;
            GameEvents.Render += Render;

            if (duration != -1)
            {
                CurrentTask = new AsyncTask(() => Close(), duration, false, 0);

                CurrentTask.Run();
            }
        }

        public static void ShowWasted(uint reason, Player killer)
        {
            Close();

            if (killer.Handle == Player.LocalPlayer.Handle)
            {
                ShowShard("~r~" + Locale.Scaleform.Wasted.Header, Locale.Scaleform.Wasted.TextSelf, -1);
            }
            else
                ShowShard("~r~" + Locale.Scaleform.Wasted.Header, string.Format(Locale.Scaleform.Wasted.TextAttacker, killer.GetName(true, false, true), Sync.Players.GetData(killer)?.CID ?? 0), -1);
        }

        public static void ShowCounter(string title, string text, int durationSec = 5, CounterSoundTypes soundType = CounterSoundTypes.None)
        {
            Close();

            AsyncTask task = null;

            task = new AsyncTask(async () =>
            {
                for (int i = durationSec; i > 0; i--)
                {
                    ShowShard(title, string.Format(text, i), -1);

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

                Close();
            }, 0, false, 0);

            CurrentTask = task;

            CurrentTask.Run();
        }

        public static void Close()
        {
            if (CurrentHandle == -1)
                return;

            CurrentTask?.Cancel();

            CurrentTask = null;

            GameEvents.Render -= Render;

            RAGE.Game.Graphics.SetScaleformMovieAsNoLongerNeeded(ref CurrentHandle);
        }

        private static void Render()
        {
            if (CurrentHandle != -1)
                RAGE.Game.Graphics.DrawScaleformMovieFullscreen(CurrentHandle, 255, 255, 255, 255, 0);
        }
    }
}
