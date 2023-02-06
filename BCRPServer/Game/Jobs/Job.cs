using GTANetworkMethods;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Jobs
{
    public abstract class Job
    {
        public const string RentedVehicleOwnerKey = "rvo";

        private static Dictionary<int, Job> AllJobs { get; set; } = new Dictionary<int, Job>();

        public static Job Get(int jobId) => AllJobs.GetValueOrDefault(jobId);

        private static void Add(Job job) => AllJobs.Add(job.Id, job);

        public enum Types
        {
            /// <summary>Работа дальнобойщика</summary>
            Trucker = 0,
        }

        public Types Type { get; set; }

        public int Id { get; set; }

        public Utils.Vector4 Position { get; set; }

        public abstract string ClientData { get; }

        public Job(Types Type, Utils.Vector4 Position)
        {
            this.Type = Type;

            this.Position = Position;

            this.Id = AllJobs.Count + 1;

            Add(this);
        }

        public static int InitializeAll()
        {
            new Trucker(new Utils.Vector4(50.92614f, 6337.754f, 31.38093f, 21.58484f))
            {
                VehicleRentPrice = 1000,
            };

            var lines = new List<string>();

            foreach (var x in AllJobs.Values)
            {
                x.Initialize();

                lines.Add($"new {x.GetType().Name}({x.ClientData});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "JOBS_TO_REPLACE", lines);

            return AllJobs.Count;
        }

        public abstract void Initialize();
    }

    public interface IVehicles
    {
        public List<VehicleData> Vehicles { get; set; }

        public string NumberplateText { get; set; }

        public uint VehicleRentPrice { get; set; }
    }
}
