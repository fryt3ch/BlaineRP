using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlaineRP.Server.EntityData.Players;
using BlaineRP.Server.Sync;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.Attachments
{
    public class AttachSystem
    {
        public const string AttachedObjectsKey = "AttachedObjects";
        public const string AttachedEntitiesKey = "AttachedEntities";
        public const string EntityIsAttachedToKey = "IAT::E";

        public const string AttachedObjectsTimersKey = AttachedObjectsKey + "::Timers";

        private static object[] EmptyArgs { get; } = new object[] { };

        public static bool IsTypeStaticObject(AttachmentType type) => type >= AttachmentType.PedRingLeft3 && type < AttachmentType.VehKey;

        public static bool IsTypeObjectInHand(AttachmentType type) => type >= AttachmentType.VehKey && type < AttachmentType.VehicleTrailer;

        public static class Models
        {
            public static uint Phone { get; } = NAPI.Util.GetHashKey("prop_phone_ing");

            public static uint VehicleRemoteFob { get; } = NAPI.Util.GetHashKey("lr_prop_carkey_fob");

            public static uint ParachuteSync { get; } = NAPI.Util.GetHashKey("p_parachute1_mp_dec");

            public static uint Cuffs { get; } = NAPI.Util.GetHashKey("p_cs_cuffs_02_s");
            public static uint CableCuffs { get; } = NAPI.Util.GetHashKey("brp_p_cablecuffs_0");
        }

        private static Dictionary<AttachmentType, AttachmentType> SameActionsTypes = new Dictionary<AttachmentType, AttachmentType>()
        {
            { AttachmentType.ItemCig1Hand, AttachmentType.ItemCigHand },
            { AttachmentType.ItemCig2Hand, AttachmentType.ItemCigHand },
            { AttachmentType.ItemCig3Hand, AttachmentType.ItemCigHand },

            { AttachmentType.ItemCig1Mouth, AttachmentType.ItemCigMouth },
            { AttachmentType.ItemCig2Mouth, AttachmentType.ItemCigMouth },
            { AttachmentType.ItemCig3Mouth, AttachmentType.ItemCigMouth },
        };

        private static Dictionary<AttachmentType, Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>> Actions = new Dictionary<AttachmentType, Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>>()
        {
            {
                AttachmentType.TrailerObjOnBoat,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.PushVehicle,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                                    var strengthCoef = pData.Info.Skills[SkillTypes.Strength] / (float)Properties.Settings.Static.PlayerMaxSkills.GetValueOrDefault(SkillTypes.Strength);

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
                AttachmentType.VehicleTrunk,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.Carry,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.ItemCigMouth,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.ItemCigHand,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.ItemFishingRodG,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.ItemFishG,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.ItemShovel,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.FarmPlantSmallShovel,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.FarmWateringCan,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.FarmOrangeBoxCarry,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.FarmMilkBucketCarry,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.EmsHealingBedFakeAttach,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.Cuffs,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.PoliceEscort,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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
                AttachmentType.PlayerResurrect,

                new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
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

        private static Action<Entity, Entity, AttachmentType, string, object[]> GetOffAction(AttachmentType type)
        {
            var action = Actions.GetValueOrDefault(type);

            if (action == null)
            {
                AttachmentType sType;

                if (SameActionsTypes.TryGetValue(type, out sType))
                    return Actions.GetValueOrDefault(sType)?.GetValueOrDefault(false);

                return null;
            }

            return action.GetValueOrDefault(false);
        }

        private static Action<Entity, Entity, AttachmentType, string, object[]> GetOnAction(AttachmentType type)
        {
            var action = Actions.GetValueOrDefault(type);

            if (action == null)
            {
                AttachmentType sType;

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
        public static bool AttachEntity(Entity entity, Entity target, AttachmentType type, string syncData, params object[] args)
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
        public static bool AttachObject(Entity entity, uint model, AttachmentType type, int detachAfter, string syncData, params object[] args)
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
                var timers = entity.GetData<Dictionary<AttachmentType, Timer>>(AttachedObjectsTimersKey);

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
        public static bool DetachObject(Entity entity, AttachmentType type, params object[] args)
        {
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey)?.ToList<AttachmentObjectNet>();

            if (list == null)
                return false;

            var item = list.Where(x => x.Type == type).FirstOrDefault();

            if (item == null)
                return false;

            var timers = entity.GetData<Dictionary<AttachmentType, Timer>>(AttachedObjectsTimersKey);

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

            var timers = entity.GetData<Dictionary<AttachmentType, Timer>>(AttachedObjectsTimersKey);

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

            var timers = entity.GetData<Dictionary<AttachmentType, Timer>>(AttachedObjectsTimersKey);

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
