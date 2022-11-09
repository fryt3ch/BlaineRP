using GTANetworkAPI;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BCRPServer.Sync
{
    public class AttachSystem : Script
    {
        public const string AttachedObjectsKey = "AttachedObjects";
        public const string AttachedEntitiesKey = "AttachedEntities";

        public const string AttachedObjectsIDsKey = AttachedObjectsKey + "::IDs";
        public const string AttachedObjectsCancelsKey = AttachedObjectsKey + "::Cancels";

        private static object[] EmptyArgs = new object[] { };

        public static Types[] StaticObjectsTypes = new Types[] { Types.WeaponRightTight, Types.WeaponLeftTight, Types.WeaponRightBack, Types.WeaponLeftBack };

        #region Types
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
        #endregion

        public static class Models
        {
            public static uint Phone = NAPI.Util.GetHashKey("prop_phone_ing");
            public static uint VehicleRemoteFob = NAPI.Util.GetHashKey("lr_prop_carkey_fob");
        }

        public class AttachmentObjectNet
        {
            public uint Model { get; set; }
            public int Id { get; set; }
            public Types Type { get; set; }

            public AttachmentObjectNet(int Id, uint Model, Types Type)
            {
                this.Id = Id;
                this.Model = Model;
                this.Type = Type;
            }
        }

        public class AttachmentEntityNet
        {
            public EntityType EntityType { get; set; }
            public int Id { get; set; }
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
                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                var pData = (target as Player).GetMainData();

                                if (!await pData.WaitAsync())
                                    return;

                                await System.Threading.Tasks.Task.Run(async () =>
                                {
                                    var vData = (root as Vehicle).GetMainData();

                                    if (!await vData.WaitAsync())
                                        return;

                                    await NAPI.Task.RunAsync(() =>
                                    {
                                        if (root?.Exists != true || target?.Exists != true)
                                            return;

                                        var baseSpeed = Settings.PUSHING_VEHICLE_STRENGTH_MIN;
                                        var strengthCoef = pData.Skills[PlayerData.SkillTypes.Strength] / (float)PlayerData.MaxSkills[PlayerData.SkillTypes.Strength];

                                        vData.ForcedSpeed = baseSpeed + strengthCoef * (Settings.PUSHING_VEHICLE_STRENGTH_MAX - Settings.PUSHING_VEHICLE_STRENGTH_MIN);

                                        pData.PlayAnim(Animations.GeneralTypes.PushingVehicle);
                                    });

                                    vData.Release();
                                });

                                pData.Release();
                            });
                        }
                    },

                    {
                        false,

                        (Entity root, Entity target, Types type, object[] args) =>
                        {
                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                var pData = (target as Player).GetMainData();

                                if (!await pData.WaitAsync())
                                    return;

                                await NAPI.Task.RunAsync(() =>
                                {
                                    if (target?.Exists != true)
                                        return;

                                    pData.StopAnim();
                                });

                                pData.Release();
                            });

                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                var vData = (root as Vehicle).GetMainData();

                                if (!await vData.WaitAsync())
                                    return;

                                await NAPI.Task.RunAsync(() =>
                                {
                                    if (root?.Exists != true)
                                        return;

                                    vData.ForcedSpeed = 0f;
                                });

                                vData.Release();
                            });
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
                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                var pData = (target as Player).GetMainData();

                                if (!await pData.WaitAsync())
                                    return;

                                await System.Threading.Tasks.Task.Run(async () =>
                                {
                                    var vData = (root as Vehicle).GetMainData();

                                    if (!await vData.WaitAsync())
                                        return;

                                    await NAPI.Task.RunAsync(() =>
                                    {
                                        if (root?.Exists != true || target?.Exists != true)
                                            return;

                                        var baseSpeed = -Settings.PUSHING_VEHICLE_STRENGTH_MIN;
                                        var strengthCoef = pData.Skills[PlayerData.SkillTypes.Strength] / (float)PlayerData.MaxSkills[PlayerData.SkillTypes.Strength];

                                        vData.ForcedSpeed = baseSpeed - strengthCoef * (Settings.PUSHING_VEHICLE_STRENGTH_MAX - Settings.PUSHING_VEHICLE_STRENGTH_MIN);

                                        pData.PlayAnim(Animations.GeneralTypes.PushingVehicle);
                                    });

                                    vData.Release();
                                });

                                pData.Release();
                            });
                        }
                    },

                    {
                        false,

                        (Entity root, Entity target, Types type, object[] args) =>
                        {
                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                var pData = (target as Player).GetMainData();

                                if (!await pData.WaitAsync())
                                    return;

                                await NAPI.Task.RunAsync(() =>
                                {
                                    if (target?.Exists != true)
                                        return;

                                    pData.StopAnim();
                                });

                                pData.Release();
                            });

                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                var vData = (root as Vehicle).GetMainData();

                                if (!await vData.WaitAsync())
                                    return;

                                await NAPI.Task.RunAsync(() =>
                                {
                                    if (root?.Exists != true)
                                        return;

                                    vData.ForcedSpeed = 0f;
                                });

                                vData.Release();
                            });
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
                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                var pData = (target as Player).GetMainData();

                                if (!await pData.WaitAsync())
                                    return;

                                await NAPI.Task.RunAsync(() =>
                                {
                                    if (root?.Exists != true || target?.Exists != true)
                                        return;

                                    pData.PlayAnim(Animations.GeneralTypes.LieInTrunk);
                                });

                                pData.Release();
                            });
                        }
                    },

                    {
                        false,

                        (Entity root, Entity target, Types type, object[] args) =>
                        {
                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                var pData = (target as Player).GetMainData();

                                if (!await pData.WaitAsync())
                                    return;

                                await NAPI.Task.RunAsync(() =>
                                {
                                    if (target?.Exists != true)
                                        return;

                                    pData.StopAnim();
                                });

                                pData.Release();
                            });
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
                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                var pData = (target as Player).GetMainData();

                                if (!await pData.WaitAsync())
                                    return;

                                await System.Threading.Tasks.Task.Run(async () =>
                                {
                                    var tData = (root as Player).GetMainData();

                                    if (!await tData.WaitAsync())
                                        return;

                                    await NAPI.Task.RunAsync(() =>
                                    {
                                        if (root?.Exists != true || target?.Exists != true)
                                            return;

                                        pData.PlayAnim(Animations.GeneralTypes.CarryB);
                                        tData.PlayAnim(Animations.GeneralTypes.CarryA);
                                    });

                                    tData.Release();
                                });

                                pData.Release();
                            });
                        }
                    },

                    {
                        false,

                        (Entity root, Entity target, Types type, object[] args) =>
                        {
                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                var pData = (target as Player).GetMainData();

                                if (!await pData.WaitAsync())
                                    return;

                                await NAPI.Task.RunAsync(() =>
                                {
                                    if (target?.Exists != true)
                                        return;

                                    pData.StopAnim();
                                });

                                pData.Release();
                            });

                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                var tData = (root as Player).GetMainData();

                                if (!await tData.WaitAsync())
                                    return;

                                await NAPI.Task.RunAsync(() =>
                                {
                                    if (root?.Exists != true)
                                        return;

                                    tData.StopAnim();
                                });

                                tData.Release();
                            });
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
            }
        };

        private static Action<Entity, Entity, Types, object[]> GetOffAction(Types type)
        {
            var action = Actions.GetValueOrDefault(type);

            if (action == null)
            {
                Types sType;

                if (SameActionsTypes.TryGetValue(type, out sType))
                    return Actions.GetValueOrDefault(sType)?[false];

                return null;
            }

            return action[false];
        }

        private static Action<Entity, Entity, Types, object[]> GetOnAction(Types type)
        {
            var action = Actions.GetValueOrDefault(type);

            if (action == null)
            {
                Types sType;

                if (SameActionsTypes.TryGetValue(type, out sType))
                    return Actions.GetValueOrDefault(sType)?[true];

                return null;
            }

            return action[true];
        }

        #region Entities
        /// <summary>Получить информацию о привязке сущности к другой сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность-владелец</param>
        /// <param name="target">Прикрепленная сущность</param>
        public static AttachmentEntityNet GetEntityAttachmentData(Entity entity, Entity target)
        {
            if (entity?.Exists != true || target?.Exists != true)
                return null;

            if (!entity.HasSharedData(AttachedEntitiesKey))
                return null;

            return entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey).ToList<AttachmentEntityNet>().Where(x => x.Id == target.Id).FirstOrDefault();
        }

        /// <summary>Прикрепить сущность к сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность, к которой прикрепляем</param>
        /// <param name="target">Сущность, которую прикрепляем</param>
        /// <param name="type">Тип прикрепления</param>
        public static bool AttachEntity(Entity entity, Entity target, Types type)
        {
            if (entity?.Exists != true || target?.Exists != true)
                return false;

            if (!entity.HasSharedData(AttachedEntitiesKey))
                return false;

            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey).ToList<AttachmentEntityNet>();

            if (list.Where(x => x.Id == target.Id).FirstOrDefault() != null)
                DetachEntity(entity, target);

            var newAttachment = new AttachmentEntityNet(target.Id, target.Type, type);
            list.Add(newAttachment);

            entity.SetSharedData(AttachedEntitiesKey, list);
            target.SetData("IsAttachedTo::Entity", (entity, type));

            GetOnAction(type)?.Invoke(entity, target, type, EmptyArgs);

            return true;
        }

        /// <summary>Открепить сущность от сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность, от которой открепляем</param>
        /// <param name="target">Сущность, которую открепляем</param>
        public static bool DetachEntity(Entity entity, Entity target)
        {
            if (entity?.Exists != true || target?.Exists != true)
                return false;

            if (!entity.HasSharedData(AttachedEntitiesKey))
                return false;

            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey).ToList<AttachmentEntityNet>(); ;
            var item = list.Where(x => x.EntityType == target.Type && x.Id == target.Id).FirstOrDefault();

            if (item == null)
                return false;

            list.Remove(item);

            entity.SetSharedData(AttachedEntitiesKey, list);
            target.SetData("IsAttachedTo::Entity", ((Entity, Types)?)null);

            GetOffAction(item.Type)?.Invoke(entity, target, item.Type, EmptyArgs);

            return true;
        }

        /// <summary>Открепить все сущности от сущности</summary>
        /// <param name="entity">Сущность, от которой открепляем</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static bool DetachAllEntities(Entity entity)
        {
            if (entity?.Exists != true)
                return false;

            if (!entity.HasSharedData(AttachedEntitiesKey))
                return false;

            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey).ToList<AttachmentEntityNet>();

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];

                Entity target = Utils.GetEntityById(item.EntityType, item.Id);

                if (target != null)
                {
                    target.SetData("IsAttachedTo::Entity", ((Entity, Types)?)null);
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
        public static int AttachObject(Entity entity, uint model, Types type, int detachAfter = -1, params object[] args)
        {
            if (entity?.Exists != true)
                return - 1;

            if (!entity.HasSharedData(AttachedObjectsKey))
                return -1;

            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey).ToList<AttachmentObjectNet>();

            var ids = entity.GetData<Queue<int>>(AttachedObjectsIDsKey);

            int id;

            if (!ids.TryDequeue(out id))
            {
                id = list.Count;
            }
            else
            {
                entity.SetData(AttachedObjectsIDsKey, ids);
            }

            var newAttachment = new AttachmentObjectNet(id, model, type);

            list.Add(newAttachment);

            GetOnAction(type)?.Invoke(entity, null, type, args);

            entity.SetSharedData(AttachedObjectsKey, list);

            if (detachAfter != -1)
            {
                var cancels = entity.GetData<Dictionary<int, CancellationTokenSource>>(AttachedObjectsCancelsKey);

                var cts = new CancellationTokenSource();

                cancels.Add(id, cts);

                entity.SetData(AttachedObjectsCancelsKey, cancels);

                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        await System.Threading.Tasks.Task.Delay(detachAfter, cts.Token);

                        NAPI.Task.Run(() =>
                        {
                            if (entity?.Exists != true)
                                return;

                            entity.DetachObject(id);
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                });

            }

            return id;
        }

        /// <summary>Открепить объект от сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность</param>
        /// <param name="id">ID привязки</param>
        public static bool DetachObject(Entity entity, int id, params object[] args)
        {
            if (entity?.Exists != true)
                return false;

            if (!entity.HasSharedData(AttachedObjectsKey))
                return false;

            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey).ToList<AttachmentObjectNet>();
            var item = list.Where(x => x.Id == id).FirstOrDefault();

            if (item == null)
                return false;

            var ids = entity.GetData<Queue<int>>(AttachedObjectsIDsKey);
            var cancels = entity.GetData<Dictionary<int, CancellationTokenSource>>(AttachedObjectsCancelsKey);

            var cts = cancels.GetValueOrDefault(id);

            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();

                cancels.Remove(id);

                entity.SetData(AttachedObjectsCancelsKey, cancels);
            }

            ids.Enqueue(id);

            entity.SetData(AttachedObjectsIDsKey, ids);

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
            if (entity?.Exists != true)
                return false;

            if (!entity.HasSharedData(AttachedObjectsKey))
                return false;

            var cancels = entity.GetData<Dictionary<int, CancellationTokenSource>>(AttachedObjectsCancelsKey);

            foreach (var x in cancels)
            {
                x.Value.Cancel();
                x.Value.Dispose();
            }

            cancels.Clear();

            entity.SetData(AttachedObjectsCancelsKey, cancels);

            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey).ToList<AttachmentObjectNet>();

            foreach (var x in list)
            {
                GetOffAction(x.Type)?.Invoke(entity, null, x.Type, EmptyArgs);
            }

            list.Clear();

            entity.SetSharedData(AttachedObjectsKey, list);

            var ids = entity.GetData<Queue<int>>(AttachedObjectsIDsKey);

            ids.Clear();

            entity.SetData(AttachedObjectsIDsKey, ids);

            return true;
        }
        #endregion
    }
}
