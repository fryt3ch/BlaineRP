using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.EntitiesData.Vehicles.Static
{
    public class Tuning
    {
        private static Dictionary<byte, byte> DefaultMods => new Dictionary<byte, byte>()
        {
            { 0, 255 }, // Spoiler
            { 1, 255 }, // FrontBumper
            { 2, 255 }, // RearBumper
            { 3, 255 }, // SideSkirt
            { 4, 255 }, // Exhaust
            { 5, 255 }, // Frame
            { 6, 255 }, // Grille
            { 7, 255 }, // Hood
            { 8, 255 }, // Fender
            { 9, 255 }, // RightFender
            { 10, 255 }, // Roof
            { 11, 255 }, // Engine
            { 12, 255 }, // Brakes
            { 13, 255 }, // Transmission
            { 14, 255 }, // Horn
            { 15, 255 }, // Suspension
            { 23, 255 }, // FrontWheels
            { 24, 255 }, // BackWheels
            { 48, 255 }, // Livery
            { 32, 255 }, // Seats
            { 33, 255 }, // SteeringWheel
        };

        /// <summary>Основной цвет</summary>
        [JsonProperty(PropertyName = "C1")]
        public Colour Colour1 { get; set; }

        /// <summary>Второстепенный цвет</summary>
        [JsonProperty(PropertyName = "C2")]
        public Colour Colour2 { get; set; }

        [JsonProperty(PropertyName = "WC")]
        public byte WheelsColour { get; set; }

        [JsonProperty(PropertyName = "CT")]
        public byte ColourType { get; set; }

        [JsonProperty(PropertyName = "NC")]
        public Colour NeonColour { get; set; }

        [JsonProperty(PropertyName = "TSC")]
        public Colour TyresSmokeColour { get; set; }

        [JsonProperty(PropertyName = "PC")]
        public byte PearlescentColour { get; set; }

        [JsonProperty(PropertyName = "WT")]
        public byte WindowTint { get; set; }

        [JsonProperty(PropertyName = "X")]
        public sbyte Xenon { get; set; }

        [JsonProperty(PropertyName = "TT")]
        public bool Turbo { get; set; }

        [JsonProperty(PropertyName = "WHT")]
        public byte WheelsType { get; set; }

        /// <summary>Словарь модификаций, где ключ - индекс модификации, а значение - тип модификации</summary>
        public Dictionary<byte, byte> Mods { get; set; }

        public Tuning()
        {

        }

        public static Tuning CreateNew(Colour Colour1, Colour Colour2)
        {
            var res = new Tuning();

            res.Colour1 = Colour1;
            res.Colour2 = Colour2;

            res.NeonColour = null;

            res.TyresSmokeColour = null;

            res.Turbo = false;
            res.Xenon = -2;
            res.WindowTint = 0;
            res.PearlescentColour = 0;

            res.ColourType = 0;
            res.WheelsColour = 0;

            res.WheelsType = 0;

            res.Mods = DefaultMods;

            return res;
        }

        /// <summary>Метод для применения тюнинга к сущности транспорта</summary>
        /// <param name="vehicle">Сущность транспорта</param>
        /// <returns>Объект класса Tuning (себя же), если транспорт существует, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public void Apply(GTANetworkAPI.Vehicle vehicle)
        {
            UpdateColour(vehicle);

            if (NeonColour != null)
            {
                vehicle.Neons = true;
                vehicle.NeonColor = NeonColour.ToRageColour();

                vehicle.SetSharedData("Mods::Neon", true);
            }
            else
            {
                vehicle.Neons = false;

                vehicle.ResetSharedData("Mods::Neon");
            }

            vehicle.WindowTint = WindowTint;

            vehicle.WheelType = WheelsType;

            if (TyresSmokeColour != null)
            {
                vehicle.SetSharedData("Mods::TSColour", TyresSmokeColour);
            }
            else
            {
                vehicle.ResetSharedData("Mods::TSColour");
            }

            if (Turbo)
            {
                vehicle.SetSharedData("Mods::Turbo", true);
            }
            else
            {
                vehicle.ResetSharedData("Mods::Turbo");
            }

            vehicle.SetSharedData("Mods::Xenon", Xenon);

            foreach (var x in Mods)
            {
                vehicle.SetMod(x.Key, x.Value);
            }
        }

        public void UpdateColour(GTANetworkAPI.Vehicle vehicle)
        {
            vehicle.CustomPrimaryColor = Colour1.ToRageColour();
            vehicle.CustomSecondaryColor = Colour2.ToRageColour();

            vehicle.PearlescentColor = PearlescentColour;

            vehicle.WheelColor = WheelsColour;

            vehicle.SetSharedData("Mods::CT", ColourType);
        }

        public void UpdateWheels(GTANetworkAPI.Vehicle vehicle)
        {
            vehicle.WheelType = WheelsType;

            vehicle.SetMod(23, Mods[23]);
            vehicle.SetMod(24, Mods[24]);
        }
    }
}