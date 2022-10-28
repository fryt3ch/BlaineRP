using GTANetworkAPI;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Sync
{
    public class AttachSystem : Script
    {
        public const string AttachedObjectsKey = "AttachedObjects";
        public const string AttachedEntitiesKey = "AttachedEntities";

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

            ItemBurger,
            ItemChips,
            ItemHotdog,
            ItemChocolate,
            ItemPizza,
            ItemCola,
            ItemJoint,
            ItemBeer,
            ItemVodka,
            ItemRum,
            ItemVegSmoothie,
            ItemSmoothie,
            ItemMilkshake,
            ItemMilk,
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

        private static Dictionary<Types[], (Action<Entity, Entity, Types> On, Action<Entity, Entity, Types> Off)> Actions = new Dictionary<Types[], (Action<Entity, Entity, Types> On, Action<Entity, Entity, Types> Off)>()
        {
            {
                new Types[] {Types.PushVehicleBack, Types.PushVehicleFront },

                ((Entity root, Entity target, Types type) =>
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

                                var baseSpeed = type == Types.PushVehicleFront ? -Settings.PUSHING_VEHICLE_STRENGTH_MIN : Settings.PUSHING_VEHICLE_STRENGTH_MIN;
                                var strengthCoef = pData.Skills[PlayerData.SkillTypes.Strength] / (float)PlayerData.MaxSkills[PlayerData.SkillTypes.Strength];

                                vData.ForcedSpeed = baseSpeed > 0f ? baseSpeed + strengthCoef * (Settings.PUSHING_VEHICLE_STRENGTH_MAX - Settings.PUSHING_VEHICLE_STRENGTH_MIN) : baseSpeed - strengthCoef * (Settings.PUSHING_VEHICLE_STRENGTH_MAX - Settings.PUSHING_VEHICLE_STRENGTH_MIN);

                                pData.PlayAnim(Animations.GeneralTypes.PushingVehicle);
                            });

                            vData.Release();
                        });

                        pData.Release();
                    });
                },

                (Entity root, Entity target, Types type) =>
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
                })
            },

            {
                new Types[] {Types.VehicleTrunk },

                ((Entity root, Entity target, Types type) =>
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
                },

                (Entity root, Entity target, Types type) =>
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
                })
            },

            {
                new Types[] {Types.Carry },

                ((Entity root, Entity target, Types type) =>
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
                },

                (Entity root, Entity target, Types type) =>
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
                })
            },
        };

        private static Action<Entity, Entity, Types> GetOffAction(Types type) => Actions.Where(x => x.Key.Contains(type)).Select(x => x.Value.Off).FirstOrDefault();
        private static Action<Entity, Entity, Types> GetOnAction(Types type) => Actions.Where(x => x.Key.Contains(type)).Select(x => x.Value.On).FirstOrDefault();

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

            GetOnAction(type)?.Invoke(entity, target, type);

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

            GetOffAction(item.Type)?.Invoke(entity, target, item.Type);

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

                GetOffAction(item.Type)?.Invoke(entity, target, item.Type);
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
        public static int AttachObject(Entity entity, uint model, Types type, int detachAfter = -1)
        {
            if (entity?.Exists != true)
                return - 1;

            if (!entity.HasSharedData(AttachedObjectsKey))
                return -1;

            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey).ToList<AttachmentObjectNet>();

            var rand = new Random();
            var id = -1;
            int counter = 0;

            do
            {
                if (counter >= 1000)
                    return -1;

                id = rand.Next(0, 30000);
            }
            while (list.Where(x => x.Id == id).Any());

            var newAttachment = new AttachmentObjectNet(id, model, type);
            list.Add(newAttachment);

            entity.SetSharedData(AttachedObjectsKey, list);

            if (detachAfter != -1)
                NAPI.Task.Run(() =>
                {
                    if (entity?.Exists != true)
                        return;

                    entity.DetachObject(id);
                }, detachAfter);

            return id;
        }

        /// <summary>Открепить объект от сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность</param>
        /// <param name="id">ID привязки</param>
        public static bool DetachObject(Entity entity, int id)
        {
            if (entity?.Exists != true)
                return false;

            if (!entity.HasSharedData(AttachedObjectsKey))
                return false;

            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey).ToList<AttachmentObjectNet>();
            var item = list.Where(x => x.Id == id).FirstOrDefault();

            if (item == null)
                return false;

            list.Remove(item);

            entity.SetSharedData(AttachedObjectsKey, list);

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

            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey).ToList<AttachmentObjectNet>();

            list.Clear();

            entity.SetSharedData(AttachedObjectsKey, list);

            return true;
        }
        #endregion
    }
}
