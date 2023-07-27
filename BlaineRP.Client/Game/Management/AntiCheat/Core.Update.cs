using BlaineRP.Client.Extensions.RAGE;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Management.Attachments;
using BlaineRP.Client.Game.Scripts.Sync;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.AntiCheat
{
    public partial class Core
    {
        private static void Update()
        {
            /*            if (Player.LocalPlayer.Vehicle is Vehicle fakeVeh && fakeVeh.IsLocal)
                        {
                            if (Player.LocalPlayer.Dimension == Settings.App.Static.MainDimension)
                            {
                                fakeVeh.Destroy();
                            }
                        }*/

            Vector3 curPos = Player.LocalPlayer.Position;

            AntiAltF4Vehicle();

            if (!AsyncTask.Methods.IsTaskStillPending(TeleportTaskKey, null))
            {
                float diff = Vector3.Distance(curPos, LastPosition);

                if (Player.LocalPlayer.Vehicle == null && Player.LocalPlayer.IsRagdoll() && Player.LocalPlayer.IsInAir() || Player.LocalPlayer.Vehicle != null)
                    diff = System.Math.Abs(diff - Player.LocalPlayer.GetSpeed());

                if (diff >= 50f)
                    //Utils.ConsoleOutput($"{RAGE.Util.Json.Serialize(curPos)}, {RAGE.Util.Json.Serialize(LastPosition)}");
                    Events.CallRemote("AC::Detect::TP", diff);
            }
            else
            {
                if ((LastTeleportWasGround ? curPos.DistanceIgnoreZ(LastAllowedPos) : Vector3.Distance(curPos, LastAllowedPos)) >= 50f)
                {
                    Player.LocalPlayer.SetCoordsNoOffset(LastAllowedPos.X, LastAllowedPos.Y, LastAllowedPos.Z, false, false, false);

                    curPos = LastAllowedPos;
                }
            }

            LastPosition = curPos;

            if (!AsyncTask.Methods.IsTaskStillPending(HealthTaskKey, null) && !LastAllowedInvincible)
            {
                int diff = Player.LocalPlayer.GetRealHealth() - LastHealth;

                if (diff > 0)
                    Player.LocalPlayer.SetRealHealth(LastHealth);
            }
            else if (Player.LocalPlayer.GetRealHealth() > LastAllowedHP)
            {
                Player.LocalPlayer.SetRealHealth(LastAllowedHP);
            }

            LastHealth = Player.LocalPlayer.GetRealHealth();

            if (!AsyncTask.Methods.IsTaskStillPending(ArmourTaskKey, null))
            {
                int diff = Player.LocalPlayer.GetArmour() - LastArmour;

                if (diff > 0)
                    Player.LocalPlayer.SetArmour(0);
            }
            else if (Player.LocalPlayer.GetArmour() > LastAllowedArm)
            {
                Player.LocalPlayer.SetArmour(LastAllowedArm);
            }

            LastArmour = Player.LocalPlayer.GetArmour();

            if (!AsyncTask.Methods.IsTaskStillPending(AlphaTaskKey, null))
                if (Player.LocalPlayer.GetAlpha() != LastAllowedAlpha)
                    Player.LocalPlayer.SetAlpha(LastAllowedAlpha, false);

            if (!AsyncTask.Methods.IsTaskStillPending(WeaponTaskKey, null))
            {
                uint curWeapon = Player.LocalPlayer.GetSelectedWeapon();

                if (curWeapon != LastAllowedWeapon && curWeapon != Weapons.Core.MobileHash)
                    Player.LocalPlayer.SetCurrentWeapon(LastAllowedWeapon, true);

                if (LastAllowedAmmo >= 0 && Player.LocalPlayer.GetAmmoInWeapon(curWeapon) > LastAllowedAmmo)
                    Player.LocalPlayer.SetAmmo(curWeapon, 0, 1);
            }
            else
            {
                uint curWeapon = Player.LocalPlayer.GetSelectedWeapon();

                if (curWeapon != LastAllowedWeapon && curWeapon != Weapons.Core.MobileHash)
                    Player.LocalPlayer.SetCurrentWeapon(LastAllowedWeapon, true);

                if (LastAllowedAmmo >= 0 && Player.LocalPlayer.GetAmmoInWeapon(LastAllowedWeapon) > LastAllowedAmmo)
                    Player.LocalPlayer.SetAmmo(LastAllowedWeapon, LastAllowedAmmo, 1);
            }

            for (var i = 0; i < Vehicles.ControlledVehicles.Count; i++)
            {
                Vehicle veh = Vehicles.ControlledVehicles[i];

                if (veh?.Exists != true)
                    continue;

                var vData = VehicleData.GetData(veh);

                if (vData == null)
                    continue;

                float lastHp = veh.GetData<float?>("LastHealth") ?? 1000f;
                float curHp = veh.GetEngineHealth();

                /*                if (!vData.IsInvincible)
                                {
                                    if (Player.LocalPlayer.Vehicle == veh || curHp < 0)
                                    {
                                        if (!veh.GetCanBeDamaged())
                                        {
                                            veh.SetCanBeDamaged(true);
                                            veh.SetInvincible(false);
                                        }
                                    }
                                    else
                                    {
                                        if (veh.GetCanBeDamaged())
                                        {
                                            veh.SetCanBeDamaged(false);
                                            veh.SetInvincible(true);
                                        }
                                    }
                                }*/

                if (veh.IsDead(0))
                {
                    if (curHp > -4000f)
                        veh.SetEngineHealth(-4000f);
                }
                else
                {
                    if (curHp - lastHp > 0)
                        veh.SetEngineHealth(lastHp);
                    else
                        veh.SetData("LastHealth", curHp);
                }

                if (vData.FrozenPosition is string posStr && (Player.LocalPlayer.Vehicle != veh || AsyncTask.Methods.IsTaskStillPending(TeleportTaskKey, null)))
                {
                    string[] posData = posStr.Split('_');

                    var vect = new Vector3(float.Parse(posData[0]), float.Parse(posData[1]), float.Parse(posData[2]));

                    Vehicle tpVeh = veh;

                    if (vData.IsAttachedToLocalTrailer is Vehicle trVeh)
                        tpVeh = trVeh;

                    if (tpVeh.GetCoords(false).DistanceTo(vect) > 0.01f)
                        tpVeh.SetCoordsNoOffset(vect.X, vect.Y, vect.Z, false, false, false);

                    if (posData.Length > 3)
                    {
                        var heading = float.Parse(posData[3]);

                        if (tpVeh.GetHeading() != heading)
                            tpVeh.SetHeading(heading);
                    }
                }

                int trailerVehHandle = -1;

                if (veh.GetTrailerVehicle(ref trailerVehHandle))
                {
                    Vehicle trailerVeh = Utils.Game.Misc.GetVehicleByHandle(trailerVehHandle, false);

                    if (trailerVeh?.Exists != true)
                    {
                        //veh.DetachFromTrailer();
                    }
                    else
                    {
                        if (trailerVeh.IsLocal)
                        {
                            trailerVeh = trailerVeh.GetData<Vehicle>("TrailerSync::Owner");

                            if (trailerVeh?.Exists != true)
                            {
                            }
                            else
                            {
                                if (vData.IsAttachedToVehicle == null)
                                {
                                    veh.DetachFromTrailer();

                                    Events.CallRemote("votc", veh, trailerVeh);
                                }
                            }
                        }
                        else
                        {
                            if (vData.IsAttachedToVehicle == null)
                            {
                                veh.DetachFromTrailer();

                                Events.CallRemote("votc", veh, trailerVeh);
                            }
                        }
                    }
                }
                else
                {
                    AttachmentEntity actualAttach = vData.IsAttachedToVehicle;

                    if (actualAttach != null && actualAttach.Type == AttachmentType.VehicleTrailerObjBoat)
                        Events.CallRemote("votc", veh, null);
                }
            }

            RAGE.Game.Player.ResetPlayerStamina();
            RAGE.Game.Player.SetPlayerHealthRechargeMultiplier(0f);
        }
    }
}