using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;

namespace BCRPClient.Sync
{
    public class Offers : Events.Script
    {
        public enum Types
        {
            /// <summary>Рукопожатие</summary>
            Handshake = 0,
            /// <summary>Обмен</summary>
            Exchange,
            /// <summary>Нести игркока</summary>
            Carry,
            /// <summary>Сыграть в орел и решка</summary>
            HeadsOrTails,
            /// <summary>Приглашение во фракцию</summary>
            InviteFraction,
            /// <summary>Приглашение в организацию</summary>
            InviteOrganisation,
            /// <summary>Передать наличные</summary>
            Cash,
            /// <summary>Показать паспорт</summary>
            ShowPassport,
            /// <summary>Показать мед. карту</summary>
            ShowMedicalCard,
            /// <summary>Показать лицензии</summary>
            ShowLicenses,
            /// <summary>Показать тех. паспорт</summary>
            ShowVehiclePassport,
            /// <summary>Показать резюме</summary>
            ShowResume,
            /// <summary>Показать удостоверение</summary>
            ShowFractionDocs,
            /// <summary>Продажа имущества</summary>
            PropertySell,
            /// <summary>Поделиться меткой</summary>
            WaypointShare,
            /// <summary>Подселить в дом/квартиру</summary>
            Settle,
            /// <summary>Продать недвижимость</summary>
            SellEstate,
            /// <summary>Продать транспорт</summary>
            SellVehicle,
            /// <summary>Продать бизнес</summary>
            SellBusiness,
            /// <summary>Штраф полиции</summary>
            PoliceFine,
            /// <summary>Лечение от врача</summary>
            EmsHeal,
        }

        public enum ReplyTypes
        {
            Deny = 0,
            Accept,
            Busy,
            AutoCancel,
        }

        private static DateTime LastSent;

        private static Player CurrentTarget { get; set; }

        public static bool IsActive { get => CurrentTarget != null; }

        private static Utils.Actions[] ActionsToCheck = new Utils.Actions[]
        {
            Utils.Actions.Knocked,
            Utils.Actions.Frozen,
            Utils.Actions.Cuffed,

            //Utils.Actions.Crouch,
            Utils.Actions.Crawl,
            Utils.Actions.Finger,
            Utils.Actions.PushingVehicle,

            Utils.Actions.Animation,
            Utils.Actions.FastAnimation,
            Utils.Actions.Scenario,

            //Utils.Actions.InVehicle,
            Utils.Actions.InWater,
            Utils.Actions.Shooting, Utils.Actions.Reloading, //Utils.Actions.HasWeapon,
            Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, //Utils.Actions.OnFoot,
        };

        public static Dictionary<Sync.Offers.Types, string> TypesStrings => new Dictionary<Sync.Offers.Types, string>()
        {
            { Sync.Offers.Types.Handshake, "OFFER_HANDSHAKE_TEXT" },
            { Sync.Offers.Types.HeadsOrTails, "OFFER_HEADSORTAILS_TEXT" },
            { Sync.Offers.Types.Exchange, "OFFER_EXCHANGE_TEXT" },
            { Sync.Offers.Types.SellEstate, "OFFER_SELLESTATE_TEXT" },
            { Sync.Offers.Types.SellVehicle, "OFFER_SELLVEHICLE_TEXT" },
            { Sync.Offers.Types.SellBusiness, "OFFER_SELLBUSINESS_TEXT" },
            { Sync.Offers.Types.Settle, "OFFER_SETTLE_TEXT" },
            { Sync.Offers.Types.Carry, "OFFER_CARRY_TEXT" },
            { Sync.Offers.Types.Cash, "OFFER_CASH_TEXT" },
            { Sync.Offers.Types.WaypointShare, "OFFER_WAYPOINTSHARE_TEXT" },
            { Sync.Offers.Types.ShowPassport, "OFFER_SHOWPASSPORT_TEXT" },
            { Sync.Offers.Types.ShowMedicalCard, "OFFER_SHOWMEDICALCARD_TEXT" },
            { Sync.Offers.Types.ShowVehiclePassport, "OFFER_SHOWVEHICLEPASSPORT_TEXT" },
            { Sync.Offers.Types.ShowLicenses, "OFFER_SHOWLICENSES_TEXT" },
            { Sync.Offers.Types.ShowResume, "OFFER_SHOWRESUME_TEXT" },
            { Sync.Offers.Types.InviteFraction, "OFFER_INVITEFRACTION_TEXT" },
            { Sync.Offers.Types.InviteOrganisation, "OFFER_INVITEORGANISATION_TEXT" },
            { Sync.Offers.Types.ShowFractionDocs, "OFFER_SHOWFRACTIONDOCS_TEXT" },
            { Sync.Offers.Types.PoliceFine, "OFFER_POLICEFINE_TEXT" },
        };

