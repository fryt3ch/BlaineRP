using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Items
{
    public class Numberplate : Item, ITagged
    {
        public static List<string> UsedTags { get; private set; } = new List<string>();

        new public class ItemData : Item.ItemData
        {
            public int Variation { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Variation}";

            public ItemData(string Name, string Model, int Number) : base(Name, 0.15f, Model)
            {
                this.Variation = Number;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "np_0", new ItemData("Номерной знак", "p_num_plate_01", 0) },
            { "np_1", new ItemData("Номерной знак", "p_num_plate_04", 1) },
            { "np_2", new ItemData("Номерной знак", "p_num_plate_02", 2) },
            { "np_3", new ItemData("Номерной знак", "p_num_plate_02", 3) },
            { "np_4", new ItemData("Номерной знак", "p_num_plate_01", 4) },
            { "np_5", new ItemData("Номерной знак", "p_num_plate_01", 5) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

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

        private static char[] Chars = new char[]
        {
            'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        /// <summary>Метод для генерации случайного номера</summary>
        /// <param name="length">Длина номера (от 1 до 8)</param>
        public string GenerateTag(int length)
        {
            if (UsedTags.Count == int.MaxValue)
                return null;

            if (length < 1 || length > 8)
                length = 8;

            Random rand = new Random();
            StringBuilder str = new StringBuilder();

            do
            {
                str.Clear();

                for (int i = 0; i < length + 1; i++)
                    str.Append(Chars[rand.Next(0, Chars.Length - 1)]);
            }
            while (UsedTags.Contains(str.ToString()));

            var retStr = str.ToString();

            UsedTags.Add(retStr);

            return retStr;
        }

        public Numberplate(string ID) : base(ID, IDList[ID], typeof(Numberplate))
        {

        }
    }
}
