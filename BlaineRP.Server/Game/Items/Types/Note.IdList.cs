using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Note
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "note_0", new ItemData("Записка", "xm_prop_x17_note_paper_01a", ItemData.Types.Read | ItemData.Types.WriteTextNullOnly, null) },
            { "note_1", new ItemData("Вечная записка", "xm_prop_x17_note_paper_01a", ItemData.Types.Read | ItemData.Types.Write, null) },

            //{ "note_q_j123", new ItemData("Вечная записка", "xm_prop_x17_note_paper_01a", ItemData.Types.Read | ItemData.Types.DefaultTextIsStatic, "Статичный текст, который не хранится в БД") },
        };
    }
}