using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data.Fractions
{
    public class Police : Fraction, IUniformable
    {
        public Police(Types Type, string Name, uint StorageContainerId, Utils.Vector4 ContainerPos, Utils.Vector4 CWbPos, byte MaxRank, Vector3 LockerRoomPosition, string CreationWorkbenchPricesJs, string ArrestCellsPositionsJs, Vector3 ArrestMenuPosition) : base(Type, Name, StorageContainerId, ContainerPos, CWbPos, MaxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(CreationWorkbenchPricesJs))
        {
            var lockerRoomCs = new Additional.Cylinder(LockerRoomPosition, 1f, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
            {
                InteractionType = Additional.ExtraColshape.InteractionTypes.FractionLockerRoomInteract,

                ActionType = Additional.ExtraColshape.ActionTypes.FractionInteract,

                Data = Type,
            };

            var lockerRoomText = new TextLabel(new Vector3(LockerRoomPosition.X, LockerRoomPosition.Y, LockerRoomPosition.Z + 1f), "Раздевалка", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.MAIN_DIMENSION)
            {
                Font = 0,
            };

            if (Type == Types.PolicePaleto)
            {
                UniformNames = new List<string>()
                {
                    "Стандартная форма",
                    "Форма для специальных операций",
                    "Форма руководства",
                };
            }

            this.ArrestCellsPositions = RAGE.Util.Json.Deserialize<Utils.Vector4[]>(ArrestCellsPositionsJs);
        }

        public static Dictionary<string, uint[]> NumberplatePrices { get; set; }

        public Utils.Vector4[] ArrestCellsPositions { get; set; }

        public List<string> UniformNames { get; set; }

        public override void OnStartMembership(params object[] args)
        {
            base.OnStartMembership(args);

            CEF.HUD.Menu.UpdateCurrentTypes(true, CEF.HUD.Menu.Types.Fraction_Police_TabletPC);

            KeyBinds.CurrentExtraAction0 = () => CuffPlayer(null, null);

            SetCurrentData("LastCuffed", DateTime.MinValue);
        }

        public override void OnEndMembership()
        {
            CEF.HUD.Menu.UpdateCurrentTypes(false, CEF.HUD.Menu.Types.Fraction_Police_TabletPC);

            KeyBinds.CurrentExtraAction0 = null;

            base.OnEndMembership();
        }

        public static void ShowPoliceTabletPc()
        {

        }

        public async void CuffPlayer(Player player, bool? state)
        {
            if (player == null)
                player = BCRPClient.Interaction.CurrentEntity as Player;

            if (player?.Exists != true)
                return;

            var tData = Sync.Players.GetData(player);

            if (tData == null)
                return;

            var cuffState = tData.IsCuffed;

            var lastSent = GetCurrentData<DateTime>("LastCuffed");

            if (lastSent.IsSpam(1500, false, true))
                return;

            SetCurrentData("LastCuffed", Sync.World.ServerTime);

            var res = (int)await Events.CallRemoteProc("Police::Cuff", player, state);

            if (res == byte.MaxValue)
            {
                if (cuffState)
                {
                    CEF.Notification.Show("Cuffs::0_0_1", Utils.GetPlayerName(player, true, false, true));
                }
                else
                {
                    CEF.Notification.Show("Cuffs::0_0_0", Utils.GetPlayerName(player, true, false, true));
                }
            }
        }

        public void EscortPlayer(Player player, bool? state)
        {
            if (player == null)
                player = BCRPClient.Interaction.CurrentEntity as Player;

            if (player?.Exists != true)
                return;

            var tData = Sync.Players.GetData(player);

            if (tData == null)
                return;

            if (!tData.IsCuffed)
            {
                // notify

                return;
            }

            if (tData.IsAttachedTo is Entity entity && entity != Player.LocalPlayer)
            {
                // notify

                return;
            }
        }
    }
}
