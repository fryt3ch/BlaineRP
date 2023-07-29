namespace BlaineRP.Server.Game.Management.Animations
{
    /// <summary>Типы быстрых анимаций</summary>
    public enum FastType
    {
        /// <summary>Никакая анимация не проигрывается</summary>
        None = -1,

        /// <summary>Блокировка транспорта</summary>
        VehLocking = 0,
        /// <summary>Рукопожатие</summary>
        Handshake,
        /// <summary>Подбирание предмета</summary>
        Pickup,
        /// <summary>Выбрасывание предмета</summary>
        Putdown,
        /// <summary>Свист</summary>
        Whistle,

        SmokePuffCig,
        SmokeTransitionCig,

        ItemBurger,
        ItemChips,
        ItemHotdog,
        ItemChocolate,
        ItemPizza,
        ItemCola,
        ItemBeer,
        ItemVodka,
        ItemRum,
        ItemVegSmoothie,
        ItemSmoothie,
        ItemMilkshake,
        ItemMilk,

        ItemBandage,
        ItemMedKit,

        FakeAnim,
    }
}