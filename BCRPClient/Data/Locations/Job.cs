using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations : Events.Script
    {
        public abstract class Job
        {
            private static Dictionary<int, Job> AllJobs { get; set; } = new Dictionary<int, Job>();

            public enum Types
            {
                /// <summary>Работа дальнобойщика</summary>
                Trucker = 0,
            }

            public int Id { get; set; }

            public int SubId => AllJobs.Values.Where(x => x.Type == Type).ToList().IndexOf(this);

            public Types Type { get; set; }

            public string Name => Locale.Property.JobNames.GetValueOrDefault(Type) ?? "null";

            public NPC JobGiver { get; set; }

            public Job(int Id, Types Type)
            {
                this.Id = Id;

                AllJobs.Add(Id, this);
            }
        }

        public class Trucker : Job
        {
            public Trucker(int Id, Utils.Vector4 Position) : base(Id, Types.Trucker) 
            {
                var subId = SubId;

                if (subId == 0)
                    JobGiver = new NPC($"job_{Id}_{subId}", "Кеннет", NPC.Types.Talkable, "ig_oneil", Position.Position, Position.RotationZ, Settings.MAIN_DIMENSION);

                JobGiver.SubName = Locale.General.NPC.TypeNames["job_trucker"];

                JobGiver.Data = this;

                JobGiver.DefaultDialogueId = "job_trucker_g_0";
            }
        }
    }
}
