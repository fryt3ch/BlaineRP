using System;
using System.Collections.Generic;
using System.Threading;
using BlaineRP.Server.Game.Items;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Craft.Workbenches
{
    public abstract partial class Workbench
    {
        public class PendingCraftData
        {
            public Timer Timer { get; private set; }

            public DateTime CreationDate { get; private set; }

            public Receipt Receipt { get; private set; }

            public int Amount { get; private set; }

            public List<Item> CraftItems { get; private set; }

            public bool IsInProcess => Timer != null;

            public PendingCraftData(List<Item> CraftItems, Receipt Receipt, int Amount = 1)
            {
                this.CraftItems = CraftItems;

                this.Receipt = Receipt;
                this.Amount = Amount;
            }

            public void Start(Workbench wb, int timeout = 0)
            {
                if (IsInProcess)
                    return;

                CreationDate = Utils.GetCurrentTime().AddMilliseconds(timeout);

                Timer = new Timer((obj) =>
                {
                    NAPI.Task.Run(() =>
                    {
                        if (!wb.Exists)
                            return;

                        wb.ProceedCraft(CraftItems, Receipt, Amount, 0);
                    });
                }, null, timeout, Timeout.Infinite);
            }

            public void Cancel()
            {
                if (!IsInProcess)
                    return;

                Timer.Dispose();

                Timer = null;
            }
        }
    }
}