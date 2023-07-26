namespace BlaineRP.Client.Game.Data.Vehicles
{
    public partial class Vehicle
    {
        public class Trunk
        {
            public Trunk(int Slots, float MaxWeight)
            {
                this.Slots = Slots;
                this.MaxWeight = MaxWeight;
            }

            /// <summary>Кол-во слотов в багажнике</summary>
            public int Slots { get; set; }

            /// <summary>Максимальный вес багажника</summary>
            public float MaxWeight { get; set; }
        }
    }
}