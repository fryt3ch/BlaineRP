using System;
using System.Collections.Generic;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Peds;
using BlaineRP.Client.Game.NPCs;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Scripts.Sync
{
    [Script(int.MaxValue)]
    public class Peds
    {
        private static Dictionary<string, Action<PedData, object, object>> DataActions = new Dictionary<string, Action<PedData, object, object>>();

        public Peds()
        {
            AddDataHandler("IsInvincible",
                (pData, value, oldValue) =>
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
                }
            );

            AddDataHandler("IsInvisible",
                (pData, value, oldValue) =>
                {
                    if ((bool)value)
                        pData.Ped.SetVisible(false, true);
                    else
                        pData.Ped.SetVisible(true, true);
                }
            );
        }

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

        private static void InvokeHandler(string dataKey, PedData pData, object value, object oldValue = null)
        {
            DataActions.GetValueOrDefault(dataKey)?.Invoke(pData, value, oldValue);
        }

        private static void AddDataHandler(string dataKey, Action<PedData, object, object> action)
        {
            Events.AddDataHandler(dataKey,
                (Entity entity, object value, object oldValue) =>
                {
                    if (entity is Ped ped)
                    {
                        PedData data = GetData(ped);

                        if (data == null)
                            return;

                        action.Invoke(data, value, oldValue);
                    }
                }
            );

            DataActions.Add(dataKey, action);
        }

        public static async System.Threading.Tasks.Task OnPedStreamIn(Ped ped)
        {
            if (ped.IsLocal)
            {
                await NPC.OnPedStreamIn(ped);

                return;
            }

            PedData data = GetData(ped);

            if (data != null)
                data.Reset();

            data = new PedData(ped);

            SetData(ped, data);

            InvokeHandler("IsInvincible", data, data.IsInvincible, null);
            InvokeHandler("IsInvisible", data, data.IsInvisible, null);
        }

        public static async System.Threading.Tasks.Task OnPedStreamOut(Ped ped)
        {
            if (ped.IsLocal)
            {
                await NPC.OnPedStreamOut(ped);

                return;
            }

            PedData data = GetData(ped);

            if (data == null)
                return;

            data.Reset();
        }
    }
}