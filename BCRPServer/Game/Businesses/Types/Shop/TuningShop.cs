﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Businesses
{
    public class TuningShop : Shop, IEnterable
    {
        public static Types DefaultType => Types.TuningShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, int>()
            {
                { "fix_0", 100 },
                { "fix_1", 100 },

                { "keys_0", 100 },
                { "keys_1", 100 },

                { "engine_0", 100 },
                { "engine_1", 100 },
                { "engine_2", 100 },
                { "engine_3", 100 },
                { "engine_4", 100 },

                { "brakes_0", 100 },
                { "brakes_1", 100 },
                { "brakes_2", 100 },
                { "brakes_3", 100 },

                { "trm_0", 100 },
                { "trm_1", 100 },
                { "trm_2", 100 },
                { "trm_3", 100 },

                { "susp_0", 100 },
                { "susp_1", 100 },
                { "susp_2", 100 },
                { "susp_3", 100 },
                { "susp_4", 100 },

                { "xenon_0", 100 },
                { "xenon_1", 100 },
                { "xenon_2", 100 },
                { "xenon_3", 100 },
                { "xenon_4", 100 },
                { "xenon_5", 100 },
                { "xenon_6", 100 },
                { "xenon_7", 100 },
                { "xenon_8", 100 },
                { "xenon_9", 100 },
                { "xenon_10", 100 },
                { "xenon_11", 100 },
                { "xenon_12", 100 },
                { "xenon_13", 100 },
                { "xenon_14", 100 },

                { "wtint_0", 100 },
                { "wtint_1", 100 },
                { "wtint_2", 100 },
                { "wtint_3", 100 },

                { "tt_0", 100 },
                { "tt_1", 100 },

                { "neon_0", 100 },
                { "neon", 100 },

                { "tsmoke_0", 100 },
                { "tsmoke", 100 },

                { "horn", 100 },

                { "spoiler", 100 },

                { "fbump", 100 },

                { "rbump", 100 },

                { "skirt", 100 },

                { "exh", 100 },

                { "frame", 100 },

                { "grill", 100 },

                { "hood", 100 },

                { "roof", 100 },

                { "livery", 100 },

                { "swheel", 100 },

                { "seats", 100 },

                { "colourt_0", 100 },
                { "colourt_1", 100 },
                { "colourt_2", 100 },
                { "colourt_3", 100 },
                { "colourt_4", 100 },
                { "colourt_5", 100 },

                { "colour", 100 },

                { "pearl_0", 100 },
                { "pearl", 100 },

                { "wcolour_0", 100 },
                { "wcolour", 100 },

                { "wheel_0", 100 },
                { "wheel_1", 100 },
                { "wheel_2", 100 },
                { "wheel_3", 100 },
                { "wheel_4", 100 },
                { "wheel_5", 100 },
                { "wheel_6", 100 },
                { "wheel_7", 100 },
                { "wheel_8", 100 },
                { "wheel_9", 100 },
                { "wheel_10", 100 },
                { "wheel_11", 100 },
                { "wheel_12", 100 },
                { "wheel_13", 100 },

                // bike rear wheel
                { "rwheel_0", 100 },
                { "rwheel_7", 100 },
            }
        };

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}";

        public Utils.Vector4 EnterProperties { get; set; }

        public Utils.Vector4[] ExitProperties { get; set; }

        public int LastExitUsed { get; set; }

        private static Dictionary<Game.Data.Vehicles.Vehicle.ClassTypes, float> VehicleClassMargins = new Dictionary<Data.Vehicles.Vehicle.ClassTypes, float>()
        {
            { Data.Vehicles.Vehicle.ClassTypes.Classic, 1f },
            { Data.Vehicles.Vehicle.ClassTypes.Premium, 1.5f },
            { Data.Vehicles.Vehicle.ClassTypes.Luxe, 2f },
            { Data.Vehicles.Vehicle.ClassTypes.Elite, 2.5f },
        };

        private static Dictionary<string, byte> ModSlots = new Dictionary<string, byte>()
        {
            { "engine", 11 },
            { "brakes", 12 },
            { "trm", 13 },
            { "susp", 15 },
            { "horn", 14 },
            { "spoiler", 0 },
            { "fbump", 1 },
            { "rbump", 2 },
            { "skirt", 3 },
            { "exh", 4 },
            { "frame", 5 },
            { "grill", 6 },
            { "hood", 7 },
            { "roof", 10 },
            { "livery", 48 },
            { "swheel", 33 },
            { "seats", 32 },
        };

        public TuningShop(int ID, Vector3 PositionInfo, Utils.Vector4 EnterProperties, Utils.Vector4[] ExitProperties, Utils.Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, DefaultType)
        {
            this.EnterProperties = EnterProperties;

            this.ExitProperties = ExitProperties;
        }

        public float GetVehicleClassMargin(Data.Vehicles.Vehicle.ClassTypes cType) => VehicleClassMargins[cType];

        public byte? GetModSlot(string id)
        {
            byte mod;

            if (!ModSlots.TryGetValue(id, out mod))
                return null;

            return mod;
        }

        public override bool BuyItem(PlayerData pData, bool useCash, string item)
        {
            var vData = pData.CurrentTuningVehicle;

            if (vData?.Vehicle?.Exists != true)
                return false;

            var iData = item.Split('_');

            if (iData.Length <= 1)
                return false;

            byte p;

            if (!byte.TryParse(iData[1], out p))
                return false;

            var slot = GetModSlot(iData[0]);

            (int MatPrice, int RealPrice)? res = null;

            if (slot is byte bSlot)
            {
                if (iData[0] == "engine" || iData[0] == "brakes" || iData[0] == "trm" || iData[0] == "susp")
                {
                    res = CanBuy(pData, useCash, item, 1);
                }
                else
                {
                    res = CanBuy(pData, useCash, iData[0], 1);
                }

                if (res == null)
                    return false;

                if (p == 0)
                    p = 255;
                else
                    p--;

                vData.Tuning.Mods[bSlot] = p;

                MySQL.VehicleTuningUpdate(vData.Info);
            }
            else
            {
                if (iData[0] == "fix")
                {
                    res = CanBuy(pData, useCash, item, 1);

                    if (res == null)
                        return false;

                    if (p == 0)
                    {
                        vData.Vehicle.SetFixed();
                    }
                    else
                    {
                        vData.Vehicle.SetVisualFixed();
                    }
                }
                else if (iData[0] == "keys")
                {

                }
                else if (iData[0] == "wheel" || iData[0] == "rwheel")
                {
                    if (iData.Length != 3)
                        return false;

                    byte n;

                    if (!byte.TryParse(iData[2], out n))
                        return false;

                    res = CanBuy(pData, useCash, $"{iData[0]}_{iData[1]}", 1);

                    if (res == null)
                        return false;

                    if (vData.Data.Type == Game.Data.Vehicles.Vehicle.Types.Motorcycle)
                    {
                        vData.Tuning.WheelsType = 6;
                    }
                    else
                    {
                        if (p > 0)
                            p--;

                        vData.Tuning.WheelsType = p;
                    }

                    if (n == 0)
                        n = 255;

                    vData.Tuning.Mods[(byte)(iData[0] == "wheel" ? 23 : 24)] = n;

                    MySQL.VehicleTuningUpdate(vData.Info);
                }
                else if (iData[0] == "neon")
                {
                    if (iData.Length == 2)
                    {
                        res = CanBuy(pData, useCash, item, 1);

                        if (res == null)
                            return false;

                        vData.Tuning.NeonColour = null;
                    }
                    else if (iData.Length == 4)
                    {
                        byte g, b;

                        if (!byte.TryParse(iData[2], out g) || !byte.TryParse(iData[3], out b))
                            return false;

                        res = CanBuy(pData, useCash, iData[0], 1);

                        if (res == null)
                            return false;

                        if (vData.Tuning.NeonColour == null)
                        {
                            vData.Tuning.NeonColour = new Utils.Colour(p, g, b, 255);
                        }
                        else
                        {
                            vData.Tuning.NeonColour.Red = p;
                            vData.Tuning.NeonColour.Green = g;
                            vData.Tuning.NeonColour.Blue = b;
                        }
                    }
                    else
                        return false;

                    MySQL.VehicleTuningUpdate(vData.Info);
                }
                else if (iData[0] == "tsmoke")
                {
                    if (iData.Length == 2)
                    {
                        res = CanBuy(pData, useCash, item, 1);

                        if (res == null)
                            return false;

                        vData.Tuning.TyresSmokeColour = null;
                    }
                    else if (iData.Length == 4)
                    {
                        byte g, b;

                        if (!byte.TryParse(iData[2], out g) || !byte.TryParse(iData[3], out b))
                            return false;

                        res = CanBuy(pData, useCash, iData[0], 1);

                        if (res == null)
                            return false;

                        if (vData.Tuning.TyresSmokeColour == null)
                        {
                            vData.Tuning.TyresSmokeColour = new Utils.Colour(p, g, b, 255);
                        }
                        else
                        {
                            vData.Tuning.TyresSmokeColour.Red = p;
                            vData.Tuning.TyresSmokeColour.Green = g;
                            vData.Tuning.TyresSmokeColour.Blue = b;
                        }
                    }
                    else
                        return false;

                    MySQL.VehicleTuningUpdate(vData.Info);
                }
                else if (iData[0] == "colour")
                {
                    if (iData.Length != 7)
                        return false;

                    byte g1, b1, r2, g2, b2;

                    if (!byte.TryParse(iData[2], out g1) || !byte.TryParse(iData[3], out b1) || !byte.TryParse(iData[4], out r2) || !byte.TryParse(iData[5], out g2) || !byte.TryParse(iData[6], out b2))
                        return false;

                    res = CanBuy(pData, useCash, iData[0], 1);

                    if (res == null)
                        return false;

                    vData.Tuning.Colour1.Red = p;
                    vData.Tuning.Colour1.Green = g1;
                    vData.Tuning.Colour1.Blue = b1;

                    vData.Tuning.Colour2.Red = r2;
                    vData.Tuning.Colour2.Green = g2;
                    vData.Tuning.Colour2.Blue = b2;

                    MySQL.VehicleTuningUpdate(vData.Info);
                }
                else
                {
                    if (iData[0] == "pearl")
                    {
                        res = CanBuy(pData, useCash, p == 0 ? item : iData[0], 1);

                        if (res == null)
                            return false;

                        vData.Tuning.PearlescentColour = p;

                        MySQL.VehicleTuningUpdate(vData.Info);
                    }
                    else if (iData[0] == "wcolour")
                    {
                        res = CanBuy(pData, useCash, p == 0 ? item : iData[0], 1);

                        if (res == null)
                            return false;

                        vData.Tuning.WheelsColour = p;

                        MySQL.VehicleTuningUpdate(vData.Info);
                    }
                    else
                    {
                        res = CanBuy(pData, useCash, item, 1);

                        if (res == null)
                            return false;

                        if (iData[0] == "colourt")
                        {
                            vData.Tuning.ColourType = p;
                        }
                        else if (iData[0] == "tt")
                        {
                            vData.Tuning.Turbo = p == 1;
                        }
                        else if (iData[0] == "wtint")
                        {
                            vData.Tuning.WindowTint = p;
                        }
                        else if (iData[0] == "xenon")
                        {
                            vData.Tuning.Xenon = (sbyte)(p - 2);
                        }
                        else
                            return false;

                        MySQL.VehicleTuningUpdate(vData.Info);
                    }
                }
            }

            if (res == null)
                return false;

            PaymentProceed(pData, useCash, res.Value.MatPrice, res.Value.RealPrice);

            return true;
        }
    }
}