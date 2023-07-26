using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.Helpers.Blips;
using RAGE;
using Core = BlaineRP.Client.Game.World.Core;

namespace BlaineRP.Client.Game.Fractions
{
    public partial class Gang
    {
        public class GangZone
        {
            public static List<GangZone> All { get; set; } = new List<GangZone>();

            public ushort Id { get; set; }

            public ExtraBlip Blip { get; set; }

            public Types OwnerType => (Types)Core.GetSharedData<int>($"GZONE_{Id}_O", 0);

            public int BlipFlashInterval => Core.GetSharedData<int>($"GZONE_{Id}_FI", 0);

            public static void AddZone(ushort id, float posX, float posY)
            {
                var gZone = new GangZone();

                gZone.Blip = new ExtraBlip(5, new Vector3(posX, posY, 0f), "", 1f, 0, 120, 0f, true, 90, 50f, Settings.App.Static.MainDimension);
                gZone.Id = id;

                All.Add(gZone);

                Core.AddDataHandler($"GZONE_{id}_O", GangZoneOwnerDataHandler);
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
}