using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.World;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Offers
{
    [Script(int.MaxValue)]
    public class Offers
    {
        public enum ReplyTypes
        {
            Deny = 0,
            Accept,
            Busy,
            AutoCancel,
        }

        private static DateTime LastSent;

        private static PlayerActions.Types[] ActionsToCheck = new PlayerActions.Types[]
        {
            PlayerActions.Types.Knocked,
            PlayerActions.Types.Frozen,
            PlayerActions.Types.Cuffed,

            //PlayerActions.Types.Crouch,
            PlayerActions.Types.Crawl,
            PlayerActions.Types.Finger,
            PlayerActions.Types.PushingVehicle,
            PlayerActions.Types.Animation,
            PlayerActions.Types.FastAnimation,
            PlayerActions.Types.Scenario,

            //PlayerActions.Types.InVehicle,
            PlayerActions.Types.InWater,
            PlayerActions.Types.Shooting,
            PlayerActions.Types.Reloading, //PlayerActions.Types.HasWeapon,
            PlayerActions.Types.Climbing,
            PlayerActions.Types.Falling,
            PlayerActions.Types.Ragdoll,
            PlayerActions.Types.Jumping, //PlayerActions.Types.OnFoot,
        };

        private static List<int> _tempBinds;

        public Offers()
        {
            Events.Add("Offer::Show",
                (args) =>
                {
                    Player player = Entities.Players.GetAtRemote(Utils.Convert.ToUInt16(args[0]));

                    if (player == null)
                        return;

                    var type = (OfferTypes)(int)args[1];
                    var text = (string)args[2];

                    if (Utils.Misc.IsAnyCefActive(false))
                    {
                        CurrentTarget = player;

                        Reply(ReplyTypes.Busy);

                        return;
                    }

                    Show(player, type, text);
                }
            );

            Events.Add("Offer::Reply::Server",
                (args) =>
                {
                    var reply = (bool)args[0];
                    var justCancelCts = (bool)args[1];
                    var ctsIsNull = (bool)args[2];

                    if (!reply)
                    {
                        if (!ctsIsNull)
                        {
                            Notification.ClearAll();

                            if (_tempBinds != null)
                            {
                                _tempBinds.ForEach(x => Input.Core.Unbind(x));

                                _tempBinds.Clear();
                                _tempBinds = null;
                            }
                        }

                        if (justCancelCts)
                            return;

                        CurrentTarget = null;

                        Main.Update -= OfferTick;
                    }
                    else
                    {
                        Main.Update -= OfferTick;
                        Main.Update += OfferTick;
                    }
                }
            );
        }

        private static Player CurrentTarget { get; set; }

        public static bool IsActive => CurrentTarget != null;

        public static void Show(Player player, OfferTypes type, string text)
        {
            if (CurrentTarget != null)
                return;

            CurrentTarget = player;

            Main.Update -= OfferTick;
            Main.Update += OfferTick;

            string name = player.GetName(true, false, true);

            text = string.Format(text, name);

            Notification.ShowOffer(text);

            _tempBinds = new List<int>()
            {
                Input.Core.Bind(RAGE.Ui.VirtualKeys.Y,
                    true,
                    () =>
                    {
                        Reply(ReplyTypes.Accept);
                    }
                ),
                Input.Core.Bind(RAGE.Ui.VirtualKeys.N,
                    true,
                    () =>
                    {
                        Reply(ReplyTypes.Deny);
                    }
                ),
            };
        }

        public static async void Request(Player player, OfferTypes type, object data = null)
        {
            if (CurrentTarget != null)
            {
                Notification.ShowError(Locale.Notifications.Offers.PlayerHasOffer);

                return;
            }

            if (player?.Exists != true)
                return;

            if (Vector3.Distance(player.Position, Player.LocalPlayer.Position) > Settings.App.Static.EntityInteractionMaxDistance &&
                (Player.LocalPlayer.Vehicle == null || player.Vehicle != Player.LocalPlayer.Vehicle))
                return;

            if (Utils.Misc.IsAnyCefActive() || LastSent.IsSpam(1000, false, true) || PlayerActions.IsAnyActionActive(true, ActionsToCheck))
                return;

            LastSent = Core.ServerTime;

            object res = await Events.CallRemoteProc("Offers::Send", player, (int)type, RAGE.Util.Json.Serialize(data ?? string.Empty));

            if (res == null)
                return;

            if (res is int resI)
            {
                if (resI == 0)
                {
                    Notification.ShowErrorDefault();
                }
                else if (resI == 255)
                {
                    CurrentTarget = player;

                    Notification.Show("Offer::Sent");
                }
            }
        }

        public static void Reply(ReplyTypes rType = ReplyTypes.AutoCancel)
        {
            if (CurrentTarget == null)
                return;

            bool isNotManual = rType == ReplyTypes.AutoCancel || rType == ReplyTypes.Busy;

            if (isNotManual || !LastSent.IsSpam(1_000, false, false))
            {
                Events.CallRemote("Offers::Reply", (int)rType);

                if (!isNotManual)
                    LastSent = Core.ServerTime;

                if (rType != ReplyTypes.Accept)
                    Main.Update -= OfferTick;
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