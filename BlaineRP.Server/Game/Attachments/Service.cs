using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Attachments
{
    public partial class Service
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
        public static AttachmentEntityNet GetEntityAttachmentData(Entity entity, Entity target) => entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey)?.ToObject<List<AttachmentEntityNet>>().Where(x => x.Id == target.Id && x.EntityType == target.Type).FirstOrDefault();

        /// <summary>Прикрепить сущность к сущности</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="entity">Сущность, к которой прикрепляем</param>
        /// <param name="target">Сущность, которую прикрепляем</param>
        /// <param name="type">Тип прикрепления</param>
        public static bool AttachEntity(Entity entity, Entity target, AttachmentType type, string syncData, params object[] args)
        {
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey)?.ToObject<List<AttachmentEntityNet>>();

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
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey)?.ToObject<List<AttachmentEntityNet>>();

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
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey)?.ToObject<List<AttachmentEntityNet>>();

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
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey)?.ToObject<List<AttachmentObjectNet>>();

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
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey)?.ToObject<List<AttachmentObjectNet>>();

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
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey)?.ToObject<List<AttachmentObjectNet>>();

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
            var list = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey).ToObject<List<AttachmentObjectNet>>();

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
