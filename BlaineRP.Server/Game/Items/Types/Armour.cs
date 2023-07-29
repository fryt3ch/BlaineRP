using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public partial class Armour : Clothes
    {
        public new class ItemData : Clothes.ItemData
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

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {MaxStrength}, {(SexAlternativeId == null ? "null" : $"\"{SexAlternativeId}\"")}";

            public ItemData(string name, float weight, bool sex, int drawable, int[] textures, int drawableTop, int maxStrength, string sexAlternativeId = null) : base(name, weight, "prop_armour_pickup", sex, drawable, textures, sexAlternativeId)
            {
                DrawableTop = drawableTop;

                MaxStrength = maxStrength;
            }

            public ItemData(string name, float weight, bool sex, int drawable, Colours[] colours, int drawableTop, int maxStrength, string sexAlternativeId = null) : this(name, weight, sex, drawable, colours.Select(x => (int)x).ToArray(), drawableTop, maxStrength, sexAlternativeId) { }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public new ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; }

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

                Update();
            }

            player.SetClothes(Slot, 0, 0);

            player.SetArmour(0);
        }

        public Armour(string id, int var = 0) : base(id, IdList[id], typeof(Armour), var)
        {
            Strength = Data.MaxStrength;
        }
    }
}
