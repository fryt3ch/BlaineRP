using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Note : Item
    {
        public new class ItemData : Item.ItemData
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

            public ItemData(string name, string model, Types type, string defaultText) : base(name, 0.01f, model)
            {
                Type = type;

                DefaultText = defaultText;
            }
        }

        [JsonIgnore]
        public new ItemData Data => (ItemData)base.Data;

        [JsonProperty(PropertyName = "T")]
        public string Text { get; set; }

        public Note(string id) : base(id, IdList[id], typeof(Note))
        {
            var data = Data;

            if ((data.Type & ItemData.Types.DefaultTextIsStatic) != ItemData.Types.DefaultTextIsStatic)
            {
                Text = data.DefaultText;
            }
        }
    }
}