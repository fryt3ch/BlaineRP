using RAGE;

namespace BlaineRP.Client.Game.Estates
{
    public partial class ApartmentsRoot
    {
        public class ShellData
        {
            public ShellData()
            {
            }

            public Vector3 EnterPosition { get; set; }

            public Vector3[][] ElevatorPositions { get; set; }

            public Vector3[][] ApartmentsPositions { get; set; }

            public ushort FloorsAmount { get; set; }
            public ushort StartFloor { get; set; }

            public Vector3 GetFloorPosition(ushort floorIdx, ushort subIdx = 0)
            {
                if (floorIdx >= ElevatorPositions.Length)
                    return null;

                if (subIdx >= ElevatorPositions[floorIdx].Length)
                    return null;

                Vector3 d = ElevatorPositions[floorIdx][subIdx];

                return d;
            }

            public Vector3 GetApartmentsPositionByIdx(int idx)
            {
                if (idx < 0)
                    return null;

                for (var i = 0; i < ApartmentsPositions.Length; i++)
                {
                    if (idx < ApartmentsPositions[i].Length)
                        return ApartmentsPositions[i][idx];

                    idx -= ApartmentsPositions[i].Length;
                }

                return null;
            }

            public Vector3 GetApartmentsPosition(ushort floorIdx, ushort subIdx)
            {
                if (floorIdx >= ApartmentsPositions.Length)
                    return null;

                if (subIdx >= ApartmentsPositions[floorIdx].Length)
                    return null;

                Vector3 d = ApartmentsPositions[floorIdx][subIdx];

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

            public bool GetClosestElevator(Vector3 pos, out int floorIdx, out int subIdx)
            {
                float minDist = float.MaxValue;

                floorIdx = -1;
                subIdx = -1;

                for (var i = 0; i < ElevatorPositions.Length; i++)
                {
                    for (var j = 0; j < ElevatorPositions[i].Length; j++)
                    {
                        float dist = pos.DistanceTo(ElevatorPositions[i][j]);

                        if (dist < minDist)
                        {
                            minDist = dist;

                            floorIdx = i;
                            subIdx = j;
                        }
                    }
                }

                return floorIdx >= 0 && subIdx >= 0;
            }
        }
    }
}