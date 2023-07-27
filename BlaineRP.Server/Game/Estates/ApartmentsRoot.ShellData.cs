using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Estates
{
    public partial class ApartmentsRoot
    {
        public class ShellData
        {
            public Vector4 EnterPosition { get; set; }

            public Vector4[][] ElevatorPositions { get; set; }

            public Vector4[][] ApartmentsPositions { get; set; }

            public ushort FloorsAmount { get; set; }
            public ushort StartFloor { get; set; }

            public ShellData()
            {

            }

            public Vector4 GetFloorPosition(ushort floorIdx, ushort subIdx = 0)
            {
                if (floorIdx >= ElevatorPositions.Length)
                    return null;

                if (subIdx >= ElevatorPositions[floorIdx].Length)
                    return null;

                var d = ElevatorPositions[floorIdx][subIdx];

                return d;
            }

            public Vector4 GetApartmentsPositionByIdx(int idx)
            {
                if (idx < 0)
                    return null;

                for (int i = 0; i < ApartmentsPositions.Length; i++)
                {
                    if (idx < ApartmentsPositions[i].Length)
                        return ApartmentsPositions[i][idx];

                    idx -= ApartmentsPositions[i].Length;
                }

                return null;
            }

            public Vector4 GetApartmentsPosition(ushort floorIdx, ushort subIdx)
            {
                if (floorIdx >= ApartmentsPositions.Length)
                    return null;

                if (subIdx >= ApartmentsPositions[floorIdx].Length)
                    return null;

                var d = ApartmentsPositions[floorIdx][subIdx];

                return d;
            }

            public int GetApartmentsIdx(ushort floorIdx, ushort subIdx)
            {
                if (floorIdx >= ApartmentsPositions.Length)
                    return -1;

                if (subIdx >= ApartmentsPositions[floorIdx].Length)
                    return -1;

                var totalCount = (int)subIdx;

                for (ushort i = 0; i < floorIdx; i++)
                {
                    totalCount += ApartmentsPositions[i].Length;
                }

                return totalCount;
            }
        }
    }
}