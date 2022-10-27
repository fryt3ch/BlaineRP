using RAGE;
using RAGE.Elements;
using RAGE.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace BCRPClient.Data
{
    class Locations : Events.Script
    {
        public abstract class Business
        {
            public static Dictionary<int, Business> All = new Dictionary<int, Business>();

            public enum Types
            {
                ClothesShop1 = 0,
                ClothesShop2,
                ClothesShop3,
            }

            public Types Type { get; set; }

            public int Id { get; set; }

            public int SubId { get; set; }

            public Blip Blip { get; set; }

            public TextLabel InfoText { get; set; }

            public Additional.ExtraColshape InfoColshape { get; set; }

            public Additional.ExtraColshape AdditionalColshape { get; set; }

            public NPC Seller { get; set; }

            private string _Name { get; set; }

            public string Name
            {
                get => _Name;

                set
                {
                    _Name = value;

                    InfoText.Text = string.Format(Locale.General.Business.InfoColshape, value, SubId);
                    Blip.SetName(value);
                }
            }

            public Business(int Id, Vector3 PositionInfo, Types Type)
            {
                this.Type = Type;

                this.Id = Id;

                InfoColshape = new Additional.Cylinder(PositionInfo, 1f, 1.5f, false, new Utils.ExtraColour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null);

                InfoColshape.ActionType = Additional.ExtraColshape.ActionTypes.BusinessInfo;
                InfoColshape.InteractionType = Additional.ExtraColshape.InteractionTypes.BusinessInfo;

                InfoColshape.Data = Id;

                InfoText = new TextLabel(new Vector3(PositionInfo.X, PositionInfo.Y, PositionInfo.Z + 0.5f), "", new RGBA(255, 255, 255, 255), 25f, 0, false, Settings.MAIN_DIMENSION) { Font = 0 };

                All.Add(Id, this);
            }

            public static async void LoadNames(Dictionary<int, string> dict)
            {
                while (!Sync.Players.CharacterLoaded)
                    await RAGE.Game.Invoker.WaitAsync(1000);

                foreach (var x in dict)
                    All[x.Key].Name = x.Value;
            }
        }

        public class ClothesShop1 : Business
        {
            private static int Counter = 1;

            public ClothesShop1(int Id, Vector3 Position, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.ClothesShop1)
            {
                this.Blip = new Blip(73, Position, "", 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Seller, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class ClothesShop2 : Business
        {
            private static int Counter = 1;

            public ClothesShop2(int Id, Vector3 Position, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.ClothesShop2)
            {
                this.Blip = new Blip(366, Position, "", 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Seller, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class ClothesShop3 : Business
        {
            private static int Counter = 1;

            public ClothesShop3(int Id, Vector3 Position, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.ClothesShop3)
            {
                this.Blip = new Blip(439, Position, "", 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Seller, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public Locations()
        {
            #region Clothes (Cheap)
            new ClothesShop1(1, new Vector3(1198f, 2701f, 38f), new Vector3(1200f, 2701f, 37f), new Vector3(1201.885f, 2710.143f, 38.2226f), 105f, "Лана", "csb_anita");
            new ClothesShop1(3, new Vector3(-1093.5f, 2703.7f, 19f), new Vector3(-1096f, 2702.9f, 18f), new Vector3(-1097.523f, 2714.026f, 19.108f), 150f, "Лана", "csb_anita");
            new ClothesShop1(4, new Vector3(1685.5f, 4820.2f, 42f), new Vector3(1683.5f, 4822.3f, 41f), new Vector3(1694.727f, 4817.582f, 42.06f), 20f, "Лана", "csb_anita");
            new ClothesShop1(5, new Vector3(-1.5f, 6517.2f, 31.2f), new Vector3(-0.26f, 6519.46f, 30.5f), new Vector3(1f, 6508.753f, 31.87f), 325, "Лана", "csb_anita");

            new ClothesShop1(11, new Vector3(-817.3f, -1079.856f, 11.133f), new Vector3(-815.1346f, -1079.327f, 10.13754f), new Vector3(-817.8808f, -1070.944f, 11.32811f), 135f, "Лана", "csb_anita");
            new ClothesShop1(12, new Vector3(83.64771f, -1391.713f, 29.41865f), new Vector3(83.63f, -1389.665f, 28.4166f), new Vector3(75.42346f, -1387.689f, 29.37614f), 195.8f, "Лана", "csb_anita");
            new ClothesShop1(13, new Vector3(416.7564f, -807.4344f, 29.38187f), new Vector3(416.326f, -805.2744f, 28.37296f), new Vector3(425.6321f, -811.4822f, 29.49114f), 11f, "Лана", "csb_anita");
            #endregion

            #region Clothes (Expensive)
            new ClothesShop2(2, new Vector3(618.5f, 2747.7f, 42f), new Vector3(622f, 2744.5f, 41f), new Vector3(613.035f, 2761.843f, 42.088f), 265f, "Лана", "csb_anita");
            new ClothesShop2(14, new Vector3(-3167.542f, 1057.887f, 20.85858f), new Vector3(-3164.073f, 1059.789f, 19.84639f), new Vector3(-3169.008f, 1044.211f, 20.86322f), 48.6f, "Лана", "csb_anita");

            new ClothesShop2(9, new Vector3(128.3956f, -207.6191f, 54.58f), new Vector3(127.03f, -205.91f, 53.55547f), new Vector3(127.3073f, -223.18f, 54.55783f), 66f, "Лана", "csb_anita");
            new ClothesShop2(10, new Vector3(-1202.328f, -778.6373f, 17.33572f), new Vector3(-1203.283f, -781.6449f, 16.3305f), new Vector3(-1194.725f, -767.6141f, 17.31629f), 208f, "Лана", "csb_anita");

            #endregion

            #region Clothes (Brand)
            new ClothesShop3(6, new Vector3(-1456f, -232f, 49.5f), new Vector3(-1455.7f, -228.9f, 48.25f), new Vector3(-1448.824f, -237.893f, 49.81332f), 45f, "Лана", "csb_anita");
            new ClothesShop3(7, new Vector3(-718.46f, -157.63f, 37f), new Vector3(-718f, -160f, 36f), new Vector3(-708.95f, -151.6612f, 37.415f), 114f, "Лана", "csb_anita");
            new ClothesShop3(8, new Vector3(-155.5432f, -305.705f, 39.08f), new Vector3(-152.62f, -304f, 37.91f), new Vector3(-165f, -303.2f, 39.73328f), 251f, "Лана", "csb_anita");
            #endregion
        }
    }
}
