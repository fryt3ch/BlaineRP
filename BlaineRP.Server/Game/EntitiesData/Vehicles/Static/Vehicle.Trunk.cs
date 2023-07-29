namespace BlaineRP.Server.Game.EntitiesData.Vehicles.Static
{
    public partial class Vehicle
    {
        public class Trunk
        {
            /// <summary>Кол-во слотов в багажнике</summary>
            public int Slots { get; set; }

            /// <summary>Максимальный вес багажника</summary>
            public float MaxWeight { get; set; }

            public Trunk(int slots, float maxWeight)
            {
                Slots = slots;
                MaxWeight = maxWeight;
            }
        }
    }
}