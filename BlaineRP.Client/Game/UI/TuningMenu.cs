using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using RAGE.Elements;
using RAGE.NUI;

namespace BlaineRP.Client.Game.UI
{
    internal class TuningMenu
    {
        private static MenuPool MenuPool;

        public static Dictionary<int, (string Id, string Name, Dictionary<int, string> ModNames)> Slots { get; private set; } =
            new Dictionary<int, (string, string, Dictionary<int, string>)>
            {
                { 0, ("spoiler", "Спойлер", null) },
                { 1, ("fbump", "Передний бампер", null) },
                { 2, ("rbump", "Задний бампер", null) },
                { 3, ("skirt", "Пороги", null) },
                { 4, ("exh", "Глушитель", null) },
                { 5, ("frame", "Рама", null) },
                { 6, ("grill", "Радиатор", null) },
                { 7, ("hood", "Капот", null) },
                { 10, ("roof", "Крыша", null) },
                {
                    11, ("engine", "Двигатель", new Dictionary<int, string>()
                    {
                        { -1, "Обычный" },
                        { 0, "Улучшенный #1" },
                        { 1, "Улучшенный #2" },
                        { 2, "Улучшенный #3" },
                        { 3, "Улучшенный #4" },
                    })
                },
                {
                    12, ("brakes", "Тормоза", new Dictionary<int, string>()
                    {
                        { -1, "Обычные" },
                        { 0, "Полуспортивные" },
                        { 1, "Спортивные" },
                        { 2, "Гоночные" },
                    })
                },
                {
                    13, ("trm", "Коробка передач", new Dictionary<int, string>()
                    {
                        { -1, "Обычная" },
                        { 0, "Полуспортивная" },
                        { 1, "Спортивная" },
                        { 2, "Гоночная" },
                    })
                },
                { 14, ("horn", "Клаксон", null) },
                {
                    15, ("susp", "Подвеска", new Dictionary<int, string>()
                    {
                        { -1, "Обычная" },
                        { 0, "Полуспортивная" },
                        { 1, "Спортивная" },
                        { 2, "Гоночная" },
                        { 3, "Раллийная" },
                    })
                },
                {
                    18, ("tt", "Турбо-тюнинг", new Dictionary<int, string>()
                    {
                        { -1, "Нет" },
                        { 0, "Есть" },
                    })
                },
                {
                    22, ("xenon", "Ксенон", new Dictionary<int, string>()
                    {
                        { -1, "Нет" },
                        { 0, "Стандарт" },
                        { 1, "Белый" },
                        { 2, "Синий" },
                        { 3, "Голубой" },
                        { 4, "Зеленый (мятный)" },
                        { 5, "Зеленый (лаймовый)" },
                        { 6, "Желтый" },
                        { 7, "Золотистый" },
                        { 8, "Оранжевый" },
                        { 9, "Красный" },
                        { 10, "Розовый (бледный)" },
                        { 11, "Розовый (насыщенный)" },
                        { 12, "Фиолетовый (бледный)" },
                        { 13, "Фиолетовый (насыщенный)" },
                    })
                },
                { 23, ("fwheel", "Покрышки", null) },
                { 24, ("rwheel", "Покрышки (зад)", null) },
                { 48, ("livery", "Принты", null) },
                { 33, ("swheel", "Руль", null) },
                { 32, ("seats", "Кресла", null) },
                {
                    55, ("wtint", "Тонировка", new Dictionary<int, string>()
                    {
                        { -1, "Нет" },
                        { 0, "Лимузин (полная)" },
                        { 1, "Темная" },
                        { 2, "Обычная" },
                    })
                },
                {
                    1000, ("colourt", "Тип покраски", new Dictionary<int, string>()
                    {
                        { 0, "Обычный" },
                        { 1, "Металлик" },
                        { 2, "Жемчужный" },
                        { 3, "Матовый" },
                        { 4, "Металлический" },
                        { 5, "Хром" },
                    })
                },
            };

        public static Dictionary<int, string> WheelTypes { get; private set; } = new Dictionary<int, string>()
        {
            { 0, "Стоковые" },
            { 1, "Спортивные" },
            { 2, "Muscle" },
            { 3, "Лоурайдерные" },
            { 4, "SUV" },
            { 5, "Для бездорожья" },
            { 6, "Tuner" },
            { 7, "Байкерские" },
            { 8, "Высочайшего класса" },
            { 9, "Benny's Original" },
            { 10, "Benny's Bespoke" },
            { 11, "Open Wheel" },
            { 12, "Street" },
            { 13, "Track" },
        };

        public static bool IsActive { get; private set; }

        public static void Show()
        {
            if (IsActive)
                return;

            Vehicle veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true || !veh.IsLocal || !veh.HasData("IsTestDrive"))
                return;

            IsActive = true;

            MenuPool = new MenuPool();

            var mainMenu = new UIMenu("Тюнинг", "ТЮНИНГ");

            MenuPool.Add(mainMenu);

            foreach (KeyValuePair<int, (string Id, string Name, Dictionary<int, string> ModNames)> x in Slots)
            {
                int totalMods = veh.GetNumMods(x.Key);

                if (totalMods > 0 || x.Key == 22 || x.Key == 18 || x.Key == 1000)
                {
                    UIMenu subMenu = MenuPool.AddSubMenu(mainMenu, x.Value.Name);

                    subMenu.SetMenuData(x.Key);

                    int curMod = x.Key == 18 ? veh.IsToggleModOn(18) ? 0 : -1 :
                        x.Key == 22 ? (veh.GetXenonColour() ?? -2) + 1 :
                        x.Key == 55 ? veh.GetWindowTint() - 1 :
                        x.Key == 1000 ? veh.GetColourType() : veh.GetMod(x.Key);

                    if (x.Value.ModNames != null)
                        foreach (KeyValuePair<int, string> y in x.Value.ModNames)
                        {
                            var item = new UIMenuItem(y.Value);

                            item.SetItemData(y.Key);

                            if (curMod == y.Key)
                                item.Enabled = false;

                            subMenu.AddItem(item);
                        }
                    else
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
                        foreach (UIMenuItem x in subMenu.MenuItems.Where(x => !x.Enabled))
                        {
                            x.Enabled = true;
                        }

                        var idx = (int)sender.MenuData;
                        var state = (int)item.ItemData;

                        if (idx == 18)
                            veh.ToggleMod(18, state >= 0);
                        else if (idx == 22)
                            veh.SetXenonColour(state < 0 ? null : (int?)state);
                        else if (idx == 1000)
                            veh.SetColourType(state);
                        else if (idx == 55)
                            veh.SetWindowTint(state + 1);
                        else
                            veh.SetMod(idx, state, false);

                        item.Enabled = false;
                    };
                }
            }

            MenuPool.RefreshIndex();

            Main.Update -= DrawMenu;
            Main.Update += DrawMenu;

            mainMenu.Visible = true;
            mainMenu.MouseControlsEnabled = false;
            mainMenu.MouseEdgeEnabled = false;
            mainMenu.FreezeAllInput = false;

            mainMenu.OnMenuClose += (sender) =>
            {
                IsActive = false;

                Main.Update -= DrawMenu;

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

            Main.Update -= DrawMenu;
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