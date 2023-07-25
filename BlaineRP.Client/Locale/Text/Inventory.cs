using System.Collections.Generic;

namespace BlaineRP.Client
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
                public static Dictionary<Game.UI.CEF.Inventory.ContainerTypes, string> Names = new Dictionary<Game.UI.CEF.Inventory.ContainerTypes, string>()
                {
                    { Game.UI.CEF.Inventory.ContainerTypes.None, "null" },
                    { Game.UI.CEF.Inventory.ContainerTypes.Trunk, "Багажник" },
                    { Game.UI.CEF.Inventory.ContainerTypes.Locker, "Шкаф" },
                    { Game.UI.CEF.Inventory.ContainerTypes.Storage, "Склад" },
                    { Game.UI.CEF.Inventory.ContainerTypes.Crate, "Ящик" },
                    { Game.UI.CEF.Inventory.ContainerTypes.Fridge, "Холодильник" },
                    { Game.UI.CEF.Inventory.ContainerTypes.Wardrobe, "Гардероб" },
                };

                public static Dictionary<Game.UI.CEF.Inventory.WorkbenchTypes, (string Name, string CraftBtnText)> WorkbenchNames = new Dictionary<Game.UI.CEF.Inventory.WorkbenchTypes, (string, string)>()
                {
                    { Game.UI.CEF.Inventory.WorkbenchTypes.None, ("null", "null") },

                    { Game.UI.CEF.Inventory.WorkbenchTypes.CraftTable, ("Верстак", "Создать") },
                    { Game.UI.CEF.Inventory.WorkbenchTypes.Grill, ("Гриль", "Приготовить") },
                    { Game.UI.CEF.Inventory.WorkbenchTypes.GasStove, ("Газовая плита", "Приготовить") },
                    { Game.UI.CEF.Inventory.WorkbenchTypes.KitchenSet, ("Кухонный гарнитур", "Приготовить") },
                };
            }
        }
    }
}
