using RAGE;
using RAGE.Elements;

namespace BCRPClient.Data
{
    partial class Commands
    {
        [Command("invisibility", true, "Включить/выключить невидимость", "inv", "invis")]
        public static void Invisibility(bool? state = null)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_invis", Player.LocalPlayer.RemoteId, state == null ? "" : state.ToString());
        }

        [Command("godmode", true, "Включить/выключить бессмертие", "gm")]
        public static void GodMode(bool? state = null)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_gm", Player.LocalPlayer.RemoteId, state == null ? "" : state.ToString());
        }

        [Command("fly", true, "Включить/выключить режим полета")]
        public static void Fly(bool? state = null)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_fly", Player.LocalPlayer.RemoteId, state == null ? "" : state.ToString());
        }

        [Command("sethealth", true, "Установить здоровье (игроку)", "sethp", "shp")]
        public static void SetHealth(uint pid, uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_hp", pid, value);

            LastSent = Sync.World.ServerTime;
        }

        [Command("health", true, "Установить здоровье (себе)", "hp")]
        public static void Health(uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_hp", Player.LocalPlayer.RemoteId, value);

            LastSent = Sync.World.ServerTime;
        }

        [Command("mood", true, "Установить настроение (себе)")]
        public static void Mood(uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_mood", Player.LocalPlayer.RemoteId, value);

            LastSent = Sync.World.ServerTime;
        }

        [Command("satiety", true, "Установить сытость (себе)")]
        public static void Satiety(uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_satiety", Player.LocalPlayer.RemoteId, value);

            LastSent = Sync.World.ServerTime;
        }

        [Command("setmood", true, "Установить настроение игроку")]
        public static void SetMood(uint pid, uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_mood", pid, value);

            LastSent = Sync.World.ServerTime;
        }

        [Command("setsatiety", true, "Установить сытость игроку")]
        public static void SetSatiety(uint pid, uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_satiety", pid, value);

            LastSent = Sync.World.ServerTime;
        }

        [Command("pos", true, "Получить текущую позицию", "position")]
        public static void Position(bool onGround = false)
        {
            var pos = Player.LocalPlayer.Position;

            if (onGround)
                pos.Z -= 1f;

            Events.CallLocal("Chat::ShowServerMessage", string.Format(Locale.Notifications.Commands.Position, pos.X, pos.Y, pos.Z, Player.LocalPlayer.GetHeading()));
        }

        [Command("posv", true, "Получить текущую позицию (veh)", "position")]
        public static void PositionVehicle()
        {
            var pos = Player.LocalPlayer.Vehicle?.Position;

            if (pos == null)
                return;

            Events.CallLocal("Chat::ShowServerMessage", string.Format(Locale.Notifications.Commands.Position, pos.X, pos.Y, pos.Z, Player.LocalPlayer.Vehicle.GetHeading()));
        }

        [Command("pos_s", true, "Получить текущую позицию", "position_save")]
        public static void PositionSave(bool onGround = false, string info = "")
        {
            var pos = Player.LocalPlayer.Position;

            if (onGround)
                pos.Z -= 1f;

            Utils.DebugServerSaveText($"POS_S ({pos.X}f, {pos.Y}f, {pos.Z}f, {Player.LocalPlayer.GetHeading()}f)");
        }

        [Command("posv_s", true, "Получить текущую позицию", "position_save")]
        public static void PositionVehicleSave(string info = "")
        {
            var pos = Player.LocalPlayer.Vehicle?.Position;

            if (pos == null)
                return;

            Utils.DebugServerSaveText($"POSV_S ({pos.X}f, {pos.Y}f, {pos.Z}f, {Player.LocalPlayer.Vehicle.GetHeading()}f)");
        }
    }
}
