namespace BlaineRP.Server.Game.Inventory
{
    /// <summary>Секции инвентаря</summary>
    public enum GroupTypes
    {
        /// <summary>Карманы</summary>
        Items = 0,
        /// <summary>Сумка</summary>
        Bag = 1,
        /// <summary>Оружие</summary>
        Weapons = 2,
        /// <summary>Кобура (оружие)</summary>
        Holster = 3,
        /// <summary>Одежда</summary>
        Clothes = 4,
        /// <summary>Аксессуары</summary>
        Accessories = 5,
        /// <summary>Предмет сумки</summary>
        BagItem = 6,
        /// <summary>Предмет кобуры</summary>
        HolsterItem = 7,
        /// <summary>Бронежилет</summary>
        Armour = 8,
        /// <summary>Контейнер</summary>
        Container = 9,

        CraftItems = 20,
        CraftTools = 21,
        CraftResult = 22,
    }
}