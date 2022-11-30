using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Additional
{
    class SkyCamera : Events.Script
    {
        public static bool IsFadedOut { get => RAGE.Game.Cam.IsScreenFadedOut() || RAGE.Game.Cam.IsScreenFadingOut(); }
        public static bool ShouldBeFadedOut { get; set; }

        public static bool IsCamOnAir { get => RAGE.Game.Streaming.GetPlayerSwitchState() != 12; }

        public enum SwitchType
        {
            ToPlayer = 0,
            OutFromPlayer,
            Move,
        }

        struct EventToCall
        {
            public string Name;
            public object[] Args;
        }

        private static Queue<EventToCall> EventsQueue = new Queue<EventToCall>();

        public SkyCamera()
        {
            #region Events
            Events.Add("SkyCamera::Move", async (object[] args) =>
            {
                SwitchType sType = (SwitchType)(int)args[0];
                bool fade = (bool)args[1];

                string eventName = null;
                object[] eventArgs = null;

                if (args.Length > 2)
                    eventName = (string)args[2];

                if (args.Length > 3)
                    eventArgs = Utils.ConvertJArrayToList<object>((JArray)args[3]).ToArray();

                if (eventName != null)
                    EventsQueue.Enqueue(new EventToCall() { Name = eventName, Args = eventArgs });

                await Move(sType, fade);
            });

            Events.Add("FadeScreen", (object[] args) =>
            {
                bool state = (bool)args[0];

                if (args.Length > 1)
                {
                    if (args.Length > 2)
                        FadeScreen(state, (int)args[1], (int)args[2]);
                    else
                        FadeScreen(state, (int)args[1]);
                }
                else
                    FadeScreen(state);
            });
            #endregion
        }

        public static void FadeScreen(bool state, int speed = Settings.DEFAULT_FADE_IN_OUT_SPEED, int inTime = -1)
        {
            if (state)
            {
                if (IsFadedOut)
                    RAGE.Game.Cam.DoScreenFadeIn(0);

                RAGE.Game.Cam.DoScreenFadeOut(speed);

                CEF.Browser.HideAll(true);
            }
            else
            {
                if (IsFadedOut)
                    RAGE.Game.Cam.DoScreenFadeIn(speed);

                CEF.Browser.HideAll(false);
            }

            ShouldBeFadedOut = state;

            if (state && inTime >= 0)
            {
                AsyncTask.RunSlim(() => FadeScreen(false, speed, -1), inTime);
            }
        }

        public static async System.Threading.Tasks.Task Move(SwitchType type, bool fade)
        {
            Player.LocalPlayer.FreezePosition(true);

            if (fade)
                FadeScreen(true);

            int counter = 0;

            if (type == SwitchType.ToPlayer)
            {
                RAGE.Game.Invoker.Invoke(0xD8295AF639FD9CB8, Player.LocalPlayer.Handle);

                (new AsyncTask(() =>
                {
                    if (!IsCamOnAir || counter > 50)
                    {
                        RAGE.Game.Invoker.Invoke(0x95C0A5BBDC189AA1);

                        Finished(true, fade);

                        return true;
                    }

                    counter++;

                    return false;
                }, 100, true)).Run();
            }
            else if (type == SwitchType.OutFromPlayer)
            {
                RAGE.Game.Invoker.Invoke(RAGE.Game.Natives.SwitchOutPlayer, Player.LocalPlayer.Handle, 513, 2);

                (new AsyncTask(() =>
                {
                    if (RAGE.Game.Streaming.GetPlayerSwitchState() == 5 || counter > 50)
                    {
                        Finished(false, fade);

                        return true;
                    }

                    counter++;

                    return false;
                }, 100, true)).Run();
            }
            else if (type == SwitchType.Move)
            {
                FadeScreen(true, 300);

                await RAGE.Game.Invoker.WaitAsync(300);

                RAGE.Game.Invoker.Invoke(0x95C0A5BBDC189AA1);

                RAGE.Game.Invoker.Invoke(RAGE.Game.Natives.SwitchOutPlayer, Player.LocalPlayer.Handle, 513, 2);

                (new AsyncTask(() =>
                {
                    if (RAGE.Game.Streaming.GetPlayerSwitchState() == 5 || counter > 50)
                    {
                        FadeScreen(false, 300);

                        Finished(false, fade);

                        return true;
                    }

                    counter++;

                    return false;
                }, 100, true)).Run();
            }
        }

        public static void Finished(bool down, bool fade)
        {
            if (fade)
                FadeScreen(false, 1500);

            if (down)
                Player.LocalPlayer.FreezePosition(false);

            if (EventsQueue.Count != 0)
            {
                var eventToCall = EventsQueue.Dequeue();

                Events.CallLocal(eventToCall.Name, eventToCall.Args);
            }
        }

        public static void WrongFadeCheck()
        {
            if (ShouldBeFadedOut && !IsFadedOut)
            {
                RAGE.Game.Cam.DoScreenFadeOut(0);
            }
            else if (!ShouldBeFadedOut && IsFadedOut)
            {
                RAGE.Game.Cam.DoScreenFadeIn(0);
            }
        }
    }
}
