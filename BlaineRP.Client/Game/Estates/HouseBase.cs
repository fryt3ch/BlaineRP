using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using RAGE;

namespace BlaineRP.Client.Game.Estates
{
    public abstract partial class HouseBase
    {
        public HouseBase(HouseBase.Types type, uint id, uint price, int roomType, int @class, uint tax)
        {
            Type = type;

            Id = id;
            Price = price;
            RoomType = (Core.Style.RoomTypes)roomType;
            Class = (ClassTypes)@class;
            Tax = tax;
        }

        public HouseBase.Types Type { get; set; }

        public uint Id { get; set; }

        public uint Price { get; set; }

        public uint Tax { get; set; }

        public ClassTypes Class { get; set; }

        public abstract string OwnerName { get; }

        public abstract Vector3 Position { get; }

        public Core.Style.RoomTypes RoomType { get; set; }

        public ExtraColshape Colshape { get; set; }

        public ExtraLabel InfoText { get; set; }

        public abstract ExtraBlip OwnerBlip { get; set; }

        public abstract void ToggleOwnerBlip(bool state);

        public abstract void UpdateOwnerName(string name);
    }
}