using System.Collections.Generic;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.NPCs;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using RAGE;

namespace BlaineRP.Client.Game.Misc
{
    public partial class Bank
    {
        public static Dictionary<int, Bank> All = new Dictionary<int, Bank>();

        public int Id { get; set; }

        public List<NPC> Workers { get; set; }

        public ExtraBlip Blip { get; set; }

        public Bank(int Id, Utils.Vector4[] NPCs)
        {
            this.Id = Id;

            Workers = new List<NPC>();

            var posBlip = new Vector3(0f, 0f, 0f);

            for (var i = 0; i < NPCs.Length; i++)
            {
                posBlip += NPCs[i].Position;

                var npc = new NPC($"bank_{this.Id}_{i}", "", NPC.Types.Talkable, "csb_anita", NPCs[i].Position, NPCs[i].RotationZ, Settings.App.Static.MainDimension)
                {
                    SubName = "NPC_SUBNAME_BANK_WORKER", DefaultDialogueId = "bank_preprocess", Data = this,
                };

                Workers.Add(npc);
            }

            var pos = posBlip / NPCs.Length;

            Blip = new ExtraBlip(605, pos, Locale.Property.BankNameDef, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            GPS.AddPosition("money", "banks", $"bank_{Id}", $"bank& #{Id + 1}", new RAGE.Ui.Cursor.Vector2(pos.X, pos.Y));
        }
    }
}