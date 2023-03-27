using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.CEF
{
    class Browser : Events.Script
    {
        public const string CurrentServer = "sandy";

        public const string DefaultServer = "default";

        public static RAGE.Ui.HtmlWindow Window { get; private set; }

        private static HashSet<IntTypes> ActiveInterfaces { get; set; } = new HashSet<IntTypes>();

        private static HashSet<IntTypes> RenderedInterfaces { get; set; } = new HashSet<IntTypes>();

        public static bool IsAnyDomElementFocused { get; private set; }

        private static bool _IsAnyCEFActive;

        public static bool IsAnyCEFActive { get => _IsAnyCEFActive || CEF.MapEditor.IsActive || CEF.Phone.IsActive; private set { _IsAnyCEFActive = value; CEF.Cursor.SwitchEscMenuAccess(!value); } }

        private static List<IntTypes> NormalInterfaces { get; set; } = new List<IntTypes>()
        {
            IntTypes.HUD_Top, IntTypes.HUD_Quest, IntTypes.HUD_Help, IntTypes.HUD_Speedometer, IntTypes.HUD_Interact, IntTypes.HUD_Left,
            IntTypes.Chat, IntTypes.Notifications,
            IntTypes.Phone,
        };

        public enum IntTypes
        {
            Login = 0, Registration, CharacterSelection, StartPlace,
            CharacterCreation,
            HUD, HUD_Top, HUD_Quest, HUD_Help, HUD_Speedometer, HUD_Interact, HUD_Menu, HUD_Left,
            Inventory_Full, Inventory, CratesInventory, Trade, Workbench,
            ActionBox,
            Animations,
            Documents,
            NPC,
            Interaction, Interaction_Character, Interaction_Vehicle_In, Interaction_Vehicle_Out, Interaction_Passengers,
            Shop, Retail, Tuning, Salon, TattooSalon,
            VehicleMisc,
            Death,
            Chat,
            Menu,
            Phone,
            MenuBusiness, MenuGarage, MenuBank, MenuHome, MenuFraction,
            AutoschoolTest,
            Estate, EstateAgency,
            Elevator,
            ATM,
            BlipsMenu,
            Notifications,

            MinigameOrangePicking, MinigameLockPicking,

            Note,
        }

        private static Dictionary<IntTypes, string> IntNames = new Dictionary<IntTypes, string>()
        {
            { IntTypes.Login, "login" }, { IntTypes.Registration, "reg" }, { IntTypes.CharacterSelection, "char_selection" }, { IntTypes.StartPlace, "start_place" },

            { IntTypes.CharacterCreation, "char_creation" },

            { IntTypes.HUD, "hud" }, { IntTypes.HUD_Top, "hud_top" }, { IntTypes.HUD_Quest, "hud_quest" }, { IntTypes.HUD_Help, "hud_help" }, { IntTypes.HUD_Speedometer, "hud_spd" }, { IntTypes.HUD_Interact, "hud_interact" }, { IntTypes.HUD_Menu, "hud_menu" }, { IntTypes.HUD_Left, "hud_left" },

            { IntTypes.Inventory, "inventory" }, { IntTypes.Inventory_Full, "full_inventory" }, { IntTypes.CratesInventory, "crates_inventory" }, { IntTypes.Trade, "trade" }, { IntTypes.Workbench, "workbench" },

            { IntTypes.ActionBox, "actionbox" },

            { IntTypes.Animations, "anims" },

            { IntTypes.Documents, "docs" },

            { IntTypes.NPC, "npc" },

            { IntTypes.Interaction, "interaction" }, { IntTypes.Interaction_Character, "char_interaction" }, { IntTypes.Interaction_Vehicle_In, "iv_interaction" }, { IntTypes.Interaction_Vehicle_Out, "ov_interaction" }, { IntTypes.Interaction_Passengers, "pass_interaction"},

            { IntTypes.Shop, "shop" }, { IntTypes.Retail, "retail" }, { IntTypes.Tuning, "tuning" }, { IntTypes.Salon, "salon" }, { IntTypes.TattooSalon, "tattoo_salon" },

            { IntTypes.VehicleMisc, "car_maint" },

            { IntTypes.Death, "death" },

            { IntTypes.Chat, "chat" },

            { IntTypes.Phone, "phone" },

            { IntTypes.Menu, "menu" },

            { IntTypes.MenuBusiness, "menu_biz" }, { IntTypes.MenuGarage, "menu_gar" }, { IntTypes.MenuBank, "menu_bank" }, { IntTypes.MenuHome, "menu_home" }, { IntTypes.MenuFraction, "menu_frac" },

            { IntTypes.AutoschoolTest, "autoschool" },

            { IntTypes.Estate, "estate" }, { IntTypes.EstateAgency, "est_agency" },

            { IntTypes.Elevator, "elevator" },

            { IntTypes.ATM, "atm" },

            { IntTypes.Note, "note" },

            { IntTypes.BlipsMenu, "blips" },

            { IntTypes.Notifications, "notifications" },

            { IntTypes.MinigameOrangePicking, "orange_picking" }, { IntTypes.MinigameLockPicking, "lock_picking" },
        };

        private static Dictionary<IntTypes, IntTypes> RenderDependencies = new Dictionary<IntTypes, IntTypes>()
        {
            { IntTypes.CratesInventory, IntTypes.Inventory_Full }, { IntTypes.Inventory, IntTypes.Inventory_Full }, { IntTypes.Trade, IntTypes.Inventory_Full }, { IntTypes.Workbench, IntTypes.Inventory_Full },

            { IntTypes.Interaction_Character, IntTypes.Interaction }, { IntTypes.Interaction_Vehicle_In, IntTypes.Interaction }, { IntTypes.Interaction_Vehicle_Out, IntTypes.Interaction }, { IntTypes.Interaction_Passengers, IntTypes.Interaction },

            { IntTypes.HUD_Top, IntTypes.HUD }, { IntTypes.HUD_Quest, IntTypes.HUD }, { IntTypes.HUD_Help, IntTypes.HUD }, { IntTypes.HUD_Speedometer, IntTypes.HUD }, { IntTypes.HUD_Interact, IntTypes.HUD }, { IntTypes.HUD_Menu, IntTypes.HUD }, { IntTypes.HUD_Left, IntTypes.HUD },
        };

        private static HashSet<IntTypes> PendingRenders = new HashSet<IntTypes>();

        private static HashSet<IntTypes> PendingOffRenders = new HashSet<IntTypes>();

        public Browser()
        {
            Window = new RAGE.Ui.HtmlWindow("package://cef/index.html");

            Events.OnBrowserDomReady += async (RAGE.Ui.HtmlWindow window) =>
            {
                if (window.Id != Window.Id)
                    return;

                await Render(IntTypes.Notifications, true, true);

                await Render(IntTypes.HUD, true, false);

                await Render(IntTypes.Menu, true, false);

                await Render(IntTypes.BlipsMenu, true, false);

                CEF.Menu.Preload();

                Window.ExecuteJs("Hud.setTop", new object[] { new object[] { CurrentServer, Player.LocalPlayer.RemoteId, Entities.Players.Count, true } });

                CEF.HUD.Update += CEF.HUD.UpdateHUD;

                //GameEvents.ScreenResolutionChange += (x, y) => Window.ExecuteCachedJs("resizeAll();");

                Window.Active = true;
            };

            Events.Add("Resize::UpdateLeftHudPos", (object[] args) => CEF.HUD.UpdateLeftHUDPos());

            Events.OnBrowserCreated += (RAGE.Ui.HtmlWindow window) =>
            {
                if (window.Id != Window.Id)
                    return;

                Window.Active = false;
            };

            Events.Add("Browser::OnRenderFinished", async (object[] args) =>
            {
                //await RAGE.Game.Invoker.WaitAsync(25);

                RenderedInterfaces.Add(IntNames.Where(x => x.Value == (string)args[0]).First().Key);

                //Utils.ConsoleOutput($"v-if: Ready, {IntNames.Where(x => x.Value == (string)args[0]).First().Key}");
            });

            Events.Add("Browser::OnFocusElem", (args) =>
            {
                var elemTypeStr = args[0] is string str ? str : null;

                if (elemTypeStr == null)
                {
                    IsAnyDomElementFocused = false;
                }
                else
                {
                    if (elemTypeStr == "text")
                    {
                        IsAnyDomElementFocused = true;
                    }
                    else
                    {
                        IsAnyDomElementFocused = false;
                    }
                }
            });
        }

        #region Utils
        public static bool IsRendered(IntTypes type) => RenderedInterfaces.Contains(type);

        public static bool IsActive(IntTypes type) => ActiveInterfaces.Contains(type);

        public static bool IsActiveAnd(params IntTypes[] types)
        {
            for (int i = 0; i < types.Length; i++)
                if (!ActiveInterfaces.Contains(types[i]))
                    return false;

            return true;
        }

        public static bool IsActiveOr(params IntTypes[] types)
        {
            for (int i = 0; i < types.Length; i++)
                if (ActiveInterfaces.Contains(types[i]))
                    return true;

            return false;
        }

        public static async System.Threading.Tasks.Task Switch(IntTypes type, bool state)
        {
            if (state)
            {
                IntTypes mType;

                if (RenderDependencies.TryGetValue(type, out mType))
                {
                    while (!IsRendered(mType))
                        await RAGE.Game.Invoker.WaitAsync(0);
                }
                else
                    while (!IsRendered(type))
                        await RAGE.Game.Invoker.WaitAsync(0);

                ActiveInterfaces.Add(type);

                if (IsAnyCEFActive != true && !NormalInterfaces.Contains(type))
                    IsAnyCEFActive = true;
            }
            else
            {
                ActiveInterfaces.Remove(type);

                IsAnyCEFActive = ActiveInterfaces.Union(NormalInterfaces).Count() > NormalInterfaces.Count;
            }

            //Utils.ConsoleOutput($"v-switch: {state}, {type}");

            Window.ExecuteCachedJs("switchTemplate", state, IntNames[type]);
        }

        public static async System.Threading.Tasks.Task<bool> Render(IntTypes type, bool state, bool switchAfter = false)
        {
            //Utils.ConsoleOutput($"v-if: {state}, {type}");

            if (state)
            {
                PendingOffRenders.Remove(type);

                if (!IsRendered(type))
                {
                    if (PendingRenders.Add(type))
                        Window.ExecuteCachedJs("renderTemplate", true, IntNames[type]);
                    else
                        return false;

                    do
                    {
                        await RAGE.Game.Invoker.WaitAsync(0);
                    }
                    while (!IsRendered(type));

                    PendingRenders.Remove(type);

                    if (PendingOffRenders.Remove(type))
                    {
                        RenderedInterfaces.Remove(type);

                        Window.ExecuteCachedJs("renderTemplate", false, IntNames[type]);

                        return false;
                    }
                }
                else
                {
                    return false;
                }

                if (switchAfter)
                    Switch(type, true);

                return true;
            }
            else
            {
                //Utils.ConsoleOutput(RAGE.Util.Json.Serialize(PendingRenders));

                if (PendingRenders.Contains(type))
                {
                    PendingOffRenders.Add(type);

                    return true;
                }
                else
                {
                    if (RenderedInterfaces.Remove(type))
                    {
                        Window.ExecuteCachedJs("renderTemplate", false, IntNames[type]);

                        if (ActiveInterfaces.Remove(type))
                        {
                            IsAnyCEFActive = ActiveInterfaces.Union(NormalInterfaces).Count() > NormalInterfaces.Count;
                        }

                        return true;
                    }

                    return false;
                }
            }
        }

        public static async System.Threading.Tasks.Task WaitAllPendingRenders()
        {
            while (PendingRenders.Count > 0)
                await RAGE.Game.Invoker.WaitAsync(0);
        }

        public static void HideAll(bool state)
        {
            foreach (var x in ActiveInterfaces)
                SwitchTemp(x, !state);

            if (CEF.Cursor.IsActive)
                CEF.Cursor.IsVisible = !state;
        }

        public static void SwitchTemp(IntTypes type, bool state)
        {
            Window.ExecuteCachedJs("switchTemplate", state, IntNames[type]);
        }
        #endregion
    }
}
