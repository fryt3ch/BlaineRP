namespace BlaineRP.Server.Game.EntitiesData.Players
{
    public partial class TempData
    {
        public enum StartPlaceTypes : byte
        {
            /// <summary>Последнее место на сервере</summary>
            Last = 0,
            /// <summary>Спавн (Округ Блэйн)</summary>
            SpawnBlaineCounty,
            /// <summary>Дом</summary>
            House,
            /// <summary>Квартира</summary>
            Apartments,
            /// <summary>Фракция</summary>
            Fraction,
            /// <summary>Организация</summary>
            Organisation,
            /// <summary>Спавн (Лос-Сантос)</summary>
            SpawnLosSantos,
            /// <summary>Фракция (филиал)</summary>
            FractionBranch,
        }
    }
}