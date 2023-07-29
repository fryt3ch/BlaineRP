using System;
using System.Threading;
using BlaineRP.Server.Game.EntitiesData.Vehicles.Static;

namespace BlaineRP.Server.Game.EntitiesData.Vehicles
{
    public partial class VehicleData
    {
        public OwnerTypes OwnerType
        {
            get => Info.OwnerType;
            set => Info.OwnerType = value;
        }

        /// <summary>Второстепенные данные транспорта</summary>
        public Vehicle Data => Info.Data;

        /// <summary>Номерной знак транспорта</summary>
        /// <value>Объект класса Game.Items.Numberplate, null - если отсутствует</value>
        public Items.Numberplate Numberplate
        {
            get => Info.Numberplate;
            set => Info.Numberplate = value;
        }

        /// <summary>Тюнинг транспорта</summary>
        public Tuning Tuning
        {
            get => Info.Tuning;
            set => Info.Tuning = value;
        }

        /// <summary>Токен отмены удаления транспорта с сервера</summary>
        /// <value>Объект класса Timer, null - если отсутствует</value>
        public Timer DeletionTimer
        {
            get => Vehicle.GetData<Timer>("DTIMER");
            set
            {
                if (value == null)
                    Vehicle.ResetData("DTIMER");
                else
                    Vehicle.SetData("DTIMER", value);
            }
        }

        /// <summary>CID владельца транспорта</summary>
        public uint OwnerID
        {
            get => Info.OwnerID;
            set => Info.OwnerID = value;
        }

        public VehicleInfo Info { get; set; }

        public LastVehicleData LastData
        {
            get => Info.LastData;
            set => Info.LastData = value;
        }

        public Jobs.Job Job
        {
            get => Jobs.Job.Get(Vehicle.GetData<int>("JID"));
            set
            {
                if (value == null)
                    Vehicle.ResetData("JID");
                else
                    Vehicle.SetData("JID", value.Id);
            }
        }

        public string GetName(byte type = 0)
        {
            if (type == 0)
                return Data.Name;
            else if (type == 1)
                return $"{Data.Name} [{(Vehicle.NumberPlate == null || Vehicle.NumberPlate.Length == 0 ? Language.Strings.Get("CHAT_VEHICLE_NP_NONE") : Vehicle.NumberPlate)}]";

            return string.Empty;
        }
    }
}