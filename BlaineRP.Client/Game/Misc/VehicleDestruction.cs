using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Input;
using BlaineRP.Client.Input.Enums;
using BlaineRP.Client.UI.CEF;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Data.Locations
{
    public class VehicleDestruction
        {
            public VehicleDestruction(int id, Vector3 Position)
            {
                var blip = new ExtraBlip(380, Position, "Свалка транспорта", 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                var cs = new Cylinder(new Vector3(Position.X, Position.Y, Position.Z), 7.5f, 5f, false, Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    ApproveType = ApproveTypes.None,

                    OnEnter = (cancel) =>
                    {
                        Player.LocalPlayer.SetData("VehicleDestruction::Id", id);

                        if (Player.LocalPlayer.Vehicle != null)
                        {
                            if (Player.LocalPlayer.Vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                                Notification.ShowHint($"Чтобы продать транспорт, выйдите из него, находитесь рядом с ним и смотрите на него, далее нажмите {Input.Core.Get(BindTypes.Interaction).GetKeyString()} - Прочее - Свалка\n\nВам будет предложена сумма, которую Вы сможете получить за этот транспорт", true);
                        }
                        else
                        {
                            Notification.ShowHint($"Чтобы продать транспорт, смотрите на него, находясь рядом с ним, и нажмите {Input.Core.Get(BindTypes.Interaction).GetKeyString()} - Прочее - Свалка\n\nВам будет предложена сумма, которую Вы сможете получить за этот транспорт", true);
                        }
                    },

                    OnExit = OnColshapeExit,
                };

                var marker = new Marker(29, new Vector3(Position.X, Position.Y, Position.Z + 1.5f), 2.5f, Vector3.Zero, Vector3.Zero, new RGBA(255, 255, 255, 125), true, Settings.App.Static.MainDimension);
            }

            private static void OnColshapeExit(Events.CancelEventArgs cancel)
            {
                Player.LocalPlayer.ResetData("VehicleDestruction::Id");

                if (ActionBox.CurrentContextStr == "VehicleDestructConfirm")
                    ActionBox.Close(true);
            }

            public static async void VehicleDestruct(RAGE.Elements.Vehicle veh)
            {
                if (!Player.LocalPlayer.HasData("VehicleDestruction::Id"))
                {
                    Notification.ShowError("Вы должны находиться на свалке и рядом с местом сдачи транспорта!");

                    return;
                }

                var posId = Player.LocalPlayer.GetData<int>("VehicleDestruction::Id");

                var vData = VehicleData.GetData(veh);

                if (vData == null)
                    return;

                var vDataData = vData.Data;

                var res = await Events.CallRemoteProc("Vehicles::VDGP", veh, posId);

                if (res == null)
                    return;

                var price = Utils.Convert.ToDecimal(res);

                await ActionBox.ShowText
                (
                    "VehicleDestructConfirm", "Сдать транспорт на свалку", $"Вы действительно хотите сдать {vDataData.Name} #{vData.VID} на свалку?\n\nЗа этот транспорт Вы получите {Locale.Get("GEN_MONEY_0", price)} наличными\n\nЕсли у транспорта есть багажник, то все вещи, которые в нём находятся безвозвратно исчезнут\n\nПодтвердите это действие", null, null,

                    ActionBox.DefaultBindAction,

                    async (rType) =>
                    {
                        if (rType != ActionBox.ReplyTypes.OK)
                        {
                            ActionBox.Close(true);

                            return;
                        }

                        var res = (bool)await Events.CallRemoteProc("Vehicles::VDC", veh, posId);

                        ActionBox.Close(true);
                    },

                    null
                );
            }
        }
}