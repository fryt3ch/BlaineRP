using RAGE.Elements;
using System;

namespace BlaineRP.Client.Data
{
    partial class Commands
    {
        [Command("eval", true, "js eval", "jseval", "jse")]
        public static void JsEval(string cmd)
        {
            Utils.JsEval(cmd);
        }

        [Command("dopayday", true, "Сделать PayDay")]
        public static void DoPayDay()
        {
            if (LastSent.IsSpam(500, false, true))
                return;

            CallRemote("s_payday");

            LastSent = Sync.World.ServerTime;
        }

        [Command("settime", true, "st")]
        public static void SetTime(byte hour, byte minute = 0, byte second = 0)
        {
            var realDate = Sync.World.ServerTime;

            if (hour >= 24)
                hour = 0;

            if (minute >= 60)
                minute = 0;

            if (second >= 60)
                second = 0;

            GameEvents.ExtraGameDate = new DateTime(realDate.Year, realDate.Month, realDate.Day, hour, minute, second);
        }

        [Command("resettime", true, "rst")]
        public static void ResetTime()
        {
            GameEvents.ExtraGameDate = null;
        }

        [Command("anim", true, "Смена измерения", "playanim")]
        public static async void PlayAnim(string dict, string name, float sp, float spMult, int dur, int fg, float pRate, bool p1 = false, bool p2 = false, bool p3 = false)
        {
            await Utils.RequestAnimDict(dict);

            Player.LocalPlayer.TaskPlayAnim(dict, name, sp, spMult, dur, fg, pRate, p1, p2, p3);
        }

        [Command("anim_stop", true, "Смена измерения", "stopanim")]
        public static void StopAnim()
        {
            Player.LocalPlayer.ClearTasks();
            Player.LocalPlayer.ClearTasksImmediately();
        }

        [Command("setclothes", true, "Надеть временную одежду", "sc", "sclothes")]
        public static void SetClothes(uint slot, uint drawable, uint texture = 0)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tclothes", Player.LocalPlayer.RemoteId, slot, drawable, texture, true);

            LastSent = Sync.World.ServerTime;
        }

        [Command("setaccs", true, "Надеть временный аксессуар", "sa", "saccs")]
        public static void SetAccs(uint slot, uint drawable, uint texture = 0)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tclothes", Player.LocalPlayer.RemoteId, slot, drawable, texture, false);

            LastSent = Sync.World.ServerTime;
        }

        [Command("resetclothes", true, "Сбросить временную одежду (и аксессуары)", "rsc", "rsclothes")]
        public static void ResetClothes()
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tclothes", Player.LocalPlayer.RemoteId, -1, -1, -1, true);

            LastSent = Sync.World.ServerTime;
        }
    }
}
