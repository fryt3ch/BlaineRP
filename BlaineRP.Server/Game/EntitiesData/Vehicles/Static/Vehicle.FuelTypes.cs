namespace BlaineRP.Server.Game.EntitiesData.Vehicles.Static
{
    public partial class Vehicle
    {
        /// <summary>Типы топлива</summary>
        public enum FuelTypes
        {
            /// <summary>Топливо не требуется</summary>
            None = -1,
            /// <summary>Бензин</summary>
            Petrol = 0,
            /// <summary>Электричество</summary>
            Electricity = 1,
        }
    }
}