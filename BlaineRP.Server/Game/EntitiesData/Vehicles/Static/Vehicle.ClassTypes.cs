namespace BlaineRP.Server.Game.EntitiesData.Vehicles.Static
{
    public partial class Vehicle
    {
        /// <summary>Классы транспорта</summary>
        /// <remarks>Зависит от цены</remarks>
        public enum ClassTypes
        {
            /// <summary>Обычный</summary>
            Classic = 0,
            /// <summary>Премиум</summary>
            Premium,
            /// <summary>Люкс</summary>
            Luxe,
            /// <summary>Элитный</summary>
            Elite,
        }
    }
}