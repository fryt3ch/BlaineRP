using Newtonsoft.Json;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Sync
{
    [Script(int.MaxValue)]
    public class AttachSystem 
    {
        public static string AttachedObjectsKey = "AttachedObjects";
        public static string AttachedEntitiesKey = "AttachedEntities";

        public static bool IsTypeStaticObject(Types type) => type >= Types.PedRingLeft3 && type < Types.VehKey;

        public static bool IsTypeObjectInHand(Types type) => type >= Types.VehKey && type < Types.VehicleTrailer;

        /*
            mp.game.streaming.requestNamedPtfxAsset('core');
            mp.game.graphics.setPtfxAssetNextCall('core');
            mp.game.graphics.startParticleFxLoopedOnEntity('water_cannon_spray', 2212114, 0.25, 0, 0, 0, 0, 0, 0.05, false, false, false);


            mp.game.graphics.removeParticleFxInRange(0, 0, 0, 1000000);
         */

        public enum Types
        {
            #region Entity-Object Attach | Типы, которые прикрепляют серверную сущность к клиентскому объекту (создается у всех клиентов в зоне стрима)

            #region Static Types | Типы, которые не открепляются при телепорте и не влияют на возможность совершения игроком каких-либо действий

            PedRingLeft3,
            PedRingRight3,

            WeaponRightTight,
            WeaponLeftTight,
            WeaponRightBack,
            WeaponLeftBack,

            PhoneSync,

            ParachuteSync,

            #endregion

            #region Object In Hand Types | Типы, наличие у игрока которых запрещает определенные действия (ведь предмет находится в руках)

            VehKey,

            Cuffs,
            CableCuffs,

            ItemFishingRodG, ItemFishG,

            ItemShovel,

            ItemMetalDetector,

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

            FarmPlantSmallShovel,

            FarmWateringCan,

            FarmOrangeBoxCarry,

            FarmMilkBucketCarry,

            EmsHealingBedFakeAttach,
            #endregion

            #endregion

            #region Entity-Entity Attach | Типы, которые прикрепляют серверную сущность с серверной сущности
            /// <summary>Прикрепление СЕРВЕРНОГО трейлера к СЕРВЕРНОМУ транспорту</summary>
            VehicleTrailer,

            /// <summary>Прикрепление ЛОКАЛЬНОГО трейлера (создается локально при прикреплении) к СЕРВЕРНОЙ лодке</summary>
            TrailerObjOnBoat,

            /// <summary>Прикрепление СЕРВЕРНОГО транспорта к СЕРВЕРНОЙ лодке (к которой должен быть прикреплен TrailerObjOnBoat)</summary>
            VehicleTrailerObjBoat,

            TractorTrailFarmHarv,

            PushVehicle,

            Carry,
            PiggyBack,
            Hostage,

            PoliceEscort,

            VehicleTrunk,

            #endregion
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

            //Utils.ConsoleOutput($"Attached {fromHandle} to {toHandle}");
        }

        public static void RemoveLocalAttachment(int fromHandle, int toHandle)
        {
            var list = StreamedAttachments.GetValueOrDefault(toHandle);

            if (list == null)
                return;

            if (list.Remove(fromHandle) && list.Count == 0)
                StreamedAttachments.Remove(toHandle);

            //Utils.ConsoleOutput($"Detached {fromHandle} from {toHandle}");
        }

        public static void DetachAllFromLocalEntity(int toHandle)
        {
            RAGE.Game.Entity.DetachEntity(toHandle, true, false);

            var list = StreamedAttachments.GetValueOrDefault(toHandle);

            if (list == null)
                return;

            list.ForEach(x =>
            {
                RAGE.Game.Entity.DetachEntity(x, true, false);
            });

            StreamedAttachments.Remove(toHandle);
        }

        public class AttachmentData
        {
            public int BoneID;

            public Vector3 PositionOffset { get; set; }
            public Vector3 Rotation { get; set; }

            public bool UseSoftPinning { get; set; }

            public bool Collision { get; set; }

            public bool IsPed { get; set; }

            public int RotationOrder { get; set; }

            public bool FixedRot { get; set; }

            public Action<object[]> EntityAction;

            public byte DisableInteraction { get; set; }

            public AttachmentData(int BoneID, Vector3 PositionOffset, Vector3 Rotation, bool UseSoftPinning, bool Collision, bool IsPed, int RotationOrder, bool FixedRot, Action<object[]> EntityAction = null)
            {
                this.BoneID = BoneID;
                this.PositionOffset = PositionOffset;
                this.Rotation = Rotation;

                this.UseSoftPinning = UseSoftPinning;
                this.Collision = Collision;
                this.IsPed = IsPed;
                this.RotationOrder = RotationOrder;
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
            public ushort Id { get; set; }

            [JsonProperty(PropertyName = "T")]
            public Types Type { get; set; }

            [JsonProperty(PropertyName = "D")]
            public string SyncData { get; set; }

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
            public ushort RemoteID { get; set; }

            public RAGE.Elements.Type EntityType { get; set; }

            public Types Type { get; set; }

            public bool WasAttached { get; set; }

            public AttachmentEntity(ushort RemoteID, RAGE.Elements.Type EntityType, Types Type)
            {
                this.RemoteID = RemoteID;
                this.EntityType = EntityType;
                this.Type = Type;
            }
        }

        private static Dictionary<uint, Dictionary<Types, AttachmentData>> ModelDependentAttachments = new Dictionary<string, Dictionary<Types, AttachmentData>>()
        {
            {
                "brp_p_ring_0_0",

                new Dictionary<Types, AttachmentData>()
                {
                    { Types.PedRingLeft3, new AttachmentData(26613, new Vector3(0.033f, -0.003f, 0.001f), new Vector3(70f, 85f, -5f), false, false, false, 2, true) },

                    { Types.PedRingRight3, new AttachmentData(58869, new Vector3(0.033f, 0.0007f, 0.0029f), new Vector3(105f, -85f, 15f), false, false, false, 2, true) },
                }
            },

            {
                "brp_p_ring_1_0",

                new Dictionary<Types, AttachmentData>()
                {
                    { Types.PedRingLeft3, new AttachmentData(26613, new Vector3(0.033f, -0.003f, 0.001f), new Vector3(80f, 95f, -5f), false, false, false, 2, true) },

                    { Types.PedRingRight3, new AttachmentData(58869, new Vector3(0.033f, 0.0013f, 0.0029f), new Vector3(115f, -105f, 15f), false, false, false, 2, true) },
                }
            },
        }.ToDictionary(x => RAGE.Util.Joaat.Hash(x.Key), x => x.Value);

        public static Dictionary<Types, AttachmentData> Attachments = new Dictionary<Types, AttachmentData>()
        {
            { Types.PushVehicle, new AttachmentData(6286, new Vector3(0f, 0f, 0.95f), new Vector3(0f, 0f, 0f), false, true, true, 2, true) },
            { Types.PhoneSync, new AttachmentData(28422, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 30f), false, false, false, 2, true) },
            { Types.VehKey, new AttachmentData(6286, new Vector3(0.08f, 0.04f, -0.015f), new Vector3(175f, -115f, -90f), false, false, false, 2, true) },
            { Types.ParachuteSync, new AttachmentData(1_000_000 + 57717, new Vector3(0f, 0f, 3f), new Vector3(0f, 0f, 0f), false, false, false, 0, true) },

            { Types.WeaponLeftTight, new AttachmentData(58271, new Vector3(0.08f, 0.03f, -0.1f), new Vector3(-80.77f, 0f, 0f), false, false, false, 2, true) },
            { Types.WeaponRightTight, new AttachmentData(51826, new Vector3(0.02f, 0.06f, 0.1f), new Vector3(-100f, 0f, 0f), false, false, false, 2, true) },
            { Types.WeaponLeftBack, new AttachmentData(24818, new Vector3(-0.1f, -0.15f, 0.11f), new Vector3(-180f, 0f, 0f), false, false, false, 2, true) },
            { Types.WeaponRightBack, new AttachmentData(24818, new Vector3(-0.1f, -0.15f, -0.13f), new Vector3(0f, 0f, 3.5f), false, false, false, 2, true) },

            { Types.Carry, new AttachmentData(1_000_000 + 0, new Vector3(0.23f, 0.18f, 0.65f), new Vector3(0.5f, 0.5f, 15f), false, false, false, 2, true) },
            { Types.PiggyBack, new AttachmentData(0, new Vector3(0f, -0.07f, 0.45f), new Vector3(0f, 0f, 0f), false, false, false, 2, true) },
            { Types.Hostage, new AttachmentData(0, new Vector3(-0.24f, 0.11f, 0f), new Vector3(0.5f, 0.5f, 0f), false, false, false, 2, true) },

            { Types.VehicleTrunk, new AttachmentData(-1, new Vector3(0f, 0.5f, 0.4f), new Vector3(0f, 0f, 0f), false, false, false, 2, true) },

            { Types.TrailerObjOnBoat, new AttachmentData(20, new Vector3(0f, -1f, 0.25f), new Vector3(0f, 0f, 0f), false, true, false, 2, true) },

            { Types.TractorTrailFarmHarv, new AttachmentData(0, new Vector3(0f, -2.7f, 0f), new Vector3(0f, 0f, 0f), false, true, false, 2, true) },

            { Types.ItemFishingRodG, new AttachmentData(60309, new Vector3(0.01f, -0.01f, 0.03f), new Vector3(0.1f, 0f, 0f), false, false, false, 2, true) },

            { Types.ItemShovel, new AttachmentData(28422, new Vector3(0.05f, -0.03f, -0.9f), new Vector3(2.1f, -4.2f, 5f), false, false, false, 2, true) },

            { Types.ItemMetalDetector, new AttachmentData(18905, new Vector3(0.15f, 0.1f, 0f), new Vector3(270f, 90f, 80f), false, false, false, 2, true) },

            { Types.ItemFishG, new AttachmentData(int.MinValue, null, new Vector3(0f, 0f, 0f), false, false, false, 2, true, async (args) =>
            {
                var gEntity = (MapObject)args[0];

                await Utils.RequestPtfx("core");

                gEntity.SetData("PtfxHandle", RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("water_splash_shark_wade", gEntity.Handle,  0f, 0f, 0f, 0f, 0f, 0f, 2.5f, false, false, false)); // water_splash_animal_wade
            }) },

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

            { Types.Cuffs, new AttachmentData(60309, new Vector3(-0.055f, 0.06f, 0.04f), new Vector3(265f, 155f, 80f), false, false, false, 0, true) },
            { Types.CableCuffs, new AttachmentData(60309, new Vector3(-0.055f, 0.06f, 0.04f), new Vector3(265f, 155f, 80f), false, false, false, 0, true) },

            { Types.EmsHealingBedFakeAttach, new AttachmentData(int.MinValue, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), false, false, false, 0, true) },

            { Types.PoliceEscort, new AttachmentData(1_000_000 + 11816, new Vector3(0.30f, 0.35f, 0f), new Vector3(0f, 0f, 0f), false, false, false, 2, true) { DisableInteraction = 1, } },

            {
                Types.FarmPlantSmallShovel, new AttachmentData(28422, new Vector3(0f, 0.01f, -0.03f), new Vector3(0f, 0f, 0f), false, false, false, 2, true, async (args) =>
                {
                    var gEntity = (MapObject)args[0];

                    await Utils.RequestPtfx("core");

                    gEntity.SetData("PtfxHandle", RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("scrape_mud", gEntity.Handle,  0.25f, 0f, 0f, 0f, 0f, 0f, 0.25f, false, false, false));
                })
            }, // rot Y -180 prop_buck_spade_09

            { Types.FarmOrangeBoxCarry, new AttachmentData(28422, new Vector3(0f, -0.02f, -0.07f), new Vector3(0f, 320f, 90f), false, false, false, 2, true) },

            { Types.FarmMilkBucketCarry, new AttachmentData(28422, new Vector3(0.05f, 0f, -0.05f), new Vector3(0f, 0f, 0f), false, false, false, 2, true) },

            {
                Types.FarmWateringCan, new AttachmentData(28422, new Vector3(0.09f, 0f, -0.2f), new Vector3(0f, 25f, 0f), false, false, false, 2, true, async (args) =>
                {
                    var gEntity = (MapObject)args[0];

                    await Utils.RequestPtfx("core");

                    gEntity.SetData("PtfxHandle", RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("water_cannon_spray", gEntity.Handle,  0.35f, 0f, 0.25f, 0f, 0f, 0f, 0.15f, false, false, false));
                })
            }, // prop_wateringcan
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
                await AttachEntity(entity, x.Type, x.Id, x.EntityType, x.SyncData);
        }

        public static async System.Threading.Tasks.Task OnEntityStreamOut(Entity entity)
        {
            var gEntity = entity as GameEntity;

            if (entity is Vehicle veh)
            {
                veh.DetachFromTrailer();
                veh.DetachFromAnyTowTruck();
            }

            if (entity.IsLocal)
            {
                if (gEntity != null)
                    DetachAllFromLocalEntity(gEntity.Handle);

                return;
            }
            else
            {
                if (gEntity != null)
                {
                    RAGE.Game.Entity.DetachEntity(gEntity.Handle, true, false);
                }

/*                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData != null)
                {
                    if (pData.IsAttachedTo == entity)
                    {
                        AsyncTask.RunSlim(() =>
                        {
                            Events.CallRemote("atsdme");
                        }, 2500);
                    }
                }*/
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
                DetachEntity(entity, obj.Type, obj.RemoteID, obj.EntityType);

            listEntities.Clear();
            entity.ResetData(AttachedEntitiesKey);
        }

        public AttachSystem()
        {
            Player.LocalPlayer.SetData(AttachedObjectsKey, new List<AttachmentObject>());
            Player.LocalPlayer.SetData(AttachedEntitiesKey, new List<AttachmentEntity>());

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
                            AttachEntity(entity, x.Type, x.Id, x.EntityType, x.SyncData);
                    }
                    else
                        DetachEntity(entity, x.Type, x.Id, x.EntityType);
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
        }

        public static async System.Threading.Tasks.Task AttachEntity(Entity entity, Types type, ushort remoteId, RAGE.Elements.Type eType, string syncData)
        {
            GameEntity gTarget = null;
            var gEntity = Utils.GetGameEntity(entity);

            if (gEntity == null)
                return;

            var list = entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);

            if (list == null)
                return;

            var aObj = new AttachmentEntity(remoteId, eType, type);

            list.Add(aObj);

            while (true)
            {
                if (gEntity?.Exists != true || entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey)?.Contains(aObj) != true)
                    return;

                gTarget = Utils.GetGameEntityAtRemoteId(eType, remoteId);

                if (gTarget?.Exists != true)
                {
                    await RAGE.Game.Invoker.WaitAsync(50);

                    continue;
                }

                break;
            }

            var positionBase = Vector3.Zero;

            var props = Attachments.GetValueOrDefault(type);

            if (entity is Vehicle veh)
            {
                if (type == Types.VehicleTrailer)
                {

                }
                else
                {
                    (Vector3 Min, Vector3 Max) vehSize = entity.GetModelDimensions();

                    if (type == Types.PushVehicle)
                    {
                        if (syncData == "1")
                        {
                            positionBase.Y = vehSize.Max.Y;
                            positionBase.Z = vehSize.Min.Z;

                            props.PositionOffset.Y = 0.35f;
                            props.Rotation.Z = 180f;
                        }
                        else
                        {
                            positionBase.Y = vehSize.Min.Y;
                            positionBase.Z = vehSize.Min.Z;

                            props.PositionOffset.Y = -0.6f;
                            props.Rotation.Z = 0f;
                        }
                    }
                    else if (type == Types.VehicleTrunk)
                    {
                        positionBase.Y = -(vehSize.Max.Y - vehSize.Min.Y) / 2f;
                    }
                }
            }

            if (props != null)
            {
                void attachMethod() => RAGE.Game.Entity.AttachEntityToEntity(gTarget.Handle, gEntity.Handle, props.BoneID >= 1_000_000 ? props.BoneID - 1_000_000 : RAGE.Game.Ped.GetPedBoneIndex(gEntity.Handle, props.BoneID), positionBase.X + props.PositionOffset.X, positionBase.Y + props.PositionOffset.Y, positionBase.Z + props.PositionOffset.Z, props.Rotation.X, props.Rotation.Y, props.Rotation.Z, false, props.UseSoftPinning, props.Collision, props.IsPed, props.RotationOrder, props.FixedRot);

                gTarget.SetData<Action>("AttachMethod", attachMethod);

                attachMethod();
            }

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

            aObj.WasAttached = true;

            if (gTarget == Player.LocalPlayer)
            {
                TargetAction(type, entity, true);

                if (props != null && (props.DisableInteraction == 1 || props.DisableInteraction == 2))
                {
                    BCRPClient.Interaction.SetEntityAsDisabled(gEntity, true);
                }
            }
            else if (gEntity  == Player.LocalPlayer)
            {
                RootAction(type, gTarget, true);

                if (props != null && (props.DisableInteraction == 1 || props.DisableInteraction == 3))
                {
                    BCRPClient.Interaction.SetEntityAsDisabled(gTarget, true);
                }
            }
        }

        public static void DetachEntity(Entity entity, Types type, ushort remoteId, RAGE.Elements.Type eType)
        {
            var gEntity = Utils.GetGameEntity(entity);

            if (gEntity == null)
                return;

            var list = entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);

            if (list == null)
                return;

            var aObj = list.Where(x => x.RemoteID == remoteId && x.EntityType == eType && x.Type == type).FirstOrDefault();

            if (aObj == null)
                return;

            list.Remove(aObj);

            var gTarget = Utils.GetGameEntityAtRemoteId(eType, remoteId);

            if (gTarget?.Exists == true)
            {
                var props = Attachments.GetValueOrDefault(aObj.Type);

                if (props != null)
                {
                    RAGE.Game.Entity.DetachEntity(gTarget.Handle, true, props.Collision);

                    gTarget.ResetData("AttachMethod");
                }

                if (aObj.Type == Types.VehicleTrailer || aObj.Type == Types.VehicleTrailerObjBoat)
                {
                    RAGE.Game.Vehicle.DetachVehicleFromTrailer(gTarget.Handle);
                }
            }

            if (aObj.WasAttached)
            {
                if (gTarget == Player.LocalPlayer)
                {
                    TargetAction(aObj.Type, null, false);

                    BCRPClient.Interaction.SetEntityAsDisabled(gEntity, false);
                }
                else if (gEntity == Player.LocalPlayer)
                {
                    RootAction(aObj.Type, null, false);

                    BCRPClient.Interaction.SetEntityAsDisabled(gTarget, false);
                }
            }
        }

        public static async System.Threading.Tasks.Task AttachObject(uint hash, Entity target, Types type, string syncData)
        {
            var res = await Utils.RequestModel(hash);

            var gTarget = Utils.GetGameEntity(target);

            if (gTarget == null)
                return;

            if (gTarget.Handle == Player.LocalPlayer.Handle)
            {
                if (type == Types.PhoneSync || type == Types.ParachuteSync)
                    return;
            }

            var list = target.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (list == null)
                return;

            GameEntity gEntity = null;

            var positionBase = Vector3.Zero;

            if (type >= Types.WeaponRightTight && type <= Types.WeaponLeftBack)
            {
                await Utils.RequestWeaponAsset(hash);

                gEntity = new MapObject(RAGE.Game.Weapon.CreateWeaponObject(hash, 0, target.Position.X, target.Position.Y, target.Position.Z, true, 0f, 0, 0, 0));

                if (syncData != null)
                    Sync.WeaponSystem.UpdateWeaponObjectComponents(gEntity.Handle, hash, syncData);
            }
            else if (type == Types.TrailerObjOnBoat)
            {
                var rot = RAGE.Game.Entity.GetEntityRotation(gTarget.Handle, 2);

                var pos = target.Position;

                RAGE.Game.Entity.SetEntityCoordsNoOffset(gTarget.Handle, pos.X, pos.Y, pos.Z + 5f, false, false, false);

                var veh = new Vehicle(hash, pos, rot.Z, "", 255, true, 0, 0, target.Dimension);

                var targetVeh = target as Vehicle;

                var targetData = Sync.Vehicles.GetData(targetVeh);

                if (targetData != null)
                {
                    if (targetData.Data.ID.StartsWith("seashark"))
                        positionBase.Z -= 0.5f;
                }

                veh.StreamInCustomActionsAdd((entity) =>
                {
                    var eVeh = entity as Vehicle;

                    eVeh.SetCanBeDamaged(false); eVeh.SetCanBeVisiblyDamaged(false); eVeh.SetCanBreak(false); eVeh.SetDirtLevel(0f); eVeh.SetDisablePetrolTankDamage(true); eVeh.SetDisablePetrolTankFires(true); eVeh.SetInvincible(true);

                    if (Sync.Vehicles.GetData(targetVeh)?.IsFrozen == true)
                        eVeh.FreezePosition(true);
                });

                gEntity = veh;

                gEntity.SetData("TrailerSync::Owner", targetVeh);
            }
            else if (type == Types.TractorTrailFarmHarv)
            {
                var veh = new Vehicle(hash, target.Position, 0f, "", 255, true, 0, 0, target.Dimension);

                veh.StreamInCustomActionsAdd((entity) =>
                {
                    var eVeh = entity as Vehicle;

                    eVeh.SetAutomaticallyAttaches(0, 0);

                    eVeh.SetCanBeDamaged(false); eVeh.SetCanBeVisiblyDamaged(false); eVeh.SetCanBreak(false); eVeh.SetDirtLevel(0f); eVeh.SetDisablePetrolTankDamage(true); eVeh.SetDisablePetrolTankFires(true); eVeh.SetInvincible(true);
                });

                gEntity = veh;
            }
            else
            {
                if (res)
                {
                    gEntity = Utils.CreateObjectNoOffsetImmediately(hash, target.Position.X, target.Position.Y, target.Position.Z);
                }
            }

            var props = ModelDependentAttachments.GetValueOrDefault(hash)?.GetValueOrDefault(type) ?? Attachments.GetValueOrDefault(type);

            list.Add(new AttachmentObject(gEntity, type, hash, syncData));

            target.SetData(AttachedObjectsKey, list);

            if (props != null)
            {
                if (props.BoneID == int.MinValue)
                {
                    if (type == Types.ItemFishG)
                    {
                        if (gEntity != null)
                        {
                            var pos = syncData.Split('&');

                            RAGE.Game.Entity.SetEntityCoordsNoOffset(gEntity.Handle, float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]), false, false, false);

                            RAGE.Game.Entity.SetEntityVisible(gEntity.Handle, false, true);
                        }
                    }
                }
                else
                {
                    if (gEntity != null)
                    {
                        if (type == Types.TrailerObjOnBoat)
                        {
                            AddLocalAttachment(gTarget.Handle, gEntity.Handle);

                            RAGE.Game.Entity.AttachEntityToEntity(gTarget.Handle, gEntity.Handle, props.BoneID >= 1_000_000 ? props.BoneID - 1_000_000 : RAGE.Game.Ped.GetPedBoneIndex(gTarget.Handle, props.BoneID), positionBase.X + props.PositionOffset.X, positionBase.Y + props.PositionOffset.Y, positionBase.Z + props.PositionOffset.Z, props.Rotation.X, props.Rotation.Y, props.Rotation.Z, false, props.UseSoftPinning, props.Collision, props.IsPed, props.RotationOrder, props.FixedRot);
                        }
                        else
                        {
                            RAGE.Game.Entity.AttachEntityToEntity(gEntity.Handle, gTarget.Handle, props.BoneID >= 1_000_000 ? props.BoneID - 1_000_000 : RAGE.Game.Ped.GetPedBoneIndex(gTarget.Handle, props.BoneID), positionBase.X + props.PositionOffset.X, positionBase.Y + props.PositionOffset.Y, positionBase.Z + props.PositionOffset.Z, props.Rotation.X, props.Rotation.Y, props.Rotation.Z, false, props.UseSoftPinning, props.Collision, props.IsPed, props.RotationOrder, props.FixedRot);
                        }
                    }
                }

                if (gEntity != null)
                    props.EntityAction?.Invoke(new object[] { gEntity });
            }

            if (gTarget is Player tPlayer && tPlayer.Handle == Player.LocalPlayer.Handle)
            {
                RootAction(type, gEntity, true);
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
            var gTarget = target as GameEntity;

            if (gTarget == null)
                return;

            var props = ModelDependentAttachments.GetValueOrDefault(item.Model)?.GetValueOrDefault(type) ?? Attachments.GetValueOrDefault(type);

            if (gEntity != null)
            {
                if (gEntity.HasData("PtfxHandle"))
                {
                    RAGE.Game.Graphics.StopParticleFxLooped(gEntity.GetData<int>("PtfxHandle"), false);
                }

                if (type == Types.TrailerObjOnBoat)
                {
                    RemoveLocalAttachment(gTarget.Handle, gEntity.Handle);

                    RAGE.Game.Entity.DetachEntity(gTarget.Handle, true, props?.Collision ?? false);

                    gEntity.Destroy();
                }
                else
                {
                    if (gEntity.IsLocal)
                    {
                        RAGE.Game.Entity.DetachEntity(gEntity.Handle, true, props?.Collision ?? false);

                        RAGE.Game.Entity.SetEntityAsMissionEntity(gEntity.Handle, false, false);

                        gEntity.Destroy();
                    }
                    else
                    {
                        RAGE.Game.Entity.DetachEntity(gEntity.Handle, true, props?.Collision ?? false);
                    }
                }
            }

            list.Remove(item);

            target.SetData(AttachedObjectsKey, list);

            if (gTarget is Player tPlayer && tPlayer.Handle == Player.LocalPlayer.Handle)
            {
                RootAction(item.Type, null, false);
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

        public static List<AttachmentObject> GetEntityObjectAttachments(Entity entity) => entity.GetData<List<AttachmentObject>>(AttachedObjectsKey);

        public static List<AttachmentEntity> GetEntityEntityAttachments(Entity entity) => entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);

        private static Dictionary<Types, Types> SameActionsTypes = new Dictionary<Types, Types>()
        {
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
                Types.PushVehicle,

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

                        if (veh?.Exists != true || Utils.AnyOnFootMovingControlJustPressed() || !Utils.CanDoSomething(false, Sync.PushVehicle.ActionsToCheckLoop) || veh.GetIsEngineRunning() || veh.HasCollidedWithAnything() || Vector3.Distance(Player.LocalPlayer.Position, veh.GetCoords(false)) > Settings.App.Static.EntityInteractionMaxDistance)
                        {
                            if (Sync.Animations.LastSent.IsSpam(500, false, false))
                                return;

                            Sync.Animations.LastSent = Sync.World.ServerTime;

                            Sync.PushVehicle.Off(false);
                        }
                        else
                        {
                            Utils.DrawText(Locale.General.Animations.CancelTextPushVehicle, 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
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
                        var pData = Sync.Players.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var root = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Vehicle;

                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        var isForced = pData.IsKnocked || pData.IsCuffed;

                        if (root?.Exists != true || (!isForced && bind.IsPressed))
                        {
                            if (Sync.Animations.LastSent.IsSpam(500, false, false))
                                return;

                            Sync.Animations.LastSent = Sync.World.ServerTime;

                            Events.CallRemote("Players::StopInTrunk");
                        }
                        else
                        {
                            if (!isForced)
                                Utils.DrawText(string.Format(Locale.General.Animations.CancelTextInTrunk, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
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
                        var pData = Sync.Players.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var root = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Player;

                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        var isForced = pData.IsKnocked || pData.IsCuffed;

                        if (root?.Exists != true || (!isForced && bind.IsJustPressed))
                        {
                            if (Sync.Animations.LastSent.IsSpam(500, false, false))
                                return;

                            Sync.Animations.LastSent = Sync.World.ServerTime;

                            Events.CallRemote("Players::StopCarry");
                        }
                        else
                        {
                            if (!isForced)
                                Utils.DrawText(string.Format(Locale.General.Animations.CancelTextCarryB, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
                        }
                    })
                )
            },

            {
                Types.PoliceEscort,

                (
                    () =>
                    {

                    },

                    () =>
                    {
                        if (Player.LocalPlayer.GetData<bool>("POLICEESCORTEFLAG"))
                        {
                            var pos = Player.LocalPlayer.GetCoords(false);

                            Player.LocalPlayer.TaskGoStraightToCoord(pos.X, pos.Y, pos.Z, 1f, 1, Player.LocalPlayer.GetHeading(), 0f);

                            Player.LocalPlayer.ResetData("POLICEESCORTEFLAG");
                        }
                    },

                    () =>
                    {
                        var rootPlayer = Utils.GetPlayerByHandle(Player.LocalPlayer.GetAttachedTo(), true);

                        var speed = 0f;

                        if (rootPlayer?.Exists != true || rootPlayer == Player.LocalPlayer || (speed = rootPlayer.GetSpeed()) <= 0.8f)
                        {
                            if (Player.LocalPlayer.GetData<bool>("POLICEESCORTEFLAG"))
                            {
                                var pos = Player.LocalPlayer.GetCoords(false);

                                Player.LocalPlayer.TaskGoStraightToCoord(pos.X, pos.Y, pos.Z, 1f, 1, Player.LocalPlayer.GetHeading(), 0f);

                                Player.LocalPlayer.ResetData("POLICEESCORTEFLAG");
                            }

                            return;
                        }
                        else
                        {
                            var heading = rootPlayer.GetHeading();

                            var pos = Additional.Camera.GetFrontOf(Player.LocalPlayer.Position, heading, 1f);

                            Player.LocalPlayer.TaskGoStraightToCoord(pos.X, pos.Y, pos.Z, speed * 0.5f, -1, heading, 0f);

                            Player.LocalPlayer.SetData("POLICEESCORTEFLAG", true);
                        }
                    }
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

                        if (target?.Exists != true || bind.IsJustPressed)
                        {
                            if (Sync.Animations.LastSent.IsSpam(500, false, false))
                                return;

                            Sync.Animations.LastSent = Sync.World.ServerTime;

                            Events.CallRemote("Players::StopCarry");
                        }
                        else
                        {
                            Utils.DrawText(string.Format(Locale.General.Animations.CancelTextCarryA, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
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

                        Player.LocalPlayer.SetData("Temp::Smoke::LastSent", Sync.World.ServerTime);
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

                        if (bind.IsJustPressed || Player.LocalPlayer.IsInWater() || puffs == 0)
                        {
                            if (Sync.Animations.LastSent.IsSpam(500, false, false))
                                return;

                            Sync.Animations.LastSent = Sync.World.ServerTime;

                            Events.CallRemote("Players::Smoke::Stop");
                        }
                        else
                        {
                            var lastSent = Player.LocalPlayer.GetData<DateTime>("Temp::Smoke::LastSent");

                            // lmb - do puff
                            if (!CEF.Cursor.IsVisible && RAGE.Game.Pad.IsDisabledControlJustPressed(0, 24) && Utils.CanDoSomething(false, Utils.Actions.Animation, Utils.Actions.FastAnimation, Utils.Actions.OtherAnimation))
                            {
                                if (!lastSent.IsSpam(1000, false, false))
                                {
                                    Events.CallRemote("Players::Smoke::Puff");

                                    Player.LocalPlayer.SetData("Temp::Smoke::LastSent", Sync.World.ServerTime);
                                }
                            }
                            // alt - to mouth
                            else if (((!CEF.Cursor.IsVisible && KeyBinds.IsJustDown(RAGE.Ui.VirtualKeys.LeftMenu)) || Player.LocalPlayer.Vehicle != null) && Utils.CanDoSomething(false, Utils.Actions.Animation, Utils.Actions.FastAnimation, Utils.Actions.OtherAnimation))
                            {
                                if (!lastSent.IsSpam(1000, false, false))
                                {
                                    Events.CallRemote("Players::Smoke::State");

                                    Player.LocalPlayer.SetData("Temp::Smoke::LastSent", Sync.World.ServerTime);
                                }
                            }

                            Utils.DrawText(string.Format(Locale.General.Animations.TextDoPuffSmoke, puffs), 0.5f, 0.90f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
                            Utils.DrawText(Locale.General.Animations.TextToMouthSmoke, 0.5f, 0.925f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
                            Utils.DrawText(string.Format(Locale.General.Animations.CancelTextSmoke, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
                        }
                    })
                )
            },

            {
               Types.ItemCigMouth,

                (
                    new Action(() =>
                    {
                        Player.LocalPlayer.SetData("Temp::Smoke::LastSent", Sync.World.ServerTime);
                    }),

                    new Action(() =>
                    {
                        Player.LocalPlayer.ResetData("Temp::Smoke::LastSent");
                    }),

                    new Action(() =>
                    {
                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        if (bind.IsJustPressed || Player.LocalPlayer.IsInWater())
                        {
                            if (Sync.Animations.LastSent.IsSpam(500, false, false))
                                return;

                            Sync.Animations.LastSent = Sync.World.ServerTime;

                            Events.CallRemote("Players::Smoke::Stop");
                        }
                        else
                        {
                            var lastSent = Player.LocalPlayer.GetData<DateTime>("Temp::Smoke::LastSent");

                            if (Player.LocalPlayer.Vehicle == null)
                            {
                                // alt - to hand
                                if (!CEF.Cursor.IsVisible && KeyBinds.IsJustDown(RAGE.Ui.VirtualKeys.LeftMenu))
                                {
                                    if (!lastSent.IsSpam(1000, false, true) && Utils.CanDoSomething(false, Utils.Actions.Animation, Utils.Actions.FastAnimation, Utils.Actions.OtherAnimation))
                                    {
                                        Events.CallRemote("Players::Smoke::State");

                                        Player.LocalPlayer.SetData("Temp::Smoke::LastSent", Sync.World.ServerTime);
                                    }
                                }

                                Utils.DrawText(Locale.General.Animations.TextToHandSmoke, 0.5f, 0.925f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
                            }

                            Utils.DrawText(string.Format(Locale.General.Animations.CancelTextSmoke, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
                        }
                    })
                )
            },

            {
                Types.FarmPlantSmallShovel,

                (
                    null,

                    null,

                    new Action(() =>
                    {
                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        if (bind.IsJustPressed)
                        {
                            if (Sync.Animations.LastSent.IsSpam(500, false, true))
                                return;

                            Sync.Animations.LastSent = Sync.World.ServerTime;

                            Events.CallRemote("Job::FARM::SCP");
                        }
                        else
                        {
                            Utils.DrawText(string.Format(Locale.General.Animations.JustStopText, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
                        }
                    })
                )
            },

            {
                Types.FarmWateringCan,

                (
                    null,

                    null,

                    new Action(() =>
                    {
                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        if (bind.IsJustPressed)
                        {
                            if (Sync.Animations.LastSent.IsSpam(500, false, false))
                                return;

                            Sync.Animations.LastSent = Sync.World.ServerTime;

                            Events.CallRemote("Job::FARM::SOTP");
                        }
                        else
                        {
                            Utils.DrawText(string.Format(Locale.General.Animations.JustStopText, bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
                        }
                    })
                )
            },

            {
                Types.FarmOrangeBoxCarry,

                (
                    new Action(() =>
                    {
                        var farmBusiness = (Sync.Players.GetData(Player.LocalPlayer)?.CurrentJob as Data.Jobs.Farmer)?.FarmBusiness;

                        if (farmBusiness == null || farmBusiness.OrangeTreeBoxPositions == null)
                            return;

                        var markers = new List<Marker>();

                        foreach (var x in farmBusiness.OrangeTreeBoxPositions)
                        {
                            markers.Add(new Marker(2, new Vector3(x.Item2.Position.X, x.Item2.Position.Y, x.Item2.Position.Z + 1f), 1f, Vector3.Zero, Vector3.Zero, new RGBA(0, 0, 255, 125), true, Settings.App.Static.MainDimension));
                        }

                        var closestOrangeBoxPos = farmBusiness.OrangeTreeBoxPositions.Select(x => x.Item1).OrderBy(x => x.DistanceTo(Player.LocalPlayer.Position)).FirstOrDefault();

                        Player.LocalPlayer.SetData("JOBATFARM::FOBC::B", new Additional.ExtraBlip(478, closestOrangeBoxPos, "Коробки с апельсинами", 1f, 21, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension));
                        Player.LocalPlayer.SetData("JOBATFARM::FOBC::MS", markers);

                        CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), "Отнесите коробку с апельсинами в место, отмеченное на карте");
                    }),

                    new Action(() =>
                    {
                        Player.LocalPlayer.GetData<Additional.ExtraBlip>("JOBATFARM::FOBC::B")?.Destroy();

                        Player.LocalPlayer.ResetData("JOBATFARM::FOBC::B");

                        var markers = Player.LocalPlayer.GetData<List<Marker>>("JOBATFARM::FOBC::MS");

                        if (markers != null)
                        {
                            foreach (var x in markers)
                                x?.Destroy();
                        }

                        Player.LocalPlayer.ResetData("JOBATFARM::FOBC::MS");
                    }),

                    new Action(() =>
                    {
                        if (!Utils.CanDoSomething(false, Utils.Actions.Ragdoll, Utils.Actions.Falling, Utils.Actions.IsSwimming, Utils.Actions.Climbing, Utils.Actions.Crawl, Utils.Actions.InVehicle, Utils.Actions.Reloading, Utils.Actions.Shooting, Utils.Actions.MeleeCombat))
                        {
                            if (Sync.Animations.LastSent.IsSpam(500, false, false))
                                return;

                            Sync.Animations.LastSent = Sync.World.ServerTime;

                            CEF.Notification.ShowError("Вы уронили коробку с апельсинами!");

                            Events.CallRemote("Job::FARM::SOTP");
                        }
                    })
                )
            },

            {
                Types.FarmMilkBucketCarry,

                (
                    new Action(() =>
                    {
                        var farmBusiness = (Sync.Players.GetData(Player.LocalPlayer)?.CurrentJob as Data.Jobs.Farmer)?.FarmBusiness;

                        if (farmBusiness == null || farmBusiness.CowBucketPositions == null)
                            return;

                        var markers = new List<Marker>();

                        foreach (var x in farmBusiness.CowBucketPositions)
                        {
                            markers.Add(new Marker(2, new Vector3(x.Item2.Position.X, x.Item2.Position.Y, x.Item2.Position.Z + 1f), 1f, Vector3.Zero, Vector3.Zero, new RGBA(0, 0, 255, 125), true, Settings.App.Static.MainDimension));
                        }

                        var closestOrangeBoxPos = farmBusiness.CowBucketPositions.Select(x => x.Item1).OrderBy(x => x.DistanceTo(Player.LocalPlayer.Position)).FirstOrDefault();


                        Player.LocalPlayer.SetData("JOBATFARM::FOBC::B", new Additional.ExtraBlip(478, closestOrangeBoxPos, "Вёдра с молоком", 1f, 21, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension));
                        Player.LocalPlayer.SetData("JOBATFARM::FOBC::MS", markers);

                        CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), "Отнесите ведро с молоком в место, отмеченное на карте");
                    }),

                    new Action(() =>
                    {
                        Player.LocalPlayer.GetData<Additional.ExtraBlip>("JOBATFARM::FOBC::B")?.Destroy();

                        Player.LocalPlayer.ResetData("JOBATFARM::FOBC::B");

                        var markers = Player.LocalPlayer.GetData<List<Marker>>("JOBATFARM::FOBC::MS");

                        if (markers != null)
                        {
                            foreach (var x in markers)
                                x?.Destroy();
                        }

                        Player.LocalPlayer.ResetData("JOBATFARM::FOBC::MS");
                    }),

                    new Action(() =>
                    {
                        if (!Utils.CanDoSomething(false, Utils.Actions.Ragdoll, Utils.Actions.Falling, Utils.Actions.IsSwimming, Utils.Actions.Climbing, Utils.Actions.Crawl, Utils.Actions.InVehicle, Utils.Actions.Reloading, Utils.Actions.Shooting, Utils.Actions.MeleeCombat))
                        {
                            if (Sync.Animations.LastSent.IsSpam(500, false, false))
                                return;

                            Sync.Animations.LastSent = Sync.World.ServerTime;

                            CEF.Notification.ShowError("Вы уронили ведро с молоком!");

                            Events.CallRemote("Job::FARM::SCOWP");
                        }
                    })
                )
            },

            {
                Types.EmsHealingBedFakeAttach,

                (
                    null,
                
                    () =>
                    {
                        Additional.ExtraColshape.All.Where(x => x.Name == "ems_healing_bed").ToList().ForEach(x => x.Destroy());
                    },

                    new Action(() =>
                    {
                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        Utils.DrawText(string.Format("Нажмите {0}, чтобы встать с койки", bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

                        if (Utils.CanShowCEF(true, true))
                        {
                            if (bind.IsJustPressed)
                            {
                                if (!Sync.Animations.LastSent.IsSpam(500, false, false))
                                {
                                    Sync.Animations.LastSent = Sync.World.ServerTime;

                                    Events.CallRemote("EMS::BedFree");
                                }
                            }
                        }
                    })
                )
            },

            {
                Types.Cuffs,

                (
                    () =>
                    {
                        Sync.WeaponSystem.DisabledFiring = true;

                        KeyBinds.Get(KeyBinds.Types.Crawl)?.Disable();

                        Player.LocalPlayer.SetEnableHandcuffs(true);

                        BCRPClient.Interaction.Enabled = false;
                    },

                    () =>
                    {
                        Sync.WeaponSystem.DisabledFiring = false;

                        KeyBinds.Get(KeyBinds.Types.Crawl)?.Enable();

                        Player.LocalPlayer.SetEnableHandcuffs(false);

                        BCRPClient.Interaction.Enabled = true;

                        if (Data.Minigames.LockPicking.CurrentContext == "POLICE_CUFFS_LOCKPICK")
                        {
                            Data.Minigames.LockPicking.Close();
                        }
                    },

                    () =>
                    {
                        if (Player.LocalPlayer.IsInAnyVehicle(false))
                        {
                            GameEvents.DisableMoveRender();
                        }

                        var lockpickItemAmount = Data.Minigames.LockPicking.GetLockpickTotalAmount();

                        if (lockpickItemAmount > 0)
                        {
                            var key = RAGE.Ui.VirtualKeys.Return;

                            if (Utils.CanShowCEF(true, true) && !Utils.IsAnyCefActive())
                            {
                                Utils.DrawText(string.Format("Нажмите {0}, чтобы воспользоваться отмычкой (x{1})", KeyBinds.ExtraBind.GetKeyString(key), lockpickItemAmount), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

                                if (KeyBinds.IsJustDown(key))
                                {
                                    if (!Sync.Animations.LastSent.IsSpam(500, false, false))
                                    {
                                        Sync.Animations.LastSent = Sync.World.ServerTime;

                                        Data.Minigames.LockPicking.Show("POLICE_CUFFS_LOCKPICK", Data.Minigames.LockPicking.DurabilityDefault, Data.Minigames.LockPicking.GetLockpickingRandomTargetRotation(), Data.Minigames.LockPicking.MaxDeviationDefault, Data.Minigames.LockPicking.RotationDefault);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Data.Minigames.LockPicking.CurrentContext == "POLICE_CUFFS_LOCKPICK")
                            {
                                CEF.Notification.Show("Inventory::NoItem");

                                Data.Minigames.LockPicking.Close();
                            }
                        }
                    }
                )
            },

            {
                Types.PoliceEscort,
                
                (
                    () =>
                    {
                        Sync.WeaponSystem.DisabledFiring = true;

                        KeyBinds.Get(KeyBinds.Types.Crouch)?.Disable();
                        KeyBinds.Get(KeyBinds.Types.Crawl)?.Disable();
                    },

                    () =>
                    {
                        Sync.WeaponSystem.DisabledFiring = false;

                        KeyBinds.Get(KeyBinds.Types.Crouch)?.Enable();
                        KeyBinds.Get(KeyBinds.Types.Crawl)?.Enable();
                    },

                    async () =>
                    {
                        RAGE.Game.Pad.DisableControlAction(32, 21, true);
                        RAGE.Game.Pad.DisableControlAction(32, 22, true);

                        var bind = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        Utils.DrawText(string.Format("Нажмите {0}, чтобы прекратить вести человека", bind.GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

                        if (Utils.CanShowCEF(true, true))
                        {
                            if (bind.IsJustPressed)
                            {
                                if (!Sync.Animations.LastSent.IsSpam(500, false, false))
                                {
                                    Sync.Animations.LastSent = Sync.World.ServerTime;

                                    await Events.CallRemoteProc("Police::Escort", null, false);
                                }
                            }
                        }
                    }
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

        public static void TargetAction(Types type, Entity root, bool attach)
        {
            var data = Sync.Players.GetData(Player.LocalPlayer);

            if (data == null)
                return;

            data.IsAttachedTo = root;

            var actions = GetTargetActions(type);

            if (actions == null)
                return;

            if (attach)
            {
                data.IsAttachedTo = root;

                actions.Value.On?.Invoke();

                if (actions.Value.Loop != null)
                {
                    GameEvents.Update -= actions.Value.Loop.Invoke;
                    GameEvents.Update += actions.Value.Loop.Invoke;
                }
            }
            else
            {
                data.IsAttachedTo = null;

                if (actions.Value.Loop != null)
                    GameEvents.Update -= actions.Value.Loop.Invoke;

                actions.Value.Off?.Invoke();
            }
        }

        public static void RootAction(Types type, Entity target, bool attach)
        {
            var data = Sync.Players.GetData(Player.LocalPlayer);

            if (data == null)
                return;

            var actions = GetRootActions(type);

            if (actions == null)
                return;

            if (attach)
            {
                actions.Value.On?.Invoke();

                if (actions.Value.Loop != null)
                {
                    GameEvents.Update -= actions.Value.Loop.Invoke;
                    GameEvents.Update += actions.Value.Loop.Invoke;
                }
            }
            else
            {
                if (actions.Value.Loop != null)
                    GameEvents.Update -= actions.Value.Loop.Invoke;

                actions.Value.Off?.Invoke();
            }
        }

        public static async System.Threading.Tasks.Task<GameEntity> AttachObjectSimpleLocal(uint hash, Entity target, Types type)
        {
            GameEntity gTarget = Utils.GetGameEntity(target);

            if (gTarget == null)
                return null;

            GameEntity gEntity = null;

            Vector3 positionBase = Vector3.Zero;

            if (type >= Types.WeaponRightTight && type <= Types.WeaponLeftBack)
            {
                await Utils.RequestWeaponAsset(hash);

                gEntity = new MapObject(RAGE.Game.Weapon.CreateWeaponObject(hash, 0, target.Position.X, target.Position.Y, target.Position.Z, true, 0f, 0, 0, 0));
            }
            else
            {
                gEntity = Utils.CreateObjectNoOffsetImmediately(hash, target.Position.X, target.Position.Y, target.Position.Z);
            }

            if (gEntity == null)
                return null;

            var props = ModelDependentAttachments.GetValueOrDefault(hash)?.GetValueOrDefault(type) ?? Attachments.GetValueOrDefault(type);

            if (props != null)
            {
                RAGE.Game.Entity.AttachEntityToEntity(gEntity.Handle, gTarget.Handle, RAGE.Game.Ped.GetPedBoneIndex(gTarget.Handle, props.BoneID), positionBase.X + props.PositionOffset.X, positionBase.Y + props.PositionOffset.Y, positionBase.Z + props.PositionOffset.Z, props.Rotation.X, props.Rotation.Y, props.Rotation.Z, false, props.UseSoftPinning, props.Collision, props.IsPed, props.RotationOrder, props.FixedRot);

                props.EntityAction?.Invoke(new object[] { gEntity });
            }

            return gEntity;
        }
    }
}
