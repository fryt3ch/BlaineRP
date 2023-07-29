namespace BlaineRP.Server.Game.EntitiesData.Players.Customization.Tattoos
{
    public class TattooData
    {
        public ZoneTypes ZoneType { get; }

        public TattooData(ZoneTypes zoneType)
        {
            ZoneType = zoneType;
        }
    }
}