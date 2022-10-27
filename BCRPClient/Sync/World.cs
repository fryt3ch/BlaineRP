using RAGE;
using RAGE.Elements;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Sync
{
    class World : Events.Script
    {
        private static bool Preloaded { get; set; }

        /// <summary>Ближайший к игроку предмет на земле</summary>
        public static MapObject ClosestItemOnGround { get; set; }

        private static bool _EnabledItemsOnGround;
        /// <summary>Включено ли взаимодействие с предметами на земле в данный момент?</summary>
        public static bool EnabledItemsOnGround { get => _EnabledItemsOnGround; set { if (!_EnabledItemsOnGround && value) { GameEvents.Render -= ItemsOnGroundRender; GameEvents.Render += ItemsOnGroundRender; } else if (_EnabledItemsOnGround && !value) GameEvents.Render -= ItemsOnGroundRender; _EnabledItemsOnGround = value; ClosestItemOnGround = null; } }

        public static List<MapObject> ItemsOnGround { get; set; }

        private static List<float> Distances { get; set; }

        public static void Preload()
        {
            if (Preloaded)
                return;

            for (int i = 0; i < RAGE.Elements.Entities.Objects.All.Count; i++)
            {
                var x = RAGE.Elements.Entities.Objects.All[i];

                if (x.GetSharedData<bool>("IOG"))
                {
                    x.NotifyStreaming = true;
                    x.SetData("IOG", true);
                }
            }

            for (int i = 0; i < RAGE.Elements.Entities.Colshapes.All.Count; i++)
            {
                var x = RAGE.Elements.Entities.Colshapes.All[i];

                if (x?.HasSharedData("Type") != true)
                    continue;

                Events.CallLocal("ExtraColshape::New", x);
            }

            Preloaded = true;
        }

        public static bool BusinessInfo()
        {
            if (Additional.ExtraColshape.LastSent.IsSpam(1000, false, false))
                return false;

            if (!Player.LocalPlayer.HasData("CurrentBusiness"))
                return false;

            Events.CallRemote("Business::ShowInfo", Player.LocalPlayer.GetData<int>("CurrentBusiness"));

            Additional.ExtraColshape.LastSent = DateTime.Now;

            return true;
        }

        public static void BusinessEnter(int id)
        {
            Events.CallRemote("Business::Enter", id);
        }

        public static bool BusinessEnter()
        {
            if (Additional.ExtraColshape.LastSent.IsSpam(1000, false, false))
                return false;

            if (!Player.LocalPlayer.HasData("CurrentBusiness"))
                return false;

            Events.CallRemote("Business::Enter", Player.LocalPlayer.GetData<int>("CurrentBusiness"));

            Additional.ExtraColshape.LastSent = DateTime.Now;

            return true;
        }

        public static bool HouseEnter()
        {
            if (Additional.ExtraColshape.LastSent.IsSpam(1000, false, false))
                return false;

            if (!Player.LocalPlayer.HasData("CurrentHouse"))
                return false;

            Events.CallRemote("House::Enter", Player.LocalPlayer.GetData<int>("CurrentHouse"));

            Additional.ExtraColshape.LastSent = DateTime.Now;

            return true;
        }

        public World()
        {
            Preloaded = false;

            ItemsOnGround = new List<MapObject>();
            Distances = new List<float>();

            ClosestItemOnGround = null;

            _EnabledItemsOnGround = false;

            #region Events
            Events.Add("IOG::Add", (object[] args) =>
            {
                if (!Preloaded)
                    return;

                var iog = (MapObject)args[0];

                if (iog == null)
                    return;

                iog.SetData("IOG", true);
                iog.NotifyStreaming = true;
            });

            Events.AddDataHandler("Amount", (Entity entity, object value, object oldValue) =>
            {
                if (entity.Type != RAGE.Elements.Type.Object || !entity.HasData("IOG"))
                    return;

                MapObject obj = entity as MapObject;

                obj.SetData("Amount", (int)value);
            });

            Events.Add("IOG::RequestUpdate", (object[] args) =>
            {
                if (!Preloaded)
                    return;

                MapObject obj = (MapObject)args[0];

                if (obj == null)
                    return;

                if (!obj.IsStreamed())
                    return;

                var newPos = obj.GetCoords(true);

                if (obj.Position != newPos)
                    Events.CallRemote("IOG::Update", obj.GetData<uint>("UID"), RAGE.Util.Json.Serialize(newPos), RAGE.Util.Json.Serialize(obj.GetRotation(2)));
            });

            Events.Add("IOG::Reset", (object[] args) =>
            {
                if (!Preloaded)
                    return;

                MapObject obj = (MapObject)args[0];

                if (obj == null)
                    return;

                if (!obj.IsStreamed())
                    return;

                var pos = obj.Position;

                obj.SetCoords(pos.X, pos.Y, pos.Z, true, false, false, false);

                obj.FreezePosition(false);
                obj.SetActivatePhysicsAsSoonAsItIsUnfrozen(true);
                obj.SetHasGravity(true);
                obj.SetCollision(true, true);
            });
            #endregion

            #region New IOG Stream
            Events.OnEntityStreamIn += ((Entity entity) =>
            {
                if (entity.Type != RAGE.Elements.Type.Object || entity.IsLocal || !entity.HasData("IOG"))
                    return;

                var obj = entity as MapObject;

                obj.FreezePosition(false);
                obj.SetActivatePhysicsAsSoonAsItIsUnfrozen(true);
                obj.SetHasGravity(true);
                obj.SetCollision(true, true);

                obj.SetData("Name", Data.Items.GetName(obj.GetSharedData<string>("ID", null)));
                obj.SetData("UID", RAGE.Util.Json.Deserialize<uint>(obj.GetSharedData<string>("UID")));
                obj.SetData("Amount", obj.GetSharedData<int>("Amount", -1));

                if (obj.GetData<string>("Name") == null)
                    return;

                if (!ItemsOnGround.Contains(obj))
                    ItemsOnGround.Add(obj);
            });

            Events.OnEntityStreamOut += ((Entity entity) =>
            {
                if (entity.Type != RAGE.Elements.Type.Object || entity.IsLocal || !entity.HasData("IOG"))
                    return;

                var obj = entity as MapObject;

                if (ItemsOnGround.Contains(obj))
                    ItemsOnGround.Remove(obj);
            });
            #endregion
        }

        #region IOG Render
        private static void ItemsOnGroundRender()
        {
            Distances.Clear();

            float minDist = Settings.ENTITY_INTERACTION_MAX_DISTANCE_RENDER;
            int minIdx = -1;

            var allItems = ItemsOnGround.ToList();

            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i]?.Exists != true)
                    continue;

                float dist = Vector3.Distance(Player.LocalPlayer.Position, allItems[i].GetCoords(true));

                if (dist < minDist)
                {
                    minDist = dist;
                    minIdx = i;
                }

                Distances.Add(dist);
            }

            if (minIdx == -1)
            {
                ClosestItemOnGround = null;

                return;
            }

            ClosestItemOnGround = allItems[minIdx];

            float screenX = 0, screenY = 0;

            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i]?.Exists != true)
                    continue;

                var temp = allItems[i];

                if (Distances[i] > Settings.ENTITY_INTERACTION_MAX_DISTANCE_RENDER)
                    continue;

                if (!Utils.GetScreenCoordFromWorldCoord(temp.GetCoords(true), ref screenX, ref screenY))
                    continue;

                if (!Settings.Interface.HideIOGNames)
                {
                    Utils.DrawText($"{temp.GetData<string>("Name")} (x{temp.GetData<int>("Amount")})", screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                }

                if (i == minIdx && !Settings.Interface.HideInteractionBtn)
                    Utils.DrawText(KeyBinds.Binds[KeyBinds.Types.TakeItem].GetKeyString(), screenX, screenY + 0.025f, 255, 0, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
            }
        }
        #endregion
    }
}
