using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BCRPServer
{
    public partial class VehicleData
    {
        public OwnerTypes OwnerType { get => Info.OwnerType; set => Info.OwnerType = value; }

        /// <summary>Второстепенный ID транспорта</summary>
        /// <value>Не уникальный ID транспорта, а его идентификатор (см. Game.Data.Vehicles)</value>
        public string ID { get => Info.ID; set => Info.ID = value; }

        /// <summary>Второстепенные данные транспорта</summary>
        public Game.Data.Vehicles.Vehicle Data { get => Info.Data; set => Info.Data = value; }

        /// <summary>Дата создания транспорта</summary>
        public DateTime RegistrationDate { get => Info.RegistrationDate; set => Info.RegistrationDate = value; }

        /// <summary>Действительные ключи от транспорта</summary>
        /// <value>Список UID предметов Game.Items.VehicleKey</value>
        public List<uint> Keys { get => Info.AllKeys; set => Info.AllKeys = value; }

        /// <summary>Номерной знак транспорта</summary>
        /// <value>Объект класса Game.Items.Numberplate, null - если отсутствует</value>
        public Game.Items.Numberplate Numberplate { get => Info.Numberplate; set => Info.Numberplate = value; }

        /// <summary>Тюнинг транспорта</summary>
        public Game.Data.Vehicles.Tuning Tuning { get => Info.Tuning; set => Info.Tuning = value; }

        /// <summary>Токен отмены удаления транспорта с сервера</summary>
        /// <value>Объект класса CancellationTokenSource, null - если отсутствует</value>
        public CancellationTokenSource CTSDelete { get => Vehicle.GetData<CancellationTokenSource>("CTSD"); set { if (value == null) Vehicle.ResetData("CTSD"); else Vehicle.SetData("CTSD", value); } }

        /// <summary>CID владельца транспорта</summary>
        public uint OwnerID { get => Info.OwnerID; set => Info.OwnerID = value; }

        public VehicleInfo Info { get; set; }

        public LastVehicleData LastData { get => Info.LastData; set => Info.LastData = value; }

        public Game.Jobs.Job Job => Game.Jobs.Job.Get(Vehicle.GetData<int>("JID"));
    }
}
