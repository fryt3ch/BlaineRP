using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class WeaponComponent : Item
    {
        new public class ItemData : Item.ItemData
        {
            public enum Types
            {
                Suppressor = 0,
                Flashlight,
                Grip,
                Scope,
            }

            private static Dictionary<Types, List<uint>> SupportedWeaponHashes = new Dictionary<Types, List<uint>>();

            public Types Type { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, Sync.WeaponSystem.Weapon.ComponentTypes.{Type}";

            public ItemData(string Name, float Weight, string Model, Types Type, params uint[] SupportedWeapons) : base(Name, Weight, Model)
            {
                this.Type = Type;

                if (!SupportedWeaponHashes.ContainsKey(Type))
                    SupportedWeaponHashes.Add(Type, new List<uint>());

                SupportedWeaponHashes[Type].AddRange(SupportedWeapons);
            }

            //public bool IsAllowedFor(uint wHash) => SupportedWeaponHashes[Type].Contains(wHash);
            public bool IsAllowedFor(uint wHash) => true;
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "wc_s", new ItemData("Глушитель (компонент)", 0.01f, "w_am_case", ItemData.Types.Suppressor, (uint)WeaponHash.Pistol, (uint)WeaponHash.Combatpistol, (uint)WeaponHash.Appistol, (uint)WeaponHash.Pistol50, (uint)WeaponHash.Heavypistol, (uint)WeaponHash.Snspistol_mk2 ) },
            { "wc_f", new ItemData("Фонарик (компонент)", 0.01f, "w_am_case", ItemData.Types.Flashlight) },
            { "wc_g", new ItemData("Рукоятка (компонент)", 0.01f, "w_am_case", ItemData.Types.Grip) },
            { "wc_sc", new ItemData("Прицел (компонент)", 0.01f, "w_am_case", ItemData.Types.Scope) },
        };

        public static ItemData GetData(string id) => (ItemData)IDList[id];

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public override float Weight { get => Amount * BaseWeight; }

        public int Amount { get; set; }

        public WeaponComponent(string ID) : base(ID, IDList[ID], typeof(WeaponComponent))
        {

        }
    }
}
