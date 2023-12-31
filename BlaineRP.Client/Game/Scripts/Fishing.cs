﻿using System;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE;
using BlaineRP.Client.Game.Attachments;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Scripts
{
    [Script(int.MaxValue)]
    public class Fishing
    {
        public Fishing()
        {
            Events.Add("MG::F::S",
                (args) =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    CancelAllTasks();

                    if (args == null || args.Length == 0)
                        return;

                    if (args.Length == 1 && args[0] is int waitTime)
                    {
                        AsyncTask task = null;

                        task = new AsyncTask(async () =>
                            {
                                DateTime sTime = World.Core.ServerTime;

                                while (true)
                                {
                                    await RAGE.Game.Invoker.WaitAsync(100);

                                    if (!AsyncTask.Methods.IsTaskStillPending("MG::F::S::D", task))
                                        break;

                                    /*                            var waterPos = Utils.FindEntityWaterIntersectionCoord(Player.LocalPlayer, new Vector3(0f, 0f, 1f), 7.5f, 7.5f, -3.5f, 360f, 0.15f);
        
                                                                if (waterPos == null)
                                                                {
                                                                    CEF.Notification.ShowError(Locale.Notifications.Inventory.FishingNotAllowedHere);
        
                                                                    Events.CallRemote("Player::SUCI");
        
                                                                    return;
                                                                }*/

                                    if (World.Core.ServerTime.Subtract(sTime).TotalMilliseconds >= waitTime)
                                    {
                                        Events.CallRemote("MG::F::P", Player.LocalPlayer.GetData<float>("MG::F::T::WZ"));

                                        break;
                                    }
                                }

                                Player.LocalPlayer.ResetData("MG::F::T::WZ");
                            },
                            0,
                            false,
                            0
                        );

                        AsyncTask.Methods.SetAsPending(task, "MG::F::S::D");
                    }
                    else
                    {
                        var timeToCatch = (int)args[0];
                        var fSpeed = (float)args[1];
                        var catchCount = (int)args[2];

                        AsyncTask task = null;

                        task = new AsyncTask(async () =>
                            {
                                GameEntity fakeFishObj = null;

                                while (task?.IsCancelled == false && fakeFishObj?.Exists != true)
                                {
                                    await RAGE.Game.Invoker.WaitAsync(25);

                                    fakeFishObj = pData.AttachedObjects.Where(x => x.Type == AttachmentType.ItemFishG).Select(x => x.Object).FirstOrDefault();
                                }

                                if (task?.IsCancelled != false)
                                    return;

                                Vector3 fishCoords = RAGE.Game.Entity.GetEntityCoords(fakeFishObj.Handle, false);

                                float pHeading = Player.LocalPlayer.GetHeading();

                                Player.LocalPlayer.SetData("MG::F::LP", fishCoords.GetFrontOf(pHeading - 90f, 5f));
                                Player.LocalPlayer.SetData("MG::F::RP", fishCoords.GetFrontOf(pHeading + 90f, 5f));
                                Player.LocalPlayer.SetData("MG::F::Interp", 0.5f);

                                Player.LocalPlayer.SetData("MG::F::E", fakeFishObj);
                                Player.LocalPlayer.SetData("MG::F::T", World.Core.ServerTime);
                                Player.LocalPlayer.SetData("MG::F::SP", fSpeed);
                                Player.LocalPlayer.SetData("MG::F::MSP", fSpeed);
                                Player.LocalPlayer.SetData("MG::F::CT", timeToCatch);
                                Player.LocalPlayer.SetData("MG::F::CTC", catchCount);

                                Main.Render += FishingProcessRender;
                            },
                            0,
                            false,
                            0
                        );

                        AsyncTask.Methods.SetAsPending(task, "MG::F::S::D");
                    }
                }
            );
        }

        private static void CancelAllTasks()
        {
            AsyncTask.Methods.CancelPendingTask("MG::F::S::D");

            Main.Render -= FishingProcessRender;

            Player.LocalPlayer.ResetData("MG::F::E");
            Player.LocalPlayer.ResetData("MG::F::T");
            Player.LocalPlayer.ResetData("MG::F::SP");
            Player.LocalPlayer.ResetData("MG::F::MSP");
            Player.LocalPlayer.ResetData("MG::F::CT");
        }

        private static void FishingProcessRender()
        {
            double timePassed = World.Core.ServerTime.Subtract(Player.LocalPlayer.GetData<DateTime>("MG::F::T")).TotalMilliseconds;
            int timeCatched = Player.LocalPlayer.GetData<int>("MG::F::CTC");

            if (timePassed > Player.LocalPlayer.GetData<int>("MG::F::CT"))
            {
                CancelAllTasks();

                Events.CallRemote("MG::F::F", false);

                return;
            }

            if (timeCatched == 0)
            {
                CancelAllTasks();

                Events.CallRemote("MG::F::F", true);

                return;
            }

            GameEntity fakeFishObj = Player.LocalPlayer.GetData<GameEntity>("MG::F::E");

            if (fakeFishObj?.Exists != true)
            {
                CancelAllTasks();

                Events.CallRemote("MG::F::F", false);

                return;
            }

            float fpsCoef = Utils.Misc.GetFpsCoef();

            float interp = Player.LocalPlayer.GetData<float>("MG::F::Interp");

            float fSpeed = Player.LocalPlayer.GetData<float>("MG::F::SP");
            float fMaxSpeed = Player.LocalPlayer.GetData<float>("MG::F::MSP");

            if (timePassed > 500)
            {
                bool leftDown = Input.Core.IsDown(RAGE.Ui.VirtualKeys.A);
                bool rightDown = Input.Core.IsDown(RAGE.Ui.VirtualKeys.D);

                var deSpeed = 0.00001f;

                if (leftDown || rightDown)
                {
                    if (fSpeed != 0f)
                    {
                        Player.LocalPlayer.SetData("MG::F::PD", fSpeed > 0f);

                        float newSpeed = leftDown ? fSpeed + deSpeed : fSpeed - deSpeed;

                        if (fSpeed > 0f && newSpeed < 0f || fSpeed < 0f && newSpeed > 0f)
                            newSpeed = 0f;

                        fSpeed = newSpeed;
                    }
                    else
                    {
                        timeCatched -= 1;

                        if (timeCatched >= 0)
                            if (Player.LocalPlayer.HasData("MG::F::PD"))
                            {
                                if (Player.LocalPlayer.GetData<bool>("MG::F::PD"))
                                    fSpeed = -fMaxSpeed;
                                else
                                    fSpeed = fMaxSpeed;

                                Player.LocalPlayer.ResetData("MG::F::PD");
                            }

                        Player.LocalPlayer.SetData("MG::F::CTC", timeCatched);
                    }
                }
                else
                {
                    if (Player.LocalPlayer.HasData("MG::F::PD"))
                    {
                        if (Player.LocalPlayer.GetData<bool>("MG::F::PD"))
                            fSpeed = fMaxSpeed;
                        else
                            fSpeed = -fMaxSpeed;

                        Player.LocalPlayer.ResetData("MG::F::PD");
                    }
                }
            }

            if (fSpeed > fMaxSpeed)
                fSpeed = fMaxSpeed;
            else if (fSpeed < -fMaxSpeed)
                fSpeed = -fMaxSpeed;

            interp += fSpeed * fpsCoef;

            if (interp > 1f)
            {
                interp = 1f;

                if (fSpeed > 0f)
                    fSpeed = -fSpeed;
            }
            else if (interp < 0f)
            {
                interp = 0f;

                if (fSpeed < 0f)
                    fSpeed = -fSpeed;
            }

            Player.LocalPlayer.SetData("MG::F::SP", fSpeed);
            Player.LocalPlayer.SetData("MG::F::Interp", interp);

            var newPos = Vector3.Lerp(Player.LocalPlayer.GetData<Vector3>("MG::F::LP"), Player.LocalPlayer.GetData<Vector3>("MG::F::RP"), interp);

            float waterH = -1f;

            RAGE.Game.Water.GetWaterHeight(newPos.X, newPos.Y, newPos.Z, ref waterH);

            if (waterH > 0f)
                newPos.Z = waterH;

            RAGE.Game.Entity.SetEntityCoords(fakeFishObj.Handle, newPos.X, newPos.Y, newPos.Z, false, false, false, false);
        }
    }
}