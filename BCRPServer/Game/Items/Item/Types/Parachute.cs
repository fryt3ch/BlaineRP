using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Game.Items
{
    public class Parachute : Item, IUsable
    {
        new public class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f";

            public ItemData(string Name, string Model, float Weight) : base(Name, Weight, Model)
            {

            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "parachute_0", new ItemData("Парашют", "p_parachute_s_shop", 0.35f) },
        };

        [JsonIgnore]
        new public ItemData Data => (ItemData)base.Data;

        [JsonIgnore]
        public bool InUse { get; set; }

        public bool StartUse(PlayerData pData, Inventory.GroupTypes group, int slot, bool needUpdate, params object[] args)
        {
            if (InUse)
                return false;

            foreach (var x in pData.Items)
                if (x is Parachute parachute && parachute.InUse)
                    return false;

            InUse = true;

            Wear(pData);

            pData.Player.TriggerEvent("Player::ParachuteS", true);

            //pData.Player.GiveWeapon(GTANetworkAPI.WeaponHash.Parachute, 0);

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, this.ToClientJson(group));
            }

            return true;
        }

        public bool StopUse(PlayerData pData, Inventory.GroupTypes group, int slot, bool needUpdate, params object[] args)
        {
            if (!InUse)
                return false;

            InUse = false;

            Unwear(pData);

            pData.Player.TriggerEvent("Player::ParachuteS", false, args.Length > 0);

            //pData.Player.RemoveWeapon(GTANetworkAPI.WeaponHash.Parachute);

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, this.ToClientJson(group));
            }

            return true;
        }

        public static void Wear(PlayerData pData)
        {
/*            if (pData.BeltOn)
                return;*/

            pData.Player.SetClothes(5, 66, 0);
        }

        public static void Unwear(PlayerData pData)
        {
/*            if (pData.BeltOn)
            {
                pData.Player.SetClothes(5, 81, 0);
            }
            else
            {
                if (pData.Bag != null)
                    pData.Bag?.Wear(pData);
                else
                    pData.Player.SetClothes(5, 0, 0);
            }*/

            if (pData.Bag != null)
                pData.Bag?.Wear(pData);
            else
                pData.Player.SetClothes(5, 0, 0);
        }

        public Parachute(string ID) : base(ID, IDList[ID], typeof(Parachute))
        {

        }
    }
}
