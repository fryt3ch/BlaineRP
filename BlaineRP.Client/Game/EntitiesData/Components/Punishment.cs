using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.EntitiesData.Components
{
    public class Punishment
    {
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

        public Punishment()
        {
        }

        public static List<Punishment> All { get; set; } = new List<Punishment>();

        private static AsyncTask CheckTask { get; set; }

        public uint Id { get; set; }

        public Types Type { get; set; }

        public DateTime EndDate { get; set; }

        public string AdditionalData { get; set; }

        public static bool IsCheckTaskStillNeeded => All.Where(x => x.Type != Types.Ban).Any();

        public void ShowErrorNotification()
        {
            TimeSpan timeLeft = EndDate.Subtract(World.Core.ServerTime);

            if (Type == Types.Mute)
            {
                Notification.ShowError($"Вы не можете сделать это сейчас!\nУ Вас есть активный мут, до его снятия осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == Types.NRPPrison)
            {
                string[] strData = AdditionalData?.Split('_');

                if (strData == null)
                    return;

                timeLeft = TimeSpan.FromSeconds(EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

                Notification.ShowError($"Вы не можете сделать это сейчас!\nВы находитесь в NonRP-тюрьме, до выхода осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == Types.Warn)
            {
                Notification.ShowError(
                    $"Вы не можете сделать это сейчас!\nУ Вас есть активное предупреждение, до его снятия осталось {timeLeft}\n\nВы можете досрочно снять предупреждение, для этого перейдите в Меню - Магазин"
                );
            }
            else if (Type == Types.FractionMute)
            {
                Notification.ShowError($"Вы не можете сделать это сейчас!\nУ Вас есть активный мут чата фракции, до его снятия осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == Types.OrganisationMute)
            {
                Notification.ShowError($"Вы не можете сделать это сейчас!\nУ Вас есть активный мут чата организации, до его снятия осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == Types.Arrest)
            {
                string[] strData = AdditionalData?.Split('_');

                if (strData == null)
                    return;

                timeLeft = TimeSpan.FromSeconds(EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

                Notification.ShowError($"Вы не можете сделать это сейчас!\nВы арестованы и находитесь в СИЗО, до выхода осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == Types.FederalPrison)
            {
                string[] strData = AdditionalData?.Split('_');

                if (strData == null)
                    return;

                timeLeft = TimeSpan.FromSeconds(EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

                Notification.ShowError($"Вы не можете сделать это сейчас!\nВы арестованы и находитесь в Федеральной тюрьме, до выхода осталось {timeLeft.GetBeautyString()}");
            }
        }

        public static void StartCheckTask()
        {
            CheckTask?.Cancel();

            CheckTask = new AsyncTask(() =>
                {
                    DateTime curTime = World.Core.ServerTime;

                    foreach (Punishment x in All)
                    {
                        if (x.Type != Types.NRPPrison && x.Type != Types.FederalPrison && x.Type != Types.Arrest)
                        {
                            TimeSpan timeLeft = x.EndDate.Subtract(curTime);

                            double secs = timeLeft.TotalSeconds;

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
                            string[] dataS = x.AdditionalData?.Split('_');

                            if (dataS != null)
                            {
                                long time = long.Parse(dataS[0]) + 1;

                                dataS[0] = time.ToString();

                                x.AdditionalData = string.Join('_', dataS);
                            }

                            if (x.Type == Types.Arrest)
                            {
                                var cs = ExtraColshape.All.Where(x => x.Name == "CopArrestCell").ToList();

                                if (!cs.Where(x => x.IsInside == true).Any())
                                    Events.CallRemote("Player::COPAR::TPME");
                            }
                        }
                    }
                },
                1000,
                true,
                0
            );

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
                Main.Render -= NonRpJailRender;
                Main.Render += NonRpJailRender;
            }
            else if (data.Type == Types.Arrest)
            {
                Main.Render -= ArrestRender;
                Main.Render += ArrestRender;

                string[] dataS = data.AdditionalData.Split('_');

                var fData = Fraction.Get((FractionTypes)int.Parse(dataS[1])) as Police;

                if (fData != null)
                    foreach (Vector3 x in fData.ArrestCellsPositions)
                    {
                        ExtraColshape cs = null;

                        cs = new Sphere(new Vector3(x.X, x.Y, x.Z), 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                        {
                            Name = "CopArrestCell",
                        };
                    }
            }
            else if (data.Type == Types.FederalPrison)
            {
                Main.Render -= FederalPrisonRender;
                Main.Render += FederalPrisonRender;
            }
        }

        public static void RemovePunishment(Punishment data)
        {
            All.Remove(data);

            if (!IsCheckTaskStillNeeded)
                StopCheckTask();

            if (data.Type == Types.NRPPrison)
            {
                Main.Render -= NonRpJailRender;
            }
            else if (data.Type == Types.Arrest)
            {
                Main.Render -= ArrestRender;

                ExtraColshape.All.Where(x => x.Name == "CopArrestCell").ToList().ForEach(x => x.Destroy());
            }
            else if (data.Type == Types.FederalPrison)
            {
                Main.Render -= FederalPrisonRender;
            }
        }

        private static void NonRpJailRender()
        {
            Punishment jailData = All.Where(x => x.Type == Types.NRPPrison).FirstOrDefault();

            if (jailData == null)
                return;

            string[] strData = jailData.AdditionalData?.Split('_');

            if (strData == null)
                return;

            var timeLeft = TimeSpan.FromSeconds(jailData.EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

            Graphics.DrawText(Locale.Get("PUNISHMENT_L_R0_NRPP"), 0.5f, 0.025f, 255, 0, 0, 255, 0.5f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

            Graphics.DrawText(Locale.Get("PUNISHMENT_L_R1_DEF", timeLeft.GetBeautyString()),
                0.5f,
                0.055f,
                255,
                255,
                255,
                255,
                0.5f,
                RAGE.Game.Font.ChaletComprimeCologne,
                true,
                true
            );
        }

        private static void ArrestRender()
        {
            Punishment jailData = All.Where(x => x.Type == Types.Arrest).FirstOrDefault();

            if (jailData == null)
                return;

            string[] strData = jailData.AdditionalData?.Split('_');

            if (strData == null)
                return;

            var timeLeft = TimeSpan.FromSeconds(jailData.EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

            Graphics.DrawText(Locale.Get("PUNISHMENT_L_R0_RPP1"), 0.5f, 0.025f, 255, 0, 0, 255, 0.5f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

            Graphics.DrawText(Locale.Get("PUNISHMENT_L_R1_DEF", timeLeft.GetBeautyString()),
                0.5f,
                0.055f,
                255,
                255,
                255,
                255,
                0.5f,
                RAGE.Game.Font.ChaletComprimeCologne,
                true,
                true
            );
        }

        private static void FederalPrisonRender()
        {
            Punishment jailData = All.Where(x => x.Type == Types.FederalPrison).FirstOrDefault();

            if (jailData == null)
                return;

            string[] strData = jailData.AdditionalData?.Split('_');

            if (strData == null)
                return;

            var timeLeft = TimeSpan.FromSeconds(jailData.EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

            Graphics.DrawText(Locale.Get("PUNISHMENT_L_R0_RPP2"), 0.5f, 0.025f, 255, 0, 0, 255, 0.5f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

            Graphics.DrawText(Locale.Get("PUNISHMENT_L_R1_DEF", timeLeft.GetBeautyString()),
                0.5f,
                0.055f,
                255,
                255,
                255,
                255,
                0.5f,
                RAGE.Game.Font.ChaletComprimeCologne,
                true,
                true
            );
        }
    }

    [Script(int.MaxValue)]
    public class PunishmentEvents
    {
        public PunishmentEvents()
        {
            Events.Add("Player::MuteShow", (args) => Punishment.All.Where(x => x.Type == Punishment.Types.Mute).FirstOrDefault()?.ShowErrorNotification());

            Events.Add("Player::FMuteShow", (args) => Punishment.All.Where(x => x.Type == Punishment.Types.FractionMute).FirstOrDefault()?.ShowErrorNotification());

            Events.Add("Player::Punish",
                (args) =>
                {
                    var id = Utils.Convert.ToUInt32(args[0]);

                    var type = (Punishment.Types)(int)args[1];

                    Player admin = Entities.Players.GetAtRemote(Utils.Convert.ToUInt16(args[2]));

                    var endDateL = Utils.Convert.ToInt64(args[3]);

                    var reason = (string)args[4];

                    string addData = args.Length > 5 ? (string)args[5] : null;

                    string getAdminStr() => $"{admin.Name} #{admin.GetSharedData<object>("CID", 0)}";

                    if (endDateL >= 0)
                    {
                        DateTime endDate = DateTimeOffset.FromUnixTimeSeconds(endDateL).DateTime;

                        Punishment mData = Punishment.All.Where(x => x.Id == id).FirstOrDefault();

                        if (mData != null)
                        {
                            TimeSpan endDateDiff = endDate.Subtract(mData.EndDate);

                            if (type == Punishment.Types.Arrest)
                            {
                                if (endDateDiff >= TimeSpan.Zero)
                                {
                                    string timeStr = endDateDiff.GetBeautyString();

                                    Notification.Show(Notification.Types.Jail2,
                                        Locale.Get("PUNISHMENT_L_RPP1"),
                                        admin == null ? Locale.Get("PUNISHMENT_U0_RPP1") : Locale.Get("PUNISHMENT_U1_RPP1", getAdminStr(), timeStr, reason)
                                    );
                                }
                                else
                                {
                                    string timeStr = endDateDiff.Negate().GetBeautyString();

                                    Notification.Show(Notification.Types.Jail2,
                                        Locale.Get("PUNISHMENT_L_RPP1"),
                                        admin == null ? Locale.Get("PUNISHMENT_D0_RPP1") : Locale.Get("PUNISHMENT_D1_RPP1", getAdminStr(), timeStr, reason)
                                    );
                                }
                            }

                            mData.EndDate = endDate;

                            return;
                        }
                        else
                        {
                            mData = new Punishment()
                            {
                                Type = type,
                                EndDate = endDate,
                                Id = id,
                                AdditionalData = addData,
                            };

                            Punishment.AddPunishment(mData);

                            string timeStr = endDate.Subtract(World.Core.ServerTime).GetBeautyString();

                            if (type == Punishment.Types.Mute)
                            {
                                Notification.Show(Notification.Types.Mute,
                                    Locale.Get("PUNISHMENT_L_MUTE"),
                                    admin == null ? Locale.Get("PUNISHMENT_S0_MUTE", timeStr, reason) : Locale.Get("PUNISHMENT_S1_MUTE", getAdminStr(), timeStr, reason)
                                );
                            }
                            else if (type == Punishment.Types.NRPPrison)
                            {
                                string[] strData = mData.AdditionalData?.Split('_');

                                if (strData == null)
                                    return;

                                timeStr = TimeSpan.FromSeconds(endDate.GetUnixTimestamp() - long.Parse(strData[0])).GetBeautyString();

                                Notification.Show(Notification.Types.Jail1,
                                    Locale.Get("PUNISHMENT_L_NRPP"),
                                    admin == null ? Locale.Get("PUNISHMENT_S0_NRPP", timeStr, reason) : Locale.Get("PUNISHMENT_S1_NRPP", getAdminStr(), timeStr, reason)
                                );
                            }
                            else if (type == Punishment.Types.Warn)
                            {
                                Notification.Show(Notification.Types.Warn,
                                    Locale.Get("PUNISHMENT_L_WARN"),
                                    admin == null ? Locale.Get("PUNISHMENT_S0_WARN", reason) : Locale.Get("PUNISHMENT_S1_WARN", getAdminStr(), reason)
                                );
                            }
                            else if (type == Punishment.Types.FractionMute)
                            {
                                Notification.Show(Notification.Types.Mute,
                                    Locale.Get("PUNISHMENT_L_FMUTE"),
                                    admin == null ? Locale.Get("PUNISHMENT_S0_FMUTE", timeStr, reason) : Locale.Get("PUNISHMENT_S1_FMUTE", getAdminStr(), timeStr, reason)
                                );
                            }
                            else if (type == Punishment.Types.OrganisationMute)
                            {
                                Notification.Show(Notification.Types.Mute,
                                    Locale.Get("PUNISHMENT_L_OMUTE"),
                                    admin == null ? Locale.Get("PUNISHMENT_S0_OMUTE", timeStr, reason) : Locale.Get("PUNISHMENT_S1_OMUTE", getAdminStr(), timeStr, reason)
                                );
                            }
                            else if (type == Punishment.Types.Arrest)
                            {
                                string[] strData = mData.AdditionalData?.Split('_');

                                if (strData == null)
                                    return;

                                timeStr = TimeSpan.FromSeconds(endDate.GetUnixTimestamp() - long.Parse(strData[0])).GetBeautyString();

                                Notification.Show(Notification.Types.Jail2,
                                    Locale.Get("PUNISHMENT_L_RPP1"),
                                    admin == null ? Locale.Get("PUNISHMENT_S0_RPP1", timeStr, reason) : Locale.Get("PUNISHMENT_S1_RPP1", getAdminStr(), timeStr, reason)
                                );
                            }
                            else if (type == Punishment.Types.FederalPrison)
                            {
                                string[] strData = mData.AdditionalData?.Split('_');

                                if (strData == null)
                                    return;

                                timeStr = TimeSpan.FromSeconds(endDate.GetUnixTimestamp() - long.Parse(strData[0])).GetBeautyString();

                                Notification.Show(Notification.Types.Jail2,
                                    Locale.Get("PUNISHMENT_L_RPP2"),
                                    admin == null ? Locale.Get("PUNISHMENT_S0_RPP2", timeStr, reason) : Locale.Get("PUNISHMENT_S1_RPP2", getAdminStr(), timeStr, reason)
                                );
                            }
                        }
                    }
                    else
                    {
                        Punishment mData = Punishment.All.Where(x => x.Type == type && x.Id == id).FirstOrDefault();

                        if (mData != null)
                            Punishment.RemovePunishment(mData);

                        if (type == Punishment.Types.FractionMute)
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("PUNISHMENT_L_FMUTE"),
                                endDateL == -2 ? Locale.Get("PUNISHMENT_F0_FRAC") : Locale.Get("PUNISHMENT_F1_DEF", admin == null ? "null" : getAdminStr(), reason)
                            );
                        else if (type == Punishment.Types.OrganisationMute)
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("PUNISHMENT_L_OMUTE"),
                                endDateL == -2 ? Locale.Get("PUNISHMENT_F0_ORG") : Locale.Get("PUNISHMENT_F1_DEF", admin == null ? "null" : getAdminStr(), reason)
                            );
                        else if (type == Punishment.Types.Arrest)
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("PUNISHMENT_L_RPP1"),
                                endDateL == -2 ? Locale.Get("PUNISHMENT_F0_LAW") : Locale.Get("PUNISHMENT_F1_DEF", admin == null ? "null" : getAdminStr(), reason)
                            );
                        else if (type == Punishment.Types.FederalPrison)
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("PUNISHMENT_L_RPP2"),
                                endDateL == -2 ? Locale.Get("PUNISHMENT_F0_LAW") : Locale.Get("PUNISHMENT_F1_DEF", admin == null ? "null" : getAdminStr(), reason)
                            );
                        else
                            Notification.Show(Notification.Types.Information,
                                type == Punishment.Types.Mute ? Locale.Get("PUNISHMENT_L_MUTE") :
                                type == Punishment.Types.NRPPrison ? Locale.Get("PUNISHMENT_L_NRPP") :
                                type == Punishment.Types.Warn ? Locale.Get("PUNISHMENT_L_WARN") : "???",
                                endDateL == -2 ? Locale.Get("PUNISHMENT_F0_DEF") : Locale.Get("PUNISHMENT_F1_DEF", admin == null ? "null" : getAdminStr(), reason)
                            );
                    }
                }
            );
        }
    }
}