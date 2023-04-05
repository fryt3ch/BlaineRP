using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Sync
{
    public class Punishment
    {
        public static List<Punishment> All { get; set; } = new List<Punishment>();

        private static AsyncTask CheckTask { get; set; }

        public enum Types
        {
            /// <summary>Блокировка</summary>
            Ban = 0,

            /// <summary>Предупреждение</summary>
            Warn = 1,

            /// <summary>Мут</summary>
            Mute = 2,

            /// <summary>NonRP тюрьма</summary>
            NRPPrison = 3,

            /// <summary>СИЗО</summary>
            Arrest = 4,

            /// <summary>Федеральная тюрьма</summary>
            FederalPrison = 5,

            /// <summary>Мут чата фракции</summary>
            FractionMute = 6,

            /// <summary>Мут чата организации</summary>
            OrganisationMute = 7,
        }

        public uint Id { get; set; }

        public Types Type { get; set; }

        public DateTime EndDate { get; set; }

        public string AdditionalData { get; set; }

        public static bool IsCheckTaskStillNeeded => All.Where(x => x.Type != Types.Ban).Any();

        public void ShowErrorNotification()
        {
            var timeLeft = EndDate.Subtract(Sync.World.ServerTime);

            if (Type == Types.Mute)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, $"Вы не можете сделать это сейчас!\nУ Вас есть активный мут, до его снятия осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == Types.NRPPrison)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, $"Вы не можете сделать это сейчас!\nВы находитесь в NonRP-тюрьме, до выхода осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == Types.Warn)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, $"Вы не можете сделать это сейчас!\nУ Вас есть активное предупреждение, до его снятия осталось {timeLeft}\n\nВы можете досрочно снять предупреждение, для этого перейдите в Меню - Магазин");
            }
            else if (Type == Types.FractionMute)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, $"Вы не можете сделать это сейчас!\nУ Вас есть активный мут чата фракции, до его снятия осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == Types.OrganisationMute)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, $"Вы не можете сделать это сейчас!\nУ Вас есть активный мут чата организации, до его снятия осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == Types.Arrest)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, $"Вы не можете сделать это сейчас!\nВы арестованы и находитесь в СИЗО, до выхода осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == Types.FederalPrison)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, $"Вы не можете сделать это сейчас!\nВы арестованы и находитесь в Федеральной тюрьме, до выхода осталось {timeLeft.GetBeautyString()}");
            }
        }

        public static void StartCheckTask()
        {
            CheckTask?.Cancel();

            CheckTask = new AsyncTask(() =>
            {
                var curTime = Sync.World.ServerTime;

                foreach (var x in All)
                {
                    var timeLeft = x.EndDate.Subtract(curTime);

                    var secs = timeLeft.TotalSeconds;

                    if (secs < 0)
                    {
                        Events.CallRemote("Player::UnpunishMe", x.Id);
                    }
                    else
                    {

                    }
                }
            }, 1000, true, 0);

            CheckTask.Run();
        }

        public static void StopCheckTask()
        {
            if (CheckTask != null)
            {
                CheckTask.Cancel();

                CheckTask = null;
            }
        }

        public static void AddPunishment(Punishment data)
        {
            All.Add(data);

            if (IsCheckTaskStillNeeded)
                StartCheckTask();

            if (data.Type == Types.NRPPrison)
            {
                GameEvents.Render -= NonRpJailRender;
                GameEvents.Render += NonRpJailRender;
            }
            else if (data.Type == Types.Arrest)
            {
                GameEvents.Render -= ArrestRender;
                GameEvents.Render += ArrestRender;
            }
            else if (data.Type == Types.FederalPrison)
            {
                GameEvents.Render -= FederalPrisonRender;
                GameEvents.Render += FederalPrisonRender;
            }
        }

        public static void RemovePunishment(Punishment data)
        {
            All.Remove(data);

            if (!IsCheckTaskStillNeeded)
                StopCheckTask();

            if (data.Type == Types.NRPPrison)
            {
                GameEvents.Render -= NonRpJailRender;
            }
            else if (data.Type == Types.Arrest)
            {
                GameEvents.Render -= ArrestRender;
            }
            else if (data.Type == Types.FederalPrison)
            {
                GameEvents.Render -= FederalPrisonRender;
            }
        }

        private static void NonRpJailRender()
        {
            var jailData = All.Where(x => x.Type == Types.NRPPrison).FirstOrDefault();

            if (jailData == null)
                return;

            var timeLeft = jailData.EndDate.Subtract(Sync.World.ServerTime);

            Utils.DrawText($"ВЫ НАХОДИТЕСЬ В NONRP-ТЮРЬМЕ", 0.5f, 0.025f, 255, 0, 0, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);

            Utils.DrawText($"ОСТАЛОСЬ ВРЕМЕНИ: {timeLeft.GetBeautyString()}", 0.5f, 0.055f, 255, 255, 255, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
        }

        private static void ArrestRender()
        {
            var jailData = All.Where(x => x.Type == Types.Arrest).FirstOrDefault();

            if (jailData == null)
                return;

            var timeLeft = jailData.EndDate.Subtract(Sync.World.ServerTime);

            Utils.DrawText($"ВЫ НАХОДИТЕСЬ В СИЗО", 0.5f, 0.025f, 255, 0, 0, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);

            Utils.DrawText($"ОСТАЛОСЬ ВРЕМЕНИ: {timeLeft.GetBeautyString()}", 0.5f, 0.055f, 255, 255, 255, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
        }

        private static void FederalPrisonRender()
        {
            var jailData = All.Where(x => x.Type == Types.FederalPrison).FirstOrDefault();

            if (jailData == null)
                return;

            var timeLeft = jailData.EndDate.Subtract(Sync.World.ServerTime);

            Utils.DrawText($"ВЫ НАХОДИТЕСЬ В ФЕДЕРАЛЬНОЙ ТЮРЬМЕ", 0.5f, 0.025f, 255, 0, 0, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);

            Utils.DrawText($"ОСТАЛОСЬ ВРЕМЕНИ: {timeLeft.GetBeautyString()}", 0.5f, 0.055f, 255, 255, 255, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
        }
    }
}
