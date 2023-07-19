using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BCRPServer
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
        public List<Sync.AttachSystem.AttachmentObjectNet> AttachedObjects { get => Ped.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedObjectsKey).ToList<Sync.AttachSystem.AttachmentObjectNet>(); set { Ped.SetSharedData(Sync.AttachSystem.AttachedObjectsKey, value); } }

        /// <summary>Прикрепленные сущности к транспорту</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentEntityNet> AttachedEntities { get => Ped.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedEntitiesKey).ToList<Sync.AttachSystem.AttachmentEntityNet>(); set { Ped.SetSharedData(Sync.AttachSystem.AttachedEntitiesKey, value); } }

        public Entity IsAttachedTo => Ped.GetEntityIsAttachedTo();

        public bool IsInvincible { get => Ped.GetSharedData<bool?>("GM") == true; set { if (value) Ped.SetSharedData("GM", true); else Ped.ResetSharedData("GM"); } }

        public PedData(uint Model, Utils.Vector4 Position, uint Dimension, Action PostCreationAction = null)
        {
            Ped = NAPI.Ped.CreatePed(Model, Position.Position, Position.RotationZ, true, false, false, true, Properties.Settings.Profile.Current.Game.StuffDimension);

            All.Add(Ped.Id, this);

            AttachedObjects = new List<Sync.AttachSystem.AttachmentObjectNet>();
            AttachedEntities = new List<Sync.AttachSystem.AttachmentEntityNet>();

            Ped.SetData(Sync.AttachSystem.AttachedObjectsTimersKey, new Dictionary<Sync.AttachSystem.Types, Timer>());

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
