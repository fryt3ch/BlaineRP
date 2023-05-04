using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Sync
{
    public class Peds : Events.Script
    {
        public static PedData GetData(Ped ped)
        {
            if (ped == null)
                return null;

            return ped.GetData<PedData>("SyncedData");
        }

        public static void SetData(Ped ped, PedData data)
        {
            if (ped == null)
                return;

            ped.SetData("SyncedData", data);
        }

        private static Dictionary<string, Action<PedData, object, object>> DataActions = new Dictionary<string, Action<PedData, object, object>>();

        private static void InvokeHandler(string dataKey, PedData pData, object value, object oldValue = null) => DataActions.GetValueOrDefault(dataKey)?.Invoke(pData, value, oldValue);

        private static void AddDataHandler(string dataKey, Action<PedData, object, object> action)
        {
            Events.AddDataHandler(dataKey, (Entity entity, object value, object oldValue) =>
            {
                if (entity is Ped ped)
                {
                    var data = Sync.Peds.GetData(ped);

                    if (data == null)
                        return;

                    action.Invoke(data, value, oldValue);
                }
            });

            DataActions.Add(dataKey, action);
        }

        public static async System.Threading.Tasks.Task OnPedStreamIn(Ped ped)
        {
            if (ped.IsLocal)
            {
                await Data.NPC.OnPedStreamIn(ped);

                return;
            }

            var data = GetData(ped);

            if (data != null)
            {
                data.Reset();
            }

            data = new PedData(ped);

            SetData(ped, data);

            InvokeHandler("IsInvincible", data, data.IsInvincible, null);
            InvokeHandler("IsInvisible", data, data.IsInvisible, null);
        }

        public static async System.Threading.Tasks.Task OnPedStreamOut(Ped ped)
        {
            if (ped.IsLocal)
            {
                await Data.NPC.OnPedStreamOut(ped);

                return;
            }

            var data = GetData(ped);

            if (data == null)
                return;

            data.Reset();
        }

        public class PedData
        {
            public Ped Ped { get; set; }

            public bool IsInvincible => Ped.GetSharedData<bool>("GM", false);

            public bool IsInvisible => Ped.GetSharedData<bool>("INV", false);

            public PedData(Ped Ped)
            {
                this.Ped = Ped;
            }

            public void Reset()
            {
                if (Ped == null)
                    return;

                Ped.ClearTasksImmediately();

                Ped.SetNoCollisionEntity(Player.LocalPlayer.Handle, false);

                Ped.ResetData();
            }
        }

        public Peds()
        {
            AddDataHandler("IsInvincible", (pData, value, oldValue) =>
            {
                if ((bool)value)
                {
                    pData.Ped.SetInvincible(true);
                    pData.Ped.SetCanBeDamaged(false);
                }
                else
                {
                    pData.Ped.SetInvincible(false);
                    pData.Ped.SetCanBeDamaged(true);
                }
            });

            AddDataHandler("IsInvisible", (pData, value, oldValue) =>
            {
                if ((bool)value)
                {
                    pData.Ped.SetVisible(false, true);
                }
                else
                {
                    pData.Ped.SetVisible(true, true);
                }
            });
        }
    }
}
