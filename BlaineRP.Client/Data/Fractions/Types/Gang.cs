using RAGE;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Data.Fractions
{
    public class Gang : Fraction
    {
        public Gang(Types Type, string Name, uint StorageContainerId, string ContainerPos, string CWbPos, byte MaxRank, string CreationWorkbenchPricesJs, uint MetaFlags) : base(Type, Name, StorageContainerId, ContainerPos, CWbPos, MaxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(CreationWorkbenchPricesJs), MetaFlags)
        {

        }

        public override void OnStartMembership(params object[] args)
        {
            base.OnStartMembership(args);
        }

        public override void OnEndMembership()
        {
            base.OnEndMembership();
        }

        public class GangZone
        {
            public static List<GangZone> All { get; set; } = new List<GangZone>();

            public ushort Id { get; set; }

            public Additional.ExtraBlip Blip { get; set; }

            public Types OwnerType => (Types)Sync.World.GetSharedData<int>($"GZONE_{Id}_O", 0);

            public int BlipFlashInterval => Sync.World.GetSharedData<int>($"GZONE_{Id}_FI", 0);

            public static void AddZone(ushort id, float posX, float posY)
            {
                var gZone = new GangZone();

                gZone.Blip = new Additional.ExtraBlip(5, new Vector3(posX, posY, 0f), "", 1f, 0, 120, 0f, true, 90, 50f, Settings.App.Static.MainDimension);
                gZone.Id = id;

                All.Add(gZone);

                Sync.World.AddDataHandler($"GZONE_{id}_O", GangZoneOwnerDataHandler);
            }

            private static void GangZoneOwnerDataHandler(string key, object value, object oldValue)
            {
                var keyD = key.Split('_');

                var id = ushort.Parse(keyD[1]);

                var zoneInfo = GetById(id);

                if (zoneInfo == null)
                    return;

                var owner = (Types)Utils.Convert.ToInt32(value ?? 0);

                zoneInfo.OnOwnerUpdate(owner);
            }

            private static void GangZoneBlipFlashDataHandler(string key, object value, object oldValue)
            {
                var keyD = key.Split('_');

                var id = ushort.Parse(keyD[1]);

                var zoneInfo = GetById(id);

                if (zoneInfo == null)
                    return;

                var interval = Utils.Convert.ToInt32(value ?? 0);

                zoneInfo.OnBlipFlashUpdate(interval);
            }

            public void OnOwnerUpdate(Types type)
            {
                var color = type == Types.GANG_VAGS ? 5 : type == Types.GANG_BALS ? 27 : type == Types.GANG_FAMS ? 2 : type == Types.GANG_MARA ? 3 : 0;

                if (color == 0)
                    Blip.Display = 0;
                else
                    Blip.Display = 2;

                Blip.Colour = (byte)color;
            }

            public void OnBlipFlashUpdate(int interval)
            {
                Blip.FlashInterval = interval;
            }

            public static void PostInitialize()
            {
                foreach (var x in All)
                {
                    x.OnOwnerUpdate(x.OwnerType);

                    x.OnBlipFlashUpdate(x.BlipFlashInterval);
                }
            }

            public static GangZone GetById(ushort id) => All.Where(x => x.Id == id).FirstOrDefault();
        }
    }

    [Script(int.MaxValue)]
    public class GangEvents
    {
        public GangEvents()
        {

        }
    }
}