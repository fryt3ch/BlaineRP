using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Server.Game.Items
{
    public class Armour : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            /// <summary>ID текстуры разных цветов</summary>
            /// <remarks>Работает только для Drawable 28!</remarks>
            public enum Colours
            {
                Green = 0,
                Orange = 1,
                Purple = 2,
                Pink = 3,
                Red = 4,
                Blue = 5,
                Grey = 6,
                LightGrey = 7,
                White = 8,
                Black = 9
            }

            public int MaxStrength { get; set; }

            public int DrawableTop { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {MaxStrength}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, int DrawableTop, int MaxStrength, string SexAlternativeID = null) : base(Name, Weight, "prop_armour_pickup", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.DrawableTop = DrawableTop;

                this.MaxStrength = MaxStrength;
            }

            public ItemData(string Name, float Weight, bool Sex, int Drawable, Colours[] Colours, int DrawableTop, int MaxStrength, string SexAlternativeID = null) : this(Name, Weight, Sex, Drawable, Colours.Select(x => (int)x).ToArray(), DrawableTop, MaxStrength, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "arm_m_s", new ItemData("Обычный бронежилет", 0.5f, true, 28, new ItemData.Colours[] { ItemData.Colours.White }, 19, 100, "arm_m_s") },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; }

        public int Strength { get; set; }

        /// <summary>Метод для надевания брони на игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            var variation = Var;

            player.SetArmour(Strength);

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetClothes(Slot, pData.Clothes[1] == null ? data.Drawable : data.DrawableTop, data.Textures[variation]);
        }

        /// <summary>Метод для снятия брони с игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            var value = player.Armor;

            if (value < 0)
                value = 0;

            if (value < Strength)
            {
                Strength = value;

                this.Update();
            }

            player.SetClothes(Slot, 0, 0);

            player.SetArmour(0);
        }

        public Armour(string ID, int Var = 0) : base(ID, IDList[ID], typeof(Armour), Var)
        {
            this.Strength = Data.MaxStrength;
        }
    }
}
