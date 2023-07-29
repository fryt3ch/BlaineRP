using System;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.UI.CEF;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Punishments
{
    [Script(int.MaxValue)]
    public class Events
    {
        public Events()
        {
            RAGE.Events.Add("Player::MuteShow", (args) => Punishment.All.Where(x => x.Type == PunishmentType.Mute).FirstOrDefault()?.ShowErrorNotification());

            RAGE.Events.Add("Player::FMuteShow", (args) => Punishment.All.Where(x => x.Type == PunishmentType.FractionMute).FirstOrDefault()?.ShowErrorNotification());

            RAGE.Events.Add("Player::Punish",
                (args) =>
                {
                    var id = Utils.Convert.ToUInt32(args[0]);

                    var type = (PunishmentType)(int)args[1];

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

                            if (type == PunishmentType.Arrest)
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

                            if (type == PunishmentType.Mute)
                            {
                                Notification.Show(Notification.Types.Mute,
                                    Locale.Get("PUNISHMENT_L_MUTE"),
                                    admin == null ? Locale.Get("PUNISHMENT_S0_MUTE", timeStr, reason) : Locale.Get("PUNISHMENT_S1_MUTE", getAdminStr(), timeStr, reason)
                                );
                            }
                            else if (type == PunishmentType.NRPPrison)
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
                            else if (type == PunishmentType.Warn)
                            {
                                Notification.Show(Notification.Types.Warn,
                                    Locale.Get("PUNISHMENT_L_WARN"),
                                    admin == null ? Locale.Get("PUNISHMENT_S0_WARN", reason) : Locale.Get("PUNISHMENT_S1_WARN", getAdminStr(), reason)
                                );
                            }
                            else if (type == PunishmentType.FractionMute)
                            {
                                Notification.Show(Notification.Types.Mute,
                                    Locale.Get("PUNISHMENT_L_FMUTE"),
                                    admin == null ? Locale.Get("PUNISHMENT_S0_FMUTE", timeStr, reason) : Locale.Get("PUNISHMENT_S1_FMUTE", getAdminStr(), timeStr, reason)
                                );
                            }
                            else if (type == PunishmentType.OrganisationMute)
                            {
                                Notification.Show(Notification.Types.Mute,
                                    Locale.Get("PUNISHMENT_L_OMUTE"),
                                    admin == null ? Locale.Get("PUNISHMENT_S0_OMUTE", timeStr, reason) : Locale.Get("PUNISHMENT_S1_OMUTE", getAdminStr(), timeStr, reason)
                                );
                            }
                            else if (type == PunishmentType.Arrest)
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
                            else if (type == PunishmentType.FederalPrison)
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

                        if (type == PunishmentType.FractionMute)
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("PUNISHMENT_L_FMUTE"),
                                endDateL == -2 ? Locale.Get("PUNISHMENT_F0_FRAC") : Locale.Get("PUNISHMENT_F1_DEF", admin == null ? "null" : getAdminStr(), reason)
                            );
                        else if (type == PunishmentType.OrganisationMute)
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("PUNISHMENT_L_OMUTE"),
                                endDateL == -2 ? Locale.Get("PUNISHMENT_F0_ORG") : Locale.Get("PUNISHMENT_F1_DEF", admin == null ? "null" : getAdminStr(), reason)
                            );
                        else if (type == PunishmentType.Arrest)
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("PUNISHMENT_L_RPP1"),
                                endDateL == -2 ? Locale.Get("PUNISHMENT_F0_LAW") : Locale.Get("PUNISHMENT_F1_DEF", admin == null ? "null" : getAdminStr(), reason)
                            );
                        else if (type == PunishmentType.FederalPrison)
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("PUNISHMENT_L_RPP2"),
                                endDateL == -2 ? Locale.Get("PUNISHMENT_F0_LAW") : Locale.Get("PUNISHMENT_F1_DEF", admin == null ? "null" : getAdminStr(), reason)
                            );
                        else
                            Notification.Show(Notification.Types.Information,
                                type == PunishmentType.Mute ? Locale.Get("PUNISHMENT_L_MUTE") :
                                type == PunishmentType.NRPPrison ? Locale.Get("PUNISHMENT_L_NRPP") :
                                type == PunishmentType.Warn ? Locale.Get("PUNISHMENT_L_WARN") : "???",
                                endDateL == -2 ? Locale.Get("PUNISHMENT_F0_DEF") : Locale.Get("PUNISHMENT_F1_DEF", admin == null ? "null" : getAdminStr(), reason)
                            );
                    }
                }
            );
        }
    }
}