using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data.Minigames
{
    public class LockPicking : Events.Script
    {
        public static string CurrentContext { get; private set; }

        public const byte DurabilityDefault = 20;
        public const int MaxDeviationDefault = 10;
        public const int RotationDefault = 0 - 90;

        private LockPicking()
        {
            Events.Add("MiniGames::LockPick", async (args) =>
            {
                var success = (bool)args[0];

                if (CurrentContext == "POLICE_CUFFS_LOCKPICK")
                {
                    var itemIdx = GetInventoryLockpickItemIdx();

                    if (itemIdx < 0)
                    {
                        CEF.Notification.Show("Inventory::NoItem");

                        Close();

                        return;
                    }

                    if (success)
                    {
                        var res = (int)await Events.CallRemoteProc("MG::LOCKPICK::Cuffs", true, itemIdx);

                        Close();

                        if (res == 255)
                        {
                            CEF.Notification.ShowSuccess(Language.Strings.Get("POLICE_CUFFS_LOCKPICK_0"));
                        }
                        else
                        {
                            CEF.Notification.ShowErrorDefault();
                        }
                    }
                    else
                    {
                        var res = (int)await Events.CallRemoteProc("MG::LOCKPICK::Cuffs", false, itemIdx);

                        if (res == 255)
                        {
                            var lockpicksLeft = GetLockpickTotalAmount();

                            if (lockpicksLeft <= 0)
                            {
                                CEF.Notification.Show("Inventory::NoItem");

                                Close();

                                return;
                            }
                            else
                            {
                                CEF.Notification.ShowError(Language.Strings.Get("POLICE_CUFFS_LOCKPICK_1", lockpicksLeft));

                                Update(0, 0, 0, 0);
                            }
                        }
                        else
                        {
                            Close();
                        }
                    }
                }
            });
        }

        public static async System.Threading.Tasks.Task Show(string context, int durability, int targetDegrees, int maxDeviation, int currentDegrees)
        {
            if (context != null)
                return;

            await CEF.Browser.Render(CEF.Browser.IntTypes.MinigameLockPicking, true, true);

            CurrentContext = context;

            Update(durability, targetDegrees, maxDeviation, currentDegrees);

            CEF.Cursor.Show(true, true);
        }

        public static void Close()
        {
            if (CurrentContext == null)
                return;

            CurrentContext = null;

            CEF.Browser.Render(CEF.Browser.IntTypes.MinigameLockPicking, false);
        }

        public static void Update(int durability, int targetDegrees, int maxDeviation, int currentDegrees)
        {
            if (CurrentContext == null)
                return;

            CEF.Browser.Window.ExecuteJs("MG.LP.draw", durability, targetDegrees, maxDeviation, currentDegrees);
        }

        public static int GetInventoryLockpickItemIdx()
        {
            int idx = -1;

            for (int i = 0; i < CEF.Inventory.ItemsParams.Length; i++)
            {
                if (CEF.Inventory.ItemsParams[i]?.Id == "mis_lockpick")
                {
                    idx = i;

                    break;
                }
            }

            return idx;
        }

        public static decimal GetLockpickTotalAmount()
        {
            decimal totalAmount = 0m;

            for (int i = 0; i < CEF.Inventory.ItemsParams.Length; i++)
            {
                if (CEF.Inventory.ItemsParams[i]?.Id == "mis_lockpick")
                {
                    totalAmount += (int)((object[][])CEF.Inventory.ItemsData[0][i])[0][3];
                }
            }

            return totalAmount;
        }
    }
}
