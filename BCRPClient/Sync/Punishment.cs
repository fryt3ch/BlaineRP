using BCRPClient.CEF;
using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BCRPClient.Sync
{
    public class Punishment : Events.Script
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
                var strData = AdditionalData?.Split('_');

                if (strData == null)
                    return;

                timeLeft = TimeSpan.FromSeconds(EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

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
                var strData = AdditionalData?.Split('_');

                if (strData == null)
                    return;

                timeLeft = TimeSpan.FromSeconds(EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, $"Вы не можете сделать это сейчас!\nВы арестованы и находитесь в СИЗО, до выхода осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == Types.FederalPrison)
            {
                var strData = AdditionalData?.Split('_');

                if (strData == null)
                    return;

                timeLeft = TimeSpan.FromSeconds(EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

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
                    if (x.Type != Types.NRPPrison && x.Type != Types.FederalPrison && x.Type != Types.Arrest)
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
                    else
                    {
                        var dataS = x.AdditionalData?.Split('_');

                        if (dataS != null)
                        {
                            var time = long.Parse(dataS[0]) + 1;

                            dataS[0] = time.ToString();

                            x.AdditionalData = string.Join('_', dataS);
                        }

                        if (x.Type == Types.Arrest)
                        {
                            var cs = Additional.ExtraColshape.All.Where(x => x.Name == "CopArrestCell").ToList();

                            if (!cs.Where(x => x.IsInside == true).Any())
                            {
                                Events.CallRemote("Player::COPAR::TPME");
                            }
                        }
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

                var dataS = data.AdditionalData.Split('_');

                var fData = Data.Fractions.Fraction.Get((Data.Fractions.Types)int.Parse(dataS[1])) as Data.Fractions.Police;

                if (fData != null)
                {
                    foreach (var x in fData.ArrestCellsPositions)
                    {
                        Additional.ExtraColshape cs = null;

                        cs = new Additional.Sphere(new Vector3(x.X, x.Y, x.Z), 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                        {
                            Name = "CopArrestCell",
                        };
                    }
                }
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

                Additional.ExtraColshape.All.Where(x => x.Name == "CopArrestCell").ToList().ForEach(x => x.Destroy());
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

            var strData = jailData.AdditionalData?.Split('_');

            if (strData == null)
                return;

            var timeLeft = TimeSpan.FromSeconds(jailData.EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

            Utils.DrawText($"ВЫ НАХОДИТЕСЬ В NONRP-ТЮРЬМЕ", 0.5f, 0.025f, 255, 0, 0, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);

            Utils.DrawText($"ОСТАЛОСЬ ВРЕМЕНИ: {timeLeft.GetBeautyString()}", 0.5f, 0.055f, 255, 255, 255, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
        }

        private static void ArrestRender()
        {
            var jailData = All.Where(x => x.Type == Types.Arrest).FirstOrDefault();

            if (jailData == null)
                return;

            var strData = jailData.AdditionalData?.Split('_');

            if (strData == null)
                return;

            var timeLeft = TimeSpan.FromSeconds(jailData.EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

            Utils.DrawText($"ВЫ НАХОДИТЕСЬ В СИЗО", 0.5f, 0.025f, 255, 0, 0, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);

            Utils.DrawText($"ОСТАЛОСЬ ВРЕМЕНИ: {timeLeft.GetBeautyString()}", 0.5f, 0.055f, 255, 255, 255, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
        }

        private static void FederalPrisonRender()
        {
            var jailData = All.Where(x => x.Type == Types.FederalPrison).FirstOrDefault();

            if (jailData == null)
                return;

            var strData = jailData.AdditionalData?.Split('_');

            if (strData == null)
                return;

            var timeLeft = TimeSpan.FromSeconds(jailData.EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

            Utils.DrawText($"ВЫ НАХОДИТЕСЬ В ФЕДЕРАЛЬНОЙ ТЮРЬМЕ", 0.5f, 0.025f, 255, 0, 0, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);

            Utils.DrawText($"ОСТАЛОСЬ ВРЕМЕНИ: {timeLeft.GetBeautyString()}", 0.5f, 0.055f, 255, 255, 255, 255, 0.5f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
        }

        public Punishment()
        {
            Events.Add("Player::MuteShow", (args) => Sync.Punishment.All.Where(x => x.Type == Sync.Punishment.Types.Mute).FirstOrDefault()?.ShowErrorNotification());

            Events.Add("Player::FMuteShow", (args) => Sync.Punishment.All.Where(x => x.Type == Sync.Punishment.Types.FractionMute).FirstOrDefault()?.ShowErrorNotification());

            Events.Add("Player::Punish", (args) =>
            {
                var id = args[0].ToUInt32();

                var type = (Sync.Punishment.Types)(int)args[1];

                var admin = RAGE.Elements.Entities.Players.GetAtRemote((ushort)(int)args[2]);

                var endDateL = Convert.ToInt64(args[3]);

                var reason = (string)args[4];

                var addData = args.Length > 5 ? (string)args[5] : null;

                if (endDateL >= 0)
                {
                    var endDate = DateTimeOffset.FromUnixTimeSeconds(endDateL).DateTime;

                    var mData = new Sync.Punishment() { Type = type, EndDate = endDate, Id = id, AdditionalData = addData };

                    Sync.Punishment.AddPunishment(mData);

                    var timeStr = endDate.Subtract(Sync.World.ServerTime).GetBeautyString();

                    if (type == Punishment.Types.Mute)
                    {
                        CEF.Notification.Show(Notification.Types.Mute, "Мут", admin == null ? $"Вам был выдан мут (запрет на использование текстового и голосового чатов) на {timeStr}\n\nПричина: {reason}" : $"Администратор {admin.Name} #{admin.GetSharedData<object>("CID", 0)} выдал Вам мут (запрет на использование текстового и голосового чатов) на {timeStr}\n\nПричина: {reason}");
                    }
                    else if (type == Punishment.Types.NRPPrison)
                    {
                        var strData = mData.AdditionalData?.Split('_');

                        if (strData == null)
                            return;

                        timeStr = TimeSpan.FromSeconds(EndDate.GetUnixTimestamp() - long.Parse(strData[0])).GetBeautyString();

                        CEF.Notification.Show(Notification.Types.Jail1, "NonRP-тюрьма", admin == null ? $"Вы были посажены в NonRP-тюрьму на {timeStr}\n\nПричина: {reason}" : $"Администратор {admin.Name} #{admin.GetSharedData<object>("CID", 0)} посадил Вас в NonRP-тюрьму на {timeStr}\n\nПричина: {reason}");
                    }
                    else if (type == Punishment.Types.Warn)
                    {
                        CEF.Notification.Show(Notification.Types.Warn, "Предупреждение", admin == null ? $"Вам было выдано предупреждение!\n\nПричина: {reason}" : $"Администратор {admin.Name} #{admin.GetSharedData<object>("CID", 0)} выдал Вам предупреждение\n\nПричина: {reason}");
                    }
                    else if (type == Punishment.Types.FractionMute)
                    {
                        CEF.Notification.Show(Notification.Types.Mute, "Мут (фракция)", admin == null ? $"Вам был выдан мут (запрет на использование чата фракции) на {timeStr}\n\nПричина: {reason}" : $"Сотрудник {admin.Name} #{admin.GetSharedData<object>("CID", 0)} выдал Вам мут (запрет на использование чата фракции) на {timeStr}\n\nПричина: {reason}");
                    }
                    else if (type == Punishment.Types.OrganisationMute)
                    {
                        CEF.Notification.Show(Notification.Types.Mute, "Мут (организация)", admin == null ? $"Вам был выдан мут (запрет на использование чата организации) на {timeStr}\n\nПричина: {reason}" : $"Сотрудник {admin.Name} #{admin.GetSharedData<object>("CID", 0)} выдал Вам мут (запрет на использование чата организации) на {timeStr}\n\nПричина: {reason}");
                    }
                    else if (type == Punishment.Types.Arrest)
                    {
                        var strData = mData.AdditionalData?.Split('_');

                        if (strData == null)
                            return;

                        timeStr = TimeSpan.FromSeconds(EndDate.GetUnixTimestamp() - long.Parse(strData[0])).GetBeautyString();

                        CEF.Notification.Show(Notification.Types.Mute, "Арест (СИЗО)", admin == null ? $"Вы были арестованы и посажены в СИЗО на {timeStr}\n\nПричина: {reason}" : $"Сотрудник {admin.Name} #{admin.GetSharedData<object>("CID", 0)} посадил Вас в СИЗО на {timeStr}\n\nПричина: {reason}");
                    }
                    else if (type == Punishment.Types.FederalPrison)
                    {
                        var strData = mData.AdditionalData?.Split('_');

                        if (strData == null)
                            return;

                        timeStr = TimeSpan.FromSeconds(EndDate.GetUnixTimestamp() - long.Parse(strData[0])).GetBeautyString();

                        CEF.Notification.Show(Notification.Types.Mute, "Арест (Федеральная тюрьма)", admin == null ? $"Вы были арестованы и посажены в Федеральную тюрьму на {timeStr}\n\nПричина: {reason}" : $"Сотрудник {admin.Name} #{admin.GetSharedData<object>("CID", 0)} посадил Вас в Федеральную тюрьму на {timeStr}\n\nПричина: {reason}");
                    }
                }
                else
                {
                    var mData = Sync.Punishment.All.Where(x => x.Type == type && x.Id == id).FirstOrDefault();

                    if (mData != null)
                    {
                        Sync.Punishment.RemovePunishment(mData);
                    }

                    if (type == Punishment.Types.FractionMute)
                    {
                        CEF.Notification.Show(Notification.Types.Information, "Мут (фракция)", endDateL == -2 ? $"Срок наказания истёк, старайтесь больше не нарушать правила Вашей фракции!" : $"Сотрудник {(admin?.Name ?? "null")} #{(admin?.GetSharedData<object>("CID", 0) ?? 0)} амнистировал Вас!\n\nПричина: {reason}");
                    }
                    else if (type == Punishment.Types.OrganisationMute)
                    {
                        CEF.Notification.Show(Notification.Types.Information, "Мут (организация)", endDateL == -2 ? $"Срок наказания истёк, старайтесь больше не нарушать правила Вашей организации!" : $"Сотрудник {(admin?.Name ?? "null")} #{(admin?.GetSharedData<object>("CID", 0) ?? 0)} амнистировал Вас!\n\nПричина: {reason}");
                    }
                    else
                    {
                        CEF.Notification.Show(Notification.Types.Information, type == Punishment.Types.Mute ? "Мут" : type == Punishment.Types.NRPPrison ? "NonRP-тюрьма" : type == Punishment.Types.Warn ? "Предупреждение" : "???", endDateL == -2 ? $"Срок наказания истёк, старайтесь больше не нарушать правила нашего сервера!" : $"Администратор {(admin?.Name ?? "null")} #{(admin?.GetSharedData<object>("CID", 0) ?? 0)} амнистировал Вас!\n\nПричина: {reason}");
                    }
                }
            });
        }
    }
}
