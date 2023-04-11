using System.Collections.Generic;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static partial class General
        {
            public static class Inventory
            {
                public static class Actions
                {
                    public static string TakeOff = "Снять";
                    public static string TakeOn = "Надеть";

                    public static string ToHands = "В руки";
                    public static string FromHands = "Из рук";

                    public static string Load = "Зарядить";
                    public static string Unload = "Разрядить";

                    public static string Eat = "Употребить";

                    public static string SetupPlaceableItem = "Поставить на землю";

                    public static Dictionary<Sync.WeaponSystem.Weapon.ComponentTypes, string> WeaponComponentsTakeOffStrings = new Dictionary<Sync.WeaponSystem.Weapon.ComponentTypes, string>()
                    {
                        { Sync.WeaponSystem.Weapon.ComponentTypes.Suppressor, "Снять глушитель" },
                        { Sync.WeaponSystem.Weapon.ComponentTypes.Grip, "Снять рукоятку" },
                        { Sync.WeaponSystem.Weapon.ComponentTypes.Scope, "Снять прицел" },
                        { Sync.WeaponSystem.Weapon.ComponentTypes.Flashlight, "Снять фонарик" },
                    };

                    public static string Drop = "Выбросить";

                    public static string Reset = "Переключить";

                    public static string Split = "Разделить";
                    public static string Shift = "Переложить";
                    public static string ShiftTrade = "В обмен";
                    public static string ShiftOutOfTrade = "Убрать из обмена";
                    public static string ShiftCraft = "Положить";
                    public static string ShiftOutOfCraft = "Убрать";

                    public static string Use = "Использовать";

                    public static string FishingRodUseBait = "Рыбачить (на приманку)";
                    public static string FishingRodUseWorms = "Рыбачить (на червя)";

                    public static string StopUse = "Перестать использовать";

                    public static string FindVehicle = "Найти транспорт";

                    public static string NoteRead = "Прочесть";
                    public static string NoteWrite = "Написать";
                }
            }

            public static class Containers
            {
                public static Dictionary<CEF.Inventory.ContainerTypes, string> Names = new Dictionary<CEF.Inventory.ContainerTypes, string>()
                {
                    { CEF.Inventory.ContainerTypes.None, "null" },
                    { CEF.Inventory.ContainerTypes.Trunk, "Багажник" },
                    { CEF.Inventory.ContainerTypes.Locker, "Шкаф" },
                    { CEF.Inventory.ContainerTypes.Storage, "Склад" },
                    { CEF.Inventory.ContainerTypes.Crate, "Ящик" },
                    { CEF.Inventory.ContainerTypes.Fridge, "Холодильник" },
                    { CEF.Inventory.ContainerTypes.Wardrobe, "Гардероб" },
                };

                public static Dictionary<CEF.Inventory.WorkbenchTypes, (string Name, string CraftBtnText)> WorkbenchNames = new Dictionary<CEF.Inventory.WorkbenchTypes, (string, string)>()
                {
                    { CEF.Inventory.WorkbenchTypes.None, ("null", "null") },

                    { CEF.Inventory.WorkbenchTypes.CraftTable, ("Верстак", "Создать") },
                    { CEF.Inventory.WorkbenchTypes.Grill, ("Гриль", "Приготовить") },
                    { CEF.Inventory.WorkbenchTypes.GasStove, ("Газовая плита", "Приготовить") },
                    { CEF.Inventory.WorkbenchTypes.KitchenSet, ("Кухонный гарнитур", "Приготовить") },
                };
            }
        }
    }
}