        private static List<int> TempBinds { get; set; }

        public Offers()
        {
            TempBinds = new List<int>();

            LastSent = Sync.World.ServerTime;

            Events.Add("Offer::Show", (object[] args) =>
            {
                Player player = (Player)args[0];
                Types type = (Types)(int)args[1];
                object data = args.Length < 3 ? null : args[2];

                if (player?.Exists != true)
                    return;

                if (Utils.IsAnyCefActive(false))
                {
                    CurrentTarget = player;

                    Reply(ReplyTypes.Busy);

                    return;
                }

                Show(player, type, data);
            });

            Events.Add("Offer::Reply::Server", (object[] args) =>
            {
                bool reply = (bool)args[0];
                bool justCancelCts = (bool)args[1];
                bool ctsIsNull = (bool)args[2];

                if (!reply)
                {
                    if (!ctsIsNull)
                    {
                        CEF.Notification.ClearAll();

                        foreach (var x in TempBinds)
                            KeyBinds.Unbind(x);

                        TempBinds.Clear();
                    }

                    if (justCancelCts)
                        return;

                    CurrentTarget = null;

                    GameEvents.Update -= OfferTick;
                }
                else
                {
                    GameEvents.Update -= OfferTick;
                    GameEvents.Update += OfferTick;
                }
            });
        }

        public static void Show(Player player, Types type, object data)
        {
            CurrentTarget = player;

            GameEvents.Update -= OfferTick;
            GameEvents.Update += OfferTick;

            var name = player.GetName(true, false, true);

            var text = Locale.Get(TypesStrings.GetValueOrDefault(type) ?? "null");

            if (type == Types.Settle)
            {
                var pType = (int)data;

                text = string.Format(text, name, pType == 0 ? Locale.Notifications.Offers.OfferSettleHouse : Locale.Notifications.Offers.OfferSettleApartments);
            }
            else if (type == Types.PoliceFine)
            {
                var d = ((string)data).Split('_');

                text = string.Format(text, name, Utils.GetPriceString(decimal.Parse(d[0])), d[1]);
            }
            else if (type == Types.InviteFraction)
            {
                var fData = Data.Fractions.Fraction.Get((Data.Fractions.Types)Utils.ToInt32(data));

                if (fData == null)
                    return;

                text = string.Format(text, name, fData.Name);
            }
            else
            {
                text = data == null ? string.Format(text, name) : string.Format(text, name, data);
            }

            CEF.Notification.ShowOffer(text);

            if (TempBinds.Count > 0)
            {
                foreach (var x in TempBinds)
                    KeyBinds.Unbind(x);

                TempBinds.Clear();
            }

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Y, true, () =>
            {
                Reply(ReplyTypes.Accept);
            }));

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.N, true, () =>
            {
                Reply(ReplyTypes.Deny);
            }));
        }

        public static async void Request(Player player, Types type, object data = null)
        {
            if (CurrentTarget != null)
            {
                CEF.Notification.ShowError(Locale.Notifications.Offers.PlayerHasOffer);

                return;
            }

            if (player?.Exists != true)
                return;

            if (Vector3.Distance(player.Position, Player.LocalPlayer.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE && (Player.LocalPlayer.Vehicle == null || player.Vehicle != Player.LocalPlayer.Vehicle))
                return;

            if (Utils.IsAnyCefActive() || LastSent.IsSpam(1000, false, true) || !Utils.CanDoSomething(true, ActionsToCheck))
                return;

            LastSent = Sync.World.ServerTime;

            var res = await Events.CallRemoteProc("Offers::Send", player, (int)type, RAGE.Util.Json.Serialize(data ?? string.Empty));

            if (res == null)
                return;

            if (res is int resI)
            {
                if (resI == 0)
                {
                    CEF.Notification.ShowErrorDefault();
                }
                else if (resI == 255)
                {
                    CurrentTarget = player;

                    CEF.Notification.Show("Offer::Sent");
                }
            }
        }

        public static void Reply(ReplyTypes rType = ReplyTypes.AutoCancel)
        {
            if (CurrentTarget == null)
                return;

            if (rType == ReplyTypes.AutoCancel || rType == ReplyTypes.Busy || !LastSent.IsSpam(2000, false, false))
            {
                Events.CallRemote("Offers::Reply", (int)rType);

                LastSent = Sync.World.ServerTime;

                if (rType != ReplyTypes.Accept)
                    GameEvents.Update -= OfferTick;
            }
        }

        public static void OfferTick()
        {
            if (CurrentTarget?.Exists != true || Vector3.Distance(CurrentTarget.Position, Player.LocalPlayer.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
            {
                Reply(ReplyTypes.AutoCancel);

                return;
            }
        }
    }
}
