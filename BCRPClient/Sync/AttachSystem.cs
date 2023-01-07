﻿using RAGE;
using RAGE.Elements;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BCRPClient.Sync
{
    public class AttachSystem : Events.Script
    {
        public static string AttachedObjectsKey = "AttachedObjects";
        public static string AttachedEntitiesKey = "AttachedEntities";

        public static Types[] StaticObjectsTypes = new Types[] { Types.WeaponRightTight, Types.WeaponLeftTight, Types.WeaponRightBack, Types.WeaponLeftBack };

        public enum Types
        {
            /// <summary>Прикрепление СЕРВЕРНОГО трейлера к СЕРВЕРНОМУ транспорту</summary>
            VehicleTrailer,

            /// <summary>Прикрепление ЛОКАЛЬНОГО трейлера (создается локально при прикреплении) к СЕРВЕРНОЙ лодке</summary>
            TrailerObjOnBoat,
            /// <summary>Прикрепление ЛОКАЛЬНОГО трейлера (создается локально при прикреплении) к СЕРВЕРНОМУ транспорту</summary>
            TrailerObjOnVehicle,
            /// <summary>Прикрепление СЕРВЕРНОГО транспорта к СЕРВЕРНОЙ лодке (к которой должен быть прикреплен TrailerObjOnBoat)</summary>
            VehicleTrailerObjBoat,

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

            ItemCigHand,
            ItemCig1Hand,
            ItemCig2Hand,
            ItemCig3Hand,

            ItemCigMouth,
            ItemCig1Mouth,
            ItemCig2Mouth,
            ItemCig3Mouth,

            ItemBurger,
            ItemChips,
            ItemHotdog,
            ItemChocolate,
            ItemPizza,
            ItemCola,
            ItemBeer,
            ItemVodka,
            ItemRum,
            ItemVegSmoothie,
            ItemSmoothie,
            ItemMilkshake,
            ItemMilk,

            ItemBandage,
            ItemMedKit,
        }

        private static Dictionary<int, List<int>> StreamedAttachments { get; set; } = new Dictionary<int, List<int>>();

        public static void AddLocalAttachment(int fromHandle, int toHandle)
        {
            var list = StreamedAttachments.GetValueOrDefault(toHandle);

            if (list == null)
            {
                list = new List<int>() { fromHandle };

                StreamedAttachments.Add(toHandle, list);
            }
            else
            {
                if (!list.Contains(fromHandle))
                    list.Add(fromHandle);
            }

            Utils.ConsoleOutput($"Attached {fromHandle} to {toHandle}");
        }

        public static void RemoveLocalAttachment(int fromHandle, int toHandle)
        {
            var list = StreamedAttachments.GetValueOrDefault(toHandle);

            if (list == null)
                return;

            if (list.Remove(fromHandle) && list.Count == 0)
                StreamedAttachments.Remove(toHandle);

            Utils.ConsoleOutput($"Detached {fromHandle} from {toHandle}");
        }

        public static void DetachAllFromLocalEntity(int toHandle)
        {
            RAGE.Game.Entity.DetachEntity(toHandle, false, false);

            var list = StreamedAttachments.GetValueOrDefault(toHandle);

            if (list == null)
                return;

            list.ForEach(x =>
            {
                RAGE.Game.Entity.DetachEntity(x, false, false);
            });

            StreamedAttachments.Remove(toHandle);
        }

        #region Classes
        public class AttachmentData
        {
            public int BoneID;

            public Vector3 PositionOffset { get; set; }
            public Vector3 Rotation { get; set; }

            public bool UseSoftPinning { get; set; }

            public bool Collision { get; set; }

            public bool IsPed { get; set; }

            public int VertexIndex { get; set; }

            public bool FixedRot { get; set; }

            public Action<object[]> EntityAction;

            public AttachmentData(int BoneID, Vector3 PositionOffset, Vector3 Rotation, bool UseSoftPinning, bool Collision, bool IsPed, int VertexIndex, bool FixedRot, Action<object[]> EntityAction = null)
            {
                this.BoneID = BoneID;
                this.PositionOffset = PositionOffset;
                this.Rotation = Rotation;

                this.UseSoftPinning = UseSoftPinning;
                this.Collision = Collision;
                this.IsPed = IsPed;
                this.VertexIndex = VertexIndex;
                this.FixedRot = FixedRot;

                this.EntityAction = EntityAction;
            }
        }

        public class AttachmentObjectNet
        {
            [JsonProperty(PropertyName = "M")]
            public uint Model { get; set; }

            [JsonProperty(PropertyName = "D")]
            public string SyncData { get; set; }

            [JsonProperty(PropertyName = "T")]
            public Types Type { get; set; }

            public AttachmentObjectNet()
            {

            }

            public override bool Equals(object obj)
            {
                if (obj is AttachmentObjectNet other)
                    return SyncData == other.SyncData && Model == other.Model && Type == other.Type;

                return false;
            }

            public override int GetHashCode() => SyncData?.GetHashCode() ?? 0 + Model.GetHashCode() + Type.GetHashCode();
        }

        public class AttachmentEntityNet
        {
            [JsonProperty(PropertyName = "E")]
            public RAGE.Elements.Type EntityType { get; set; }

            [JsonProperty(PropertyName = "I")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "T")]
            public Types Type { get; set; }

            public AttachmentEntityNet()
            {

            }

            public override bool Equals(object obj)
            {
                if (obj is AttachmentEntityNet other)
                    return Id == other.Id && EntityType == other.EntityType && Type == other.Type;

                return false;
            }

            public override int GetHashCode() => EntityType.GetHashCode() + Id.GetHashCode() + Type.GetHashCode();
        }

        public class AttachmentObject
        {
            public string SyncData { get; set; }

            public GameEntity Object { get; set; }

            public Types Type { get; set; }

            public uint Model { get; set; }

            public AttachmentObject(GameEntity Object, Types Type, uint Model, string SyncData)
            {
                this.SyncData = SyncData;
                this.Object = Object;
                this.Type = Type;

                this.Model = Model;
            }
        }

        public class AttachmentEntity
        {
            public int RemoteID { get; set; }

            public RAGE.Elements.Type EntityType { get; set; }

            public Types Type { get; set; }

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

            { Types.TrailerObjOnBoat, new AttachmentData(20, new Vector3(0f, -1f, 0.25f), new Vector3(0f, 0f, 0f), false, true, false, 2, true) },

            {
                Types.ItemCigHand, new AttachmentData(64097, new Vector3(0.02f, 0.02f, -0.008f), new Vector3(100f, 0f, 100f), false, false, false, 2, true, async (args) =>
                {
                    var gEntity = (MapObject)args[0];

                    await Utils.RequestPtfx("core");

                    gEntity.SetData("PtfxHandle", RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle,  -0.05f, 0f, 0f, 0f, 0f, 0f, 0.04f, false, false, false));
                })
            },

            {
                Types.ItemCig1Hand, new AttachmentData(64097, new Vector3(0.02f, 0.02f + 0.0365f, -0.008f), new Vector3(100f, 0f, -80f), false, false, false, 2, true, async (args) =>
                {
                    var gEntity = (MapObject)args[0];

                    await Utils.RequestPtfx("core");

                    gEntity.SetData("PtfxHandle", RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle,  0.125f, 0f, 0f, 0f, 0f, 0f, 0.05f, false, false, false));
                })
            },

            {
                Types.ItemCig2Hand, new AttachmentData(64097, new Vector3(0.02f, 0.02f, -0.008f), new Vector3(100f, 0f, -100f), false, false, false, 2, true, async (args) =>
                {
                    var gEntity = (MapObject)args[0];

                    await Utils.RequestPtfx("core");

                    gEntity.SetData("PtfxHandle", RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle,  0.05f, 0f, 0f, 0f, 0f, 0f, 0.075f, false, false, false));
                })
            },

            {
                Types.ItemCig3Hand, new AttachmentData(64097, new Vector3(0.02f, 0.02f, -0.008f), new Vector3(100f, 0f, 100f), false, false, false, 2, true, async (args) =>
                {
                    var gEntity = (MapObject)args[0];

                    await Utils.RequestPtfx("core");

                    gEntity.SetData("PtfxHandle", RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle,  -0.09f, 0f, 0f, 0f, 0f, 0f, 0.06f, false, false, false));
                })
            },

            {
                Types.ItemCigMouth, new AttachmentData(47419, new Vector3(0.015f, -0.009f, 0.003f), new Vector3(55f, 0f, 110f), false, false, false, 2, true, async (args) =>
                {
                    var gEntity = (MapObject)args[0];

                    await Utils.RequestPtfx("core");

                    gEntity.SetData("PtfxHandle", RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle,  -0.05f, 0f, 0f, 0f, 0f, 0f, 0.04f, false, false, false));
                })
            },

            {
                Types.ItemCig1Mouth, new AttachmentData(47419, new Vector3(0.001f, 0.036f, 0.005f), new Vector3(55f, 0f, -70f), false, false, false, 2, true, async (args) =>
                {
                    var gEntity = (MapObject)args[0];

                    await Utils.RequestPtfx("core");

                    gEntity.SetData("PtfxHandle", RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle,  0.125f, 0f, 0f, 0f, 0f, 0f, 0.05f, false, false, false));
                })
            },

            {
                Types.ItemCig2Mouth, new AttachmentData(47419, new Vector3(0.01f, 0f, 0f), new Vector3(50f, 0f, -80f), false, false, false, 2, true, async (args) =>
                {
                    var gEntity = (MapObject)args[0];

                    await Utils.RequestPtfx("core");

                    gEntity.SetData("PtfxHandle", RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle,  0.05f, 0f, 0f, 0f, 0f, 0f, 0.075f, false, false, false));
                })
            },

            {
                Types.ItemCig3Mouth, new AttachmentData(47419, new Vector3(0.01f, 0f, 0f), new Vector3(50f, 0f, 80f), false, false, false, 2, true, async (args) =>
                {
                    var gEntity = (MapObject)args[0];

                    await Utils.RequestPtfx("core");

                    gEntity.SetData("PtfxHandle", RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle,  -0.09f, 0f, 0f, 0f, 0f, 0f, 0.06f, false, false, false));
                })
            },

            { Types.ItemBandage, new AttachmentData(36029, new Vector3(-0.04f, 0f, -0.01f), new Vector3(160f, 0f, 90f), false, false, false, 2, true) },
            { Types.ItemMedKit, new AttachmentData(36029, new Vector3(0.03f, 0.01f, 0.12f), new Vector3(180f, -10f, 90f), false, false, false, 2, true) },

            { Types.ItemChips, new AttachmentData(28422, new Vector3(-0.04f, 0.02f, -0.04f), new Vector3(15f, 20f, 10f), false, false, false, 2, true) },
            { Types.ItemBurger, new AttachmentData(28422, new Vector3(-0.01f, -0.01f, 0f), new Vector3(20f, 0f, 0f), false, false, false, 2, true) },
            { Types.ItemHotdog, new AttachmentData(60309, new Vector3(0.05f, 0.02f, -0.01f), new Vector3(0f, 0f, 90f), false, false, false, 2, true) },
            { Types.ItemChocolate, new AttachmentData(28422, new Vector3(-0.01f, -0.01f, 0f), new Vector3(20f, 0f, 0f), false, false, false, 2, true) },
            { Types.ItemPizza, new AttachmentData(28422, new Vector3(-0.01f, -0.01f, 0f), new Vector3(20f, 0f, 0f), false, false, false, 2, true) },
            { Types.ItemBeer, new AttachmentData(28422, new Vector3(0.012f, 0.028f, -0.1f), new Vector3(5f, 0f, 0f), false, false, false, 2, true) },
        };

        public static async System.Threading.Tasks.Task OnEntityStreamIn(Entity entity)
        {
            if (entity.IsLocal)
                return;

            var objects = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey, null);

            if (objects == null)
                return;

            var listObjectsNet = Utils.ConvertJArrayToList<AttachmentObjectNet>(objects);

            entity.SetData(AttachedObjectsKey, new List<AttachmentObject>());

            foreach (var x in listObjectsNet)
                await AttachObject(x.Model, entity, x.Type, x.SyncData);

            entity.SetData(AttachedEntitiesKey, new List<AttachmentEntity>());

            var entities = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey, null);

            if (entities == null)
                return;

            var listEntitiesNet = Utils.ConvertJArrayToList<AttachmentEntityNet>(entities);

            foreach (var x in listEntitiesNet)
                await AttachEntity(entity, x.Type, x.Id, x.EntityType);
        }

        public static async System.Threading.Tasks.Task OnEntityStreamOut(Entity entity)
        {
            if (entity.IsLocal)
            {
                var gEntity = entity as GameEntity;

                if (gEntity != null)
                    DetachAllFromLocalEntity(gEntity.Handle);

                return;
            }

            var listObjects = entity.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (listObjects == null)
                return;

            foreach (var obj in listObjects.ToList())
                DetachObject(entity, obj.Type);

            listObjects.Clear();
            entity.ResetData(AttachedObjectsKey);

            var listEntities = entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);

            foreach (var obj in listEntities.ToList())
                DetachEntity(entity, obj.RemoteID, obj.EntityType);

            listEntities.Clear();
            entity.ResetData(AttachedEntitiesKey);
        }

        public AttachSystem()
        {
            Player.LocalPlayer.SetData(AttachedObjectsKey, new List<AttachmentObject>());
            Player.LocalPlayer.SetData(AttachedEntitiesKey, new List<AttachmentEntity>());

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
                            AttachEntity(entity, x.Type, x.Id, x.EntityType);
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

                //Utils.ConsoleOutput(RAGE.Util.Json.Serialize(value));

                var currentListEntitiesNet = oldValue == null ? new List<AttachmentObjectNet>() : Utils.ConvertJArrayToList<AttachmentObjectNet>((Newtonsoft.Json.Linq.JArray)oldValue);
                var newListEntitiesNet = Utils.ConvertJArrayToList<AttachmentObjectNet>((Newtonsoft.Json.Linq.JArray)value);

                foreach (var x in currentListEntitiesNet.Union(newListEntitiesNet))
                {
                    if (newListEntitiesNet.Contains(x))
                    {
                        if (currentListEntitiesNet.Contains(x))
                            continue;
                        else
                            AttachObject(x.Model, entity, x.Type, x.SyncData);
                    }
                    else
                        DetachObject(entity, x.Type);
                }
            });
            #endregion
        }

        #region Entity Methods
        public static async System.Threading.Tasks.Task AttachEntity(Entity entity, Types type, int remoteId, RAGE.Elements.Type eType)
        {
            GameEntity gTarget = Utils.GetGameEntityAtRemoteId(eType, remoteId);
            GameEntity gEntity = Utils.GetGameEntity(entity);

            if (gEntity == null || gTarget == null)
                return;

            Vector3 positionBase = Vector3.Zero;

            if (entity is Vehicle veh)
            {
                if (type == Types.VehicleTrailer)
                {

                }
                else
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
            }

            AttachmentData props = Attachments.GetValueOrDefault(type);

            var list = entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);

            if (list == null)
                return;

            if (props != null)
                RAGE.Game.Entity.AttachEntityToEntity(gTarget.Handle, gEntity.Handle, RAGE.Game.Ped.GetPedBoneIndex(gEntity.Handle, props.BoneID), positionBase.X + props.PositionOffset.X, positionBase.Y + props.PositionOffset.Y, positionBase.Z + props.PositionOffset.Z, props.Rotation.X, props.Rotation.Y, props.Rotation.Z, false, props.UseSoftPinning, props.Collision, props.IsPed, props.VertexIndex, props.FixedRot);

            if (type == Types.VehicleTrailer)
            {
                RAGE.Game.Vehicle.AttachVehicleToTrailer(gEntity.Handle, gTarget.Handle, float.MaxValue);
            }
            else if (type == Types.VehicleTrailerObjBoat)
            {
                var trailerObj = gEntity.GetData<List<AttachmentObject>>(AttachedObjectsKey)?.Where(x => x.Type == Types.TrailerObjOnBoat).FirstOrDefault()?.Object;

                if (trailerObj != null)
                {
                    RAGE.Game.Vehicle.AttachVehicleToTrailer(gTarget.Handle, trailerObj.Handle, float.MaxValue);
                }
            }

            list.Add(new AttachmentEntity(remoteId, eType, type));

            entity.SetData(AttachedEntitiesKey, list);

            if (gTarget is Player tPlayer && tPlayer.Handle == Player.LocalPlayer.Handle)
            {
                TargetAction(type, entity);
            }
            else if (gEntity is Player ePlayer && ePlayer.Handle == Player.LocalPlayer.Handle)
            {
                RootAction(type, gTarget);
            }
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

            AttachmentData props = Attachments.GetValueOrDefault(item.Type);

            list.Remove(item);

            entity.SetData(AttachedEntitiesKey, list);

            if (gTarget == null || gEntity == null)
                return;

            if (props != null)
                RAGE.Game.Entity.DetachEntity(gTarget.Handle, true, props.Collision);

            if (item.Type == Types.VehicleTrailer || item.Type == Types.VehicleTrailerObjBoat)
            {
                RAGE.Game.Vehicle.DetachVehicleFromTrailer(gTarget.Handle);
            }

            if (gTarget is Player tPlayer && tPlayer.Handle == Player.LocalPlayer.Handle)
            {
                TargetAction(item.Type, null);
            }
            else if (gEntity is Player ePlayer && ePlayer.Handle == Player.LocalPlayer.Handle)
            {
                RootAction(item.Type, null);
            }
        }
        #endregion

        #region Object Methods
        public static async System.Threading.Tasks.Task AttachObject(uint hash, Entity target, Types type, string syncData)
        {
            await Utils.RequestModel(hash);

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

            GameEntity gEntity = null;

            if (type >= Types.WeaponRightTight && type <= Types.WeaponLeftBack)
            {
                await Utils.RequestWeaponAsset(hash);

                gEntity = new MapObject(RAGE.Game.Weapon.CreateWeaponObject(hash, 0, target.Position.X, target.Position.Y, target.Position.Z, true, 0f, 0, 0, 0));

                if (syncData != null)
                    Sync.WeaponSystem.UpdateWeaponObjectComponents(gEntity.Handle, hash, syncData);
            }
            else if (type >= Types.TrailerObjOnBoat && type <= Types.TrailerObjOnVehicle)
            {
                var vTypeData = Data.Vehicles.GetByModel(hash);

                var rot = RAGE.Game.Entity.GetEntityRotation(gTarget.Handle, 2);

                var veh = new Vehicle(hash, target.Position, rot.Z, "", 255, true, 0, 0, target.Dimension);

                veh.SetCanBeVisiblyDamaged(false); veh.SetCanBreak(false); veh.SetDirtLevel(0f); veh.SetDisablePetrolTankDamage(true); veh.SetDisablePetrolTankFires(true); veh.SetInvincible(true);

                var targetVeh = target as Vehicle;

                var targetData = Sync.Vehicles.GetData(targetVeh);

                if (targetData != null)
                {
                    if (targetData.IsFrozen)
                        veh.FreezePosition(true);
                }

                gEntity = veh;

                gEntity.SetData("TrailerSync::Owner", targetVeh);
            }
            else
            {
                gEntity = new MapObject(RAGE.Game.Object.CreateObject(hash, target.Position.X, target.Position.Y, target.Position.Z, false, false, false));
            }

            if (gEntity == null)
                return;

            AttachmentData props = Attachments[type];

            Vector3 positionBase = Vector3.Zero;

            list.Add(new AttachmentObject(gEntity, type, hash, syncData));

            target.SetData(AttachedObjectsKey, list);

            if (type >= Types.TrailerObjOnBoat && type <= Types.TrailerObjOnVehicle)
            {
                AddLocalAttachment(gTarget.Handle, gEntity.Handle);

                RAGE.Game.Entity.AttachEntityToEntity(gTarget.Handle, gEntity.Handle, RAGE.Game.Ped.GetPedBoneIndex(gTarget.Handle, props.BoneID), positionBase.X + props.PositionOffset.X, positionBase.Y + props.PositionOffset.Y, positionBase.Z + props.PositionOffset.Z, props.Rotation.X, props.Rotation.Y, props.Rotation.Z, false, props.UseSoftPinning, props.Collision, props.IsPed, props.VertexIndex, props.FixedRot);
            }
            else
            {
                RAGE.Game.Entity.AttachEntityToEntity(gEntity.Handle, gTarget.Handle, RAGE.Game.Ped.GetPedBoneIndex(gTarget.Handle, props.BoneID), positionBase.X + props.PositionOffset.X, positionBase.Y + props.PositionOffset.Y, positionBase.Z + props.PositionOffset.Z, props.Rotation.X, props.Rotation.Y, props.Rotation.Z, false, props.UseSoftPinning, props.Collision, props.IsPed, props.VertexIndex, props.FixedRot);
            }

            if (gEntity is MapObject mObj)
            {
                mObj.Hidden = false;
            }

            props.EntityAction?.Invoke(new object[] { gEntity });

            if (gTarget is Player tPlayer && tPlayer.Handle == Player.LocalPlayer.Handle)
            {
                RootAction(type, gEntity);
            }
        }

        public static void DetachObject(Entity target, Types type)
        {
            var list = target.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (list == null)
                return;

            var item = list.Where(x => x.Type == type).FirstOrDefault();

            if (item == null)
                return;

            var gEntity = item.Object;
            var gTarget = Utils.GetGameEntity(target);

            if (gTarget == null || gEntity == null)
                return;

            AttachmentData props = Attachments[item.Type];

            if (type >= Types.TrailerObjOnBoat && type <= Types.TrailerObjOnVehicle)
            {
                RemoveLocalAttachment(gTarget.Handle, gEntity.Handle);

                RAGE.Game.Entity.DetachEntity(gTarget.Handle, false, props.Collision);
            }
            else
                RAGE.Game.Entity.DetachEntity(gEntity.Handle, false, props.Collision);

            if (gEntity.HasData("PtfxHandle"))
            {
                RAGE.Game.Graphics.StopParticleFxLooped(gEntity.GetData<int>("PtfxHandle"), false);
            }

            if (gEntity.IsLocal)
            {
                gEntity.Destroy();
            }

            gEntity = null;

            list.Remove(item);

            target.SetData(AttachedObjectsKey, list);

            if (gTarget is Player tPlayer && tPlayer.Handle == Player.LocalPlayer.Handle)
            {
                RootAction(item.Type, null);
            }
        }

        public static void ReattachObjects(Entity target)
        {
            var list = target.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (list == null)
                return;

            foreach (var x in list.ToList())
            {
                if (x == null)
                    continue;

                DetachObject(target, x.Type);

                AttachObject(x.Model, target, x.Type, x.SyncData);
            }
        }

        public static void DetachAllObjects(Entity target)
        {
            var list = target.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (list == null)
                return;

            foreach (var x in list.ToList())
            {
                if (x == null)
                    continue;

                DetachObject(target, x.Type);
            }
        }

        public static async System.Threading.Tasks.Task AttachAllObjects(Entity target)
        {
            var objects = target.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey, null);

            if (objects == null)
                return;

            var listObjectsNet = Utils.ConvertJArrayToList<AttachmentObjectNet>(objects);

            //target.SetData(AttachedObjectsKey, new List<AttachmentObject>());

            foreach (var x in listObjectsNet)
            {
                await AttachObject(x.Model, target, x.Type, x.SyncData);
            }
        }

        private static Dictionary<Types, Types> SameActionsTypes = new Dictionary<Types, Types>()
        {
            { Types.PushVehicleBack, Types.PushVehicleFront },

            { Types.ItemCig1Hand, Types.ItemCigHand },
            { Types.ItemCig2Hand, Types.ItemCigHand },
            { Types.ItemCig3Hand, Types.ItemCigHand },

            { Types.ItemCig1Mouth, Types.ItemCigMouth },
            { Types.ItemCig2Mouth, Types.ItemCigMouth },
            { Types.ItemCig3Mouth, Types.ItemCigMouth },
        };

        private static Dictionary<Types, (Action On, Action Off, Action Loop)?> TargetActions = new Dictionary<Types, (Action On, Action Off, Action Loop)?>()
        {
            {
                Types.PushVehicleFront,

                (
                    new Action(() =>
                    {
                        var veh = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Vehicle;

                        if (veh == null)
                            return;

                        Sync.PushVehicle.On(true, veh);

                        Sync.WeaponSystem.DisabledFiring = true;
                    }),

                    new Action(() =>
                    {
                        Sync.PushVehicle.Off(true);

                        Sync.WeaponSystem.DisabledFiring = false;
                    }),

                    new Action(() =>
                    {
                        var veh = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Vehicle;

                        if (veh?.Exists != true || Utils.AnyOnFootMovingControlPressed() || !Utils.CanDoSomething(Sync.PushVehicle.ActionsToCheckLoop) || veh.GetIsEngineRunning() || veh.HasCollidedWithAnything() || Vector3.Distance(Player.LocalPlayer.Position, veh.GetCoords(false)) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                        {
                            GameEvents.Update -= GetTargetActions(Types.PushVehicleFront).Value.Loop.Invoke;

                            Sync.PushVehicle.Off(false);
                        }
                        else
                        {
                            Utils.DrawText(Locale.General.Animations.CancelTextPushVehicle, 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
                        }
                    })
                )
            },

            {
                Types.VehicleTrunk,

                (
                    new Action(() =>
                    {
                        Sync.WeaponSystem.DisabledFiring = true;
                    }),

                    new Action(() =>
                    {
                        Sync.WeaponSystem.DisabledFiring = false;
                    }),

                    new Action(() =>
                    {
                        var root = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Vehicle;

                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        if (root?.Exists != true || bind.IsPressed || Vector3.Distance(Player.LocalPlayer.Position, root.GetRealPosition()) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                        {
                            GameEvents.Update -= GetTargetActions(Types.Carry).Value.Loop.Invoke;

                            Events.CallRemote("Players::StopInTrunk");
                        }
                        else
                        {
                            Utils.DrawText(string.Format(Locale.General.Animations.CancelTextInTrunk, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
                        }
                    })
                )
            },

            {
                Types.Carry,

                (
                    new Action(() =>
                    {
                        Sync.WeaponSystem.DisabledFiring = true;
                    }),

                    new Action(() =>
                    {
                        Sync.WeaponSystem.DisabledFiring = false;
                    }),

                    new Action(() =>
                    {
                        var root = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Player;

                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        if (root?.Exists != true || bind.IsPressed || Vector3.Distance(Player.LocalPlayer.Position, root.GetRealPosition()) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                        {
                            GameEvents.Update -= GetTargetActions(Types.Carry).Value.Loop.Invoke;

                            Events.CallRemote("Players::StopCarry");
                        }
                        else
                        {
                            Utils.DrawText(string.Format(Locale.General.Animations.CancelTextCarryB, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
                        }
                    })
                )
            },
        };

        private static Dictionary<Types, (Action On, Action Off, Action Loop)?> RootActions = new Dictionary<Types, (Action On, Action Off, Action Loop)?>()
        {
            {
                Types.Carry,

                (
                    new Action(() =>
                    {
                        Sync.WeaponSystem.DisabledFiring = true;
                    }),

                    new Action(() =>
                    {
                        Sync.WeaponSystem.DisabledFiring = false;
                    }),

                    new Action(() =>
                    {
                        var target = Player.LocalPlayer.GetData<List<AttachmentEntity>>(AttachedEntitiesKey).Where(x => x.Type == Types.Carry).Select(x => Utils.GetPlayerByRemoteId(x.RemoteID, true)).FirstOrDefault();

                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        if (target?.Exists != true || bind.IsPressed || Vector3.Distance(Player.LocalPlayer.Position, target.GetRealPosition()) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                        {
                            GameEvents.Update -= GetRootActions(Types.Carry).Value.Loop.Invoke;

                            Events.CallRemote("Players::StopCarry");
                        }
                        else
                        {
                            Utils.DrawText(string.Format(Locale.General.Animations.CancelTextCarryA, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
                        }
                    })
                )
            },

            {
                Types.ItemCigHand,

                (
                    new Action(() =>
                    {
                        Sync.WeaponSystem.DisabledFiring = true;

                        Player.LocalPlayer.SetData("Temp::Smoke::LastSent", DateTime.Now);
                    }),

                    new Action(() =>
                    {
                        Sync.WeaponSystem.DisabledFiring = false;

                        Player.LocalPlayer.ResetData("Temp::Smoke::LastSent");
                    }),

                    new Action(() =>
                    {
                        if (!Player.LocalPlayer.HasData("Smoke::Data::Puffs"))
                            return;

                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        var puffs = Player.LocalPlayer.GetData<int>("Smoke::Data::Puffs");

                        if (bind.IsPressed || Player.LocalPlayer.IsInWater() || puffs == 0)
                        {
                            GameEvents.Update -= GetRootActions(Types.ItemCigHand).Value.Loop.Invoke;

                            Events.CallRemote("Players::Smoke::Stop");
                        }
                        else
                        {

                            var lastSent = Player.LocalPlayer.GetData<DateTime>("Temp::Smoke::LastSent");

                            // lmb - do puff
                            if (!CEF.Cursor.IsVisible && RAGE.Game.Pad.IsDisabledControlJustPressed(0, 24))
                            {
                                if (!lastSent.IsSpam(1000, false, false))
                                {
                                    Events.CallRemote("Players::Smoke::Puff");

                                    Player.LocalPlayer.SetData("Temp::Smoke::LastSent", DateTime.Now);
                                }
                            }
                            // alt - to mouth
                            else if ((!CEF.Cursor.IsVisible && RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.LeftMenu)) || Player.LocalPlayer.Vehicle != null)
                            {
                                if (!lastSent.IsSpam(1000, false, false))
                                {
                                    Events.CallRemote("Players::Smoke::State", true);

                                    Player.LocalPlayer.SetData("Temp::Smoke::LastSent", DateTime.Now);
                                }
                            }

                            Utils.DrawText(string.Format(Locale.General.Animations.TextDoPuffSmoke, puffs), 0.5f, 0.90f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
                            Utils.DrawText(Locale.General.Animations.TextToMouthSmoke, 0.5f, 0.925f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
                            Utils.DrawText(string.Format(Locale.General.Animations.CancelTextSmoke, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
                        }
                    })
                )
            },

            {
               Types.ItemCigMouth,

                (
                    new Action(() =>
                    {
                        Player.LocalPlayer.SetData("Temp::Smoke::LastSent", DateTime.Now);
                    }),

                    new Action(() =>
                    {
                        Player.LocalPlayer.ResetData("Temp::Smoke::LastSent");
                    }),

                    new Action(() =>
                    {
                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        if (bind.IsPressed || Player.LocalPlayer.IsInWater())
                        {
                            GameEvents.Update -= GetRootActions(Types.ItemCigMouth).Value.Loop.Invoke;

                            Events.CallRemote("Players::Smoke::Stop");
                        }
                        else
                        {
                            var lastSent = Player.LocalPlayer.GetData<DateTime>("Temp::Smoke::LastSent");

                            if (Player.LocalPlayer.Vehicle == null)
                            {
                                // alt - to hand
                                if (!CEF.Cursor.IsVisible && RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.LeftMenu))
                                {
                                    if (!lastSent.IsSpam(1000, false, false))
                                    {
                                        Events.CallRemote("Players::Smoke::State", false);

                                        Player.LocalPlayer.SetData("Temp::Smoke::LastSent", DateTime.Now);
                                    }
                                }

                                Utils.DrawText(Locale.General.Animations.TextToHandSmoke, 0.5f, 0.925f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
                            }

                            Utils.DrawText(string.Format(Locale.General.Animations.CancelTextSmoke, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
                        }
                    })
                )
            },
        };

        private static (Action On, Action Off, Action Loop)? GetTargetActions(Types type)
        {
            var action = TargetActions.GetValueOrDefault(type);

            if (action == null)
            {
                Types sType;

                if (SameActionsTypes.TryGetValue(type, out sType))
                    return TargetActions.GetValueOrDefault(sType);

                return null;
            }

            return action;
        }

        private static (Action On, Action Off, Action Loop)? GetRootActions(Types type)
        {
            var action = RootActions.GetValueOrDefault(type);

            if (action == null)
            {
                Types sType;

                if (SameActionsTypes.TryGetValue(type, out sType))
                    return RootActions.GetValueOrDefault(sType);

                return null;
            }

            return action;
        }

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

            var actions = GetRootActions(type);

            if (actions == null)
                return;

            if (target == null)
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
        #endregion
    }
}
