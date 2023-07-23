﻿using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BlaineRP.Server.Sync
{
    public class AttachSystem
    {
        public const string AttachedObjectsKey = "AttachedObjects";
        public const string AttachedEntitiesKey = "AttachedEntities";
        public const string EntityIsAttachedToKey = "IAT::E";

        public const string AttachedObjectsTimersKey = AttachedObjectsKey + "::Timers";

        private static object[] EmptyArgs { get; } = new object[] { };

        public static bool IsTypeStaticObject(Types type) => type >= Types.PedRingLeft3 && type < Types.VehKey;

        public static bool IsTypeObjectInHand(Types type) => type >= Types.VehKey && type < Types.VehicleTrailer;

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

            PlayerResurrect,

            #endregion
        }

        public static class Models
        {
            public static uint Phone { get; } = NAPI.Util.GetHashKey("prop_phone_ing");

            public static uint VehicleRemoteFob { get; } = NAPI.Util.GetHashKey("lr_prop_carkey_fob");

            public static uint ParachuteSync { get; } = NAPI.Util.GetHashKey("p_parachute1_mp_dec");

            public static uint Cuffs { get; } = NAPI.Util.GetHashKey("p_cs_cuffs_02_s");
            public static uint CableCuffs { get; } = NAPI.Util.GetHashKey("brp_p_cablecuffs_0");
        }

        public class AttachmentObjectNet
        {
            [JsonProperty(PropertyName = "M")]
            public uint Model { get; set; }

            [JsonProperty(PropertyName = "T")]
            public Types Type { get; set; }

            [JsonProperty(PropertyName = "D", NullValueHandling = NullValueHandling.Ignore)]
            public string SyncData { get; set; }

            public AttachmentObjectNet(uint Model, Types Type, string SyncData = null)
            {
                this.Model = Model;
                this.Type = Type;

                this.SyncData = SyncData;
            }
        }

        public class AttachmentEntityNet
        {
            [JsonProperty(PropertyName = "E")]
            public EntityType EntityType { get; set; }

            [JsonProperty(PropertyName = "I")]
            public ushort Id { get; set; }

            [JsonProperty(PropertyName = "T")]
            public Types Type { get; set; }

            [JsonProperty(PropertyName = "D", NullValueHandling = NullValueHandling.Ignore)]
            public string SyncData { get; set; }

            public AttachmentEntityNet(ushort Id, EntityType EntityType, Types Type, string SyncData)
            {
                this.Id = Id;
                this.EntityType = EntityType;

                this.Type = Type;

                this.SyncData = SyncData;
            }
        }

        private static Dictionary<Types, Types> SameActionsTypes = new Dictionary<Types, Types>()
        {
            { Types.ItemCig1Hand, Types.ItemCigHand },
            { Types.ItemCig2Hand, Types.ItemCigHand },
            { Types.ItemCig3Hand, Types.ItemCigHand },

            { Types.ItemCig1Mouth, Types.ItemCigMouth },
            { Types.ItemCig2Mouth, Types.ItemCigMouth },
            { Types.ItemCig3Mouth, Types.ItemCigMouth },
        };

        private static Dictionary<Types, Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>> Actions = new Dictionary<Types, Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>>()
        {
            {
                Types.TrailerObjOnBoat,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {

                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Vehicle veh)
                            {
                                var attachedTo = veh.GetEntityIsAttachedTo();

                                if (attachedTo != null)
                                    attachedTo.DetachEntity(veh);
                            }
                        }
                    }
                }
            },

            {
                Types.PushVehicle,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (target is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                if (root is Vehicle veh)
                                {
                                    var vData = veh.GetMainData();

                                    if (vData == null)
                                        return;

                                    var baseSpeed = Properties.Settings.Static.PUSHING_VEHICLE_STRENGTH_MIN;
                                    var strengthCoef = pData.Info.Skills[PlayerData.SkillTypes.Strength] / (float)PlayerData.MaxSkills[PlayerData.SkillTypes.Strength];

                                    if (syncData == "1")
                                    {
                                        baseSpeed = -baseSpeed;
                                        strengthCoef = -strengthCoef;
                                    }

                                    vData.ForcedSpeed = baseSpeed + strengthCoef * (Properties.Settings.Static.PUSHING_VEHICLE_STRENGTH_MAX - Properties.Settings.Static.PUSHING_VEHICLE_STRENGTH_MIN);

                                    pData.PlayAnim(Animations.GeneralTypes.PushingVehicle);
                                }
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (target is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData != null)
                                {
                                    if (pData.GeneralAnim == Animations.GeneralTypes.PushingVehicle)
                                        pData.StopGeneralAnim();
                                }
                            }

                            if (root is Vehicle veh)
                            {
                                var vData = veh.GetMainData();

                                if (vData != null)
                                {
                                    vData.ForcedSpeed = 0f;
                                }
                            }
                        }
                    }
                }
            },

            {
                Types.VehicleTrunk,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (target is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                pData.PlayAnim(Animations.GeneralTypes.LieInTrunk);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (target is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                if (pData.GeneralAnim == Animations.GeneralTypes.LieInTrunk)
                                {
                                    if (pData.IsCuffed)
                                    {
                                        pData.PlayAnim(Animations.GeneralTypes.CuffedStatic0);
                                    }
                                    else
                                    {
                                        pData.StopGeneralAnim();
                                    }
                                }
                            }
                        }
                    }
                }
            },

            {
                Types.Carry,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {

                            if (target is Player tPlayer)
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    var tData = tPlayer.GetMainData();

                                    if (tData == null)
                                        return;

                                    pData.PlayAnim(Animations.GeneralTypes.CarryA);
                                    tData.PlayAnim(Animations.GeneralTypes.CarryB);
                                }
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData != null)
                                {
                                    if (pData.GeneralAnim == Animations.GeneralTypes.CarryA)
                                        pData.StopGeneralAnim();
                                }
                            }

                            if (target is Player tPlayer)
                            {
                                var tData = tPlayer.GetMainData();

                                if (tData != null)
                                {
                                    if (tData.GeneralAnim == Animations.GeneralTypes.CarryB)
                                    {
                                        if (tData.IsCuffed)
                                        {
                                            tData.PlayAnim(Animations.GeneralTypes.CuffedStatic0);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },

            {
                Types.ItemCigMouth,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (args.Length == 0)
                                return;

                            int maxTime = (int)args[0];
                            int maxPuffs = (int)args[1];

                            if (root is Player player)
                            {
                                player.TriggerEvent("Player::Smoke::Start", maxTime, maxPuffs);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (args.Length == 0)
                            {
                                if (root is Player player)
                                {
                                    player.TriggerEvent("Player::Smoke::Stop");
                                }
                            }
                        }
                    }
                }
            },

            {
                Types.ItemCigHand,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (args.Length == 0)
                                return;

                            int maxTime = (int)args[0];
                            int maxPuffs = (int)args[1];

                            if (root is Player player)
                            {
                                player.TriggerEvent("Player::Smoke::Start", maxTime, maxPuffs);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (args.Length == 0)
                            {
                                if (root is Player player)
                                {
                                    player.TriggerEvent("Player::Smoke::Stop");
                                }
                            }
                        }
                    }
                }
            },

            {
                Types.ItemFishingRodG,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                player.TriggerEvent("MG::F::S", args);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                player.TriggerEvent("MG::F::S");
                            }
                        }
                    }
                }
            },

            {
                Types.ItemFishG,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                player.TriggerEvent("MG::F::S", args);
                            }
                        }
                    },
                }
            },

            {
                Types.ItemShovel,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                player.TriggerEvent("MG::SHOV::S", args);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                player.TriggerEvent("MG::SHOV::S");
                            }
                        }
                    },
                }
            },

            {
                Types.FarmPlantSmallShovel,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                pData.PlayAnim(Sync.Animations.GeneralTypes.FarmPlantSmallShovelProcess0);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                var farmerJob = pData.CurrentJob as Game.Jobs.Farmer;

                                if (farmerJob == null)
                                    return;

                                int cFieldIdx; byte cRow, cCol;

                                if (Game.Jobs.Farmer.TryGetPlayerCurrentCropInfo(pData, out cFieldIdx, out cRow, out cCol))
                                    Game.Jobs.Farmer.ResetPlayerCurrentCropInfo(pData);

                                pData.StopGeneralAnim();
                            }
                        }
                    },
                }
            },

            {
                Types.FarmWateringCan,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                pData.PlayAnim(Sync.Animations.GeneralTypes.WateringCan0);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                var farmerJob = pData.CurrentJob as Game.Jobs.Farmer;

                                if (farmerJob == null)
                                    return;

                                int idx;

                                if (Game.Jobs.Farmer.TryGetPlayerCurrentOrangeTreeInfo(pData, out idx))
                                    Game.Jobs.Farmer.ResetPlayerCurrentOrangeTreeInfo(pData);

                                if (pData.GeneralAnim == Animations.GeneralTypes.WateringCan0)
                                    pData.StopGeneralAnim();
                            }
                        }
                    },
                }
            },

            {
                Types.FarmOrangeBoxCarry,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                pData.PlayAnim(Sync.Animations.GeneralTypes.BoxCarry0);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                var farmerJob = pData.CurrentJob as Game.Jobs.Farmer;

                                if (farmerJob == null)
                                    return;

                                int idx;

                                if (Game.Jobs.Farmer.TryGetPlayerCurrentOrangeTreeInfo(pData, out idx))
                                    Game.Jobs.Farmer.ResetPlayerCurrentOrangeTreeInfo(pData);

                                if (pData.GeneralAnim == Animations.GeneralTypes.BoxCarry0)
                                    pData.StopGeneralAnim();
                            }
                        }
                    },
                }
            },

            {
                Types.FarmMilkBucketCarry,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                if (pData.CrouchOn)
                                    pData.CrouchOn = false;

                                pData.PlayAnim(Sync.Animations.GeneralTypes.BucketCarryOneHand0);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                var farmerJob = pData.CurrentJob as Game.Jobs.Farmer;

                                if (farmerJob == null)
                                    return;

                                int idx;

                                if (Game.Jobs.Farmer.TryGetPlayerCurrentCowInfo(pData, out idx))
                                    Game.Jobs.Farmer.ResetPlayerCurrentCowInfo(pData);

                                if (pData.GeneralAnim == Animations.GeneralTypes.BucketCarryOneHand0)
                                    pData.StopGeneralAnim();
                            }
                        }
                    },
                }
            },

            {
                Types.EmsHealingBedFakeAttach,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                Game.Fractions.EMS ems;
                                int bedIdx;

                                if (Game.Fractions.EMS.TryGetCurrentPlayerBed(pData, out ems, out bedIdx))
                                {
                                    ems.SetBedAsFree(bedIdx);
                                }

                                if (pData.GeneralAnim == Animations.GeneralTypes.BedLie0)
                                    pData.StopGeneralAnim();
                            }
                        }
                    },
                }
            },

            {
                Types.Cuffs,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                if (player.GetEntityIsAttachedTo() is Entity entityAttachedTo)
                                {
                                    entityAttachedTo.DetachEntity(player);
                                }

                                pData.StopUseCurrentItem();

                                pData.UnequipActiveWeapon();

                                pData.Player.DetachAllEntities();
                                pData.Player.DetachAllObjectsInHand();

                                pData.PlayAnim(Animations.GeneralTypes.CuffedStatic0);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                if (player.GetEntityIsAttachedTo() is Entity entityAttachedTo)
                                {
                                    entityAttachedTo.DetachEntity(player);
                                }

                                if (pData.GeneralAnim == Animations.GeneralTypes.CuffedStatic0)
                                    pData.StopGeneralAnim();
                            }
                        }
                    },
                }
            },

            {
                Types.PoliceEscort,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                pData.PlayAnim(Animations.GeneralTypes.PoliceEscort0);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                if (pData.GeneralAnim == Animations.GeneralTypes.PoliceEscort0)
                                    pData.StopGeneralAnim();
                            }
                        }
                    },
                }
            },

            {
                Types.PlayerResurrect,

                new Dictionary<bool, Action<Entity, Entity, Types, string, object[]>>()
                {
                    {
                        true,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                pData.PlayAnim(Animations.GeneralTypes.MedicalRevive);
                            }
                        }
                    },

                    {
                        false,

                        (root, target, type, syncData, args) =>
                        {
                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                if (pData.GeneralAnim == Animations.GeneralTypes.MedicalRevive)
                                    pData.StopGeneralAnim();
                            }
                        }
                    },
                }
            },
        };

        private static Action<Entity, Entity, Types, string, object[]> GetOffAction(Types type)
        {
            var action = Actions.GetValueOrDefault(type);

            if (action == null)
            {
                Types sType;

                if (SameActionsTypes.TryGetValue(type, out sType))
                    return Actions.GetValueOrDefault(sType)?.GetValueOrDefault(false);

                return null;
            }

            return action.GetValueOrDefault(false);
        }

        private static Action<Entity, Entity, Types, string, object[]> GetOnAction(Types type)
        {
            var action = Actions.GetValueOrDefault(type);

            if (action == null)
            {
                Types sType;

                if (SameActionsTypes.TryGetValue(type, out sType))
                    return Actions.GetValueOrDefault(sType)?.GetValueOrDefault(true);

                return null;
            }

            return action.GetValueOrDefault(true);
        }

        /// <summary>Получить информацию о привязке сущности к другой сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность-владелец</param>
        /// <param name="target">Прикрепленная сущность</param>
        public static AttachmentEntityNet GetEntityAttachmentData(Entity entity, Entity target) => entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey)?.ToList<AttachmentEntityNet>().Where(x => x.Id == target.Id && x.EntityType == target.Type).FirstOrDefault();

        /// <summary>Прикрепить сущность к сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность, к которой прикрепляем</param>
        /// <param name="target">Сущность, которую прикрепляем</param>
        /// <param name="type">Тип прикрепления</param>
        public static bool AttachEntity(Entity entity, Entity target, Types type, string syncData, params object[] args)
        {
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey)?.ToList<AttachmentEntityNet>();

            if (list == null)
                return false;

            if (target.GetEntityIsAttachedTo() is Entity attachedToEntity)
                DetachEntity(attachedToEntity, target);

            var newAttachment = new AttachmentEntityNet(target.Id, target.Type, type, syncData);
            list.Add(newAttachment);

            entity.SetSharedData(AttachedEntitiesKey, list);
            target.SetData(EntityIsAttachedToKey, entity);

            GetOnAction(type)?.Invoke(entity, target, type, syncData, args);

            return true;
        }

        /// <summary>Открепить сущность от сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность, от которой открепляем</param>
        /// <param name="target">Сущность, которую открепляем</param>
        public static bool DetachEntity(Entity entity, Entity target)
        {
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey)?.ToList<AttachmentEntityNet>();

            if (list == null)
                return false;

            var item = list.Where(x => x.EntityType == target.Type && x.Id == target.Id).FirstOrDefault();

            if (item == null)
                return false;

            list.Remove(item);

            entity.SetSharedData(AttachedEntitiesKey, list);
            target.ResetData(EntityIsAttachedToKey);

            GetOffAction(item.Type)?.Invoke(entity, target, item.Type, item.SyncData, EmptyArgs);

            return true;
        }

        /// <summary>Открепить все сущности от сущности</summary>
        /// <param name="entity">Сущность, от которой открепляем</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static bool DetachAllEntities(Entity entity)
        {
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey)?.ToList<AttachmentEntityNet>();

            if (list == null)
                return false;

            foreach (var x in list)
            {
                var target = Utils.GetEntityById(x.EntityType, x.Id);

                if (target != null)
                {
                    target.ResetData(EntityIsAttachedToKey);
                }

                GetOffAction(x.Type)?.Invoke(entity, target, x.Type, x.SyncData, EmptyArgs);
            }

            list.Clear();

            entity.SetSharedData(AttachedEntitiesKey, list);

            return true;
        }

        /// <summary>Прикрепить объект к сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность</param>
        /// <param name="model">Модель объекта</param>
        /// <param name="type">Тип прикрепления</param>
        /// <param name="detachAfter">Открепить после (время в мс.), -1 - если не требуется</param>
        /// <returns>-1, если произошла ошибка, число > 0 - ID привязки в противном случае</returns>
        public static bool AttachObject(Entity entity, uint model, Types type, int detachAfter, string syncData, params object[] args)
        {
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey)?.ToList<AttachmentObjectNet>();

            if (list == null)
                return false;

            if (list.Where(x => x.Type == type).Any())
                return false;

            var newAttachment = new AttachmentObjectNet(model, type, syncData);

            list.Add(newAttachment);

            GetOnAction(type)?.Invoke(entity, null, type, syncData, args);

            entity.SetSharedData(AttachedObjectsKey, list);

            if (detachAfter != -1)
            {
                var timers = entity.GetData<Dictionary<Types, Timer>>(AttachedObjectsTimersKey);

                timers.Add(type, new Timer((obj) =>
                {
                    NAPI.Task.Run(() =>
                    {
                        if (entity?.Exists != true)
                            return;

                        entity.DetachObject(type);
                    });
                }, null, detachAfter, Timeout.Infinite));
            }

            return true;
        }

        /// <summary>Открепить объект от сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность</param>
        /// <param name="id">ID привязки</param>
        public static bool DetachObject(Entity entity, Types type, params object[] args)
        {
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey)?.ToList<AttachmentObjectNet>();

            if (list == null)
                return false;

            var item = list.Where(x => x.Type == type).FirstOrDefault();

            if (item == null)
                return false;

            var timers = entity.GetData<Dictionary<Types, Timer>>(AttachedObjectsTimersKey);

            var timer = timers.GetValueOrDefault(type);

            if (timer != null)
            {
                timer.Dispose();

                timers.Remove(type);
            }

            list.Remove(item);

            entity.SetSharedData(AttachedObjectsKey, list);

            GetOffAction(item.Type)?.Invoke(entity, null, item.Type, item.SyncData, args);

            return true;
        }

        /// <summary>Открепить все объекты от сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность</param>
        public static bool DetachAllObjects(Entity entity)
        {
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey)?.ToList<AttachmentObjectNet>();

            if (list == null)
                return false;

            var timers = entity.GetData<Dictionary<Types, Timer>>(AttachedObjectsTimersKey);

            foreach (var x in list)
            {
                if (timers.GetValueOrDefault(x.Type) is Timer timer)
                    timer.Dispose();

                GetOffAction(x.Type)?.Invoke(entity, null, x.Type, x.SyncData, EmptyArgs);
            }

            timers.Clear();

            list.Clear();

            entity.SetSharedData(AttachedObjectsKey, list);

            return true;
        }

        /// <summary>Открепить все объекты в руках (не статичные) от сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность</param>
        public static bool DetachAllObjectsInHand(Entity entity)
        {
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey).ToList<AttachmentObjectNet>();

            if (list == null)
                return false;

            var timers = entity.GetData<Dictionary<Types, Timer>>(AttachedObjectsTimersKey);

            list.ToList().ForEach(x =>
            {
                if (!IsTypeObjectInHand(x.Type))
                    return;

                if (timers.GetValueOrDefault(x.Type) is Timer timer)
                {
                    timer.Dispose();

                    timers.Remove(x.Type);
                }

                GetOffAction(x.Type)?.Invoke(entity, null, x.Type, x.SyncData, EmptyArgs);

                list.Remove(x);
            });

            entity.SetSharedData(AttachedObjectsKey, list);

            return true;
        }

        public static Entity GetEntityIsAttachedToEntity(Entity entity) => entity.GetData<Entity>(EntityIsAttachedToKey);
    }
}