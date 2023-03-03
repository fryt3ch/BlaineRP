using BCRPServer.Game.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Items
{
    public class Gloves : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public Dictionary<int, int> BestTorsos { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, new Dictionary<int, int>() {{ {string.Join(", ", BestTorsos.Select(x => $"{{ {x.Key}, {x.Value} }}"))} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, Dictionary<int, int> BestTorsos, string SexAlternativeID = null) : base(Name, 0.1f, "prop_ld_tshirt_02", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.BestTorsos = BestTorsos;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "gloves_m_0", new ItemData("Перчатки вязаные", true, 51, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 51 }, { 14, 50 }, { 12, 49 }, { 11, 48 }, { 8, 47 }, { 6, 46 }, { 5, 45 }, { 4, 44 }, { 2, 43 }, { 1, 42 }, { 0, 41 }, { 184, 187 }, { 112, 117 }, { 113, 124 }, { 114, 131 }
                }, "gloves_f_0")
            },
            { "gloves_m_1", new ItemData("Перчатки без пальцев", true, 62, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 62 }, { 14, 61 }, { 12, 60 }, { 11, 59 }, { 8, 58 }, { 6, 57 }, { 5, 56 }, { 4, 55 }, { 2, 54 }, { 1, 53 }, { 0, 52 }, { 184, 188 }, { 112, 118 }, { 113, 125 }, { 114, 132 }
                }, "gloves_f_1")
            },
            { "gloves_m_2", new ItemData("Перчатки рабочего", true, 73, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 73 }, { 14, 72 }, { 12, 71 }, { 11, 70 }, { 8, 69 }, { 6, 68 }, { 5, 67 }, { 4, 66 }, { 2, 65 }, { 1, 64 }, { 0, 63 }, { 184, 189 }, { 112, 119 }, { 113, 126 }, { 114, 133 }
                }, "gloves_f_2")
            },
            { "gloves_m_3", new ItemData("Перчатки вязаные разноцветные", true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Dictionary<int, int>()
                {
                    { 15, 109 }, { 14, 108 }, { 12, 107 }, { 11, 106 }, { 8, 105 }, { 6, 104 }, { 5, 103 }, { 4, 102 }, { 2, 101 }, { 1, 100 }, { 0, 99 }
                }, "gloves_f_3")
            },
            { "gloves_m_4", new ItemData("Перчатки резиновые", true, 95, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 95 }, { 14, 94 }, { 12, 93 }, { 11, 92 }, { 8, 91 }, { 6, 90 }, { 5, 89 }, { 4, 88 }, { 2, 87 }, { 1, 86 }, { 0, 85 }, { 184, 191 }, { 112, 121 }, { 113, 128 }, { 114, 135 }
                }, "gloves_f_4")
            },
            { "gloves_m_5", new ItemData("Перчатки с вырезом", true, 29, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 29 }, { 14, 28 }, { 12, 27 }, { 11, 26 }, { 8, 25 }, { 6, 24 }, { 5, 23 }, { 4, 22 }, { 2, 21 }, { 1, 20 }, { 0, 19 }, { 184, 185 }, { 112, 115 }, { 113, 122 }, { 114, 129 }
                }, "gloves_f_5")
            },
            { "gloves_m_6", new ItemData("Перчатки из кожи", true, 40, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 40 }, { 14, 39 }, { 12, 38 }, { 11, 37 }, { 8, 36 }, { 6, 35 }, { 5, 34 }, { 4, 33 }, { 2, 32 }, { 1, 31 }, { 0, 30 }, { 184, 186 }, { 112, 116 }, { 113, 123 }, { 114, 130 }
                }, "gloves_f_6")
            },
            { "gloves_m_7", new ItemData("Перчатки по крою", true, 84, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 84 }, { 14, 83 }, { 12, 82 }, { 11, 81 }, { 8, 80 }, { 6, 79 }, { 5, 78 }, { 4, 77 }, { 2, 76 }, { 1, 75 }, { 0, 74 }, { 184, 190 }, { 112, 120 }, { 113, 127 }, { 114, 134 }
                }, "gloves_f_7")
            },
            { "gloves_m_8", new ItemData("Перчатки с протектором", true, 170, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new Dictionary<int, int>()
                {
                    { 15, 170 }, { 14, 180 }, { 12, 179 }, { 11, 178 }, { 8, 177 }, { 6, 176 }, { 5, 175 }, { 4, 174 }, { 2, 173 }, { 1, 172 }, { 0, 171 }, { 184, 194 }, { 112, 181 }, { 113, 182 }, { 114, 183 }
                }, "gloves_f_8")
            },
            #endregion

            #region ItemData Female
            { "gloves_f_0", new ItemData("Перчатки вязаные", false, 58, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 58 }, { 14, 57 }, { 12, 56 }, { 11, 55 }, { 9, 54 }, { 7, 53 }, { 6, 52 }, { 5, 51 }, { 4, 50 }, { 3, 49 }, { 2, 48 }, { 1, 47 }, { 0, 46 }, { 129, 134 }, { 130, 141 }, { 131, 148 }, { 153, 156 }, { 161, 164 }, { 229, 232 }
                }, "gloves_m_0")
            },
            { "gloves_f_1", new ItemData("Перчатки без пальцев", false, 71, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 71 }, { 14, 70 }, { 12, 69 }, { 11, 68 }, { 9, 67 }, { 7, 66 }, { 6, 65 }, { 5, 64 }, { 4, 63 }, { 3, 62 }, { 2, 61 }, { 1, 60 }, { 0, 59 }, { 129, 135 }, { 130, 142 }, { 131, 149 }, { 153, 157 }, { 161, 165 }, { 229, 233 }
                }, "gloves_m_1")
            },
            { "gloves_f_2", new ItemData("Перчатки рабочего", false, 84, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 84 }, { 14, 83 }, { 12, 82 }, { 11, 81 }, { 9, 80 }, { 7, 79 }, { 6, 78 }, { 5, 77 }, { 4, 76 }, { 3, 75 }, { 2, 74 }, { 1, 73 }, { 0, 72 }, { 129, 136 }, { 130, 143 }, { 131, 150 }, { 153, 158 }, { 161, 166 }, { 229, 234 }
                }, "gloves_m_2")
            },
            { "gloves_f_3", new ItemData("Перчатки вязаные разноцветные", false, 126, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Dictionary<int, int>()
                {
                    { 15, 126 }, { 14, 125 }, { 12, 124 }, { 11, 123 }, { 9, 122 }, { 7, 121 }, { 6, 120 }, { 5, 119 }, { 4, 118 }, { 3, 117 }, { 2, 116 }, { 1, 115 }, { 0, 114 }
                }, "gloves_m_3")
            },
            { "gloves_f_4", new ItemData("Перчатки резиновые", false, 110, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 110 }, { 14, 109 }, { 12, 108 }, { 11, 107 }, { 9, 106 }, { 7, 105 }, { 6, 104 }, { 5, 103 }, { 4, 102 }, { 3, 101 }, { 2, 100 }, { 1, 99 }, { 0, 98 }, { 129, 138 }, { 130, 145 }, { 131, 152 }, { 153, 160 }, { 161, 168 }, { 229, 236 }
                }, "gloves_m_4")
            },
            { "gloves_f_5", new ItemData("Перчатки с вырезом", false, 32, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 32 }, { 14, 31 }, { 12, 30 }, { 11, 29 }, { 9, 28 }, { 7, 27 }, { 6, 26 }, { 5, 25 }, { 4, 24 }, { 3, 23 }, { 2, 22 }, { 1, 21 }, { 0, 20 }, { 129, 132 }, { 130, 139 }, { 131, 146 }, { 153, 154 }, { 161, 162 }, { 229, 230 }
                }, "gloves_m_5")
            },
            { "gloves_f_6", new ItemData("Перчатки из кожи", false, 45, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 45 }, { 14, 44 }, { 12, 43 }, { 11, 42 }, { 9, 41 }, { 7, 40 }, { 6, 39 }, { 5, 38 }, { 4, 37 }, { 3, 36 }, { 2, 35 }, { 1, 34 }, { 0, 33 }, { 129, 133 }, { 130, 140 }, { 131, 147 }, { 153, 155 }, { 161, 163 }, { 229, 231 }
                }, "gloves_m_6")
            },
            { "gloves_f_7", new ItemData("Перчатки по крою", false, 97, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 97 }, { 14, 96 }, { 12, 95 }, { 11, 94 }, { 9, 93 }, { 7, 92 }, { 6, 91 }, { 5, 90 }, { 4, 89 }, { 3, 88 }, { 2, 87 }, { 1, 86 }, { 0, 85 }, { 129, 137 }, { 130, 144 }, { 131, 151 }, { 153, 159 }, { 161, 167 }, { 229, 235 }
                }, "gloves_m_7")
            },
            { "gloves_f_8", new ItemData("Перчатки с протектором", false, 211, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new Dictionary<int, int>()
                {
                    { 15, 211 }, { 14, 223 }, { 12, 222 }, { 11, 221 }, { 9, 220 }, { 7, 219 }, { 6, 218 }, { 5, 217 }, { 4, 216 }, { 3, 215 }, { 2, 214 }, { 1, 213 }, { 0, 212 }, { 129, 224 }, { 130, 225 }, { 131, 226 }, { 153, 227 }, { 161, 228 }, { 229, 239 }
                }, "gloves_m_8")
            },
            #endregion
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            int curTorso = player.GetClothesDrawable(3);

            int bestTorso;

            if (data.BestTorsos.TryGetValue(curTorso, out bestTorso))
                player.SetClothes(3, bestTorso, data.Textures[variation]);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            if (player.GetClothesDrawable(11) == Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Top, pData.Sex))
                player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Gloves, pData.Sex), 0);

            if (pData.Clothes[1] != null)
                pData.Clothes[1].Wear(pData);
            else
                pData.Clothes[2]?.Wear(pData);
        }

        public Gloves(string ID, int Variation) : base(ID, IDList[ID], typeof(Gloves), Variation)
        {

        }
    }
}
