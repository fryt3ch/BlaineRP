using GTANetworkAPI;
using System;
using System.Collections.Generic;

namespace BCRPServer.Game.Businesses
{
    public class WeaponShop : Shop
    {
        public static Types DefaultType => Types.WeaponShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "w_asrifle", 100 },
                { "w_asrifle_mk2", 100 },
                { "w_advrifle", 100 },
                { "w_carbrifle", 100 },
                { "w_comprifle", 100 },
                { "w_heavyrifle", 100 },

                { "w_microsmg", 100 },
                { "w_minismg", 100 },
                { "w_smg", 100 },
                { "w_smg_mk2", 100 },
                { "w_asmsg", 100 },
                { "w_combpdw", 100 },

                { "w_combmg", 100 },
                { "w_gusenberg", 100 },

                { "w_heavysnp", 100 },
                { "w_markrifle", 100 },
                { "w_musket", 100 },

                { "w_assgun", 100 },
                { "w_heavysgun", 100 },
                { "w_pumpsgun", 100 },
                { "w_pumpsgun_mk2", 100 },
                { "w_sawnsgun", 100 },

                { "w_pistol", 100 },
                { "w_pistol_mk2", 100 },
                { "w_appistol", 100 },
                { "w_combpistol", 100 },
                { "w_heavypistol", 100 },
                { "w_machpistol", 100 },
                { "w_markpistol", 100 },
                { "w_vintpistol", 100 },

                { "w_revolver", 100 },
                { "w_revolver_mk2", 100 },

                { "w_bat", 100 },
                { "w_bottle", 100 },
                { "w_crowbar", 100 },
                { "w_dagger", 100 },
                { "w_flashlight", 100 },
                { "w_golfclub", 100 },
                { "w_hammer", 100 },
                { "w_hatchet", 100 },
                { "w_knuckles", 100 },
                { "w_machete", 100 },
                { "w_nightstick", 100 },
                { "w_poolcue", 100 },
                { "w_switchblade", 100 },
                { "w_wrench", 100 },

                { "am_5.56", 1 },
                { "am_7.62", 1 },
                { "am_9", 1 },
                { "am_11.43", 1 },
                { "am_12", 1 },
                { "am_12.7", 1 },

                { "wc_s", 10 },
                { "wc_f", 10 },
                { "wc_g", 10 },
                { "wc_sc", 10 },
            }
        };

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}, {PositionShootingRangeEnter.Position.ToCSharpStr()}";

        public Utils.Vector4 PositionShootingRangeEnter { get; set; }

        public static uint ShootingRangePrice { get => (uint)Sync.World.GetSharedData<int>("SRange::Price"); set => Sync.World.SetSharedData("SRange::Price", value); }

        public static Utils.Vector4 ShootingRangePosition { get; private set; } = new Utils.Vector4(13.00517f, -1098.977f, 29.79701f, 337.5131f);

        public WeaponShop(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract, Utils.Vector4 PositionShootingRangeEnter) : base(ID, PositionInfo, PositionInteract, DefaultType)
        {
            PositionShootingRangeEnter.Position.Z -= 0.5f;

            this.PositionShootingRangeEnter = PositionShootingRangeEnter;
        }

        public bool IsPlayerNearShootingRangeEnterPosition(PlayerData pData)
        {
            return Vector3.Distance(pData.Player.Position, PositionShootingRangeEnter.Position) <= 10f;
        }

        public bool TryBuyShootingRange(PlayerData pData)
        {
            var price = (ulong)ShootingRangePrice;

            ulong newCash;

            if (!pData.TryRemoveCash(price, out newCash, true))
                return false;

            try
            {
                var bizPrice = GetBusinessPriceFixed(price, true);

                ulong newBalance;

                if (!TryAddMoneyCash(bizPrice, out newBalance, false))
                    return false;

                ProceedPayment(pData, true, Materials, newBalance, newCash);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
