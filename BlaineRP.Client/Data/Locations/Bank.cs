using BlaineRP.Client.CEF.Phone.Apps;
using RAGE;
using System.Collections.Generic;

namespace BlaineRP.Client.Data
{
    public partial class Locations
    {
        public class Bank
        {
            public static Dictionary<int, Bank> All = new Dictionary<int, Bank>();

            public int Id { get; set; }

            public List<NPC> Workers { get; set; }

            public Additional.ExtraBlip Blip { get; set; }

            public Bank(int Id, Utils.Vector4[] NPCs)
            {
                this.Id = Id;

                Workers = new List<NPC>();

                Vector3 posBlip = new Vector3(0f, 0f, 0f);

                for (int i = 0; i < NPCs.Length; i++)
                {
                    posBlip += NPCs[i].Position;

                    var npc = new NPC($"bank_{this.Id}_{i}", "", NPC.Types.Talkable, "csb_anita", NPCs[i].Position, NPCs[i].RotationZ, Settings.App.Static.MainDimension)
                    {
                        SubName = "NPC_SUBNAME_BANK_WORKER",

                        DefaultDialogueId = "bank_preprocess",

                        Data = this,
                    };

                    Workers.Add(npc);
                }

                var pos = posBlip / NPCs.Length;

                Blip = new Additional.ExtraBlip(605, pos, Locale.Property.BankNameDef, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                GPS.AddPosition("money", "banks", $"bank_{Id}", $"bank& #{Id + 1}", new RAGE.Ui.Cursor.Vector2(pos.X, pos.Y));
            }
        }

        public class ATM
        {
            public static Dictionary<int, ATM> All = new Dictionary<int, ATM>();

            public int Id { get; set; }

            public Additional.ExtraColshape Colshape { get; set; }

            public Additional.ExtraBlip Blip { get; set; }

            public ATM(int Id, Utils.Vector4 PositionParams)
            {
                this.Id = Id;

                All.Add(Id, this);

                Colshape = new Additional.Sphere(PositionParams.Position, PositionParams.RotationZ, false, new Utils.Colour(255, 0, 0, 255), Settings.App.Static.MainDimension, null)
                {
                    Data = this,

                    InteractionType = Additional.ExtraColshape.InteractionTypes.ATM,
                    ActionType = Additional.ExtraColshape.ActionTypes.ATM,

                    Name = $"atm_{Id}",
                };

                Blip = new Additional.ExtraBlip(108, PositionParams.Position, Locale.Property.AtmNameDef, 0.4f, 25, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                GPS.AddPosition("money", "atms", $"atm_{Id}", $"atm& #{Id + 1}", new RAGE.Ui.Cursor.Vector2(PositionParams.Position.X, PositionParams.Position.Y));
            }
        }
    }
}
