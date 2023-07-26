using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Items;
using BlaineRP.Client.Game.Management.Attachments;
using RAGE.Elements;
using Core = BlaineRP.Client.Game.Management.Attachments.Core;

namespace BlaineRP.Client.Game.Data.Customization
{
    class Clothes
    {
        private static Dictionary<System.Type, int> Slots = new Dictionary<System.Type, int>()
        {
            { typeof(Top), 11 },
            { typeof(Under), 8 },
            { typeof(Pants), 4 },
            { typeof(Shoes), 6 },
            { typeof(Gloves), 3 },
            { typeof(Mask), 1 },
            { typeof(Accessory), 7 },
            { typeof(Bag), 5 },

            { typeof(Hat), 0 },
            { typeof(Glasses), 1 },
            { typeof(Earrings), 2 },
            { typeof(Watches), 6 },
            { typeof(Bracelet), 7 },

            { typeof(Ring), int.MinValue },
        };

        private static Dictionary<bool, Dictionary<System.Type, int>> NudeClothes = new Dictionary<bool, Dictionary<System.Type, int>>()
        {
            {
                true,

                new Dictionary<System.Type, int>()
                {
                    { typeof(Top), 15 },
                    { typeof(Under), 15 },
                    { typeof(Gloves), 15 },
                    { typeof(Pants), 21 },
                    { typeof(Shoes), 34 },
                    { typeof(Accessory), 0 },
                    { typeof(Mask), 0 },
                    { typeof(Bag), 0 },

                    { typeof(Ring), 0 },
                }
            },

            {
                false,

                new Dictionary<System.Type, int>()
                {
                    { typeof(Top), 15 },
                    { typeof(Under), 15 },
                    { typeof(Gloves), 15 },
                    { typeof(Pants), 15 },
                    { typeof(Shoes), 35 },
                    { typeof(Accessory), 0 },
                    { typeof(Mask), 0 },
                    { typeof(Bag), 0 },

                    { typeof(Ring), 0 },
                }
            },
        };

        public class TempClothes
        {
            public string ID { get; set; }
            public int Variation { get; set; }
            public bool Toggled { get; set; }

            public TempClothes(string ID, int Variation, bool Toggled = false)
            {
                this.ID = ID;
                this.Variation = Variation;

                this.Toggled = Toggled;
            }
        }
        
        public static int GetSlot(System.Type type) => Slots.GetValueOrDefault(type);

        public static int GetNudeDrawable(System.Type type, bool sex) => NudeClothes[sex].GetValueOrDefault(type);

