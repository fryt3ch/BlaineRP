using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using System;
using BlaineRP.Client.Game.Misc;
using BlaineRP.Client.Game.World;

namespace BlaineRP.Client.Sync
{
    [Script(int.MaxValue)]
    public class Finger
    {
        private static DateTime LastSwitchTime;
        private static DateTime LastSentEntityTime;
        private static DateTime LastSentSameEntityTime;

        private static Entity _lastSentEntity;

        public static bool Toggled = false;

        private static PlayerActions.Types[] ActionsToCheck = new PlayerActions.Types[]
        {
            PlayerActions.Types.Knocked,
            PlayerActions.Types.Frozen,
            PlayerActions.Types.Cuffed,

            PlayerActions.Types.Crawl,

            PlayerActions.Types.Animation,
            PlayerActions.Types.FastAnimation,
            PlayerActions.Types.Scenario,

            PlayerActions.Types.InWater,
            PlayerActions.Types.Shooting, PlayerActions.Types.Reloading,
            PlayerActions.Types.Climbing, PlayerActions.Types.Falling, PlayerActions.Types.Ragdoll, PlayerActions.Types.Jumping, PlayerActions.Types.NotOnFoot,
        };

        public Finger()
        {
            Events.Add("Players::FingerPointUpdate", (object[] args) =>
            {
                if (!Settings.User.Interface.FingerOn)
                    return;

                var entity = Raycast.GetEntityPedPointsAt(Player.LocalPlayer, Settings.App.Static.FINGER_POINT_ENTITY_MAX_DISTANCE);

                if (entity == null)
                    return;

                if (entity != _lastSentEntity && !LastSentEntityTime.IsSpam(2500, false, false))
                {
                    if (entity.Type == RAGE.Elements.Type.Vehicle)
                        Events.CallRemote("Players::FingerPoint::Vehicle", (Vehicle)entity);
                    else if (entity.Type == RAGE.Elements.Type.Player)
                        Events.CallRemote("Players::FingerPoint::Player", (Player)entity);
                    else if (entity.Type == RAGE.Elements.Type.Ped)
                        Events.CallRemote("Players::FingerPoint::Ped");

                    _lastSentEntity = entity;
                    LastSentEntityTime = Core.ServerTime;
                }

                if (entity != _lastSentEntity)
                    _lastSentEntity = null;
            });
        }

        public static void Start()
        {
            if (Toggled)
                return;

            if (LastSwitchTime.IsSpam(1000, false, false) || Utils.Misc.IsAnyCefActive() || PlayerActions.IsAnyActionActive(false, ActionsToCheck))
                return;

            _lastSentEntity = null;

            PushVehicle.Off();

            Events.CallLocal("fpoint_toggle", true);

            Toggled = true;
            LastSwitchTime = Core.ServerTime;
        }

        public static void Stop()
        {
            if (!Toggled)
                return;

            Events.CallLocal("fpoint_toggle", false);

            Toggled = false;
        }
    }
}
