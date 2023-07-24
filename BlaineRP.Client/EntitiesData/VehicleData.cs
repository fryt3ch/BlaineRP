﻿using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using Newtonsoft.Json.Linq;
using RAGE.Elements;

namespace BlaineRP.Client.EntitiesData
{
    public class VehicleData
    {
        public VehicleData(Vehicle vehicle)
        {
            Vehicle = vehicle;

            Data = Client.Data.Vehicles.GetByModel(vehicle.Model);
        }

        public Vehicle Vehicle { get; set; }

        public bool IsInvincible => Vehicle.GetSharedData<bool>("IsInvincible", false);

        public bool IsFrozen => FrozenPosition != null;

        public bool EngineOn => Vehicle.GetSharedData<bool>("Engine::On", false);

        public bool DoorsLocked => Vehicle.GetSharedData<bool>("Doors::Locked", false);

        public bool TrunkLocked => Vehicle.GetSharedData<bool>("Trunk::Locked", false);

        public bool HoodLocked => Vehicle.GetSharedData<bool>("Hood::Locked", false);

        public bool LightsOn => Vehicle.GetSharedData<bool>("Lights::On", false);

        public byte IndicatorsState => (byte)Vehicle.GetSharedData<int>("Inds", 0);

        public Sync.Radio.StationTypes Radio => (Sync.Radio.StationTypes)Vehicle.GetSharedData<int>("Radio", 0);

        public float ForcedSpeed => Vehicle.GetSharedData<float>("ForcedSpeed", 0f);

        public float FuelLevel
        {
            get => Vehicle.GetData<float?>("Fuel") ?? 0f;
            set => Vehicle.SetData("Fuel", value);
        }

        public float Mileage
        {
            get => Vehicle.GetData<float?>("Mileage") ?? 0f;
            set => Vehicle.SetData("Mileage", value);
        }

        public uint VID => Utils.Convert.ToUInt32(Vehicle.GetSharedData<object>("VID", 0));

        public uint TID => Utils.Convert.ToUInt32(Vehicle.GetSharedData<object>("TID", 0));

        public bool HasNeonMod => Vehicle.GetSharedData<bool>("Mods::Neon", false);

        public bool HasTurboTuning => Vehicle.GetSharedData<bool>("Mods::Turbo", false);

        public bool IsAnchored => Vehicle.GetSharedData<bool>("Anchor", false);

        public bool IsPlaneChassisOff => Vehicle.GetSharedData<bool>("IPCO", false);

        public Utils.Colour TyreSmokeColour => Vehicle.GetSharedData<JObject>("Mods::TSColour")?.ToObject<Utils.Colour>();

        public byte DirtLevel => (byte)Vehicle.GetSharedData<int>("DirtLevel", 0);

        public string FrozenPosition => Vehicle.GetSharedData<string>("IsFrozen");

        public float ColshapeLimitedMaxSpeed
        {
            get => Vehicle.GetData<float>("CLMS");
            set
            {
                if (value <= 0f) Vehicle.ResetData("CLMS");
                else Vehicle.SetData("CLMS", value);
            }
        }

        public Data.Vehicles.Vehicle Data { get; set; }

        public Sync.AttachSystem.AttachmentEntity IsAttachedToVehicle
        {
            get
            {
                var streamed = Entities.Vehicles.Streamed;

                for (var i = 0; i < streamed.Count; i++)
                {
                    var t = streamed[i]
                           .GetData<List<Sync.AttachSystem.AttachmentEntity>>(Sync.AttachSystem.AttachedEntitiesKey)
                          ?.Where(x => x.RemoteID == Vehicle.RemoteId)
                           .FirstOrDefault();

                    if (t != null)
                        return t;
                }

                return null;
            }
        }

        public Vehicle IsAttachedToLocalTrailer => Vehicle.GetData<List<Sync.AttachSystem.AttachmentObject>>(Sync.AttachSystem.AttachedObjectsKey)
                                                         ?.Where(x => x.Type == Sync.AttachSystem.Types.TrailerObjOnBoat)
                                                          .FirstOrDefault()
                                                         ?.Object as Vehicle;

        public void Reset()
        {
            if (Vehicle == null)
                return;

            Sync.Vehicles.ControlledVehicles.Remove(Vehicle);

            Vehicle.ResetData();
        }

        public static VehicleData GetData(Vehicle vehicle)
        {
            if (vehicle == null)
                return null;

            return vehicle.GetData<VehicleData>("SyncedData");
        }

        public static void SetData(Vehicle vehicle, VehicleData data)
        {
            if (vehicle == null)
                return;

            vehicle.SetData("SyncedData", data);
        }
    }
}