using BCRPClient.CEF;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

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

        private static List<int> TempBinds { get; set; }

        public Offers()
        {
            TempBinds = new List<int>();

            LastSent = DateTime.Now;

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
                            RAGE.Input.Unbind(x);

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

            string text = null;

            if (type == Types.Settle)
            {
                var pType = (int)data;

                text = string.Format(Locale.Notifications.Offers.Types.GetValueOrDefault(type) ?? "null", name, pType == 0 ? Locale.Notifications.Offers.OfferSettleHouse : Locale.Notifications.Offers.OfferSettleApartments);
            }
            else
            {
                text = data == null ? string.Format(Locale.Notifications.Offers.Types.GetValueOrDefault(type), name) : string.Format(Locale.Notifications.Offers.Types.GetValueOrDefault(type) ?? "null", name, data);
            }

            CEF.Notification.ShowOffer(text);

            if (TempBinds.Count > 0)
            {
                foreach (var x in TempBinds)
                    RAGE.Input.Unbind(x);

                TempBinds.Clear();
            }

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Y, true, () =>
            {
                Reply(ReplyTypes.Accept);
            }));

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.N, true, () =>
            {
                Reply(ReplyTypes.Deny);
            }));
        }

        public static void Request(Player player, Types type, object data = null)
        {
            if (CurrentTarget != null)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Offers.PlayerHasOffer);

                return;
            }

            if (player?.Exists != true)
                return;

            if (Vector3.Distance(player.Position, Player.LocalPlayer.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE && (Player.LocalPlayer.Vehicle == null || player.Vehicle != Player.LocalPlayer.Vehicle))
                return;

            if (Utils.IsAnyCefActive() || LastSent.IsSpam(2000, false, false) || !Utils.CanDoSomething(ActionsToCheck))
                return;

            Events.CallRemote("Offers::Send", player, (int)type, RAGE.Util.Json.Serialize(data));

            CurrentTarget = player;

            LastSent = DateTime.Now;
        }

        public static void Reply(ReplyTypes rType = ReplyTypes.AutoCancel)
        {
            if (CurrentTarget == null)
                return;

            if (rType == ReplyTypes.AutoCancel || rType == ReplyTypes.Busy || !LastSent.IsSpam(2000, false, false))
            {
                Events.CallRemote("Offers::Reply", (int)rType);

                LastSent = DateTime.Now;

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
