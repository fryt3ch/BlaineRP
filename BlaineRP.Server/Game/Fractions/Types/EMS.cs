using BlaineRP.Server.Game.Data;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Game.Management.Attachments;
using BlaineRP.Server.Sync;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Fractions
{
    public partial class EMS : Fraction, IUniformable
    {
        public EMS(FractionType Type, string Name) : base(Type, Name)
        {

        }

        public override string ClientData => $"FractionTypes.{Type}, \"{Name}\", {ContainerId}, \"{ContainerPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPositions.SerializeToJson().Replace('\"', '\'')}\", {Ranks.Count - 1}, \"{LockerRoomPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPrices.SerializeToJson().Replace('"', '\'')}\", {(uint)MetaFlags}, \"{Beds.Select(x => x.Position).ToList().SerializeToJson().Replace('"', '\'')}\"";

        private static TimeSpan HealingBedTimeout = TimeSpan.FromSeconds(20);
        private const byte HealingBedHealthIncrease = 10;

        public const uint PlayerPsychHealPrice = 200;
        public const byte PlayerPsychHealSetAmount = 90;
        public const decimal PlayerPsychHealPricePlayerGetCoef = 0.90m;

        public static TimeSpan PlayerDrugHealCooldownA { get; } = TimeSpan.FromHours(1);

        public const uint PlayerDrugHealPrice = 2_000;
        public const int PlayerDrugHealAmount = 20;
        public const decimal PlayerDrugHealPriceFractionGetCoef = 0.00m;
        public const decimal PlayerDrugHealPricePlayerGetCoef = 0.90m;

        public const uint PlayerHealMaxPrice = 200;
        public const uint PlayerHealMinPrice = 80;
        public const decimal PlayerHealPriceFractionGetCoef = 0.00m;
        public const decimal PlayerHealPricePlayerGetCoef = 0.90m;

        public const uint PlayerSellMaskPrice = 2_000;
        public const decimal PlayerSellMaskPriceFractionGetCoef = 0.00m;
        public const decimal PlayerSellMaskPricePlayerGetCoef = 0.30m;

        private static Dictionary<ushort, CallInfo> AllCalls { get; set; } = new Dictionary<ushort, CallInfo>();

        public List<Customization.UniformTypes> UniformTypes { get; set; }

        public Vector3[] LockerRoomPositions { get; set; }

        private BedInfo[] Beds { get; set; }

        public Vector4[] AfterDeathSpawnPositions { get; set; }

        public override void PostInitialize()
        {
            base.PostInitialize();

            Vector3[] bedsPositions = null;

            if (Type == FractionType.EMS_BLAINE)
            {
                bedsPositions = new Vector3[]
                {
                    new Vector3(-257.7963f, 6317.284f, 31.97997f),
                    new Vector3(-256.0888f, 6315.577f, 31.97997f),
                    new Vector3(-254.3901f, 6313.879f, 31.97997f),
                    new Vector3(-252.6319f, 6312.12f, 31.97997f),
                    new Vector3(-251.0258f, 6310.514f, 31.97997f),
                    new Vector3(-245.2629f, 6316.227f, 31.97997f),
                    new Vector3(-246.9854f, 6317.949f, 31.97997f),
                    new Vector3(-250.6555f, 6321.62f, 31.97997f),
                    new Vector3(-252.1485f, 6323.112f, 31.97997f),

                    new Vector3(1823.362f, 3680.794f, 33.83718f),
                    new Vector3(1821.666f, 3679.814f, 33.83718f),
                    new Vector3(1819.968f, 3678.835f, 33.83718f),
                    new Vector3(1818.271f, 3677.854f, 33.83718f),
                    new Vector3(1817.131f, 3674.695f, 33.83718f),
                    new Vector3(1818.111f, 3672.997f, 33.83718f),
                    new Vector3(1819.092f, 3671.299f, 33.83718f),
                    new Vector3(1820.073f, 3669.601f, 33.83718f),
                    new Vector3(1822.24f, 3674.044f, 33.83718f),
                    new Vector3(1823.291f, 3672.224f, 33.83718f),
                };
            }
            else if (Type == FractionType.EMS_LS)
            {
                bedsPositions = new Vector3[]
                {
                    new Vector3(322.6169f, -587.1685f, 42.84177f),
                    new Vector3(317.6706f, -585.3683f, 42.84177f),
                    new Vector3(324.2628f, -582.8009f, 42.84177f),
                    new Vector3(319.412f, -581.0392f, 42.84177f),
                    new Vector3(314.4655f, -584.2017f, 42.84177f),
                    new Vector3(311.0575f, -582.9613f, 42.84177f),
                    new Vector3(313.9297f, -579.0439f, 42.84177f),
                    new Vector3(307.7171f, -581.7455f, 42.84177f),
                    new Vector3(309.3536f, -577.3783f, 42.84177f),
                };
            }

            SetBedPositions(bedsPositions);
        }

        public void SetBedPositions(params Vector3[] BedPositions)
        {
            Beds = new BedInfo[BedPositions.Length];

            for (int i = 0; i < BedPositions.Length; i++)
            {
                Beds[i] = new BedInfo() { Position = BedPositions[i], RID = ushort.MaxValue };
            }
        }

        public bool IsPlayerInAnyUniform(PlayerData pData, bool notifyIfNot = false)
        {
            var res = UniformTypes.Contains(pData.CurrentUniform);

            if (res)
                return true;

            if (notifyIfNot)
            {
                pData.Player.Notify("Fraction::NIUF");
            }

            return false;
        }

        protected override void FractionDataTriggerEvent(PlayerData pData)
        {
            var calls = AllCalls.Select(x => $"{x.Key}_{x.Value.Type}_{x.Value.Position.X}_{x.Value.Position.Y}_{x.Value.Position.Z}").ToList();

            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}").ToList(), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{GetMemberStatus(x)}&{x.LastJoinDate.GetUnixTimestamp()}").ToList(), calls);
        }

        public static void GetClosestAfterDeathFractionAndPosIdx(Vector3 pos, out EMS fraction, out int posIdx)
        {
            fraction = null;
            posIdx = 0;

            var minDist = float.MaxValue;

            foreach (var x in Fraction.All.Values)
            {
                if (x is EMS ems)
                {
                    for (int i = 0; i < ems.AfterDeathSpawnPositions.Length; i++)
                    {
                        var dist = pos.DistanceTo(ems.AfterDeathSpawnPositions[i].Position);

                        if (pos.DistanceTo(ems.AfterDeathSpawnPositions[i].Position) <= minDist)
                        {
                            fraction = ems;

                            minDist = dist;
                            posIdx = i;
                        }
                    }
                }
            }
        }

        public static void SetPlayerToEmsAfterDeath(PlayerData pData, Vector3 curPos)
        {
            if (pData.IsKnocked)
            {
                pData.SetAsNotKnocked();
            }

            Game.Fractions.EMS emsFraction;
            int posIdx;

            Game.Fractions.EMS.GetClosestAfterDeathFractionAndPosIdx(curPos, out emsFraction, out posIdx);

            var pos = emsFraction.AfterDeathSpawnPositions[posIdx];

            pData.Player.Teleport(pos.Position, false, Properties.Settings.Static.MainDimension, pos.RotationZ, false);

            NAPI.Player.SpawnPlayer(pData.Player, pos.Position, pos.RotationZ);

            pData.Player.SetHealth(10);
        }

        public static CallInfo GetCallByRID(ushort rid) => AllCalls.GetValueOrDefault(rid);

        public static void AddCall(ushort rid, CallInfo call)
        {
            AllCalls.Add(rid, call);


        }

        public static void RemoveCall(ushort rid, CallInfo call)
        {
            if (AllCalls.Remove(rid))
            {

            }
        }

        public static bool TryGetCurrentPlayerBed(PlayerData pData, out EMS fData, out int idx)
        {
            foreach (var x in All.Values)
            {
                if (x is EMS ems)
                {
                    for (int i = 0; i < ems.Beds.Length; i++)
                    {
                        if (ems.Beds[i].RID == pData.Player.Id && ems.Beds[i].Timer != null)
                        {
                            fData = ems;
                            idx = i;

                            return true;
                        }
                    }
                }
            }

            fData = null;
            idx = -1;

            return false;
        }

        public void SetBedAsFree(int idx)
        {
            var bed = GetBedInfoById(idx);

            if (bed == null)
                return;

            bed.RID = ushort.MaxValue;

            if (bed.Timer != null)
            {
                bed.Timer.Dispose();

                bed.Timer = null;
            }

            World.Service.ResetSharedData($"EMS::{(int)Type}::BED::{idx}");
        }

        public void SetBedAsOccupied(int idx, PlayerData pData)
        {
            var bed = GetBedInfoById(idx);

            if (bed == null)
                return;

            bed.RID = pData.Player.Id;

            if (bed.Timer != null)
                bed.Timer.Dispose();

            World.Service.SetSharedData($"EMS::{(int)Type}::BED::{idx}", true);

            bed.Timer = new Timer((_) =>
            {
                NAPI.Task.Run(() =>
                {
                    if (pData.Player?.Exists != true)
                        return;

                    if (pData.Player.Position.DistanceTo(bed.Position) > 5f)
                    {
                        pData.Player.DetachObject(AttachmentType.EmsHealingBedFakeAttach);

                        return;
                    }

                    var curHealth = pData.Player.Health;

                    var diff = Utils.CalculateDifference(pData.Player.Health, HealingBedHealthIncrease, 0, Properties.Settings.Static.PlayerMaxHealth);

                    var stopHealing = false;

                    if (diff > 0)
                    {
                        var newHealth = curHealth + diff;

                        pData.Player.SetHealth(newHealth);

                        if (newHealth >= Properties.Settings.Static.PlayerMaxHealth)
                            stopHealing = true;
                    }
                    else
                    {
                        stopHealing = true;
                    }

                    if (stopHealing)
                    {
                        if (pData.Player.DetachObject(AttachmentType.EmsHealingBedFakeAttach))
                        {
                            pData.Player.Notify("EMS::HBEDS");
                        }

                        return;
                    }
                });
            }, null, HealingBedTimeout, HealingBedTimeout);
        }

        public BedInfo GetBedInfoById(int idx) => idx < 0 || idx >= Beds.Length ? null : Beds[idx];
    }
}