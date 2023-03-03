using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace BCRPServer.Sync
{
    public class AttachSystem
    {
        public const string AttachedObjectsKey = "AttachedObjects";
        public const string AttachedEntitiesKey = "AttachedEntities";
        public const string EntityIsAttachedToKey = "IAT::E";

        public const string AttachedObjectsCancelsKey = AttachedObjectsKey + "::Cancels";

        private static object[] EmptyArgs { get; } = new object[] { };

        public static bool IsTypeStaticObject(Types type) => type >= Types.PedRingLeft3 && type <= Types.WeaponRightBack;

        public static bool IsTypeObjectInHand(Types type) => type >= Types.VehKey && type <= Types.ItemMedKit;

        #region Types
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

            Phone,

            #endregion

            #region Object In Hand Types | Типы, наличие у игрока которых запрещает определенные действия (ведь предмет находится в руках)

            VehKey,

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
            #endregion

            #endregion

            #region Entity-Entity Attach | Типы, которые прикрепляют серверную сущность с серверной сущности
            /// <summary>Прикрепление СЕРВЕРНОГО трейлера к СЕРВЕРНОМУ транспорту</summary>
            VehicleTrailer,

            /// <summary>Прикрепление ЛОКАЛЬНОГО трейлера (создается локально при прикреплении) к СЕРВЕРНОЙ лодке</summary>
            TrailerObjOnBoat,

            /// <summary>Прикрепление СЕРВЕРНОГО транспорта к СЕРВЕРНОЙ лодке (к которой должен быть прикреплен TrailerObjOnBoat)</summary>
            VehicleTrailerObjBoat,

            PushVehicleFront,
            PushVehicleBack,

            Carry,
            PiggyBack,
            Hostage,

            VehicleTrunk, VehicleTrunkForced,

            #endregion
        }
        #endregion

        public static class Models
        {
            public static uint Phone { get; } = NAPI.Util.GetHashKey("prop_phone_ing");

            public static uint VehicleRemoteFob { get; } = NAPI.Util.GetHashKey("lr_prop_carkey_fob");
        }

        public class AttachmentObjectNet
        {
            [JsonProperty(PropertyName = "M")]
            public uint Model { get; set; }

            [JsonProperty(PropertyName = "D", NullValueHandling = NullValueHandling.Ignore)]
            public string SyncData { get; set; }

            [JsonProperty(PropertyName = "T")]
            public Types Type { get; set; }

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
            public int Id { get; set; }

            [JsonProperty(PropertyName = "T")]
            public Types Type { get; set; }

            public AttachmentEntityNet(int Id, EntityType EntityType, Types Type)
            {
                this.Id = Id;
                this.EntityType = EntityType;

                this.Type = Type;
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

        private static Dictionary<Types, Dictionary<bool, Action<Entity, Entity, Types, object[]>>> Actions = new Dictionary<Types, Dictionary<bool, Action<Entity, Entity, Types, object[]>>>()
        {
            {
                Types.PushVehicleBack,

                new Dictionary<bool, Action<Entity, Entity, Types, object[]>>()
                {
                    {
                        true,

                        (Entity root, Entity target, Types type, object[] args) =>
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

                                    var baseSpeed = Settings.PUSHING_VEHICLE_STRENGTH_MIN;
                                    var strengthCoef = pData.Skills[PlayerData.SkillTypes.Strength] / (float)PlayerData.MaxSkills[PlayerData.SkillTypes.Strength];

                                    vData.ForcedSpeed = baseSpeed + strengthCoef * (Settings.PUSHING_VEHICLE_STRENGTH_MAX - Settings.PUSHING_VEHICLE_STRENGTH_MIN);

                                    pData.PlayAnim(Animations.GeneralTypes.PushingVehicle);
                                }
                            }
                        }
                    },

                    {
                        false,

                        (Entity root, Entity target, Types type, object[] args) =>
                        {
                            if (target is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData != null)
                                {
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
                Types.PushVehicleFront,

                new Dictionary<bool, Action<Entity, Entity, Types, object[]>>()
                {
                    {
                        true,

                        (Entity root, Entity target, Types type, object[] args) =>
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

                                    var baseSpeed = -Settings.PUSHING_VEHICLE_STRENGTH_MIN;
                                    var strengthCoef = pData.Skills[PlayerData.SkillTypes.Strength] / (float)PlayerData.MaxSkills[PlayerData.SkillTypes.Strength];

                                    vData.ForcedSpeed = baseSpeed - strengthCoef * (Settings.PUSHING_VEHICLE_STRENGTH_MAX - Settings.PUSHING_VEHICLE_STRENGTH_MIN);

                                    pData.PlayAnim(Animations.GeneralTypes.PushingVehicle);
                                }
                            }
                        }
                    },

                    {
                        false,

                        (Entity root, Entity target, Types type, object[] args) =>
                        {
                            if (target is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData != null)
                                {
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

                new Dictionary<bool, Action<Entity, Entity, Types, object[]>>()
                {
                    {
                        true,

                        (Entity root, Entity target, Types type, object[] args) =>
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

                        (Entity root, Entity target, Types type, object[] args) =>
                        {
                            if (target is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                pData.StopGeneralAnim();
                            }
                        }
                    }
                }
            },

            {
                Types.Carry,

                new Dictionary<bool, Action<Entity, Entity, Types, object[]>>()
                {
                    {
                        true,

                        (Entity root, Entity target, Types type, object[] args) =>
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

                        (Entity root, Entity target, Types type, object[] args) =>
                        {
                            if (target is Player tPlayer)
                            {
                                var tData = tPlayer.GetMainData();

                                if (tData != null)
                                {
                                    tData.StopGeneralAnim();
                                }
                            }

                            if (root is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData != null)
                                {
                                    pData.StopGeneralAnim();
                                }
                            }
                        }
                    }
                }
            },

            {
                Types.ItemCigMouth,

                new Dictionary<bool, Action<Entity, Entity, Types, object[]>>()
                {
                    {
                        true,

                        (entity, entity2, type, args) =>
                        {
                            if (args.Length == 0)
                                return;

                            int maxTime = (int)args[0];
                            int maxPuffs = (int)args[1];

                            if (entity is Player player)
                            {
                                player.TriggerEvent("Player::Smoke::Start", maxTime, maxPuffs);
                            }
                        }
                    },

                    {
                        false,

                        (entity, entity2, type, args) =>
                        {
                            if (args.Length == 0)
                            {
                                if (entity is Player player)
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

                new Dictionary<bool, Action<Entity, Entity, Types, object[]>>()
                {
                    {
                        true,

                        (entity, entity2, type, args) =>
                        {
                            if (args.Length == 0)
                                return;

                            int maxTime = (int)args[0];
                            int maxPuffs = (int)args[1];

                            if (entity is Player player)
                            {
                                player.TriggerEvent("Player::Smoke::Start", maxTime, maxPuffs);
                            }
                        }
                    },

                    {
                        false,

                        (entity, entity2, type, args) =>
                        {
                            if (args.Length == 0)
                            {
                                if (entity is Player player)
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

                new Dictionary<bool, Action<Entity, Entity, Types, object[]>>()
                {
                    {
                        true,

                        (entity, entity2, type, args) =>
                        {
                            if (entity is Player player)
                            {
                                player.TriggerEvent("MG::F::S", args);
                            }
                        }
                    },

                    {
                        false,

                        (entity, entity2, type, args) =>
                        {
                            if (entity is Player player)
                            {
                                player.TriggerEvent("MG::F::S");
                            }
                        }
                    }
                }
            },

            {
                Types.ItemFishG,

                new Dictionary<bool, Action<Entity, Entity, Types, object[]>>()
                {
                    {
                        true,

                        (entity, entity2, type, args) =>
                        {
                            if (entity is Player player)
                            {
                                player.TriggerEvent("MG::F::S", args);
                            }
                        }
                    },
                }
            },

            {
                Types.ItemShovel,

                new Dictionary<bool, Action<Entity, Entity, Types, object[]>>()
                {
                    {
                        true,

                        (entity, entity2, type, args) =>
                        {
                            if (entity is Player player)
                            {
                                player.TriggerEvent("MG::SHOV::S", args);
                            }
                        }
                    },

                    {
                        false,

                        (entity, entity2, type, args) =>
                        {
                            if (entity is Player player)
                            {
                                player.TriggerEvent("MG::SHOV::S");
                            }
                        }
                    },
                }
            },

            {
                Types.FarmPlantSmallShovel,

                new Dictionary<bool, Action<Entity, Entity, Types, object[]>>()
                {
                    {
                        true,

                        (entity, entity2, type, args) =>
                        {
                            if (entity is Player player)
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

                        (entity, entity2, type, args) =>
                        {
                            if (entity is Player player)
                            {
                                var pData = player.GetMainData();

                                if (pData == null)
                                    return;

                                var farmerJob = pData.CurrentJob as Game.Jobs.Farmer;

                                if (farmerJob == null)
                                    return;

                                int cFieldIdx; byte cRow, cCol;

                                if (Game.Jobs.Farmer.TryGetPlayerCurrentCropInfo(pData, out cFieldIdx, out cRow, out cCol))
                                {
                                    var info = farmerJob.FarmBusiness.CropFields[cFieldIdx].CropsData[cRow][cCol];

                                    if (info.CTS != null)
                                    {
                                        info.CTS.Cancel();
                                        info.CTS.Dispose();

                                        info.CTS = null;
                                    }

                                    Game.Jobs.Farmer.ResetPlayerCurrentCropInfo(pData);
                                }

                                pData.StopGeneralAnim();
                            }
                        }
                    },
                }
            },
        };

        private static Action<Entity, Entity, Types, object[]> GetOffAction(Types type)
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

        private static Action<Entity, Entity, Types, object[]> GetOnAction(Types type)
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

        #region Entities
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
        public static bool AttachEntity(Entity entity, Entity target, Types type)
        {
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey)?.ToList<AttachmentEntityNet>();

            if (list == null)
                return false;

            if (list.Where(x => x.Id == target.Id).Any())
                DetachEntity(entity, target);

            var newAttachment = new AttachmentEntityNet(target.Id, target.Type, type);
            list.Add(newAttachment);

            entity.SetSharedData(AttachedEntitiesKey, list);
            target.SetData(EntityIsAttachedToKey, entity);

            GetOnAction(type)?.Invoke(entity, target, type, EmptyArgs);

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

            GetOffAction(item.Type)?.Invoke(entity, target, item.Type, EmptyArgs);

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

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];

                Entity target = Utils.GetEntityById(item.EntityType, item.Id);

                if (target != null)
                {
                    target.ResetData(EntityIsAttachedToKey);
                }

                GetOffAction(item.Type)?.Invoke(entity, target, item.Type, EmptyArgs);
            }

            list.Clear();

            entity.SetSharedData(AttachedEntitiesKey, list);

            return true;
        }
        #endregion

        #region Objects
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

            GetOnAction(type)?.Invoke(entity, null, type, args);

            entity.SetSharedData(AttachedObjectsKey, list);

            if (detachAfter != -1)
            {
                var cancels = entity.GetData<Dictionary<Types, CancellationTokenSource>>(AttachedObjectsCancelsKey);

                var cts = new CancellationTokenSource();

                cancels.Add(type, cts);

                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        await System.Threading.Tasks.Task.Delay(detachAfter, cts.Token);

                        NAPI.Task.Run(() =>
                        {
                            if (entity?.Exists != true)
                                return;

                            entity.DetachObject(type);
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                });

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

            var cancels = entity.GetData<Dictionary<Types, CancellationTokenSource>>(AttachedObjectsCancelsKey);

            var cts = cancels.GetValueOrDefault(type);

            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();

                cancels.Remove(type);
            }

            list.Remove(item);

            entity.SetSharedData(AttachedObjectsKey, list);

            GetOffAction(item.Type)?.Invoke(entity, null, item.Type, args);

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

            var cancels = entity.GetData<Dictionary<Types, CancellationTokenSource>>(AttachedObjectsCancelsKey);

            foreach (var x in list)
            {
                if (cancels.GetValueOrDefault(x.Type) is CancellationTokenSource cts)
                {
                    cts.Cancel();

                    cts.Dispose();

                    cancels.Remove(x.Type);
                }

                GetOffAction(x.Type)?.Invoke(entity, null, x.Type, EmptyArgs);

                list.Remove(x);
            }

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

            var cancels = entity.GetData<Dictionary<Types, CancellationTokenSource>>(AttachedObjectsCancelsKey);

            list.ToList().ForEach(x =>
            {
                if (!IsTypeObjectInHand(x.Type))
                    return;

                if (cancels.GetValueOrDefault(x.Type) is CancellationTokenSource cts)
                {
                    cts.Cancel();

                    cts.Dispose();

                    cancels.Remove(x.Type);
                }

                GetOffAction(x.Type)?.Invoke(entity, null, x.Type, EmptyArgs);

                list.Remove(x);
            });

            entity.SetSharedData(AttachedObjectsKey, list);

            return true;
        }

        public static Entity GetEntityIsAttachedToEntity(Entity entity) => entity.GetData<Entity>(EntityIsAttachedToKey);
        #endregion
    }
}
