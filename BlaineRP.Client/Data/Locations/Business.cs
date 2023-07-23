using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Data
{
    public partial class Locations
    {
        public abstract class Business
        {
            public static Dictionary<int, Business> All = new Dictionary<int, Business>();

            public enum Types
            {
                ClothesShop1 = 0,
                ClothesShop2,
                ClothesShop3,

                JewelleryShop,

                MaskShop,

                BagShop,

                BarberShop,

                TattooShop,

                CarShop1,
                CarShop2,
                CarShop3,

                MotoShop,

                BoatShop,

                AeroShop,

                FurnitureShop,

                Market,

                GasStation,

                TuningShop,

                WeaponShop,

                Farm,
            }

            public Types Type { get; set; }

            public int Id { get; set; }

            public int SubId { get; set; }

            public string OwnerName => Sync.World.GetSharedData<string>($"Business::{Id}::OName");

            public Additional.ExtraBlip OwnerBlip { get => Player.LocalPlayer.GetData<Additional.ExtraBlip>($"Business::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Business::{Id}::OBlip"); Player.LocalPlayer.SetData($"Business::{Id}::OBlip", value); } }

            public Additional.ExtraBlip Blip { get; set; }

            public Additional.ExtraLabel InfoText { get; set; }

            public Additional.ExtraColshape InfoColshape { get; set; }

            public NPC Seller { get; set; }

            public string Name => Locale.Property.BusinessNames.GetValueOrDefault(Type) ?? "null";

            public uint Price { get; set; }

            public uint Rent { get; set; }

            public float Tax { get; set; }

            public Business(int Id, Vector3 PositionInfo, Types Type, uint Price, uint Rent, float Tax)
            {
                this.Type = Type;

                this.Id = Id;

                this.SubId = All.Where(x => x.Value.Type == Type).Count() + 1;

                this.Price = Price;
                this.Rent = Rent;
                this.Tax = Tax;

                if (PositionInfo != null)
                {
                    InfoColshape = new Additional.Cylinder(new Vector3(PositionInfo.X, PositionInfo.Y, PositionInfo.Z - 1f), 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Settings.App.Static.MainDimension, null)
                    {
                        ActionType = Additional.ExtraColshape.ActionTypes.BusinessInfo,
                        InteractionType = Additional.ExtraColshape.InteractionTypes.BusinessInfo,

                        Data = this,
                    };

                    InfoText = new Additional.ExtraLabel(new Vector3(PositionInfo.X, PositionInfo.Y, PositionInfo.Z - 0.5f), $"{Name} #{SubId}", new RGBA(255, 255, 255, 255), 15f, 0, false, Settings.App.Static.MainDimension) { Font = 0 };
                }

                All.Add(Id, this);
            }

            public void UpdateOwnerName(string name)
            {
                if (name == null)
                    name = Locale.Property.NoOwner;
            }

            public void ToggleOwnerBlip(bool state)
            {
                if (state)
                {
                    Blip.Display = 0;

                    var oBlip = OwnerBlip;

                    oBlip?.Destroy();

                    OwnerBlip = new Additional.ExtraBlip(207, InfoColshape.Position, Name, 1f, 5, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension);
                }
                else
                {
                    Blip.Display = 2;

                    var oBlip = OwnerBlip;

                    if (oBlip != null)
                    {
                        oBlip.Destroy();

                        OwnerBlip = null;
                    }
                }
            }
        }

        public class ClothesShop1 : Business
        {
            public ClothesShop1(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.ClothesShop1, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(73, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "s_f_y_shop_low", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };

                CEF.PhoneApps.GPSApp.AddPosition("clothes", "clothes1", $"clothes_{Id}", $"clothess& #{SubId}", new RAGE.Ui.Cursor.Vector2(PositionInteract.X, PositionInteract.Y));
            }
        }

        public class ClothesShop2 : Business
        {
            public ClothesShop2(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.ClothesShop2, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(366, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "s_f_y_shop_mid", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };

                CEF.PhoneApps.GPSApp.AddPosition("clothes", "clothes2", $"clothes_{Id}", $"clothess& #{SubId}", new RAGE.Ui.Cursor.Vector2(PositionInteract.X, PositionInteract.Y));
            }
        }

        public class ClothesShop3 : Business
        {
            public ClothesShop3(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.ClothesShop3, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(439, PositionInteract.Position, Name, 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "s_f_m_shop_high", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };

                CEF.PhoneApps.GPSApp.AddPosition("clothes", "clothes3", $"clothes_{Id}", $"clothess& #{SubId}", new RAGE.Ui.Cursor.Vector2(PositionInteract.X, PositionInteract.Y));
            }
        }

        public class BagShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("a_m_o_ktown_01", "Чжан"),
            };

            public BagShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.BagShop, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(377, PositionInteract.Position, Name, 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"vendor_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_VENDOR",

                    Data = this,

                    DefaultDialogueId = "seller_bags_preprocess",
                };

                CEF.PhoneApps.GPSApp.AddPosition("clothes", "clothesother", $"clothes_{Id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(PositionInteract.X, PositionInteract.Y));
            }
        }

        public class MaskShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("a_m_y_jetski_01", "Джулиан"), // s_m_y_shop_mask
            };

            public MaskShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.MaskShop, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(362, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"vendor_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_VENDOR",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };

                CEF.PhoneApps.GPSApp.AddPosition("clothes", "clothesother", $"clothes_{Id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(PositionInteract.X, PositionInteract.Y));
            }
        }


        public class JewelleryShop : Business
        {
            public JewelleryShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.JewelleryShop, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(617, PositionInteract.Position, Name, 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "csb_anita", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };

                CEF.PhoneApps.GPSApp.AddPosition("clothes", "clothesother", $"clothes_{Id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(PositionInteract.X, PositionInteract.Y));
            }
        }

        public class TattooShop : Business
        {
            public TattooShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.TattooShop, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(75, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"tatseller_{Id}", "", NPC.Types.Talkable, "u_m_y_tattoo_01", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER_TATTOO",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class BarberShop : Business
        {
            public BarberShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.BarberShop, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(71, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "s_f_y_clubbar_01", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER_BARBER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class Market : Business
        {
            public Market(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.Market, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(52, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "mp_m_shopkeep_01", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };

                CEF.PhoneApps.GPSApp.AddPosition("bizother", "market", $"bizother_{Id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(PositionInteract.X, PositionInteract.Y));
            }
        }

        public class GasStation : Business
        {
            public GasStation(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionGas, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.GasStation, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(361, PositionGas.Position, Name, 0.75f, 47, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                var cs = new Additional.Cylinder(new Vector3(PositionGas.X, PositionGas.Y, PositionGas.Z - 1f), PositionGas.RotationZ, 2.5f, false, new Utils.Colour(255, 0, 0, 125), Settings.App.Static.MainDimension, null)
                {
                    Data = this.Id,

                    ActionType = Additional.ExtraColshape.ActionTypes.GasStation,

                    ApproveType = Additional.ExtraColshape.ApproveTypes.None,
                };

                //this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Seller, ModelPed, PositionPed, HeadingPed, Settings.App.Static.MainDimension, "seller_clothes_greeting_0");

                //this.Seller.Data = this;

                CEF.PhoneApps.GPSApp.AddPosition("bizother", "gas", $"bizother_{Id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(PositionGas.X, PositionGas.Y));
            }
        }

        public class CarShop1 : Business
        {
            public CarShop1(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.CarShop1, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(225, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "ig_mrk", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class CarShop2 : Business
        {
            public CarShop2(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.CarShop2, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(530, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "ig_agatha", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class CarShop3 : Business
        {
            public CarShop3(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.CarShop3, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(523, PositionInteract.Position, Name, 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "csb_anita", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class MotoShop : Business
        {
            public MotoShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.MotoShop, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(522, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "u_m_y_sbike", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class BoatShop : Business
        {
            public BoatShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.BoatShop, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(410, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "ig_djsolrobt", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class AeroShop : Business
        {
            public AeroShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.AeroShop, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(602, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "ig_jewelass", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class TuningShop : Business
        {
            public Additional.ExtraColshape EnteranceColshape { get; set; }

            public TuningShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.TuningShop, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(72, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                var tPos = new Vector3(PositionInteract.Position.X, PositionInteract.Position.Y, PositionInteract.Position.Z - 0.5f);

                this.EnteranceColshape = new Additional.Cylinder(tPos, 2.5f, 2f, false, new Utils.Colour(255, 0, 0, 125), Settings.App.Static.MainDimension, null)
                {
                    ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                    InteractionType = Additional.ExtraColshape.InteractionTypes.TuningEnter,
                    ActionType = Additional.ExtraColshape.ActionTypes.TuningEnter,

                    Data = this,
                };

                new Marker(44, new Vector3(tPos.X, tPos.Y, tPos.Z + 0.75f), 1f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new RGBA(255, 255, 255), true, Settings.App.Static.MainDimension);

                CEF.PhoneApps.GPSApp.AddPosition("bizother", "tuning", $"bizother_{Id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(PositionInteract.X, PositionInteract.Y));
            }
        }

        public class WeaponShop : Business
        {
            public static uint ShootingRangePrice => Convert.ToUInt32(Sync.World.GetSharedData<object>("SRange::Price", 0));

            public WeaponShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract, Vector3 ShootingRangePosition) : base(Id, PositionInfo, Types.WeaponShop, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(110, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "s_m_y_ammucity_01", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };

                var tPos = new Vector3(ShootingRangePosition.X, ShootingRangePosition.Y, ShootingRangePosition.Z - 1f);

                var shootingRangeEnterCs = new Additional.Cylinder(tPos, 1.5f, 2f, false, Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    Data = this,

                    ActionType = Additional.ExtraColshape.ActionTypes.ShootingRangeEnter,
                    InteractionType = Additional.ExtraColshape.InteractionTypes.ShootingRangeEnter,
                };

                var shootingRangeText = new Additional.ExtraLabel(new Vector3(tPos.X, tPos.Y, tPos.Z + 0.75f), Locale.Get("SHOP_WEAPON_SRANGE_L"), new RGBA(255, 255, 255, 255), 10f, 0, true, Settings.App.Static.MainDimension);

                CEF.PhoneApps.GPSApp.AddPosition("bizother", "weapon", $"bizother_{Id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(PositionInteract.X, PositionInteract.Y));
            }
        }

        public class FurnitureShop : Business
        {
            public FurnitureShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.FurnitureShop, Price, Rent, Tax)
            {
                this.Blip = new Additional.ExtraBlip(779, PositionInteract.Position, Name, 1f, 8, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.Seller = new NPC($"seller_{Id}", "", NPC.Types.Talkable, "ig_natalia", PositionInteract.Position, PositionInteract.RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_SELLER",

                    Data = this,

                    DefaultDialogueId = "seller_furn_g_0",
                };

                CEF.PhoneApps.GPSApp.AddPosition("bizother", "furn", $"bizother_{Id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(PositionInteract.X, PositionInteract.Y));
            }
        }
    }
}
