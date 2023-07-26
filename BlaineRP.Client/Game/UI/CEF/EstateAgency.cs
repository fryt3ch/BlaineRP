using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class EstateAgency
    {
        public EstateAgency()
        {
            Events.Add("EstAgency::Close", (args) => Close(false));

            Events.Add("EstAgency::GPS",
                async (args) =>
                {
                    string[] idS = ((string)args[0])?.Split('_');

                    if (idS == null || idS.Length < 2)
                        return;

                    if (idS[0] == "h")
                    {
                        var houseId = uint.Parse(idS[1]);

                        House houseData = House.All[houseId];

                        if (houseData.OwnerName != null)
                        {
                            Notification.Show("House::AB");

                            return;
                        }

                        if (Estate.LastSent.IsSpam(1000, false, true))
                            return;

                        Estate.LastSent = World.Core.ServerTime;

                        var res = (bool)await Events.CallRemoteProc("EstAgency::GPS", AgencyId, PosId, (byte)0);

                        if (res)
                            Helpers.Blips.Core.CreateGPS(houseData.Position, Settings.App.Static.MainDimension, true);
                    }
                    else if (idS[0] == "a")
                    {
                        var apsId = uint.Parse(idS[1]);

                        Apartments apsData = Apartments.All[apsId];

                        if (apsData.OwnerName != null)
                        {
                            Notification.Show("House::AB");

                            return;
                        }

                        Helpers.Blips.Core.CreateGPS(ApartmentsRoot.All[apsData.RootId].PositionEnter,
                            Settings.App.Static.MainDimension,
                            true,
                            $"\n\nЭтаж: {ApartmentsRoot.All[apsData.RootId].Shell.StartFloor + apsData.FloorIdx}, кв. {apsData.NumberInRoot + 1}"
                        );
                    }
                    else if (idS[0] == "g")
                    {
                        var garageId = uint.Parse(idS[1]);

                        Garage garageData = Garage.All[garageId];

                        if (garageData.OwnerName != null)
                        {
                            Notification.Show("House::AB");

                            return;
                        }

                        Helpers.Blips.Core.CreateGPS(GarageRoot.All[garageData.RootId].EnterColshape.Position,
                            Settings.App.Static.MainDimension,
                            true,
                            $"\n\nНомер гаража в комплексе: {garageData.NumberInRoot + 1}"
                        );
                    }
                }
            );
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.EstateAgency);

        private static int EscBindIdx { get; set; } = int.MinValue;

        public static bool WasShowed => EscBindIdx != int.MinValue;

        private static int AgencyId { get; set; }
        private static int PosId { get; set; }

        private static ExtraColshape CloseColshape { get; set; }

        public static async System.Threading.Tasks.Task Show(int agencyId, int posId, decimal houseGpsPrice, decimal apsGpsPrice, decimal garageGpsPrice)
        {
            if (IsActive)
                return;

            // id, name, price, tax, rooms, garage capacity
            IEnumerable<object[]> houses = House.All.Where(x => x.Value.OwnerName == null)
                                                .Select(x => new object[]
                                                     {
                                                         $"h_{x.Key}",
                                                         $"{Utils.Game.Misc.GetStreetName(x.Value.Position)} [#{x.Key}]",
                                                         x.Value.Price,
                                                         x.Value.Tax,
                                                         (int)x.Value.RoomType,
                                                         x.Value.GarageType == null ? 0 : (int)x.Value.GarageType,
                                                     }
                                                 );

            // id, name, price, tax, rooms
            var apartments = new List<object>();

            foreach (ApartmentsRoot x in ApartmentsRoot.All.Values)
            {
                string arName = x.Name;

                uint counter = 1;

                foreach (Apartments y in x.AllApartments)
                {
                    if (y.OwnerName == null)
                        apartments.Add(new object[]
                            {
                                $"a_{y.Id}",
                                string.Format(Locale.General.Blip.ApartmentsOwnedBlip, arName, counter),
                                y.Price,
                                y.Tax,
                                (uint)y.RoomType,
                            }
                        );

                    counter++;
                }
            }

            // id, name, price, tax, garage capacity, complex num
            var garages = new List<object>();

            uint gCounter = 1;

            foreach (GarageRoot x in GarageRoot.All.Values)
            {
                uint counter = 1;

                foreach (Garage y in x.AllGarages)
                {
                    if (y.OwnerName == null)
                        garages.Add(new object[]
                            {
                                $"g_{y.Id}",
                                string.Format(Locale.General.Blip.GarageOwnedBlip, gCounter, counter),
                                y.Price,
                                y.Tax,
                                (uint)y.Type,
                                gCounter,
                            }
                        );
                }

                gCounter++;
            }

            await Browser.Render(Browser.IntTypes.EstateAgency, true, true);

            AgencyId = agencyId;
            PosId = posId;

            Browser.Window.ExecuteJs("EstAgency.draw",
                new object[]
                {
                    new object[]
                    {
                        houses,
                        apartments,
                        garages,
                        new object[]
                        {
                            houseGpsPrice,
                            apsGpsPrice,
                            garageGpsPrice,
                        },
                    },
                }
            );

            if (!WasShowed)
                Browser.Window.ExecuteCachedJs("EstAgency.selectOption", "-info", 0);
            else
                Browser.Window.ExecuteCachedJs("EstAgency.selectOption", "", 0);

            Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close(false);
                },
            };
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            Browser.Render(Browser.IntTypes.EstateAgency, false);

            Cursor.Show(false, false);

            Input.Core.Unbind(EscBindIdx);

            EscBindIdx = -1;
        }
    }
}