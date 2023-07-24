using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Input;
using BlaineRP.Client.Utils;
using RAGE;

namespace BlaineRP.Client.Data.Minigames
{
    [Script(int.MaxValue)]
    public class LockPicking
    {
        public const byte DurabilityDefault = 10;
        public const int MaxDeviationDefault = 10;
        public const int RotationDefault = 0;

        public static string CurrentContext { get; private set; }

        private static byte _currentDurability;
        private static int _currentMaxDeviation;

        private static int _escBindIdx;

        public LockPicking()
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
                        var targetRotation = Utils.Convert.ToInt32(args[1]);

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

                                Update(_currentDurability, targetRotation, _currentMaxDeviation, RotationDefault);
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

        public static async System.Threading.Tasks.Task Show(string context, byte durability, int targetRotation, int maxDeviation, int currentRotation)
        {
            if (CurrentContext != null)
                return;

            await CEF.Browser.Render(CEF.Browser.IntTypes.MinigameLockPicking, true, true);

            CEF.HUD.ShowHUD(false);

            CEF.Chat.Show(false);

            RAGE.Game.Graphics.TransitionToBlurred(0f);

            _currentMaxDeviation = maxDeviation;
            _currentDurability = durability;

            CurrentContext = context;

            Update(durability, targetRotation, maxDeviation, currentRotation);

            CEF.Cursor.Show(true, true);

            _escBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close()
        {
            if (CurrentContext == null)
                return;

            CurrentContext = null;

            CEF.Browser.Render(CEF.Browser.IntTypes.MinigameLockPicking, false);

            CEF.Cursor.Show(false, false);

            if (!Settings.User.Interface.HideHUD)
                CEF.HUD.ShowHUD(true);

            CEF.Chat.Show(true);

            RAGE.Game.Graphics.TransitionFromBlurred(0f);

            Core.Unbind(_escBindIdx);

            _escBindIdx = -1;
        }

        public static void Update(int durability, int targetRotation, int maxDeviation, int currentRotation)
        {
            if (CurrentContext == null)
                return;

            CEF.Browser.Window.ExecuteJs("MG.LP.draw", durability, targetRotation + 90, maxDeviation, currentRotation - 90);
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
                    totalAmount += (int)((object[])CEF.Inventory.ItemsData[i][0])[3];
                }
            }

            return totalAmount;
        }

        public static int GetLockpickingRandomTargetRotation()
        {
            return Misc.Random.Next(0, 360 + 1);
        }
    }
}
