namespace BlaineRP.Client.Game.World
{
    public partial class ItemOnGround
    {
        public enum Types : byte
        {
            /// <summary>Стандартный тип предмета на земле</summary>
            /// <remarks>Автоматически удаляется с определенными условиями, может быть подобран кем угодно</remarks>
            Default = 0,

            /// <summary>
            ///     Тип предмета на земле, который был намеренно установлен игроком (предметы, наследующие вбстрактный класс
            ///     PlaceableItem)
            /// </summary>
            /// <remarks>
            ///     Предметы данного типа не удаляется автоматически, так же не могут быть подобраны кем угодно (пока действуют
            ///     определенные условия)
            /// </remarks>
            PlacedItem,
        }
    }
}