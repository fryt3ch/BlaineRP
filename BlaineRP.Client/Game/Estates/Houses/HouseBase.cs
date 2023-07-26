using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using RAGE;

namespace BlaineRP.Client.Game.Estates
{
    public abstract class HouseBase
    {
        public enum ClassTypes
        {
            A = 0,
            B,
            C,
            D,

            FA,
            FB,
            FC,
            FD,
        }

        public Core.HouseTypes Type { get; set; }

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

        public HouseBase(Core.HouseTypes type, uint id, uint price, int roomType, int @class, uint tax)
        {
            Type = type;

            Id = id;
            Price = price;
            RoomType = (Core.Style.RoomTypes)roomType;
            Class = (ClassTypes)@class;
            Tax = tax;
        }

        public abstract void ToggleOwnerBlip(bool state);

        public abstract void UpdateOwnerName(string name);
    }
}