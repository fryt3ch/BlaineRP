using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public abstract class ClothesShop : Shop, IEnterable
    {
        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}";

        public Vector4 EnterProperties { get; set; }

        public Vector4[] ExitProperties { get; set; }

        public int LastExitUsed { get; set; }

        public ClothesShop(int ID, Vector3 Position, Vector4 EnterProperies, BusinessType Type, Vector4 PositionInteract) : base(ID, Position, PositionInteract, Type)
        {
            this.EnterProperties = EnterProperies;

            this.ExitProperties = new Vector4[] { new Vector4(PositionInteract.Position.GetFrontOf(PositionInteract.RotationZ, 1.5f), Utils.GetOppositeAngle(PositionInteract.RotationZ)) };
        }
    }
}
