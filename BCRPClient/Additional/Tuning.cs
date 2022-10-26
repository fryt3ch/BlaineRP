using RAGE;
using RAGE.Elements;
using RAGE.NUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Additional
{
    class Tuning : Events.Script
    {
        public Tuning()
        {
            RAGE.Input.Bind(RAGE.Ui.VirtualKeys.F4, true, ShowTunningMenu);
        }

        List<string> slotNames = new List<string>()
        {
            "Spoiler",
            "Front Bumper",
            "Rear Bumper",
            "Side Skirts",
            "Exhaust",
            "Rollcage",
            "Grille",
            "Bonnet",
            "Fenders and Arches",
            "Fenders",
            "Roof",
            "Engine",
            "Brakes",
            "Transmission",
            "Horn",
            "Suspension",
            "Armor",
            "",
            "Turbo",
            "",
            "",
            "",
            "Headlights",
            "Front Wheels",
            "Back Wheels",
            "Plate Holders",
            "Vanity Plates",
            "Interior Trim",
            "Ornaments",
            "Interior Dash",
            "Dials",
            "Door Speakers",
            "Leather Seats",
            "Steering Wheels",
            "Column Shifters",
            "Plaques",
            "ICE",
            "Speakers",
            "Hydraulics",
            "Engine Block",
            "Air Filters",
            "Strut Braces",
            "Arch Covers",
            "Aerials",
            "Exterior Trim",
            "Tank",
            "Windows",
            "",
            "Livery"
        };

        private MenuPool menuPool;

        private bool menuactive = false;

        public void ShowTunningMenu()
        {
            if (menuactive) return;
            menuactive = true;
            if (Player.LocalPlayer.Vehicle == null)
            {
                return;
            }
            menuPool = new MenuPool();
            var mainMenu = new UIMenu("Тюнинг", "ТЮНИНГ");

            menuPool.Add(mainMenu);

            for (int i = 0; i < slotNames.Count; i++)
            {
                int totalmods = Player.LocalPlayer.Vehicle.GetNumMods(i);
                if (totalmods > 0 && slotNames[i].Length > 0)
                {
                    var submenu = menuPool.AddSubMenu(mainMenu, slotNames[i].ToString());

                    submenu.OnItemSelect += (sender, item, index) =>
                    {
                        Player.LocalPlayer.Vehicle.SetMod((int)sender.MenuData, index, false);
                    };
                    for (int modIndex = 0; modIndex < totalmods; modIndex++)
                    {
                        string lablename = RAGE.Game.Ui.GetLabelText(Player.LocalPlayer.Vehicle.GetModTextLabel(i, modIndex));
                        var newitem = new UIMenuItem(lablename == "NULL" ? $"{slotNames[i]} {modIndex}" : lablename, "Описание");
                        submenu.SetMenuData(i);
                        submenu.AddItem(newitem);
                        Chat.Output("");
                    }
                }
            }
            menuPool.RefreshIndex();
            Events.Tick += DrawMenu;
            mainMenu.Visible = true;

            mainMenu.OnMenuClose += (sender) =>
            {
                menuactive = false;
                Events.Tick -= DrawMenu;
            };
        }
        private void DrawMenu(List<Events.TickNametagData> nametags)
        {
            menuPool.ProcessMenus();
        }
    }
}
