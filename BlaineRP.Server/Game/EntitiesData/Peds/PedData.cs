using System;
using System.Collections.Generic;
using System.Threading;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.EntitiesData.Peds
{
    public class PedData
    {
        public static Dictionary<ushort, PedData> All = new Dictionary<ushort, PedData>();

        public static PedData GetData(ushort id) => All.GetValueOrDefault(id);

        public static PedData GetData(Ped ped) => All.GetValueOrDefault(ped.Id);

        public void Destroy()
        {
            if (!All.Remove(Ped.Id))
                return;

            Ped.Delete();
        }

        public Ped Ped { get; set; }

        /// <summary>Прикрепленные объекты к транспорту</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<AttachmentObjectNet> AttachedObjects { get => Ped.GetSharedData<Newtonsoft.Json.Linq.JArray>(Service.AttachedObjectsKey).ToObject<List<AttachmentObjectNet>>(); set { Ped.SetSharedData(Service.AttachedObjectsKey, value); } }

        /// <summary>Прикрепленные сущности к транспорту</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<AttachmentEntityNet> AttachedEntities { get => Ped.GetSharedData<Newtonsoft.Json.Linq.JArray>(Service.AttachedEntitiesKey).ToObject<List<AttachmentEntityNet>>(); set { Ped.SetSharedData(Service.AttachedEntitiesKey, value); } }

        public Entity IsAttachedTo => Ped.GetEntityIsAttachedTo();

        public bool IsInvincible { get => Ped.GetSharedData<bool?>("GM") == true; set { if (value) Ped.SetSharedData("GM", true); else Ped.ResetSharedData("GM"); } }

        public PedData(uint Model, Vector4 Position, uint Dimension, Action PostCreationAction = null)
        {
            Ped = NAPI.Ped.CreatePed(Model, Position.Position, Position.RotationZ, true, false, false, true, Properties.Settings.Static.StuffDimension);

            All.Add(Ped.Id, this);

            AttachedObjects = new List<AttachmentObjectNet>();
            AttachedEntities = new List<AttachmentEntityNet>();

            Ped.SetData(Service.AttachedObjectsTimersKey, new Dictionary<AttachmentType, Timer>());

            NAPI.Task.Run(() =>
            {
                if (Ped?.Exists != true)
                    return;

                Ped.Dimension = Dimension;

                PostCreationAction?.Invoke();
            }, 2000);
        }
    }
}
