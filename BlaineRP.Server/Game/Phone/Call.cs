using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Phone
{
    public partial class Call
    {
        private static List<Call> AllCalls { get; set; } = new List<Call>();

        public static Call GetByPlayer(PlayerData pData) => AllCalls.Where(x => x.Caller == pData || x.Receiver == pData).FirstOrDefault();

        public static Call GetByCaller(PlayerData pData) => AllCalls.Where(x => x.Caller == pData).FirstOrDefault();

        public static Call GetByReceiver(PlayerData pData) => AllCalls.Where(x => x.Receiver == pData).FirstOrDefault();

        public PlayerData Caller { get; private set; }

        public PlayerData Receiver { get; private set; }

        public StatusTypes StatusType { get; private set; }

        public DateTime LastStatusChangeTime { get; private set; }

        public bool Exists => AllCalls.Contains(this);

        private Timer Timer { get; set; }

        public Call(PlayerData Caller, PlayerData Receiver)
        {
            this.LastStatusChangeTime = Utils.GetCurrentTime();

            this.Caller = Caller;
            this.Receiver = Receiver;

            this.StatusType = StatusTypes.Outgoing;

            AllCalls.Add(this);

            Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    if (!Exists || StatusType != StatusTypes.Outgoing)
                        return;

                    Cancel(CancelTypes.ServerAuto);
                });
            }, null, Properties.Settings.Static.PHONE_MAX_CALL_OUT_TIME, Timeout.Infinite);

            Receiver.Player.TriggerEvent("Phone::ACS", false, Caller.Info.PhoneNumber);
        }

        public void SetAsProcess()
        {
            if (Timer != null)
            {
                Timer.Dispose();
            }

            var maxCallTime = (int)((Caller.Info.PhoneBalance / Properties.Settings.Static.PHONE_CALL_COST_X) * Properties.Settings.Static.PHONE_CALL_X);

            Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    if (!Exists)
                        return;

                    Cancel(CancelTypes.NotEnoughBalance);
                });
            }, null, maxCallTime, Timeout.Infinite);

            StatusType = StatusTypes.Process;

            Caller.Player.TriggerEvent("Phone::ACS", true, Receiver.Player.Id);
            Receiver.Player.TriggerEvent("Phone::ACS", true, Caller.Player.Id);
        }

        public void Cancel(CancelTypes cancelType)
        {
            if (Timer == null)
                return;

            Timer.Dispose();

            Timer = null;

            AllCalls.Remove(this);

            Caller.Player.TriggerEvent("Phone::ACS", (byte)cancelType);
            Receiver.Player.TriggerEvent("Phone::ACS", (byte)cancelType);
        }
    }
}
