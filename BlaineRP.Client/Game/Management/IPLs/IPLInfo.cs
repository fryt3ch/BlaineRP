using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;

namespace BlaineRP.Client.Game.Management.IPLs
{
    public class IPLInfo
    {
        public IPLInfo(string name, Vector3 position, float radius, uint dimension = uint.MaxValue, params string[] ipLs)
        {
            Name = name;
            Position = position;

            Colshape = new Circle(position, radius, false, new Utils.Colour(0, 0, 255, 125), dimension, null);

            Colshape.ActionType = ActionTypes.IPL;

            Colshape.Data = this;

            IPLs = ipLs;
        }

        public string Name { get; }
        public string[] IPLs { get; }
        public Vector3 Position { get; }
        public ExtraColshape Colshape { get; }

        public static void Load(Vector3 pos, params string[] ipls)
        {
            foreach (string x in ipls)
            {
                RAGE.Game.Streaming.RequestIpl(x);
            }

            if (pos != null)
            {
                int intid = RAGE.Game.Interior.GetInteriorAtCoords(pos.X, pos.Y, pos.Z);

                RAGE.Game.Interior.LoadInterior(intid);
                RAGE.Game.Interior.RefreshInterior(intid);
            }
        }

        public static void Unload(Vector3 pos, params string[] ipls)
        {
            foreach (string x in ipls)
            {
                RAGE.Game.Streaming.RemoveIpl(x);
            }

            if (pos != null)
            {
                int intid = RAGE.Game.Interior.GetInteriorAtCoords(pos.X, pos.Y, pos.Z);

                RAGE.Game.Interior.UnpinInterior(intid);
                RAGE.Game.Interior.RefreshInterior(intid);
            }
        }

        public void Load()
        {
            Load(Position, IPLs);
        }

        public void Unload()
        {
            Unload(Position, IPLs);
        }
    }
}