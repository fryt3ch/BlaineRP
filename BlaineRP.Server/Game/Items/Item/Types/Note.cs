using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlaineRP.Server.Game.Items
{
    public class Note : Item
    {
        new public class ItemData : Item.ItemData
        {
            [Flags]
            public enum Types : byte
            {
                None = 0,

                Read = 1 << 0,

                Write = 1 << 1,

                WriteTextNullOnly = 1 << 2,

                DefaultTextIsStatic = 1 << 3,
            }

            public override string ClientData => $"\"{Name}\", {Weight}f";

            public Types Type { get; set; }

            public string DefaultText { get; set; }

            public ItemData(string Name, string Model, Types Type, string DefaultText) : base(Name, 0.01f, Model)
            {
                this.Type = Type;

                this.DefaultText = DefaultText;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "note_0", new ItemData("Записка", "xm_prop_x17_note_paper_01a", ItemData.Types.Read | ItemData.Types.WriteTextNullOnly, null) },
            { "note_1", new ItemData("Вечная записка", "xm_prop_x17_note_paper_01a", ItemData.Types.Read | ItemData.Types.Write, null) },

            //{ "note_q_j123", new ItemData("Вечная записка", "xm_prop_x17_note_paper_01a", ItemData.Types.Read | ItemData.Types.DefaultTextIsStatic, "Статичный текст, который не хранится в БД") },
        };

        [JsonIgnore]
        new public ItemData Data => (ItemData)base.Data;

        [JsonProperty(PropertyName = "T")]
        public string Text { get; set; }

        public Note(string ID) : base(ID, IDList[ID], typeof(Note))
        {
            var data = Data;

            if ((data.Type & ItemData.Types.DefaultTextIsStatic) != ItemData.Types.DefaultTextIsStatic)
            {
                Text = data.DefaultText;
            }
        }
    }
}