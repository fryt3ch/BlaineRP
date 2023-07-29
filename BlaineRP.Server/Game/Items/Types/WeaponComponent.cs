using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Client.Game.Management.Weapons;

namespace BlaineRP.Server.Game.Items
{
    public partial class WeaponComponent : Item
    {
        public new class ItemData : Item.ItemData
        {
            public enum Types
            {
                Suppressor = 0,
                Flashlight,
                Grip,
                Scope,
            }

            private static Dictionary<Types, List<uint>> _supportedWeaponHashes = new Dictionary<Types, List<uint>>();

            public Types Type { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {typeof(WeaponComponentTypes).FullName}.{Type}";

            public ItemData(string name, float weight, string model, Types type, params uint[] supportedWeapons) : base(name, weight, model)
            {
                Type = type;

                if (!_supportedWeaponHashes.ContainsKey(type))
                    _supportedWeaponHashes.Add(type, new List<uint>());

                _supportedWeaponHashes[type].AddRange(supportedWeapons);
            }

            //public bool IsAllowedFor(uint wHash) => SupportedWeaponHashes[Type].Contains(wHash);
            public bool IsAllowedFor(uint wHash) => true;
        }

        public static ItemData GetData(string id) => (ItemData)IdList[id];

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public override float Weight { get => Amount * BaseWeight; }

        public int Amount { get; set; }

        public WeaponComponent(string id) : base(id, IdList[id], typeof(WeaponComponent))
        {

        }
    }
}
