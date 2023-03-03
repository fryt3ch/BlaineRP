using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data
{
    public partial class Locations : Events.Script
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

            public Blip OwnerBlip { get => Player.LocalPlayer.GetData<Blip>($"Business::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Business::{Id}::OBlip"); Player.LocalPlayer.SetData($"Business::{Id}::OBlip", value); } }

            public Blip Blip { get; set; }

            public TextLabel InfoText { get; set; }

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
                    InfoColshape = new Additional.Cylinder(PositionInfo, 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null)
                    {
                        ActionType = Additional.ExtraColshape.ActionTypes.BusinessInfo,
                        InteractionType = Additional.ExtraColshape.InteractionTypes.BusinessInfo,

                        Data = this,
                    };

                    InfoText = new TextLabel(new Vector3(PositionInfo.X, PositionInfo.Y, PositionInfo.Z + 0.5f), $"{Name} #{SubId}", new RGBA(255, 255, 255, 255), 15f, 0, false, Settings.MAIN_DIMENSION) { Font = 0 };
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
                    Blip.SetDisplay(0);

                    var oBlip = OwnerBlip;

                    oBlip?.Destroy();

                    OwnerBlip = new Blip(207, InfoColshape.Position, Name, 1f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);
                }
                else
                {
                    Blip.SetDisplay(2);

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
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public ClothesShop1(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.ClothesShop1, Price, Rent, Tax)
            {
                this.Blip = new Blip(73, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class ClothesShop2 : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public ClothesShop2(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.ClothesShop2, Price, Rent, Tax)
            {
                this.Blip = new Blip(366, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class ClothesShop3 : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public ClothesShop3(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.ClothesShop3, Price, Rent, Tax)
            {
                this.Blip = new Blip(439, PositionInteract.Position, Name, 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
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
                this.Blip = new Blip(377, PositionInteract.Position, Name, 1f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"vendor_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_bags_preprocess",
                };
            }
        }

        public class MaskShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("a_m_y_jetski_01", "Джулиан"),
            };

            public MaskShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.MaskShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(362, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"vendor_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }


        public class JewelleryShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public JewelleryShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.JewelleryShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(617, PositionInteract.Position, Name, 1f, 1, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class TattooShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("u_m_y_tattoo_01", "Рикардо"),
                ("u_m_y_tattoo_01", "Сантьяго"),
            };

            public TattooShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.TattooShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(75, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"tatseller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class BarberShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public BarberShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.BarberShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(71, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class Market : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public Market(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.Market, Price, Rent, Tax)
            {
                this.Blip = new Blip(52, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class GasStation : Business
        {
            public GasStation(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Vector3 PositionGas, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.GasStation, Price, Rent, Tax)
            {
                this.Blip = new Blip(361, PositionGas, Name, 0.75f, 47, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var cs = new Additional.Sphere(PositionGas, 5f, false, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null)
                {
                    Data = this.Id,

                    ActionType = Additional.ExtraColshape.ActionTypes.GasStation,

                    ApproveType = Additional.ExtraColshape.ApproveTypes.None,
                };

                //this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Seller, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                //this.Seller.Data = this;
            }
        }

        public class CarShop1 : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public CarShop1(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.CarShop1, Price, Rent, Tax)
            {
                this.Blip = new Blip(225, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class CarShop2 : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public CarShop2(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.CarShop2, Price, Rent, Tax)
            {
                this.Blip = new Blip(530, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class CarShop3 : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public CarShop3(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.CarShop3, Price, Rent, Tax)
            {
                this.Blip = new Blip(523, PositionInteract.Position, Name, 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class MotoShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public MotoShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.MotoShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(522, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class BoatShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public BoatShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.BoatShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(410, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class AeroShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public AeroShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.AeroShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(602, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
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
                this.Blip = new Blip(72, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.EnteranceColshape = new Additional.Sphere(PositionInteract.Position, 2.5f, false, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null)
                {
                    ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                    InteractionType = Additional.ExtraColshape.InteractionTypes.TuningEnter,
                    ActionType = Additional.ExtraColshape.ActionTypes.TuningEnter,

                    Data = this,
                };

                new Marker(44, PositionInteract.Position, 1f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new RGBA(255, 255, 255), true, Settings.MAIN_DIMENSION);
            }
        }

        public class WeaponShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public static uint ShootingRangePrice => (uint)Sync.World.GetSharedData<int>("SRange::Price", 0);

            public WeaponShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract, Vector3 ShootingRangePosition) : base(Id, PositionInfo, Types.WeaponShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(110, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };

                var shootingRangeEnterCs = new Additional.Cylinder(ShootingRangePosition, 1.5f, 2f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                {
                    Data = this,

                    ActionType = Additional.ExtraColshape.ActionTypes.ShootingRangeEnter,
                    InteractionType = Additional.ExtraColshape.InteractionTypes.ShootingRangeEnter,
                };

                var shootingRangeText = new TextLabel(new Vector3(ShootingRangePosition.X, ShootingRangePosition.Y, ShootingRangePosition.Z + 0.5f), Locale.General.Business.ShootingRangeTitle, new RGBA(255, 255, 255, 255), 10f, 0, true, Settings.MAIN_DIMENSION);
            }
        }

        public class FurnitureShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public FurnitureShop(int Id, Vector3 PositionInfo, uint Price, uint Rent, float Tax, Utils.Vector4 PositionInteract, List<Vector3> PositionsInteract) : base(Id, PositionInfo, Types.FurnitureShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(779, PositionInteract.Position, Name, 1f, 8, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_furn_g_0",
                };
            }
        }
    }
}
