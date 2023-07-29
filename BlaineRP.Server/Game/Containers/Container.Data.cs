namespace BlaineRP.Server.Game.Containers
{
    public partial class Container
    {
        public class Data
        {
            public int Slots { get; set; }

            public float MaxWeight { get; set; }

            public AllowedItemTypes AllowedItemsType { get; set; }

            public ContainerTypes ContainerType { get; set; }

            public Data(int slots, float maxWeight, AllowedItemTypes allowedItemsType, ContainerTypes containerType)
            {
                Slots = slots;
                MaxWeight = maxWeight;
                AllowedItemsType = allowedItemsType;
                ContainerType = containerType;
            }
        }
    }
}