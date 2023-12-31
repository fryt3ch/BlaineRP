﻿using System.Collections.Generic;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Misc
{
    [Script(int.MaxValue)]
    public class SkyCamera
    {
        public enum SwitchType
        {
            ToPlayer = 0,
            OutFromPlayer,
            Move,
        }

        private static Queue<EventToCall> EventsQueue = new Queue<EventToCall>();

        public SkyCamera()
        {
            Events.Add("SkyCamera::Move",
                async (args) =>
                {
                    var sType = (SwitchType)(int)args[0];
                    var fade = (bool)args[1];

                    string eventName = null;
                    object[] eventArgs = null;

                    if (args.Length > 2)
                        eventName = (string)args[2];

                    if (args.Length > 3)
                        eventArgs = ((JArray)args[3]).ToObject<object[]>();

                    if (eventName != null)
                        EventsQueue.Enqueue(new EventToCall()
                            {
                                Name = eventName,
                                Args = eventArgs,
                            }
                        );

                    await Move(sType, fade);
                }
            );

            Events.Add("FadeScreen",
                (object[] args) =>
                {
                    var state = (bool)args[0];

                    if (args.Length > 1)
                    {
                        if (args.Length > 2)
                            FadeScreen(state, (int)args[1], (int)args[2]);
                        else
                            FadeScreen(state, (int)args[1]);
                    }
                    else
                    {
                        FadeScreen(state, 500);
                    }
                }
            );
        }

        public static bool IsFadedOut => RAGE.Game.Cam.IsScreenFadedOut() || RAGE.Game.Cam.IsScreenFadingOut();
        public static bool ShouldBeFadedOut { get; set; }

        public static bool IsCamOnAir => RAGE.Game.Streaming.GetPlayerSwitchState() != 12;

        public static void FadeScreen(bool state, int speed, int inTime = -1)
        {
            Utils.Console.Output(state);

            if (state)
            {
                if (IsFadedOut)
                    RAGE.Game.Cam.DoScreenFadeIn(0);

                RAGE.Game.Cam.DoScreenFadeOut(speed);

                Browser.HideAll(true);
            }
            else
            {
                if (IsFadedOut)
                    RAGE.Game.Cam.DoScreenFadeIn(speed);

                Browser.HideAll(false);
            }

            ShouldBeFadedOut = state;

            if (state && inTime >= 0)
                AsyncTask.Methods.Run(() => FadeScreen(false, speed, -1), inTime);
        }

        public static async System.Threading.Tasks.Task Move(SwitchType type, bool fade)
        {
            Player.LocalPlayer.FreezePosition(true);

            if (fade)
                FadeScreen(true, 500);

            var counter = 0;

            if (type == SwitchType.ToPlayer)
            {
                RAGE.Game.Invoker.Invoke(0xD8295AF639FD9CB8, Player.LocalPlayer.Handle);

                new AsyncTask(() =>
                    {
                        if (!IsCamOnAir || counter > 50)
                        {
                            RAGE.Game.Invoker.Invoke(0x95C0A5BBDC189AA1);

                            Finished(true, fade);

                            return true;
                        }

                        counter++;

                        return false;
                    },
                    100,
                    true
                ).Run();
            }
            else if (type == SwitchType.OutFromPlayer)
            {
                RAGE.Game.Invoker.Invoke(RAGE.Game.Natives.SwitchOutPlayer, Player.LocalPlayer.Handle, 513, 2);

                new AsyncTask(() =>
                    {
                        if (RAGE.Game.Streaming.GetPlayerSwitchState() == 5 || counter > 50)
                        {
                            Finished(false, fade);

                            return true;
                        }

                        counter++;

                        return false;
                    },
                    100,
                    true
                ).Run();
            }
            else if (type == SwitchType.Move)
            {
                FadeScreen(true, 300);

                await RAGE.Game.Invoker.WaitAsync(300);

                RAGE.Game.Invoker.Invoke(0x95C0A5BBDC189AA1);

                RAGE.Game.Invoker.Invoke(RAGE.Game.Natives.SwitchOutPlayer, Player.LocalPlayer.Handle, 513, 2);

                new AsyncTask(() =>
                    {
                        if (RAGE.Game.Streaming.GetPlayerSwitchState() == 5 || counter > 50)
                        {
                            FadeScreen(false, 300);

                            Finished(false, fade);

                            return true;
                        }

                        counter++;

                        return false;
                    },
                    100,
                    true
                ).Run();
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
                EventToCall eventToCall = EventsQueue.Dequeue();

                Events.CallLocal(eventToCall.Name, eventToCall.Args);
            }
        }

        public static void WrongFadeCheck()
        {
            if (ShouldBeFadedOut && !IsFadedOut)
                RAGE.Game.Cam.DoScreenFadeOut(0);
            else if (!ShouldBeFadedOut && IsFadedOut)
                RAGE.Game.Cam.DoScreenFadeIn(0);
        }

        private struct EventToCall
        {
            public string Name;
            public object[] Args;
        }
    }
}