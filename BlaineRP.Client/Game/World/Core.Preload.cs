using System;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.World.Enums;
using RAGE;

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
                });

            for (int i = 0; i < RAGE.Elements.Entities.Objects.All.Count; i++)
            {
                var x = RAGE.Elements.Entities.Objects.All[i];

                if (x.GetSharedData<int>("IOG", -1) >= 0)
                {
                    x.NotifyStreaming = true;
                }
            }

            for (int i = 0; i < RAGE.Elements.Entities.Colshapes.All.Count; i++)
            {
                var x = RAGE.Elements.Entities.Colshapes.All[i];

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
                });

            InvokeHandler("Weather", GetSharedData<int>("Weather"), 0);

            foreach (var x in Business.All.Values)
            {
                var id = x.Id;
                var obj = x;

                AddDataHandler($"Business::{id}::OName",
                    (value, oldValue) =>
                    {
                        var name = (string)value;

                        obj.UpdateOwnerName(name);
                    });

                InvokeHandler($"Business::{id}::OName", obj.OwnerName, null);
            }

            foreach (var x in House.All.Values)
            {
                var id = x.Id;
                var obj = x;

                AddDataHandler($"House::{id}::OName",
                    (value, oldValue) =>
                    {
                        var name = (string)value;

                        obj.UpdateOwnerName(name);
                    });

                InvokeHandler($"House::{id}::OName", obj.OwnerName, null);
            }

            foreach (var x in Apartments.All.Values)
            {
                var id = x.Id;
                var obj = x;

                AddDataHandler($"Apartments::{id}::OName",
                    (value, oldValue) =>
                    {
                        var name = (string)value;

                        obj.UpdateOwnerName(name);
                    });

                //InvokeHandler($"Apartments::{id}::OName", GetSharedData<string>($"Apartments::{id}::OName"), null);
            }

            foreach (var x in ApartmentsRoot.All.Values)
                x.UpdateTextLabel();

            foreach (var x in Garage.All.Values)
            {
                var id = x.Id;
                var obj = x;

                AddDataHandler($"Garages::{id}::OName",
                    (value, oldValue) =>
                    {
                        var name = (string)value;

                        obj.UpdateOwnerName(name);
                    });

                InvokeHandler($"Garages::{id}::OName", obj.OwnerName, null);
            }

            Gang.GangZone.PostInitialize();

            Management.Doors.Core.Door.PostInitializeAll();

            foreach (var x in Fraction.All)
            {
                Fraction.OnStorageLockedChanged($"FRAC::SL_{(int)x.Key}", x.Value.StorageLocked, null);
                Fraction.OnCreationWorkbenchLockedChanged($"FRAC::CWBL_{(int)x.Key}", x.Value.CreationWorkbenchLocked, null);
            }
        }
    }
}