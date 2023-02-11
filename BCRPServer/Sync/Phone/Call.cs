using GTANetworkAPI;
using GTANetworkMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BCRPServer.Sync.Phone
{
    public class Call
    {
        private static List<Call> AllCalls { get; set; } = new List<Call>();

        public static Call GetByPlayer(PlayerData pData) => AllCalls.Where(x => x.Caller == pData || x.Receiver == pData).FirstOrDefault();

        public static Call GetByCaller(PlayerData pData) => AllCalls.Where(x => x.Caller == pData).FirstOrDefault();

        public static Call GetByReceiver(PlayerData pData) => AllCalls.Where(x => x.Receiver == pData).FirstOrDefault();

        public enum StatusTypes : byte
        {
            /// <summary>Вызов начат, но еще не принят вторым игроком</summary>
            Outgoing = 0,
            /// <summary>Второй игрок принял вызов, идет процесс разговора</summary>
            Process,
        }

        public enum CancelTypes : byte
        {
            /// <summary>Вызов отменен сервером</summary>
            ServerAuto = 0,
            /// <summary>Вызов отменен первым игроком</summary>
            Caller,
            /// <summary>Вызов отменен вторым игроком</summary>
            Receiver,
            /// <summary>Вызов отменен по причине недостатка средств</summary>
            NotEnoughBalance,
        }

        public PlayerData Caller { get; private set; }

        public PlayerData Receiver { get; private set; }

        public StatusTypes StatusType { get; private set; }

        public DateTime LastStatusChangeTime { get; private set; }

        public bool Exists => AllCalls.Contains(this);

        private CancellationTokenSource CTS { get; set; }

        public Call(PlayerData Caller, PlayerData Receiver)
        {
            this.LastStatusChangeTime = Utils.GetCurrentTime();

            this.Caller = Caller;
            this.Receiver = Receiver;

            this.StatusType = StatusTypes.Outgoing;

            AllCalls.Add(this);

            CTS = new CancellationTokenSource();

            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await System.Threading.Tasks.Task.Delay(Settings.PHONE_MAX_CALL_OUT_TIME, CTS.Token);

                    NAPI.Task.Run(() =>
                    {
                        if (!Exists || StatusType != StatusTypes.Outgoing)
                            return;

                        Cancel(CancelTypes.ServerAuto);
                    });
                }
                catch (Exception)
                {

                }
            });

            Receiver.Player.TriggerEvent("Phone::ACS", false, Caller.Info.PhoneNumber);
        }

        public void SetAsProcess()
        {
            if (CTS != null)
            {
                CTS.Cancel();
                CTS.Dispose();
            }

            var maxCallTime = (int)((Caller.Info.PhoneBalance / Settings.PHONE_CALL_COST_X) * Settings.PHONE_CALL_X);

            CTS = new CancellationTokenSource();

            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await System.Threading.Tasks.Task.Delay(maxCallTime, CTS.Token);

                    NAPI.Task.Run(() =>
                    {
                        if (!Exists)
                            return;

                        Cancel(CancelTypes.NotEnoughBalance);
                    });
                }
                catch (Exception)
                {

                }
            });

            Caller.Player.TriggerEvent("Phone::ACS", true, Receiver.Player.Id);
            Receiver.Player.TriggerEvent("Phone::ACS", true, Caller.Player.Id);
        }

        public void Cancel(CancelTypes cancelType)
        {
            if (CTS == null)
                return;

            CTS.Cancel();
            CTS.Dispose();

            CTS = null;

            AllCalls.Remove(this);

            Caller.Player.TriggerEvent("Phone::ACS", (byte)cancelType);
            Receiver.Player.TriggerEvent("Phone::ACS", (byte)cancelType);
        }
    }
}
