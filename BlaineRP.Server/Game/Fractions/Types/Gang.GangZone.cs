using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Fractions
{
    public partial class Gang
    {
        public partial class GangZone
        {
            public static List<GangZone> All { get; private set; }

            public const uint ZONE_INCOME = 1_000;

            public Vector2 Position { get; set; }

            public FractionType OwnerType { get; set; }

            public ushort Id { get; set; }

            public GangZone(ushort Id, float PosX, float PosY)
            {
                this.Id = Id;

                this.Position = new Vector2(PosX, PosY);
            }

            public void UpdateOwner(bool updateDb)
            {
                if (OwnerType == FractionType.None)
                {
                    World.Service.ResetSharedData($"GZONE_{Id}_O");
                }
                else
                {
                    World.Service.SetSharedData($"GZONE_{Id}_O", (int)OwnerType);
                }
            }

            public void UpdateBlipFlash(int interval)
            {
                if (interval <= 0)
                {
                    World.Service.ResetSharedData($"GZONE_{Id}_FI");
                }
                else
                {
                    World.Service.SetSharedData($"GZONE_{Id}_FI", interval);
                }
            }

            public static GangZone GetZoneById(ushort id) => All.Where(x => x.Id == id).FirstOrDefault();

            public static List<GangZone> GetAllOwnedBy(FractionType type) => All.Where(x => x.OwnerType == type).ToList();
        }
    }
}