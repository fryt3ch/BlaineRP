using System.Collections.Generic;
using BlaineRP.Client.Game.Helpers.Blips;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Estates
{
    public class Apartments : HouseBase
    {
        public static Dictionary<uint, Apartments> All = new Dictionary<uint, Apartments>();

        public Apartments(uint id, uint rootId, ushort floorIdx, ushort subIdx, int roomType, uint price, int @class, uint tax) : base(Core.HouseTypes.Apartments,
            id,
            price,
            roomType,
            @class,
            tax
        )
        {
            RootId = rootId;

            All.Add(id, this);

            NumberInRoot = ApartmentsRoot.All[rootId].Shell.GetApartmentsIdx(floorIdx, subIdx);
        }

        public override string OwnerName => World.Core.GetSharedData<string>($"Apartments::{Id}::OName");

        public override ExtraBlip OwnerBlip
        {
            get => Player.LocalPlayer.GetData<ExtraBlip>($"Apartments::{Id}::OBlip");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData($"Apartments::{Id}::OBlip");
                else
                    Player.LocalPlayer.SetData($"Apartments::{Id}::OBlip", value);
            }
        }

        public uint RootId { get; }

        public ushort FloorIdx { get; set; }

        public ushort SubIdx { get; set; }

        public int NumberInRoot { get; }

        public override Vector3 Position => ApartmentsRoot.All[RootId].Shell.GetApartmentsPosition(FloorIdx, SubIdx);

        public override void ToggleOwnerBlip(bool state)
        {
            ExtraBlip oBlip = OwnerBlip;

            oBlip?.Destroy();

            ApartmentsRoot aRoot = ApartmentsRoot.All[RootId];

            if (state)
            {
                ApartmentsRoot curARoot = Player.LocalPlayer.GetData<ApartmentsRoot>("ApartmentsRoot::Current");

                if (curARoot == null)
                    //aRoot.Blip.SetDisplay(0);
                    OwnerBlip = new ExtraBlip(475,
                        aRoot.PositionEnter,
                        string.Format(Locale.General.Blip.ApartmentsOwnedBlip, aRoot.Name, NumberInRoot + 1),
                        1f,
                        5,
                        255,
                        0f,
                        false,
                        0,
                        0f,
                        Settings.App.Static.MainDimension
                    );
                else if (curARoot.Id == aRoot.Id)
                    //aRoot.Blip.SetDisplay(2);
                    OwnerBlip = new ExtraBlip(475,
                        Position,
                        string.Format(Locale.General.Blip.ApartmentsOwnedBlip, aRoot.Name, NumberInRoot + 1),
                        1f,
                        5,
                        255,
                        0f,
                        false,
                        0,
                        0f,
                        Player.LocalPlayer.Dimension
                    );
            }
            else
            {
                //aRoot.Blip.SetDisplay(2);

                OwnerBlip = null;
            }
        }

        public override void UpdateOwnerName(string name)
        {
            ApartmentsRoot root = ApartmentsRoot.All[RootId];

            root.UpdateTextLabel();

            if (InfoText != null)
                InfoText.Text = string.Format(Locale.Property.ApartmentsTextLabel, NumberInRoot + 1, name ?? Locale.Property.NoOwner);
        }
    }
}