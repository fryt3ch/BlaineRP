namespace BCRPClient
{
    public static partial class Locale
    {
        public static partial class Notifications
        {
            public static class Inventory
            {
                public static string Header = "Инвентарь";

                public static string FishingHeader = "Рыбалка";

                public static string ActionRestricted = "В данный момент вы не можете делать это!";
                public static string PlaceRestricted = "Вы не можете положить данный предмет в это место!";
                public static string NoSpace = "Нет свободного места!";

                public static string AddedOne = "{0} у вас в инвентаре!";
                public static string Added = "{0} x{1} у вас в инвентаре!";

                public static string FishCatchedOne = "Вы выловили {0}";
                public static string FishCatched = "Вы выловили {0} x{1}";

                public static string TempItem = "Этот предмет является временным!\nВы не можете выполнить данное действие!";
                public static string TempItemDeleted = "Этот предмет был временным и был удалён";

                public static string ArmourBroken = "Ваш бронежилет сломался!";

                public static string Wounded = "Нельзя использовать это сразу после ранения!";

                public static string NoSuchItem = "У вас нет необходимого предмета в инвентаре!";
                public static string NoSuchItemAmount = "У вас нет необходимого кол-ва нужного предмета в инвентаре!";

                public static string InventoryBlocked = "В данный момент вы не можете взаимодействовать с инвентарем!";

                public static string WeaponHasThisComponent = "На это оружие уже установлен этот компонент!";
                public static string WeaponWrongComponent = "На это оружие нельзя установить этот компонент!";

                public static string NoWeaponSkins = "У вас не активна ни одна раскраска на оружие!";

                public static string FishingNotAllowedHere = "Вы не можете здесь рыбачить! Найдите достаточно глубокий водоем, встаньте поближе к воде и смотрите в ее сторону";
                public static string FishingNotCatched = "Вы не смогли выловить добычу!";

                public static string DiggingNotAllowedHere = "Вы не можете здесь копать! Найдите подходящую поверхность";

                public static string WorkbenchResultItemExists = "Перед тем, как начать создание нового предмета, заберите уже созданный!";

                public static string PlacedItemOnGroundNotAllowed = "Этот предмет принадлежит другому игроку!";

                public static string MaxAmountFurnitureOwned = "Вы уже владеете максимальным кол-вом мебели ({0})!";
                public static string MaxAmountFurnitureHouse = "Вы разместили максимально возможное кол-во мебели ({0})!";
            }

            public static class Container
            {
                public static string Header = "Контейнер";

                public static string Wait = "Подождите, пока кто-то прекратит пользоваться этим (максимально - {0} человек одновременно)";
            }
        }
    }
}
