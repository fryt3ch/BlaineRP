using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public class VehicleDestruction
        {
            public VehicleDestruction(int id, Vector3 Position)
            {
                var blip = new Additional.ExtraBlip(380, Position, "Свалка транспорта", 1f, 1, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var cs = new Additional.Cylinder(new Vector3(Position.X, Position.Y, Position.Z), 7.5f, 5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                {
                    ApproveType = Additional.ExtraColshape.ApproveTypes.None,

                    OnEnter = (cancel) =>
                    {
                        Player.LocalPlayer.SetData("VehicleDestruction::Id", id);

                        if (Player.LocalPlayer.Vehicle != null)
                        {
                            if (Player.LocalPlayer.Vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                                CEF.Notification.ShowHint($"Чтобы продать транспорт, выйдите из него, находитесь рядом с ним и смотрите на него, далее нажмите {KeyBinds.Get(KeyBinds.Types.Interaction).GetKeyString()} - Прочее - Свалка\n\nВам будет предложена сумма, которую Вы сможете получить за этот транспорт", true);
                        }
                        else
                        {
                            CEF.Notification.ShowHint($"Чтобы продать транспорт, смотрите на него, находясь рядом с ним, и нажмите {KeyBinds.Get(KeyBinds.Types.Interaction).GetKeyString()} - Прочее - Свалка\n\nВам будет предложена сумма, которую Вы сможете получить за этот транспорт", true);
                        }
                    },

                    OnExit = OnColshapeExit,
                };

                var marker = new Marker(29, new Vector3(Position.X, Position.Y, Position.Z + 1.5f), 2.5f, Vector3.Zero, Vector3.Zero, new RGBA(255, 255, 255, 125), true, Settings.MAIN_DIMENSION);
            }

            private static void OnColshapeExit(Events.CancelEventArgs cancel)
            {
                Player.LocalPlayer.ResetData("VehicleDestruction::Id");

                if (CEF.ActionBox.CurrentContextStr == "VehicleDestructConfirm")
                    CEF.ActionBox.Close(true);
            }

            public static async void VehicleDestruct(Vehicle veh)
            {
                if (!Player.LocalPlayer.HasData("VehicleDestruction::Id"))
                {
                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, "Вы должны находиться на свалке и рядом с местом сдачи транспорта!");

                    return;
                }

                var posId = Player.LocalPlayer.GetData<int>("VehicleDestruction::Id");

                var vData = Sync.Vehicles.GetData(veh);

                if (vData == null)
                    return;

                var vDataData = vData.Data;

                var res = await Events.CallRemoteProc("Vehicles::VDGP", veh, posId);

                if (res == null)
                    return;

                var price = res.ToDecimal();

                await CEF.ActionBox.ShowText
                (
                    "VehicleDestructConfirm", "Сдать транспорт на свалку", $"Вы действительно хотите сдать {vDataData.Name} #{vData.VID} на свалку?\n\nЗа этот транспорт Вы получите {Utils.GetPriceString(price)} наличными\n\nЕсли у транспорта есть багажник, то все вещи, которые в нём находятся безвозвратно исчезнут\n\nПодтвердите это действие", null, null,

                    CEF.ActionBox.DefaultBindAction,

                    async (rType) =>
                    {
                        if (rType != CEF.ActionBox.ReplyTypes.OK)
                        {
                            CEF.ActionBox.Close(true);

                            return;
                        }

                        var res = (bool)await Events.CallRemoteProc("Vehicles::VDC", veh, posId);

                        CEF.ActionBox.Close(true);
                    },

                    null
                );
            }
        }
    }
}