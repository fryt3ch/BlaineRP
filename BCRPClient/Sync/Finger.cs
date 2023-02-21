using RAGE;
using RAGE.Elements;
using System;

namespace BCRPClient.Sync
{
    public class Finger : Events.Script
    {
        private static DateTime LastSwitchTime;
        private static DateTime LastSentEntityTime;
        private static DateTime LastSentSameEntityTime;

        private static Entity LastSentEntity;

        public static bool Toggled = false;

        private static Utils.Actions[] ActionsToCheck = new Utils.Actions[]
        {
            Utils.Actions.Knocked,
            Utils.Actions.Frozen,
            Utils.Actions.Cuffed,

            Utils.Actions.Crawl,

            Utils.Actions.Animation,
            Utils.Actions.FastAnimation,
            Utils.Actions.Scenario,

            Utils.Actions.InWater,
            Utils.Actions.Shooting, Utils.Actions.Reloading,
            Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.NotOnFoot,
        };

        public Finger()
        {
            LastSwitchTime = DateTime.Now;
            LastSentEntityTime = DateTime.Now;

            LastSentEntity = null;

            Events.Add("Players::FingerPointUpdate", (object[] args) =>
            {
                if (!Settings.Interface.FingerOn)
                    return;

                var entity = Utils.GetEntityPlayerPointsAt(Settings.FINGER_POINT_ENTITY_MAX_DISTANCE);

                if (entity == null)
                    return;

                if (entity != LastSentEntity && !LastSentEntityTime.IsSpam(2500, false, false))
                {
                    if (entity.Type == RAGE.Elements.Type.Vehicle)
                        Events.CallRemote("Players::FingerPoint::Vehicle", (Vehicle)entity);
                    else if (entity.Type == RAGE.Elements.Type.Player)
                        Events.CallRemote("Players::FingerPoint::Player", (Player)entity);
                    else if (entity.Type == RAGE.Elements.Type.Ped)
                        Events.CallRemote("Players::FingerPoint::Ped");

                    LastSentEntity = entity;
                    LastSentEntityTime = DateTime.Now;
                }

                if (entity != LastSentEntity)
                    LastSentEntity = null;
            });
        }

        public static void Start()
        {
            if (Toggled)
                return;

            if (LastSwitchTime.IsSpam(1000, false, false) || Utils.IsAnyCefActive() || !Utils.CanDoSomething(false, ActionsToCheck))
                return;

            LastSentEntity = null;

            PushVehicle.Off();

            Events.CallLocal("fpoint_toggle", true);

            Toggled = true;
            LastSwitchTime = DateTime.Now;
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
