using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Additional
{
    class Scaleform : Events.Script
    {
        private static int CurrentHandle;

        public Scaleform()
        {
            CurrentHandle = -1;
        }

        public static void ShowShard(string title, string text, int titleColor, int backgroundColor, int duration = -1)
        {
            Close();

            int handle = RAGE.Game.Graphics.RequestScaleformMovie("mp_big_message_freemode");

            while (!RAGE.Game.Graphics.HasScaleformMovieLoaded(handle))
                RAGE.Game.Invoker.Wait(0);

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
                (new AsyncTask(() => Close(), duration, false, 0)).Run();
        }

        public static void ShowWasted(uint reason, Player killer)
        {
            if (killer.Handle == Player.LocalPlayer.Handle)
            {
                ShowShard("~r~" + Locale.Scaleform.Wasted.Header, Locale.Scaleform.Wasted.TextSelf, 6, 2);
            }
            else
                ShowShard("~r~" + Locale.Scaleform.Wasted.Header, string.Format(Locale.Scaleform.Wasted.TextAttacker, killer.GetName(true, false, true), Sync.Players.GetData(killer)?.CID ?? -1), 6, 2);
        }

        public static void Close()
        {
            if (CurrentHandle == -1)
                return;

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
