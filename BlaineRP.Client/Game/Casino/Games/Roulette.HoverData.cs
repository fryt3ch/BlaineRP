using RAGE;

namespace BlaineRP.Client.Game.Casino.Games
{
    public partial class Roulette
    {
        public class HoverData
        {
            public HoverData(string HoverModel = null)
            {
                if (HoverModel != null)
                    this.HoverModel = RAGE.Util.Joaat.Hash(HoverModel);
            }

            public uint HoverModel { get; set; }

            public byte[] HoverNumbers { get; set; }

            public Vector3 HoverPosition { get; set; }
            public Vector3 ObjectPosition { get; set; }
            public Vector3 Position { get; set; }

            public string DisplayName { get; set; }
        }
    }
}