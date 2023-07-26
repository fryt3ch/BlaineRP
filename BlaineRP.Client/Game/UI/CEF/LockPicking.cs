using BlaineRP.Client.Extensions.RAGE.Ui;
using RAGE;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class LockPicking
    {
        public const byte DurabilityDefault = 10;
        public const int MaxDeviationDefault = 10;
        public const int RotationDefault = 0;

        private static byte _currentDurability;
        private static int _currentMaxDeviation;

        private static int _escBindIdx;

        public LockPicking()
        {
            Events.Add("MiniGames::LockPick",
                async (args) =>
                {
                    var success = (bool)args[0];

                    if (CurrentContext == "POLICE_CUFFS_LOCKPICK")
                    {
                        int itemIdx = GetInventoryLockpickItemIdx();

                        if (itemIdx < 0)
                        {
                            Notification.Show("Inventory::NoItem");

                            Close();

                            return;
                        }

                        if (success)
                        {
                            var res = (int)await Events.CallRemoteProc("MG::LOCKPICK::Cuffs", true, itemIdx);

                            Close();

                            if (res == 255)
                                Notification.ShowSuccess(Language.Strings.Get("POLICE_CUFFS_LOCKPICK_0"));
                            else
                                Notification.ShowErrorDefault();
                        }
                        else
                        {
                            var targetRotation = Utils.Convert.ToInt32(args[1]);

                            var res = (int)await Events.CallRemoteProc("MG::LOCKPICK::Cuffs", false, itemIdx);

                            if (res == 255)
                            {
                                decimal lockpicksLeft = GetLockpickTotalAmount();

                                if (lockpicksLeft <= 0)
                                {
                                    Notification.Show("Inventory::NoItem");

                                    Close();

                                    return;
                                }
                                else
                                {
                                    Notification.ShowError(Language.Strings.Get("POLICE_CUFFS_LOCKPICK_1", lockpicksLeft));

                                    Update(_currentDurability, targetRotation, _currentMaxDeviation, RotationDefault);
                                }
                            }
                            else
                            {
                                Close();
                            }
                        }
                    }
                }
            );
        }

        public static string CurrentContext { get; private set; }

        public static async System.Threading.Tasks.Task Show(string context, byte durability, int targetRotation, int maxDeviation, int currentRotation)
        {
            if (CurrentContext != null)
                return;

            await Browser.Render(Browser.IntTypes.MinigameLockPicking, true, true);

            HUD.ShowHUD(false);

            Chat.Show(false);

            RAGE.Game.Graphics.TransitionToBlurred(0f);

            _currentMaxDeviation = maxDeviation;
            _currentDurability = durability;

            CurrentContext = context;

            Update(durability, targetRotation, maxDeviation, currentRotation);

            Cursor.Show(true, true);

            _escBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close()
        {
            if (CurrentContext == null)
                return;

            CurrentContext = null;

            Browser.Render(Browser.IntTypes.MinigameLockPicking, false);

            Cursor.Show(false, false);

            if (!Settings.User.Interface.HideHUD)
                HUD.ShowHUD(true);

            Chat.Show(true);

            RAGE.Game.Graphics.TransitionFromBlurred(0f);

            Input.Core.Unbind(_escBindIdx);

            _escBindIdx = -1;
        }

        public static void Update(int durability, int targetRotation, int maxDeviation, int currentRotation)
        {
            if (CurrentContext == null)
                return;

            Browser.Window.ExecuteJs("MG.LP.draw", durability, targetRotation + 90, maxDeviation, currentRotation - 90);
        }

        public static int GetInventoryLockpickItemIdx()
        {
            int idx = -1;

            for (var i = 0; i < Inventory.ItemsParams.Length; i++)
            {
                if (Inventory.ItemsParams[i]?.Id == "mis_lockpick")
                {
                    idx = i;

                    break;
                }
            }

            return idx;
        }

        public static decimal GetLockpickTotalAmount()
        {
            var totalAmount = 0m;

            for (var i = 0; i < Inventory.ItemsParams.Length; i++)
            {
                if (Inventory.ItemsParams[i]?.Id == "mis_lockpick")
                    totalAmount += (int)((object[])Inventory.ItemsData[i][0])[3];
            }

            return totalAmount;
        }

        public static int GetLockpickingRandomTargetRotation()
        {
            return Utils.Misc.Random.Next(0, 360 + 1);
        }
    }
}