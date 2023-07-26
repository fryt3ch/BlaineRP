using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.World.Enums;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.World
{
    public partial class Core
    {
        public static void Preload()
        {
            if (Preloaded)
                return;

            Preloaded = true;

            AddDataHandler("cst",
                (value, oldValue) =>
                {
                    var of = DateTimeOffset.FromUnixTimeMilliseconds((long)value);

                    ServerTime = of.DateTime;
                    LocalTime = of.Add(-Settings.App.Profile.Current.General.TimeUtcOffset).LocalDateTime;
                }
            );

            for (var i = 0; i < Entities.Objects.All.Count; i++)
            {
                MapObject x = Entities.Objects.All[i];

                if (x.GetSharedData<int>("IOG", -1) >= 0)
                    x.NotifyStreaming = true;
            }

            for (var i = 0; i < Entities.Colshapes.All.Count; i++)
            {
                Colshape x = Entities.Colshapes.All[i];

                if (x == null)
                    continue;

                if (x.HasSharedData("Type") != true)
                    continue;

                Events.CallLocal("ExtraColshape::New", x);
            }

            AddDataHandler("Weather",
                (value, oldValue) =>
                {
                    var weather = (WeatherTypes)(int)value;

                    if (CurrentWeatherCustom != null || CurrentWeatherSpecial != null)
                        return;

                    SetWeatherNow(weather);
                }
            );

            InvokeHandler("Weather", GetSharedData<int>("Weather"), 0);

            foreach (Business x in Business.All.Values)
            {
                int id = x.Id;
                Business obj = x;

                AddDataHandler($"Business::{id}::OName",
                    (value, oldValue) =>
                    {
                        var name = (string)value;

                        obj.UpdateOwnerName(name);
                    }
                );

                InvokeHandler($"Business::{id}::OName", obj.OwnerName, null);
            }

            foreach (House x in House.All.Values)
            {
                uint id = x.Id;
                House obj = x;

                AddDataHandler($"House::{id}::OName",
                    (value, oldValue) =>
                    {
                        var name = (string)value;

                        obj.UpdateOwnerName(name);
                    }
                );

                InvokeHandler($"House::{id}::OName", obj.OwnerName, null);
            }

            foreach (Apartments x in Apartments.All.Values)
            {
                uint id = x.Id;
                Apartments obj = x;

                AddDataHandler($"Apartments::{id}::OName",
                    (value, oldValue) =>
                    {
                        var name = (string)value;

                        obj.UpdateOwnerName(name);
                    }
                );

                //InvokeHandler($"Apartments::{id}::OName", GetSharedData<string>($"Apartments::{id}::OName"), null);
            }

            foreach (ApartmentsRoot x in ApartmentsRoot.All.Values)
            {
                x.UpdateTextLabel();
            }

            foreach (Garage x in Garage.All.Values)
            {
                uint id = x.Id;
                Garage obj = x;

                AddDataHandler($"Garages::{id}::OName",
                    (value, oldValue) =>
                    {
                        var name = (string)value;

                        obj.UpdateOwnerName(name);
                    }
                );

                InvokeHandler($"Garages::{id}::OName", obj.OwnerName, null);
            }

            Gang.GangZone.PostInitialize();

            Management.Doors.Core.Door.PostInitializeAll();

            foreach (KeyValuePair<FractionTypes, Fraction> x in Fraction.All)
            {
                Fraction.OnStorageLockedChanged($"FRAC::SL_{(int)x.Key}", x.Value.StorageLocked, null);
                Fraction.OnCreationWorkbenchLockedChanged($"FRAC::CWBL_{(int)x.Key}", x.Value.CreationWorkbenchLocked, null);
            }
        }
    }
}