        public static async void Wear(string id, int var = 0, params object[] args)
        {
            var type = Items.Core.GetType(id, true);

            if (type == null)
                return;

            var data = (Items.Clothes.ItemData)Items.Core.GetData(id, type);

            if (data == null)
                return;

            var sex = Player.LocalPlayer.GetSex();

            var variation = var < data.Textures.Length && var >= 0 ? data.Textures[var] : 0;

            var slot = GetSlot(type);

            if (slot < 0)
            {
                if (data is Ring.ItemData ringData)
                {
                    Unwear(typeof(Ring));

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData != null)
                    {
                        if (pData.WearedRing is AttachmentObject atObj && atObj.Object?.Exists == true)
                        {
                            RAGE.Game.Entity.DetachEntity(atObj.Object.Handle, true, false);

                            RAGE.Game.Entity.SetEntityAsMissionEntity(atObj.Object.Handle, false, false);

                            atObj.Object.Destroy();
                        }
                    }

                    var ringObj = await Core.AttachObjectSimpleLocal(ringData.Model, Player.LocalPlayer, (args[0] as bool? ?? false) ? AttachmentTypes.PedRingLeft3 : AttachmentTypes.PedRingRight3);

                    if (ringObj == null)
                        return;

                    Player.LocalPlayer.SetData("TempClothes::Ring", ringObj);
                }
            }
            if (type.GetInterfaces().Contains(typeof(Items.Clothes.IProp)))
            {
                if (data is Hat.ItemData hData)
                {
                    TempClothes currentHatTemp = null;

                    if (Player.LocalPlayer.HasData("TempClothes::Hat"))
                        currentHatTemp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Hat");

                    if (currentHatTemp != null && currentHatTemp.Toggled && id == currentHatTemp.ID)
                    {
                        Player.LocalPlayer.SetPropIndex(slot, hData.ExtraData.Drawable, variation, true);

                        currentHatTemp.Variation = variation;

                        Player.LocalPlayer.SetData("TempClothes::Hat", currentHatTemp);
                    }
                    else
                    {
                        Player.LocalPlayer.SetPropIndex(slot, data.Drawable, variation, true);

                        Player.LocalPlayer.SetData("TempClothes::Hat", new TempClothes(id, variation));
                    }

                    return;
                }

                Player.LocalPlayer.SetPropIndex(slot, data.Drawable, variation, true);
            }
            else
            {
                if (data is Top.ItemData tData)
                {
                    TempClothes currentTopTemp = null;

                    if (Player.LocalPlayer.HasData("TempClothes::Top"))
                    {
                        currentTopTemp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Top");

                        Unwear(type);
                    }

                    if (currentTopTemp != null && currentTopTemp.Toggled && id == currentTopTemp.ID)
                    {
                        Player.LocalPlayer.SetComponentVariation(slot, tData.ExtraData.Drawable, variation, 2);
                        Player.LocalPlayer.SetComponentVariation(3, tData.ExtraData.BestTorso, 0, 2);

                        currentTopTemp.Variation = variation;

                        Player.LocalPlayer.SetData("TempClothes::Top", currentTopTemp);
                    }
                    else
                    {
                        Player.LocalPlayer.SetComponentVariation(slot, data.Drawable, variation, 2);
                        Player.LocalPlayer.SetComponentVariation(3, tData.BestTorso, 0, 2);

                        Player.LocalPlayer.SetData("TempClothes::Top", new TempClothes(id, variation));
                    }

                    if (Player.LocalPlayer.HasData("TempClothes::Under"))
                    {
                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Under");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }
                    else if (Player.LocalPlayer.HasData("TempClothes::Gloves"))
                    {
                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Gloves");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }

                    return;
                }
                else if (data is Under.ItemData uData)
                {
                    TempClothes currentUnderTemp = null;

                    if (Player.LocalPlayer.HasData("TempClothes::Under"))
                    {
                        currentUnderTemp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Under");

                        Unwear(type);
                    }

                    if (!Player.LocalPlayer.HasData("TempClothes::Top") && uData.BestTop != null)
                    {
                        if (currentUnderTemp != null && currentUnderTemp.Toggled && id == currentUnderTemp.ID && uData.BestTop.ExtraData != null)
                        {
                            Player.LocalPlayer.SetComponentVariation(11, uData.BestTop.ExtraData.Drawable, variation, 2);
                            Player.LocalPlayer.SetComponentVariation(3, uData.BestTop.ExtraData.BestTorso, 0, 2);
                        }
                        else
                        {
                            Player.LocalPlayer.SetComponentVariation(11, uData.BestTop.Drawable, variation, 2);
                            Player.LocalPlayer.SetComponentVariation(3, uData.BestTop.BestTorso, 0, 2);
                        }
                    }
                    else
                    {
                        if (currentUnderTemp != null && currentUnderTemp.Toggled && id == currentUnderTemp.ID)
                        {
                            Player.LocalPlayer.SetComponentVariation(slot, uData.ExtraData.Drawable, variation, 2);
                            Player.LocalPlayer.SetComponentVariation(3, uData.ExtraData.BestTorso, 0, 2);
                        }
                        else
                        {
                            Player.LocalPlayer.SetComponentVariation(slot, data.Drawable, variation, 2);
                            Player.LocalPlayer.SetComponentVariation(3, uData.BestTorso, 0, 2);
                        }
                    }

                    if (currentUnderTemp != null)
                    {
                        currentUnderTemp.Variation = var;
                        currentUnderTemp.ID = id;

                        if (id != currentUnderTemp.ID)
                            currentUnderTemp.Toggled = false;

                        Player.LocalPlayer.SetData("TempClothes::Under", currentUnderTemp);
                    }
                    else
                        Player.LocalPlayer.SetData("TempClothes::Under", new TempClothes(id, variation));

                    if (Player.LocalPlayer.HasData("TempClothes::Gloves"))
                    {
                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Gloves");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }

                    return;
                }
                else if (data is Gloves.ItemData gData)
                {
                    if (Player.LocalPlayer.HasData("TempClothes::Gloves"))
                        Unwear(type);

                    var curTorso = Player.LocalPlayer.GetDrawableVariation(slot);

                    if (gData.BestTorsos.ContainsKey(curTorso))
                    {
                        Player.LocalPlayer.SetComponentVariation(slot, gData.BestTorsos[curTorso], variation, 2);

                        Player.LocalPlayer.SetData("TempClothes::Gloves", new TempClothes(id, variation));
                    }

                    return;
                }

                Player.LocalPlayer.SetComponentVariation(slot, data.Drawable, variation, 2);
            }
        }

        public static async void Action(string id, int var, params object[] args)
        {
            var type = Items.Core.GetType(id, true);

            if (type == null)
                return;

            var data = (Items.Clothes.ItemData)Items.Core.GetData(id, type);

            if (data == null)
                return;

            var slot = GetSlot(type);

            var = var < data.Textures.Length && var >= 0 ? data.Textures[var] : 0;

            if (slot < 0)
            {
                if (data is Ring.ItemData ringData)
                {
                    Unwear(typeof(Ring));

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData != null)
                    {
                        if (pData.WearedRing is AttachmentObject atObj && atObj.Object?.Exists == true)
                        {
                            RAGE.Game.Entity.DetachEntity(atObj.Object.Handle, true, false);

                            RAGE.Game.Entity.SetEntityAsMissionEntity(atObj.Object.Handle, false, false);

                            atObj.Object.Destroy();
                        }
                    }

                    var ringObj = await Core.AttachObjectSimpleLocal(ringData.Model, Player.LocalPlayer, (args[0] as bool? ?? false) ? AttachmentTypes.PedRingLeft3 : AttachmentTypes.PedRingRight3);

                    if (ringObj == null)
                        return;

                    Player.LocalPlayer.SetData("TempClothes::Ring", ringObj);
                }
            }
            if (data is Hat.ItemData hData)
            {
                if (hData.ExtraData == null || !Player.LocalPlayer.HasData("TempClothes::Hat"))
                    return;

                var current = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Hat");

                if (!current.Toggled)
                {
                    Player.LocalPlayer.SetComponentVariation(slot, hData.ExtraData.Drawable, var, 2);

                    current.Toggled = true;

                    Player.LocalPlayer.SetData("TempClothes::Hat", current);
                }
                else
                {
                    Player.LocalPlayer.SetComponentVariation(slot, data.Drawable, var, 2);

                    current.Toggled = false;

                    Player.LocalPlayer.SetData("TempClothes::Hat", current);
                }
            }
            else if (data is Top.ItemData tData)
            {
                if (tData.ExtraData == null || !Player.LocalPlayer.HasData("TempClothes::Top"))
                    return;

                var current = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Top");

                if (!current.Toggled)
                {
                    Player.LocalPlayer.SetComponentVariation(slot, tData.ExtraData.Drawable, var, 2);

                    current.Toggled = true;

                    Player.LocalPlayer.SetData("TempClothes::Top", current);

                    Player.LocalPlayer.SetComponentVariation(3, tData.ExtraData.BestTorso, 0, 2);

                    if (Player.LocalPlayer.HasData("TempClothes::Under"))
                    {
                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Under");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }
                    else if (Player.LocalPlayer.HasData("TempClothes::Gloves"))
                    {
                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Gloves");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }
                }
                else
                {
                    Player.LocalPlayer.SetComponentVariation(slot, data.Drawable, var, 2);

                    current.Toggled = false;

                    Player.LocalPlayer.SetData("TempClothes::Top", current);

                    Player.LocalPlayer.SetComponentVariation(3, tData.BestTorso, var, 2);

                    if (Player.LocalPlayer.HasData("TempClothes::Under"))
                    {
                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Under");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }
                    else if (Player.LocalPlayer.HasData("TempClothes::Gloves"))
                    {
                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Gloves");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }
                }
            }
            else if (data is Under.ItemData uData)
            {
                if (uData.ExtraData == null || !Player.LocalPlayer.HasData("TempClothes::Under"))
                    return;

                var current = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Under");

                if (current == null)
                    return;

                if (!Player.LocalPlayer.HasData("TempClothes::Top") && uData.BestTop != null)
                {
                    current.Toggled = !current.Toggled;

                    Player.LocalPlayer.SetData("TempClothes::Under", current);

                    if (!current.Toggled)
                    {
                        if (uData.BestTop.ExtraData == null)
                            return;

                        Player.LocalPlayer.SetComponentVariation(11, uData.BestTop.ExtraData.Drawable, var, 2);
                        Player.LocalPlayer.SetComponentVariation(3, uData.ExtraData.BestTorso, 0, 2);

                        if (Player.LocalPlayer.HasData("TempClothes::Gloves"))
                        {
                            var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Gloves");

                            if (temp != null)
                                Wear(temp.ID, temp.Variation);
                        }
                    }
                    else
                    {
                        Player.LocalPlayer.SetComponentVariation(11, uData.BestTop.Drawable, var, 2);
                        Player.LocalPlayer.SetComponentVariation(3, uData.BestTop.BestTorso, 0, 2);

                        if (Player.LocalPlayer.HasData("TempClothes::Gloves"))
                        {
                            var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Gloves");

                            if (temp != null)
                                Wear(temp.ID, temp.Variation);
                        }
                    }
                }
                else if (uData.ExtraData != null)
                {
                    current.Toggled = !current.Toggled;

                    Player.LocalPlayer.SetData("TempClothes::Under", current);

                    if (current.Toggled)
                    {
                        Player.LocalPlayer.SetComponentVariation(slot, uData.ExtraData.Drawable, var, 2);
                        Player.LocalPlayer.SetComponentVariation(3, uData.ExtraData.BestTorso, 0, 2);

                        if (Player.LocalPlayer.HasData("TempClothes::Gloves"))
                        {
                            var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Gloves");

                            if (temp != null)
                                Wear(temp.ID, temp.Variation);
                        }
                    }
                    else
                    {
                        Player.LocalPlayer.SetComponentVariation(slot, data.Drawable, var, 2);
                        Player.LocalPlayer.SetComponentVariation(3, uData.BestTorso, 0, 2);

                        if (Player.LocalPlayer.HasData("TempClothes::Gloves"))
                        {
                            var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Gloves");

                            if (temp != null)
                                Wear(temp.ID, temp.Variation);
                        }
                    }
                }
                else
                    return;
            }
        }

        public static void Unwear(System.Type type)
        {
            if (RAGE.Elements.Player.LocalPlayer.Model != 0x705E61F2 && RAGE.Elements.Player.LocalPlayer.Model != 0x9C9EFFD8)
                return;

            var sex = Player.LocalPlayer.GetSex();

            var slot = GetSlot(type);

            if (slot < 0)
            {
                if (type == typeof(Ring))
                {
                    if (Player.LocalPlayer.GetData<GameEntity>("TempClothes::Ring") is GameEntity gEntity)
                    {
                        RAGE.Game.Entity.DetachEntity(gEntity.Handle, true, false);

                        RAGE.Game.Entity.SetEntityAsMissionEntity(gEntity.Handle, false, false);

                        gEntity.Destroy();

                        Player.LocalPlayer.ResetData("TempClothes::Ring");
                    }
                }
            }
            if (type.GetInterfaces().Contains(typeof(Items.Clothes.IProp)))
            {
                Player.LocalPlayer.ClearProp(slot);

                if (type == typeof(Hat))
                {
                    Player.LocalPlayer.ResetData("TempClothes::Hat");
                }
            }
            else
            {
                if (type == typeof(Top))
                {
                    Player.LocalPlayer.SetComponentVariation(11, GetNudeDrawable(type, sex), 0, 2);
                    Player.LocalPlayer.SetComponentVariation(3, GetNudeDrawable(typeof(Gloves), sex), 0, 2);

                    Player.LocalPlayer.ResetData("TempClothes::Top");

                    if (Player.LocalPlayer.HasData("TempClothes::Under"))
                    {
                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Under");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }
                    else if (Player.LocalPlayer.HasData("TempClothes::Gloves"))
                    {
                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Gloves");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }
                }
                else if (type == typeof(Under))
                {
                    Player.LocalPlayer.ResetData("TempClothes::Under");

                    if (!Player.LocalPlayer.HasData("TempClothes::Top"))
                    {
                        Player.LocalPlayer.SetComponentVariation(11, GetNudeDrawable(typeof(Top), sex), 0, 2);
                        Player.LocalPlayer.SetComponentVariation(8, GetNudeDrawable(type, sex), 0, 2);
                        Player.LocalPlayer.SetComponentVariation(3, GetNudeDrawable(typeof(Gloves), sex), 0, 2);

                        if (Player.LocalPlayer.HasData("TempClothes::Gloves"))
                        {
                            var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Gloves");

                            if (temp != null)
                                Wear(temp.ID, temp.Variation);
                        }
                    }
                    else
                    {
                        Player.LocalPlayer.SetComponentVariation(8, GetNudeDrawable(type, sex), 0, 2);

                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Top");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }
                }
                else if (type == typeof(Gloves))
                {
                    Player.LocalPlayer.ResetData("TempClothes::Gloves");

                    if (Player.LocalPlayer.GetDrawableVariation(11) == GetNudeDrawable(typeof(Top), sex))
                        Player.LocalPlayer.SetComponentVariation(3, GetNudeDrawable(type, sex), 0, 2);

                    if (Player.LocalPlayer.HasData("TempClothes::Top"))
                    {
                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Top");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }
                    else if (Player.LocalPlayer.HasData("TempClothes::Under"))
                    {
                        var temp = Player.LocalPlayer.GetData<TempClothes>("TempClothes::Under");

                        if (temp != null)
                            Wear(temp.ID, temp.Variation);
                    }
                }
                else
                {
                    Player.LocalPlayer.SetComponentVariation(slot, GetNudeDrawable(type, sex), 0, 2);
                }
            }
        }

        public static void UndressAll()
        {
            var sex = Player.LocalPlayer.GetSex();

            Player.LocalPlayer.ResetData("TempClothes::Under");
            Player.LocalPlayer.ResetData("TempClothes::Hat");
            Player.LocalPlayer.ResetData("TempClothes::Top");
            Player.LocalPlayer.ResetData("TempClothes::Gloves");

            Player.LocalPlayer.ClearAllProps();

            Player.LocalPlayer.SetComponentVariation(11, GetNudeDrawable(typeof(Top), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(8, GetNudeDrawable(typeof(Under), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(3, GetNudeDrawable(typeof(Gloves), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(4, GetNudeDrawable(typeof(Pants), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(6, GetNudeDrawable(typeof(Shoes), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(5, GetNudeDrawable(typeof(Bag), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(7, GetNudeDrawable(typeof(Accessory), sex), 0, 2);

            Player.LocalPlayer.SetComponentVariation(10, 0, 0, 2);
            Player.LocalPlayer.SetComponentVariation(1, 0, 0, 2);
        }

        public static Dictionary<int, (int, int)> GetAllRealClothes(Player player)
        {
            return new Dictionary<int, (int, int)>()
            {
                { 1, (player.GetDrawableVariation(1), player.GetTextureVariation(1)) },
                { 2, (player.GetDrawableVariation(2), 0) },
                { 3, (player.GetDrawableVariation(3), player.GetTextureVariation(3)) },
                { 4, (player.GetDrawableVariation(4), player.GetTextureVariation(4)) },
                { 5, (player.GetDrawableVariation(5), player.GetTextureVariation(5)) },
                { 6, (player.GetDrawableVariation(6), player.GetTextureVariation(6)) },
                { 7, (player.GetDrawableVariation(7), player.GetTextureVariation(7)) },
                { 8, (player.GetDrawableVariation(8), player.GetTextureVariation(8)) },
                { 9, (player.GetDrawableVariation(9), player.GetTextureVariation(9)) },
                { 10, (player.GetDrawableVariation(10), player.GetTextureVariation(10)) },
                { 11, (player.GetDrawableVariation(11), player.GetTextureVariation(11)) },
            };
        }

        public static Dictionary<int, (int, int)> GetAllRealAccessories(Player player)
        {
            return new Dictionary<int, (int, int)>()
            {
                { 0, (player.GetPropIndex(0), player.GetPropTextureIndex(0)) },
                { 1, (player.GetPropIndex(1), player.GetPropTextureIndex(1)) },
                { 2, (player.GetPropIndex(2), player.GetPropTextureIndex(2)) },
                { 6, (player.GetPropIndex(6), player.GetPropTextureIndex(6)) },
                { 7, (player.GetPropIndex(7), player.GetPropTextureIndex(7)) },
            };
        }
    }
}
