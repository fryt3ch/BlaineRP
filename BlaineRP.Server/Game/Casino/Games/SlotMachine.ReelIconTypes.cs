namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class SlotMachine
    {
        public enum ReelIconTypes : byte
        {
            Seven = 0,
            Grape = 1,
            Watermelon = 2,
            Microphone = 3,
            Bell = 5,
            Cherry = 6,
            Superstar = 13,

            Loose = 255,
        }
    }
}