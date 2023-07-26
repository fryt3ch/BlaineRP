using System.Collections.Generic;

namespace BlaineRP.Client.Game.Management.Weapons
{
    public class WeaponInfo
    {
        public uint Hash { get; set; }

        public string GameName { get; set; }

        public float BaseDamage { get; set; }

        public float MaxDistance { get; set; }

        public float DistanceRatio { get; set; }

        private Dictionary<BodyPartTypes, float> BodyRatios { get; set; }

        public Dictionary<WeaponComponentTypes, uint> ComponentsHashes { get; set; }

        public bool HasAmmo { get; set; }

        public float GetBodyRatio(BodyPartTypes type)
        {
            var ratio = 1f;

            BodyRatios?.TryGetValue(type, out ratio);

            return ratio;
        }

        public uint? GetComponentHash(WeaponComponentTypes cType)
        {
            return ComponentsHashes != null && ComponentsHashes.TryGetValue(cType, out var hash) ? (uint?)hash : (uint?)null;
        }

        public WeaponInfo(string gameName, float baseDamage, float maxDistance, float distanceRatio, float headRatio, float chestRatio, float limbRatio, bool hasAmmo = true)
        {
            Hash = RAGE.Util.Joaat.Hash(gameName);
            GameName = gameName;
            BaseDamage = baseDamage;
            MaxDistance = maxDistance;
            DistanceRatio = distanceRatio;

            BodyRatios = new Dictionary<BodyPartTypes, float>()
                {
                    { BodyPartTypes.Head, headRatio },
                    { BodyPartTypes.Chest, chestRatio },
                    { BodyPartTypes.Limb, limbRatio },
                };

            HasAmmo = hasAmmo;
        }
    }
}
