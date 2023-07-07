using GTANetworkAPI;
using System.Collections.Generic;

namespace BCRPServer
{
    public partial class VehicleData
    {
        /// <summary>ID багажника</summary>
        /// <remarks>Фактически, это ID контейнера (см. Game.Items.Container)</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>ID багажника, если отсутвует - null</value>
        public uint TID { get => Info.TID; set { if (value == 0) Vehicle.ResetSharedData("TID"); else Vehicle.SetSharedData("TID", value); Info.TID = value; } }

        /// <summary>Уровень топлива</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public float FuelLevel { get => Info.LastData.Fuel; set { Vehicle.TriggerEventOccupants("Vehicles::Fuel", value); Info.LastData.Fuel = value; } }

        /// <summary>Пробег</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Пробег в метрах</value>
        public float Mileage { get => Info.LastData.Mileage; set { Vehicle.TriggerEventOccupants("Vehicles::Mileage", value); Info.LastData.Mileage = value; } }

        /// <summary>Включён ли двигатель?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool EngineOn { get => Vehicle.GetSharedData<bool>("Engine::On"); set { Vehicle.SetSharedData("Engine::On", value); } }

        /// <summary>Заблокированы ли двери?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool Locked { get => Vehicle.GetSharedData<bool>("Doors::Locked"); set { Vehicle.SetSharedData("Doors::Locked", value); Vehicle.Locked = value; } }

        /// <summary>Включёны ли фары?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool LightsOn { get => Vehicle.GetSharedData<bool>("Lights::On"); set { Vehicle.SetSharedData("Lights::On", value); } }

        /// <summary>Включён ли левый поворотник?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public byte IndicatorsState { get => (byte)Vehicle.GetSharedData<int>("Inds"); set { Vehicle.SetSharedData("Inds", value); } }


        /// <summary>Текущая радиостанция</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>255 - радио выключено</value>
        public StationTypes Radio { get => (StationTypes)Vehicle.GetSharedData<int>("Radio"); set { Vehicle.SetSharedData("Radio", (byte)value); } }

        /// <summary>Текущая скорость толкания транспорта</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public float ForcedSpeed { get => Vehicle.GetSharedData<float?>("ForcedSpeed") ?? 0f; set { if (value != 0f) Vehicle.SetSharedData("ForcedSpeed", value); else Vehicle.ResetSharedData("ForcedSpeed"); } }

        /// <summary>Заблокирован ли багажник?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool TrunkLocked { get => Vehicle.GetSharedData<bool>("Trunk::Locked"); set { Vehicle.SetSharedData("Trunk::Locked", value); } }

        /// <summary>Заблокирован ли капот?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool HoodLocked { get => Vehicle.GetSharedData<bool>("Hood::Locked"); set { Vehicle.SetSharedData("Hood::Locked", value); } }

        public bool IsInvincible { get => Vehicle.GetSharedData<bool?>("IsInvincible") ?? false; set { if (value) Vehicle.SetSharedData("IsInvincible", value); else Vehicle.ResetSharedData("IsInvincible"); } }

        public byte DirtLevel { get => (byte)Vehicle.GetSharedData<int>("DirtLevel"); set { Vehicle.SetSharedData("DirtLevel", value); } }

        public bool IsAnchored { get => Vehicle.GetSharedData<bool?>("Anchor") ?? false; set { if (value) Vehicle.SetSharedData("Anchor", value); else Vehicle.ResetSharedData("Anchor"); } }

        public bool IsPlaneChassisOff { get => Vehicle.GetSharedData<bool?>("IPCO") ?? false; set { if (value) Vehicle.SetSharedData("IPCO", value); else Vehicle.ResetSharedData("IPCO"); } }

        public bool IsFrozen { get => Vehicle.GetSharedData<string>("IsFrozen") != null; set { if (value) SetFreezePosition(Vehicle.Position, Vehicle.Heading); else Vehicle.ResetSharedData("IsFrozen"); } }

        public Utils.Vector4 FrozenPosition
        {
            get
            {
                if (Vehicle.GetSharedData<string>("IsFrozen") is string str)
                {
                    var posData = str.Split('_');

                    return new Utils.Vector4(float.Parse(posData[0]), float.Parse(posData[1]), float.Parse(posData[2]), posData.Length > 3 ? float.Parse(posData[3]) : float.MinValue);
                }

                return null;
            }
        }

        public bool IsDead { get => Vehicle.GetData<bool?>("IVD") ?? false; set { if (value) Vehicle.SetData("IVD", value); else Vehicle.ResetData("IVD"); } }

        /// <summary>Уникальный ID транспорта</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>VID транспорта, null - если отсутствует</value>
        public uint VID { get => Info.VID; set { Vehicle.SetSharedData("VID", value); Info.VID = value; } }

        /// <summary>Прикрепленные объекты к транспорту</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentObjectNet> AttachedObjects { get => Vehicle.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedObjectsKey).ToList<Sync.AttachSystem.AttachmentObjectNet>(); set { Vehicle.SetSharedData(Sync.AttachSystem.AttachedObjectsKey, value); } }

        /// <summary>Прикрепленные сущности к транспорту</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentEntityNet> AttachedEntities { get => Vehicle.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedEntitiesKey).ToList<Sync.AttachSystem.AttachmentEntityNet>(); set { Vehicle.SetSharedData(Sync.AttachSystem.AttachedEntitiesKey, value); } }

        public Entity IsAttachedTo => Vehicle.GetEntityIsAttachedTo();
    }
}
