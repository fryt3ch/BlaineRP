using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;

namespace BlaineRP.Client.Game.Management.Punishments
{
    public class Punishment
    {
        public Punishment()
        {
        }

        public static List<Punishment> All { get; set; } = new List<Punishment>();

        private static AsyncTask CheckTask { get; set; }

        public uint Id { get; set; }

        public PunishmentType Type { get; set; }

        public DateTime EndDate { get; set; }

        public string AdditionalData { get; set; }

        public static bool IsCheckTaskStillNeeded => All.Where(x => x.Type != PunishmentType.Ban).Any();

        public void ShowErrorNotification()
        {
            TimeSpan timeLeft = EndDate.Subtract(World.Core.ServerTime);

            if (Type == PunishmentType.Mute)
            {
                Notification.ShowError($"Вы не можете сделать это сейчас!\nУ Вас есть активный мут, до его снятия осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == PunishmentType.NRPPrison)
            {
                string[] strData = AdditionalData?.Split('_');

                if (strData == null)
                    return;

                timeLeft = TimeSpan.FromSeconds(EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

                Notification.ShowError($"Вы не можете сделать это сейчас!\nВы находитесь в NonRP-тюрьме, до выхода осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == PunishmentType.Warn)
            {
                Notification.ShowError(
                    $"Вы не можете сделать это сейчас!\nУ Вас есть активное предупреждение, до его снятия осталось {timeLeft}\n\nВы можете досрочно снять предупреждение, для этого перейдите в Меню - Магазин"
                );
            }
            else if (Type == PunishmentType.FractionMute)
            {
                Notification.ShowError($"Вы не можете сделать это сейчас!\nУ Вас есть активный мут чата фракции, до его снятия осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == PunishmentType.OrganisationMute)
            {
                Notification.ShowError($"Вы не можете сделать это сейчас!\nУ Вас есть активный мут чата организации, до его снятия осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == PunishmentType.Arrest)
            {
                string[] strData = AdditionalData?.Split('_');

                if (strData == null)
                    return;

                timeLeft = TimeSpan.FromSeconds(EndDate.GetUnixTimestamp() - long.Parse(strData[0]));

                Notification.ShowError($"Вы не можете сделать это сейчас!\nВы арестованы и находитесь в СИЗО, до выхода осталось {timeLeft.GetBeautyString()}");
            }
            else if (Type == PunishmentType.FederalPrison)
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
                        if (x.Type != PunishmentType.NRPPrison && x.Type != PunishmentType.FederalPrison && x.Type != PunishmentType.Arrest)
                        {
                            TimeSpan timeLeft = x.EndDate.Subtract(curTime);

                            double secs = timeLeft.TotalSeconds;

                            if (secs < 0)
                            {
                                RAGE.Events.CallRemote("Player::UnpunishMe", x.Id);
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

                            if (x.Type == PunishmentType.Arrest)
                            {
                                var cs = ExtraColshape.All.Where(x => x.Name == "CopArrestCell").ToList();

                                if (!cs.Where(x => x.IsInside == true).Any())
                                    RAGE.Events.CallRemote("Player::COPAR::TPME");
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

            if (data.Type == PunishmentType.NRPPrison)
            {
                Main.Render -= NonRpJailRender;
                Main.Render += NonRpJailRender;
            }
            else if (data.Type == PunishmentType.Arrest)
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
            else if (data.Type == PunishmentType.FederalPrison)
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

            if (data.Type == PunishmentType.NRPPrison)
            {
                Main.Render -= NonRpJailRender;
            }
            else if (data.Type == PunishmentType.Arrest)
            {
                Main.Render -= ArrestRender;

                ExtraColshape.All.Where(x => x.Name == "CopArrestCell").ToList().ForEach(x => x.Destroy());
            }
            else if (data.Type == PunishmentType.FederalPrison)
            {
                Main.Render -= FederalPrisonRender;
            }
        }

        private static void NonRpJailRender()
        {
            Punishment jailData = All.Where(x => x.Type == PunishmentType.NRPPrison).FirstOrDefault();

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
            Punishment jailData = All.Where(x => x.Type == PunishmentType.Arrest).FirstOrDefault();

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
            Punishment jailData = All.Where(x => x.Type == PunishmentType.FederalPrison).FirstOrDefault();

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
}