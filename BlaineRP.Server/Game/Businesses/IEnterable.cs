using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Businesses
{
    public interface IEnterable
    {
        public Vector4 EnterProperties { get; set; }

        public Vector4[] ExitProperties { get; set; }

        public int LastExitUsed { get; set; }
    }
}