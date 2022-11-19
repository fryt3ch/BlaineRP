using RAGE;
using RAGE.Elements;
using RAGE.NUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Additional
{
    class TuningMenu
    {
        private static Dictionary<int, (string Name, Dictionary<int, string> ModNames)> Slots = new Dictionary<int, (string, Dictionary<int, string>)>
        {
            {
                0, ("Спойлер", null)
            },

            {
                1, ("Передний бампер", null)
            },

            {
                2, ("Задний бампер", null)
            },

            {
                3, ("Пороги", null)
            },

            {
                4, ("Глушитель", null)
            },

            {
                5, ("Рама", null)
            },

            {
                6, ("Радиатор", null)
            },

            {
                7, ("Капот", null)
            },

            {
                10, ("Крыша", null)
            },

            {
                11, ("Двигатель",
                
                new Dictionary<int, string>()
                {
                    { -1, "Обычный" },

                    { 0, "Улучшенный #1" },
                    { 1, "Улучшенный #2" },
                    { 2, "Улучшенный #3" },
                    { 3, "Улучшенный #4" },
                })
            },

            {
                12, ("Тормоза", new Dictionary<int, string>()
                {
                    { -1, "Обычные" },

                    { 0, "Полуспортивные" },
                    { 1, "Спортивные" },
                    { 2, "Гоночные" },
                })
            },

            {
                13, ("Коробка передач", new Dictionary<int, string>()
                {
                    { -1, "Обычная" },

                    { 0, "Полуспортивная" },
                    { 1, "Спортивная" },
                    { 2, "Гоночная" },
                })
            },

            {
                14, ("Клаксон", null)
            },

            {
                15, ("Подвеска", new Dictionary<int, string>()
                {
                    { -1, "Обычная" },

                    { 0, "Полуспортивная" },
                    { 1, "Спортивная" },
                    { 2, "Гоночная" },
                    { 3, "Раллийная" },
                })
            },

            {
                18, ("Турбо-тюнинг", new Dictionary<int, string>()
                {
                    { -1, "Нет" },

                    { 0, "Есть" },
                })
            },

            {
                22, ("Ксенон", new Dictionary<int, string>()
                {
                    { -1, "Нет" },

                    { 0, "Есть" },
                })
            },

            {
                23, ("Покрышки", null)
            },

            {
                24, ("Покрышки (зад)", null)
            },

            {
                48, ("Принты", null)
            },

            {
                33, ("Руль", null)
            },

            {
                38, ("Гидравлика", null)
            },

            {
                27, ("Внешняя отделка", null)
            },

            {
                28, ("Орнаменты", null)
            },

            {
                32, ("Кресла", null)
            },

            {
                29, ("Приборная панель", null)
            },

            {
                30, ("Спидометр", null)
            },

            {
                37, ("Багажник", null)
            },

            {
                55, ("Тонировка", new Dictionary<int, string>()
                {
                    { -1, "Нет" },

                    { 0, "Лимузин (полная)" },
                    { 1, "Темная" },
                    { 2, "Обычная" },
                })
            },
        };

        private static MenuPool MenuPool;

        public static bool IsActive { get; private set; }

        public static void Show()
        {
            if (IsActive)
                return;

            var veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true || !veh.IsLocal || !veh.HasData("IsTestDrive"))
                return;

            IsActive = true;

            MenuPool = new MenuPool();

            var mainMenu = new UIMenu("Тюнинг", "ТЮНИНГ");

            MenuPool.Add(mainMenu);

            /*            for (int x = 0; x < 100; x++)
                        {
                            int totalMods = veh.GetNumMods(x);

                            if (totalMods > 0)
                            {
                                var subMenu = MenuPool.AddSubMenu(mainMenu, x.ToString());

                                subMenu.SetMenuData(x);

                                var curMod = veh.GetMod(x);

                                for (int i = -1; i < totalMods; i++)
                                {
                                    var item = new UIMenuItem(i > -1 ? $"Вариант #{i + 1}" : "Стандарт");

                                    if (curMod == i)
                                        item.Enabled = false;

                                    item.SetItemData(i);

                                    subMenu.AddItem(item);
                                }

                                subMenu.OnItemSelect += (sender, item, index) =>
                                {
                                    foreach (var x in subMenu.MenuItems.Where(x => !x.Enabled))
                                        x.Enabled = true;

                                    veh.SetMod((int)sender.MenuData, (int)item.ItemData, false);

                                    item.Enabled = false;
                                };
                            }
                        }*/

            foreach (var x in Slots)
            {
                int totalMods = veh.GetNumMods(x.Key);

                if (totalMods > 0 || x.Key == 22)
                {
                    var subMenu = MenuPool.AddSubMenu(mainMenu, x.Value.Name);

                    subMenu.SetMenuData(x.Key);

                    var curMod = veh.GetMod(x.Key);

                    if (x.Value.ModNames != null)
                    {
                        foreach (var y in x.Value.ModNames)
                        {
                            var item = new UIMenuItem(y.Value);

                            item.SetItemData(y.Key);

                            if (curMod == y.Key)
                                item.Enabled = false;

                            subMenu.AddItem(item);
                        }
                    }
                    else
                    {
                        for (int i = -1; i < totalMods; i++)
                        {
                            var item = new UIMenuItem(i > -1 ? $"Вариант #{i + 1}" : "Стандарт");

                            if (curMod == i)
                                item.Enabled = false;

                            item.SetItemData(i);

                            subMenu.AddItem(item);
                        }
                    }

                    subMenu.OnItemSelect += (sender, item, index) =>
                    {
                        foreach (var x in subMenu.MenuItems.Where(x => !x.Enabled))
                            x.Enabled = true;

                        var idx = (int)sender.MenuData;
                        var state = (int)item.ItemData;

                        if (idx == 22)
                        {
                            veh.ToggleMod(idx, state != -1);

                            RAGE.Game.Invoker.Invoke(0xE41033B25D003A07, veh.Handle, -1);
                        }
                        else if (idx == 55)
                        {
                            veh.SetWindowTint(state + 1);
                        }
                        else
                        {
                            veh.SetMod(idx, state, false);
                        }

/*                        if (idx == 14)
                        {
                            veh.StartHorn(2500, RAGE.Util.Joaat.Hash("HELDDOWN"), false);
                        }*/

                        item.Enabled = false;
                    };
                }
            }

            MenuPool.RefreshIndex();

            GameEvents.Update -= DrawMenu;
            GameEvents.Update += DrawMenu;

            mainMenu.Visible = true;
            mainMenu.ResetCursorOnOpen = true;

            mainMenu.OnMenuClose += (sender) =>
            {
                IsActive = false;

                GameEvents.Update -= DrawMenu;

                MenuPool?.CloseAllMenus();

                MenuPool = null;
            };
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            MenuPool?.CloseAllMenus();

            MenuPool = null;

            IsActive = false;

            GameEvents.Update -= DrawMenu;
        }

        private static void DrawMenu()
        {
            if (Player.LocalPlayer.Vehicle == null)
            {
                Close();

                return;
            }

            MenuPool?.ProcessMenus();
        }
    }
}
