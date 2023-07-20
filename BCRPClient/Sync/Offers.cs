using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;

namespace BCRPClient.Sync
{
    [Script(int.MaxValue)]
    public class Offers 
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
            /// <summary>Лечение (психики) от врача</summary>
            EmsPsychHeal,
            /// <summary>Лечение (наркозавимиости) от врача</summary>
            EmsDrugHeal,
            /// <summary>Проверка здоровья от врача</summary>
            EmsDiagnostics,
            /// <summary>Выдача мед. карты от врача</summary>
            EmsMedicalCard,
            /// <summary>Продажа мед. маски от врача</summary>
            EmsSellMask,
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

        private static List<int> _tempBinds;

        public Offers()
        {
            Events.Add("Offer::Show", (args) =>
            {
                var player = Utils.GetPlayerByRemoteId(Utils.ToUInt16(args[0]));

                if (player == null)
                    return;

                var type = (Types)(int)args[1];
                var text = (string)args[2];

                if (Utils.IsAnyCefActive(false))
                {
                    CurrentTarget = player;

                    Reply(ReplyTypes.Busy);

                    return;
                }

                Show(player, type, text);
            });

            Events.Add("Offer::Reply::Server", (args) =>
            {
                var reply = (bool)args[0];
                var justCancelCts = (bool)args[1];
                var ctsIsNull = (bool)args[2];

                if (!reply)
                {
                    if (!ctsIsNull)
                    {
                        CEF.Notification.ClearAll();

                        if (_tempBinds != null)
                        {
                            _tempBinds.ForEach(x => KeyBinds.Unbind(x));

                            _tempBinds.Clear();
                            _tempBinds = null;
                        }
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

        public static void Show(Player player, Types type, string text)
        {
            if (CurrentTarget != null)
                return;

            CurrentTarget = player;

            GameEvents.Update -= OfferTick;
            GameEvents.Update += OfferTick;

            var name = player.GetName(true, false, true);

            text = string.Format(text, name);

            CEF.Notification.ShowOffer(text);

            _tempBinds = new List<int>()
            {
                KeyBinds.Bind(RAGE.Ui.VirtualKeys.Y, true, () =>
                {
                    Reply(ReplyTypes.Accept);
                }),

                KeyBinds.Bind(RAGE.Ui.VirtualKeys.N, true, () =>
                {
                    Reply(ReplyTypes.Deny);
                }),
            };
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

            if (Vector3.Distance(player.Position, Player.LocalPlayer.Position) > Settings.App.Static.EntityInteractionMaxDistance && (Player.LocalPlayer.Vehicle == null || player.Vehicle != Player.LocalPlayer.Vehicle))
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

            var isNotManual = rType == ReplyTypes.AutoCancel || rType == ReplyTypes.Busy;

            if (isNotManual || !LastSent.IsSpam(1_000, false, false))
            {
                Events.CallRemote("Offers::Reply", (int)rType);

                if (!isNotManual)
                    LastSent = Sync.World.ServerTime;

                if (rType != ReplyTypes.Accept)
                    GameEvents.Update -= OfferTick;
            }
        }

        public static void OfferTick()
        {
            if (CurrentTarget?.Exists != true || Vector3.Distance(CurrentTarget.Position, Player.LocalPlayer.Position) > Settings.App.Static.EntityInteractionMaxDistance)
            {
                Reply(ReplyTypes.AutoCancel);

                return;
            }
        }
    }
}
