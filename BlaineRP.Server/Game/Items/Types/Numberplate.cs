using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Items
{
    public partial class Numberplate : Item, ITaggedFull
    {
        public static HashSet<string>[] UsedTags { get; private set; } = new HashSet<string>[]
        {
            new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(),
        };

        public new class ItemData : Item.ItemData
        {
            public int Variation { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Variation}";

            public ItemData(string name, string model, int number) : base(name, 0.15f, model)
            {
                Variation = number;
            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        public string Tag { get; set; }

        public void Setup(VehicleData vData)
        {
            var veh = vData.Vehicle;

            veh.NumberPlateStyle = Data.Variation;
            veh.NumberPlate = Tag;
        }

        public void Take(VehicleData vData)
        {
            var veh = vData.Vehicle;

            veh.NumberPlateStyle = 0;
            veh.NumberPlate = "";
        }

        private static char[] _chars = new char[]
        {
            'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        /// <summary>Метод для генерации случайного номера</summary>
        /// <param name="length">Длина номера (от 1 до 8)</param>
        public static string GenerateTag(byte length)
        {
            if (length < 1 || length > 8)
                length = 8;

            var strBuilder = new StringBuilder();

            var hashSet = UsedTags[length - 1];

            if (hashSet.Count >= Math.Pow(_chars.Length, length))
                return null;

            while (true)
            {
                for (int i = 0; i < length; i++)
                    strBuilder.Append(_chars[SRandom.NextInt32(0, _chars.Length)]);

                var retStr = strBuilder.ToString();

                if (!hashSet.Contains(retStr))
                {
                    return retStr;
                }

                strBuilder.Clear();
            }
        }

        public void RemoveTagFromUsed()
        {
            if (Tag == null || Tag.Length < 1 || Tag.Length > 8)
                return;

            UsedTags[Tag.Length - 1].Remove(Tag);
        }

        public void AddTagToUsed()
        {
            if (Tag == null || Tag.Length < 1 || Tag.Length > 8)
                return;

            UsedTags[Tag.Length - 1].Add(Tag);
        }

        public Numberplate(string id) : base(id, IdList[id], typeof(Numberplate))
        {

        }
    }
}
