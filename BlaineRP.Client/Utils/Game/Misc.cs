using System.Collections.Generic;
using System.Linq;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Utils.Game
{
    internal static class Misc
    {
        public static bool AnyOnFootMovingControlJustPressed()
        {
            return RAGE.Game.Pad.IsControlJustPressed(0, 32) ||
                   RAGE.Game.Pad.IsControlJustPressed(0, 33) ||
                   RAGE.Game.Pad.IsControlJustPressed(0, 34) ||
                   RAGE.Game.Pad.IsControlJustPressed(0, 35) ||
                   RAGE.Game.Pad.IsControlJustPressed(0, 22) ||
                   RAGE.Game.Pad.IsControlJustPressed(0, 44) ||
                   RAGE.Game.Pad.IsControlJustPressed(0, 75);
        }

        public static void ClearTvChannelPlaylist(int tvChannel)
        {
            RAGE.Game.Invoker.Invoke(0xBEB3D46BB7F043C0, tvChannel);
        }

        public static int CreateNamedRenderTargetForModel(string name, uint modelHash)
        {
            if (name == null)
                return 0;

            if (RAGE.Game.Ui.IsNamedRendertargetRegistered(name))
            {
                int handle = RAGE.Game.Ui.GetNamedRendertargetRenderId(name);

                RAGE.Game.Ui.ReleaseNamedRendertarget(ref handle);
            }

            RAGE.Game.Ui.RegisterNamedRendertarget(name, false);

            if (!RAGE.Game.Ui.IsNamedRendertargetLinked(modelHash))
                RAGE.Game.Ui.LinkNamedRendertarget(modelHash);

            if (RAGE.Game.Ui.IsNamedRendertargetRegistered(name))
                return RAGE.Game.Ui.GetNamedRendertargetRenderId(name);

            return 0;
        }

        public static Vector3 GetBonePositionOfEntity(GameEntity entity, object boneId)
        {
            if (entity is Player player)
                return player.GetBoneCoords((int)boneId, 0f, 0f, 0f);
            else if (entity is Ped ped)
                return ped.GetBoneCoords((int)boneId, 0f, 0f, 0f);
            else if (entity is Vehicle vehicle)
                return vehicle.GetWorldPositionOfBone(vehicle.GetBoneIndexByName((string)boneId));

            return null;
        }

        public static GameEntity GetGameEntityAtRemoteId(Type type, ushort remoteId)
        {
            switch (type)
            {
                case Type.Ped:
                    return Entities.Peds.GetAtRemote(remoteId);
                case Type.Player:
                    return Entities.Players.GetAtRemote(remoteId);
                case Type.Vehicle:
                    return Entities.Vehicles.GetAtRemote(remoteId);
                case Type.Object:
                    return Entities.Objects.GetAtRemote(remoteId);
            }

            return null;
        }

        public static float GetGroundZCoord(Vector3 pos, bool ignoreWater = false)
        {
            float z = pos.Z;

            return RAGE.Game.Misc.GetGroundZFor3dCoord(pos.X, pos.Y, pos.Z, ref z, ignoreWater) ? z : pos.Z;
        }

        public static MapObject GetMapObjectByHandle(int handle, bool streamedOnly)
        {
            return Entities.Objects.All.Where(x => x.Handle == handle).FirstOrDefault();

            // doesn't work anymore, ragemp bug?
            return (streamedOnly ? Entities.Objects.Streamed : Entities.Objects.All).Where(x => x.Handle == handle).FirstOrDefault();
        }

        public static Ped GetPedByHandle(int handle, bool streamedOnly)
        {
            return (streamedOnly ? Entities.Peds.Streamed : Entities.Peds.All).Where(x => x.Handle == handle).FirstOrDefault();
        }

        public static List<Ped> GetPedsOnScreen(int maxCount = 5)
        {
            return Entities.Peds.Streamed.Where(x => x.IsOnScreen()).OrderBy(x => Vector3.Distance(x.Position, Player.LocalPlayer.Position)).Take(maxCount).ToList();
        }

        public static Player GetPlayerByHandle(int handle, bool streamedOnly)
        {
            return (streamedOnly ? Entities.Players.Streamed : Entities.Players.All).Where(x => x.Handle == handle).FirstOrDefault();
        }

        public static string GetStreetName(Vector3 pos)
        {
            if (pos == null)
                return "null";

            int streetNameHash = 0, crossingRoadNameHash = 0;

            RAGE.Game.Pathfind.GetStreetNameAtCoord(pos.X, pos.Y, pos.Z, ref streetNameHash, ref crossingRoadNameHash);

            return RAGE.Game.Ui.GetStreetNameFromHashKey((uint)streetNameHash) ?? "null";
        }

        public static Vehicle GetVehicleByHandle(int handle, bool streamedOnly)
        {
            return (streamedOnly ? Entities.Vehicles.Streamed : Entities.Vehicles.All).Where(x => x.Handle == handle).FirstOrDefault();
        }

        public static List<Vehicle> GetVehiclesOnScreen(int maxCount = 5)
        {
            return Entities.Vehicles.Streamed.Where(x => x.IsOnScreen()).OrderBy(x => Vector3.Distance(x.Position, Player.LocalPlayer.Position)).Take(maxCount).ToList();
        }

        public static int GetWarningMessageTitleHash()
        {
            return RAGE.Game.Invoker.Invoke<int>(0x81DF9ABA6C83DFF9);
        }

        /// <summary>Получить handle блипа метки на карте</summary>
        /// <returns>0, если не найдена</returns>
        public static int GetWaypointBlip()
        {
            if (!RAGE.Game.Ui.IsWaypointActive())
                return 0;

            int blipIterator = RAGE.Game.Invoker.Invoke<int>(0x186E5D252FA50E7D); // GetWaypointBlipEnumId

            int firstBlip = RAGE.Game.Ui.GetFirstBlipInfoId(blipIterator);
            int nextBlip = RAGE.Game.Ui.GetNextBlipInfoId(blipIterator);

            for (int i = firstBlip; RAGE.Game.Ui.DoesBlipExist(i); i = nextBlip)
            {
                if (RAGE.Game.Ui.GetBlipInfoIdType(i) == 4)
                    return i;
            }

            return 0;
        }

        /// <summary>Получить координаты текущей метки на карте</summary>
        /// <returns>null, если метка не стоит</returns>
        public static Vector3 GetWaypointPosition()
        {
            int blip = GetWaypointBlip();

            return blip == 0 ? null : RAGE.Game.Ui.GetBlipInfoIdCoord(blip);
        }

        public static bool IsCoordInCountrysideV(float x, float y, float z)
        {
            return RAGE.Game.Zone.GetHashOfMapAreaAtCoords(x, y, z) == 2072609373;
        }

        public static bool IsInteriorEntitySetActive(int intId, string entitySetName)
        {
            return RAGE.Game.Invoker.Invoke<bool>(0x35F7DD45E8C0A16D, intId, entitySetName);
        }

        public static void SetInteriorEntitySetColour(int intId, string entitySetName, int colour)
        {
            RAGE.Game.Invoker.Invoke(0xC1F1920BAF281317, intId, entitySetName, colour);
        }

        public static void SetTvChannelPlaylist(int tvChannel, string playlistName, bool restart)
        {
            RAGE.Game.Invoker.Invoke(0xF7B38B8305F1FE8B, tvChannel, playlistName, restart);
        }

        public static void SetWaypoint(float x, float y)
        {
            RAGE.Game.Ui.SetWaypointOff();

            RAGE.Game.Ui.SetNewWaypoint(x, y);
        }

        public static void ToggleInteriorEntitySet(int intId, string entitySetName, bool state)
        {
            RAGE.Game.Invoker.Invoke(state ? 0x55E86AF2712B36A1 : (ulong)0x420BD37289EEE162, intId, entitySetName);
        }
    }
}