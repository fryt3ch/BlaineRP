﻿using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Data
{
    class Clothes
    {
        private static Dictionary<System.Type, int> Slots = new Dictionary<System.Type, int>()
        {
            { typeof(Data.Items.Top), 11 },
            { typeof(Data.Items.Under), 8 },
            { typeof(Data.Items.Pants), 4 },
            { typeof(Data.Items.Shoes), 6 },
            { typeof(Data.Items.Gloves), 3 },
            { typeof(Data.Items.Mask), 1 },
            { typeof(Data.Items.Accessory), 7 },
            { typeof(Data.Items.Bag), 5 },

            { typeof(Data.Items.Hat), 0 },
            { typeof(Data.Items.Glasses), 1 },
            { typeof(Data.Items.Earrings), 2 },
            { typeof(Data.Items.Watches), 6 },
            { typeof(Data.Items.Bracelet), 7 },

            { typeof(Data.Items.Ring), int.MinValue },
        };

        private static Dictionary<bool, Dictionary<System.Type, int>> NudeClothes = new Dictionary<bool, Dictionary<System.Type, int>>()
        {
            {
                true,

                new Dictionary<System.Type, int>()
                {
                    { typeof(Data.Items.Top), 15 },
                    { typeof(Data.Items.Under), 15 },
                    { typeof(Data.Items.Gloves), 15 },
                    { typeof(Data.Items.Pants), 21 },
                    { typeof(Data.Items.Shoes), 34 },
                    { typeof(Data.Items.Accessory), 0 },
                    { typeof(Data.Items.Mask), 0 },
                    { typeof(Data.Items.Bag), 0 },

                    { typeof(Data.Items.Ring), 0 },
                }
            },

            {
                false,

                new Dictionary<System.Type, int>()
                {
                    { typeof(Data.Items.Top), 15 },
                    { typeof(Data.Items.Under), 15 },
                    { typeof(Data.Items.Gloves), 15 },
                    { typeof(Data.Items.Pants), 15 },
                    { typeof(Data.Items.Shoes), 35 },
                    { typeof(Data.Items.Accessory), 0 },
                    { typeof(Data.Items.Mask), 0 },
                    { typeof(Data.Items.Bag), 0 },

                    { typeof(Data.Items.Ring), 0 },
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

        #region Stuff
        public static int GetSlot(System.Type type) => Slots.GetValueOrDefault(type);

        public static int GetNudeDrawable(System.Type type, bool sex) => NudeClothes[sex].GetValueOrDefault(type);
        #endregion

        #region Client-side Clothes Actions
        public static async void Wear(string id, int var = 0, params object[] args)
        {
            var type = Data.Items.GetType(id, true);

            if (type == null)
                return;

            var data = (Data.Items.Clothes.ItemData)Data.Items.GetData(id, type);

            if (data == null)
                return;

            var sex = Player.LocalPlayer.GetSex();

            var variation = var < data.Textures.Length && var >= 0 ? data.Textures[var] : 0;

            var slot = GetSlot(type);

            if (slot < 0)
            {
                if (data is Data.Items.Ring.ItemData ringData)
                {
                    Unwear(typeof(Data.Items.Ring));

                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData != null)
                    {
                        if (pData.WearedRing is Sync.AttachSystem.AttachmentObject atObj && atObj.Object?.Exists == true)
                        {
                            RAGE.Game.Entity.DetachEntity(atObj.Object.Handle, true, false);

                            RAGE.Game.Entity.SetEntityAsMissionEntity(atObj.Object.Handle, false, false);

                            atObj.Object.Destroy();
                        }
                    }

                    var ringObj = await Sync.AttachSystem.AttachObjectSimpleLocal(ringData.Model, Player.LocalPlayer, (args[0] as bool? ?? false) ? Sync.AttachSystem.Types.PedRingLeft3 : Sync.AttachSystem.Types.PedRingRight3);

                    if (ringObj == null)
                        return;

                    Player.LocalPlayer.SetData("TempClothes::Ring", ringObj);
                }
            }
            if (type.GetInterfaces().Contains(typeof(Data.Items.Clothes.IProp)))
            {
                if (data is Data.Items.Hat.ItemData hData)
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
                if (data is Data.Items.Top.ItemData tData)
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
                else if (data is Data.Items.Under.ItemData uData)
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
                else if (data is Data.Items.Gloves.ItemData gData)
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
            var type = Data.Items.GetType(id, true);

            if (type == null)
                return;

            var data = (Data.Items.Clothes.ItemData)Data.Items.GetData(id, type);

            if (data == null)
                return;

            var slot = GetSlot(type);

            var = var < data.Textures.Length && var >= 0 ? data.Textures[var] : 0;

            if (slot < 0)
            {
                if (data is Data.Items.Ring.ItemData ringData)
                {
                    Unwear(typeof(Data.Items.Ring));

                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData != null)
                    {
                        if (pData.WearedRing is Sync.AttachSystem.AttachmentObject atObj && atObj.Object?.Exists == true)
                        {
                            RAGE.Game.Entity.DetachEntity(atObj.Object.Handle, true, false);

                            RAGE.Game.Entity.SetEntityAsMissionEntity(atObj.Object.Handle, false, false);

                            atObj.Object.Destroy();
                        }
                    }

                    var ringObj = await Sync.AttachSystem.AttachObjectSimpleLocal(ringData.Model, Player.LocalPlayer, (args[0] as bool? ?? false) ? Sync.AttachSystem.Types.PedRingLeft3 : Sync.AttachSystem.Types.PedRingRight3);

                    if (ringObj == null)
                        return;

                    Player.LocalPlayer.SetData("TempClothes::Ring", ringObj);
                }
            }
            if (data is Data.Items.Hat.ItemData hData)
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
            else if (data is Data.Items.Top.ItemData tData)
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
            else if (data is Data.Items.Under.ItemData uData)
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
                if (type == typeof(Data.Items.Ring))
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
            if (type.GetInterfaces().Contains(typeof(Data.Items.Clothes.IProp)))
            {
                Player.LocalPlayer.ClearProp(slot);

                if (type == typeof(Data.Items.Hat))
                {
                    Player.LocalPlayer.ResetData("TempClothes::Hat");
                }
            }
            else
            {
                if (type == typeof(Data.Items.Top))
                {
                    Player.LocalPlayer.SetComponentVariation(11, GetNudeDrawable(type, sex), 0, 2);
                    Player.LocalPlayer.SetComponentVariation(3, GetNudeDrawable(typeof(Data.Items.Gloves), sex), 0, 2);

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
                else if (type == typeof(Data.Items.Under))
                {
                    Player.LocalPlayer.ResetData("TempClothes::Under");

                    if (!Player.LocalPlayer.HasData("TempClothes::Top"))
                    {
                        Player.LocalPlayer.SetComponentVariation(11, GetNudeDrawable(typeof(Data.Items.Top), sex), 0, 2);
                        Player.LocalPlayer.SetComponentVariation(8, GetNudeDrawable(type, sex), 0, 2);
                        Player.LocalPlayer.SetComponentVariation(3, GetNudeDrawable(typeof(Data.Items.Gloves), sex), 0, 2);

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
                else if (type == typeof(Data.Items.Gloves))
                {
                    Player.LocalPlayer.ResetData("TempClothes::Gloves");

                    if (Player.LocalPlayer.GetDrawableVariation(11) == GetNudeDrawable(typeof(Data.Items.Top), sex))
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

            Player.LocalPlayer.SetComponentVariation(11, GetNudeDrawable(typeof(Data.Items.Top), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(8, GetNudeDrawable(typeof(Data.Items.Under), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(3, GetNudeDrawable(typeof(Data.Items.Gloves), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(4, GetNudeDrawable(typeof(Data.Items.Pants), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(6, GetNudeDrawable(typeof(Data.Items.Shoes), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(5, GetNudeDrawable(typeof(Data.Items.Bag), sex), 0, 2);
            Player.LocalPlayer.SetComponentVariation(7, GetNudeDrawable(typeof(Data.Items.Accessory), sex), 0, 2);

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
        #endregion
    }
}