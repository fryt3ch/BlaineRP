using RAGE;
using RAGE.Elements;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BCRPClient.Sync
{
    class AttachSystem : Events.Script
    {
        public static string AttachedObjectsKey = "AttachedObjects";
        public static string AttachedEntitiesKey = "AttachedEntities";

        public enum Types
        {
            PushVehicleFront,
            PushVehicleBack,
            Phone,
            VehKey,

            WeaponRightTight,
            WeaponLeftTight,
            WeaponRightBack,
            WeaponLeftBack,

            Carry,
            PiggyBack,
            Hostage,

            VehicleTrunk, VehicleTrunkForced,
        }

        #region Classes
        public class AttachmentData
        {
            public int BoneID;

            public Vector3 PositionOffset = null;
            public Vector3 Rotation = null;

            public bool UseSoftPinning;
            public bool Collision;
            public bool IsPed;
            public int VertexIndex;
            public bool FixedRot;

            public AttachmentData(int BoneID, Vector3 PositionOffset, Vector3 Rotation, bool UseSoftPinning, bool Collision, bool IsPed, int VertexIndex, bool FixedRot)
            {
                this.BoneID = BoneID;
                this.PositionOffset = PositionOffset;
                this.Rotation = Rotation;

                this.UseSoftPinning = UseSoftPinning;
                this.Collision = Collision;
                this.IsPed = IsPed;
                this.VertexIndex = VertexIndex;
                this.FixedRot = FixedRot;
            }
        }

        public class AttachmentObjectNet
        {
            public int Id;
            public string Model;
            public Types Type;

            public AttachmentObjectNet(int Id, string Model, Types Type)
            {
                this.Id = Id;
                this.Model = Model;
                this.Type = Type;
            }

            public override bool Equals(object? obj)
            {
                if (obj is AttachmentObjectNet other)
                    return Id == other.Id && Model == other.Model && Type == other.Type;

                return false;
            }
            public override int GetHashCode() => Id.GetHashCode() + Model.GetHashCode() + Type.GetHashCode();
        }

        public class AttachmentEntityNet
        {
            public int Id;
            public RAGE.Elements.Type EntityType;
            public Types Type;

            public AttachmentEntityNet(int Id, RAGE.Elements.Type EntityType, Types Type)
            {
                this.Id = Id;
                this.EntityType = EntityType;
                this.Type = Type;
            }

            public override bool Equals(object? obj)
            {
                if (obj is AttachmentEntityNet other)
                    return Id == other.Id && EntityType == other.EntityType && Type == other.Type;

                return false;
            }
            public override int GetHashCode() => EntityType.GetHashCode() + Id.GetHashCode() + Type.GetHashCode();
        }

        public class AttachmentObject
        {
            public int Id;
            public GameEntity Object;
            public Types Type;

            public AttachmentObject(GameEntity Object, Types Type, int Id)
            {
                this.Id = Id;
                this.Object = Object;
                this.Type = Type;
            }
        }

        public class AttachmentEntity
        {
            public int RemoteID;
            public RAGE.Elements.Type EntityType;
            public Types Type;

            public AttachmentEntity(int RemoteID, RAGE.Elements.Type EntityType, Types Type)
            {
                this.RemoteID = RemoteID;
                this.EntityType = EntityType;
                this.Type = Type;
            }
        }
        #endregion

        public static Dictionary<Types, AttachmentData> Attachments = new Dictionary<Types, AttachmentData>()
        {
            { Types.PushVehicleFront, new AttachmentData(6286, new Vector3(0f, 0.35f, 0.95f), new Vector3(0f, 0f, 180f), false, false, true, 2, true) },
            { Types.PushVehicleBack, new AttachmentData(6286, new Vector3(0f, -0.6f, 0.95f), new Vector3(0f, 0f, 0f), false, false, true, 2, true) },
            { Types.Phone, new AttachmentData(6286, new Vector3(0.05f, 0.01f, -0.025f), new Vector3(70f, -30f, 135f), false, false, false, 2, true) },
            { Types.VehKey, new AttachmentData(6286, new Vector3(0.08f, 0.04f, -0.015f), new Vector3(175f, -115f, -90f), false, false, false, 2, true) },

            { Types.WeaponLeftTight, new AttachmentData(58271, new Vector3(0.08f, 0.03f, -0.1f), new Vector3(-80.77f, 0f, 0f), false, false, false, 2, true) },
            { Types.WeaponRightTight, new AttachmentData(51826, new Vector3(0.02f, 0.06f, 0.1f), new Vector3(-100f, 0f, 0f), false, false, false, 2, true) },
            { Types.WeaponLeftBack, new AttachmentData(24818, new Vector3(-0.1f, -0.15f, 0.11f), new Vector3(-180f, 0f, 0f), false, false, false, 2, true) },
            { Types.WeaponRightBack, new AttachmentData(24818, new Vector3(-0.1f, -0.15f, -0.13f), new Vector3(0f, 0f, 3.5f), false, false, false, 2, true) },

            { Types.Carry, new AttachmentData(0, new Vector3(0.23f, 0.18f, 0.65f), new Vector3(0.5f, 0.5f, 15f), false, false, false, 2, true) },
            { Types.PiggyBack, new AttachmentData(0, new Vector3(0f, -0.07f, 0.45f), new Vector3(0f, 0f, 0f), false, false, false, 2, true) },
            { Types.Hostage, new AttachmentData(0, new Vector3(-0.24f, 0.11f, 0f), new Vector3(0.5f, 0.5f, 0f), false, false, false, 2, true) },

            { Types.VehicleTrunk, new AttachmentData(-1, new Vector3(0f, 0.5f, 0.4f), new Vector3(0f, 0f, 0f), false, false, false, 2, true) },
            { Types.VehicleTrunkForced, new AttachmentData(-1, new Vector3(0f, 0.5f, 0.4f), new Vector3(0f, 0f, 0f), false, false, false, 2, true) },
        };

        private static Dictionary<string, uint> Models = new Dictionary<string, uint>()
        {
            { "phone", RAGE.Util.Joaat.Hash("prop_phone_ing") },
            { "veh_key", RAGE.Util.Joaat.Hash("lr_prop_carkey_fob") },

            { "w_asrifle", RAGE.Util.Joaat.Hash("w_ar_assaultrifle") },
        };

        public AttachSystem()
        {
            Player.LocalPlayer.SetData(AttachedObjectsKey, new List<AttachmentObject>());
            Player.LocalPlayer.SetData(AttachedEntitiesKey, new List<AttachmentEntity>());

            #region New Entity Stream
            Events.OnEntityStreamIn += (Entity entity) =>
            {
                if (entity.IsLocal)
                    return;

                var objects = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey, null);

                if (objects == null)
                    return;

                var listObjectsNet = Utils.ConvertJArrayToList<AttachmentObjectNet>(objects);

                entity.SetData(AttachedObjectsKey, new List<AttachmentObject>());

                foreach (var x in listObjectsNet)
                    AttachObject(x.Id, x.Model, entity, x.Type, true);

                entity.SetData(AttachedEntitiesKey, new List<AttachmentEntity>());

                var entities = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey, null);

                if (entities == null)
                    return;

                var listEntitiesNet = Utils.ConvertJArrayToList<AttachmentEntityNet>(entities);

                foreach (var x in listEntitiesNet)
                    AttachEntity(entity, x.Type, x.Id, x.EntityType, true);
            };

            Events.OnEntityStreamOut += (Entity entity) =>
            {
                if (entity.IsLocal)
                    return;

                var listObjects = entity.GetData<List<AttachmentObject>>(AttachedObjectsKey);

                if (listObjects == null)
                    return;

                foreach (var obj in listObjects.ToList())
                    DetachObject(entity, obj.Id);

                listObjects.Clear();
                entity.ResetData(AttachedObjectsKey);

                var listEntities = entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);

                foreach (var obj in listEntities.ToList())
                    DetachEntity(entity, obj.RemoteID, obj.EntityType);

                listEntities.Clear();
                entity.ResetData(AttachedEntitiesKey);
            };
            #endregion

            #region Events
            Events.AddDataHandler(AttachedEntitiesKey, (Entity entity, object value, object oldValue) =>
            {
                if (entity.IsLocal)
                    return;

                if (!entity.HasData(AttachedEntitiesKey))
                    return;

                var currentListEntitiesNet = oldValue == null ? new List<AttachmentEntityNet>() : Utils.ConvertJArrayToList<AttachmentEntityNet>((Newtonsoft.Json.Linq.JArray)oldValue);
                var newListEntitiesNet = Utils.ConvertJArrayToList<AttachmentEntityNet>((Newtonsoft.Json.Linq.JArray)value);

                foreach (var x in currentListEntitiesNet.Union(newListEntitiesNet))
                {
                    if (newListEntitiesNet.Contains(x))
                    {
                        if (currentListEntitiesNet.Contains(x))
                            continue;
                        else
                            AttachEntity(entity, x.Type, x.Id, x.EntityType, false);
                    }
                    else
                        DetachEntity(entity, x.Id, x.EntityType);
                }
            });

            Events.AddDataHandler(AttachedObjectsKey, (Entity entity, object value, object oldValue) =>
            {
                if (entity.IsLocal)
                    return;

                if (!entity.HasData(AttachedObjectsKey))
                    return;

                var currentListEntitiesNet = oldValue == null ? new List<AttachmentObjectNet>() : Utils.ConvertJArrayToList<AttachmentObjectNet>((Newtonsoft.Json.Linq.JArray)oldValue);
                var newListEntitiesNet = Utils.ConvertJArrayToList<AttachmentObjectNet>((Newtonsoft.Json.Linq.JArray)value);

                foreach (var x in currentListEntitiesNet.Union(newListEntitiesNet))
                {
                    if (newListEntitiesNet.Contains(x))
                    {
                        if (currentListEntitiesNet.Contains(x))
                            continue;
                        else
                            AttachObject(x.Id, x.Model, entity, x.Type, false);
                    }
                    else
                        DetachObject(entity, x.Id);
                }
            });
            #endregion
        }

        #region Entity Methods
        public static void AttachEntity(Entity entity, Types type, int remoteId, RAGE.Elements.Type eType, bool streamIn = false)
        {
            GameEntity gTarget = Utils.GetGameEntityAtRemoteId(eType, remoteId);
            GameEntity gEntity = Utils.GetGameEntity(entity);

            if (gEntity == null || gTarget == null)
                return;

            Vector3 positionBase = Vector3.Zero;

            if (entity.Type == RAGE.Elements.Type.Vehicle)
            {
                (Vector3 Min, Vector3 Max) vehSize = entity.GetModelDimensions();

                if (type == Types.PushVehicleFront || type == Types.PushVehicleBack)
                {
                    if (type == Types.PushVehicleFront)
                    {
                        positionBase.Y = vehSize.Max.Y;
                        positionBase.Z = vehSize.Min.Z;
                    }
                    else
                    {
                        positionBase.Y = vehSize.Min.Y;
                        positionBase.Z = vehSize.Min.Z;
                    }
                }
                else if (type == Types.VehicleTrunk || type == Types.VehicleTrunkForced)
                {
                    positionBase.Y = -(vehSize.Max.Y - vehSize.Min.Y) / 2f;
                }
            }

            AttachmentData props = Attachments[type];

            var list = entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);

            if (list == null)
                return;

            (new AsyncTask(() =>
            {
                if (gEntity?.Exists != true)
                    return true;

                if (gEntity.Handle != 0 && gTarget.Handle != 0)
                {
                    RAGE.Game.Entity.AttachEntityToEntity(gTarget.Handle, gEntity.Handle, RAGE.Game.Ped.GetPedBoneIndex(gEntity.Handle, props.BoneID), positionBase.X + props.PositionOffset.X, positionBase.Y + props.PositionOffset.Y, positionBase.Z + props.PositionOffset.Z, props.Rotation.X, props.Rotation.Y, props.Rotation.Z, false, props.UseSoftPinning, props.Collision, props.IsPed, props.VertexIndex, props.FixedRot);

                    list.Add(new AttachmentEntity(remoteId, eType, type));

                    entity.SetData(AttachedEntitiesKey, list);

                    if (gTarget?.Type == RAGE.Elements.Type.Player && (gTarget as Player).Handle == Player.LocalPlayer.Handle)
                    {
                        TargetAction(type, entity);
                    }
                    else if (gEntity?.Type == RAGE.Elements.Type.Player && (gEntity as Player).Handle == Player.LocalPlayer.Handle)
                    {
                        RootAction(type, gTarget);
                    }

                    return true;
                }

                return false;
            }, 10, true, streamIn ? 500 : 0)).Run();
        }

        public static void DetachEntity(Entity entity, int remoteId, RAGE.Elements.Type eType)
        {
            var list = entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);

            if (list == null)
                return;

            var item = list.Where(x => x.EntityType == eType && x.RemoteID == remoteId).FirstOrDefault();

            if (item == null)
                return;

            GameEntity gEntity = Utils.GetGameEntity(entity);
            GameEntity gTarget = Utils.GetGameEntityAtRemoteId(eType, remoteId);

            AttachmentData props = Attachments[item.Type];

            list.Remove(item);

            entity.SetData(AttachedEntitiesKey, list);

            if (gTarget == null || gEntity == null)
                return;

            RAGE.Game.Entity.DetachEntity(gTarget.Handle, true, props.Collision);

            if (gTarget?.Type == RAGE.Elements.Type.Player && (gTarget as Player).Handle == Player.LocalPlayer.Handle)
            {
                TargetAction(item.Type, null);
            }
            else if (gEntity?.Type == RAGE.Elements.Type.Player && (gEntity as Player).Handle == Player.LocalPlayer.Handle)
            {
                RootAction(item.Type, null);
            }
        }
        #endregion

        #region Object Methods
        public static void AttachObject(int id, string name, Entity target, Types type, bool streamIn = false)
        {
            if (!Models.ContainsKey(name))
                return;

            var hash = Models[name];

            Utils.RequestModel(hash);

            GameEntity gTarget = Utils.GetGameEntity(target);

            if (gTarget == null)
                return;

            if (gTarget.Handle == Player.LocalPlayer.Handle)
            {
                if (type == Types.Phone)
                    return;
            }

            var list = target.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (list == null)
                return;

            GameEntity gEntity = new MapObject(hash, target.Position, Vector3.Zero, 255, target.Dimension);

            if (gEntity == null)
                return;

            (gEntity as MapObject).Hidden = true;

            AttachmentData props = Attachments[type];

            Vector3 positionBase = Vector3.Zero;

            list.Add(new AttachmentObject(gEntity, type, id));

            target.SetData(AttachedObjectsKey, list);

            (new AsyncTask(() =>
            {
                if (gEntity?.Exists != true)
                    return true;

                if (gEntity.Handle != 0 && gTarget.Handle != 0)
                {
                    RAGE.Game.Entity.AttachEntityToEntity(gEntity.Handle, gTarget.Handle, RAGE.Game.Ped.GetPedBoneIndex(gTarget.Handle, props.BoneID), positionBase.X + props.PositionOffset.X, positionBase.Y + props.PositionOffset.Y, positionBase.Z + props.PositionOffset.Z, props.Rotation.X, props.Rotation.Y, props.Rotation.Z, false, props.UseSoftPinning, props.Collision, props.IsPed, props.VertexIndex, props.FixedRot);

                    (gEntity as MapObject).Hidden = false;

                    //Utils.ConsoleOutput(streamIn);

                    return true;
                }

                return false;
            }, 10, true, streamIn ? 500 : 0)).Run();
        }

        public static void DetachObject(Entity target, int id)
        {
            var list = target.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (list == null)
                return;

            var item = list.Where(x => x.Id == id).FirstOrDefault();

            if (item == null)
                return;

            GameEntity gEntity = item.Object;

            AttachmentData props = Attachments[item.Type];

            RAGE.Game.Entity.DetachEntity(gEntity.Handle, false, props.Collision);

            if (gEntity.Type == RAGE.Elements.Type.Object)
                gEntity.Destroy();

            gEntity = null;

            list.Remove(item);

            target.SetData(AttachedObjectsKey, list);
        }

        public static void ReattachObjects(Entity target, bool streamIn)
        {
            if (target == null)
                return;

            if (!target.HasData(AttachedObjectsKey))
                return;

            var list = target.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (list == null)
                return;

            list = list.ToList();

            if (list == null)
                return;

            foreach (var x in list)
            {
                var name = Models.Where(y => y.Value == x.Object.Model).Select(x => x.Key).FirstOrDefault();

                if (name == null)
                    continue;

                DetachObject(target, x.Id);
                AttachObject(x.Id, name, target, x.Type, streamIn);
            }
        }

        private static Dictionary<Types[], (Action On, Action Off, Action Loop)?> TargetActions = new Dictionary<Types[], (Action On, Action Off, Action Loop)?>()
        {
            {
                new Types[] { Types.PushVehicleFront, Types.PushVehicleBack },

                (
                new Action(() =>
                {
                    var veh = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Vehicle;

                    if (veh == null)
                        return;

                    Sync.PushVehicle.On(true, veh);
                }),

                new Action(() =>
                {
                    Sync.PushVehicle.Off(true);
                }),

                new Action(() =>
                {
                    var veh = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Vehicle;

                    if (veh == null || Utils.AnyOnFootMovingControlPressed() || !Utils.CanDoSomething(Sync.PushVehicle.ActionsToCheckLoop) || !veh.Exists || veh.GetIsEngineRunning() || veh.HasCollidedWithAnything() || Vector3.Distance(Player.LocalPlayer.Position, veh.GetCoords(false)) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                    {
                        GameEvents.Update -= GetTargetActions(Types.PushVehicleFront).Value.Loop.Invoke;

                        Sync.PushVehicle.Off(false);
                    }
                })
                )
            }
        };

        private static (Action On, Action Off, Action Loop)? GetTargetActions(Types type) => TargetActions.Where(x => x.Key.Contains(type)).Select(x => x.Value).FirstOrDefault();

        public static void TargetAction(Types type, Entity root)
        {
            var data = Sync.Players.GetData(Player.LocalPlayer);

            if (data == null)
                return;

            data.IsAttachedTo = root;

            var actions = GetTargetActions(type);

            if (actions == null)
                return;

            if (root == null)
            {
                if (actions.Value.Loop != null)
                    GameEvents.Update -= actions.Value.Loop.Invoke;

                actions.Value.Off?.Invoke();
            }
            else
            {
                actions.Value.On?.Invoke();

                if (actions.Value.Loop != null)
                {
                    GameEvents.Update -= actions.Value.Loop.Invoke;
                    GameEvents.Update += actions.Value.Loop.Invoke;
                }
            }
        }

        public static void RootAction(Types type, Entity target)
        {
            var data = Sync.Players.GetData(Player.LocalPlayer);

            if (data == null)
                return;
        }
        #endregion
    }
}
