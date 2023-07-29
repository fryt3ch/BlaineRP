using System;
using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.DoorSystem
{
    public class Door
    {
        public Func<PlayerData, bool> CheckFunction { get; set; }

        public uint Model { get; set; }

        public Vector3 Position { get; set; }

        public uint Dimension { get; set; }

        public Door(uint Id, string Model, float PosX, float PosY, float PosZ, uint Dimension) : this(Id, NAPI.Util.GetHashKey(Model), PosX, PosY, PosZ, Dimension)
        {

        }

        public Door(uint Id, uint Model, float PosX, float PosY, float PosZ, uint Dimension)
        {
            this.Model = Model;

            this.Position = new Vector3(PosX, PosY, PosZ);

            this.Dimension = Dimension;

            Service.AllDoors.Add(Id, this);
        }

        public bool GetCheckFunctionResult(PlayerData pData)
        {
            if (CheckFunction == null)
                return true;

            return CheckFunction.Invoke(pData);
        }

        public static bool GetLockState(uint id) => World.Service.GetSharedData<bool?>($"DOORS_{id}_L") == true;

        public static void SetLockState(uint id, bool state, bool updateDb)
        {
            if (state)
            {
                World.Service.SetSharedData($"DOORS_{id}_L", true);
            }
            else
            {
                World.Service.ResetSharedData($"DOORS_{id}_L");
            }
        }
    }
}