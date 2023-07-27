using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.AntiCheat
{
    public static partial class Service
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("AC::Detect::TP")]
            private static void DetectTP(Player sender, float dist)
            {
                var data = sender.GetMainData();

                if (data?.AdminLevel > 0)
                    return;

                /*            if (sender.CheckSpamAttack(false, 10000).IsSpammer)
                                return;*/

                //Utils.MsgToAdmins(string.Format(Locale.Chat.Admin.TeleportWarning, sender.Name + $"({sender.Id}) | #{data?.CID.ToString() ?? "null"}"));

                NAPI.Util.ConsoleOutput($"BAC: Обнаружен TP | Дистанция: {dist} м. Игрок: {sender.Name} ({sender.Id}) | #{data?.CID.ToString() ?? "null"}");
            }
        }
    }
}