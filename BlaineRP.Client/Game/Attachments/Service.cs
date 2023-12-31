﻿using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.EntitiesData.Vehicles;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Jobs;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.Scripts.Misc;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using RAGE.Ui;

namespace BlaineRP.Client.Game.Attachments
{
    [Script(int.MaxValue)]
    public class Service
    {
        public static string AttachedObjectsKey = "AttachedObjects";
        public static string AttachedEntitiesKey = "AttachedEntities";

        private static Dictionary<uint, Dictionary<AttachmentType, AttachmentData>> ModelDependentAttachments =
            new Dictionary<string, Dictionary<AttachmentType, AttachmentData>>()
            {
                {
                    "brp_p_ring_0_0", new Dictionary<AttachmentType, AttachmentData>()
                    {
                        { AttachmentType.PedRingLeft3, new AttachmentData(26613, new Vector3(0.033f, -0.003f, 0.001f), new Vector3(70f, 85f, -5f), false, false, false, 2, true) },
                        {
                            AttachmentType.PedRingRight3,
                            new AttachmentData(58869, new Vector3(0.033f, 0.0007f, 0.0029f), new Vector3(105f, -85f, 15f), false, false, false, 2, true)
                        },
                    }
                },
                {
                    "brp_p_ring_1_0", new Dictionary<AttachmentType, AttachmentData>()
                    {
                        { AttachmentType.PedRingLeft3, new AttachmentData(26613, new Vector3(0.033f, -0.003f, 0.001f), new Vector3(80f, 95f, -5f), false, false, false, 2, true) },
                        {
                            AttachmentType.PedRingRight3,
                            new AttachmentData(58869, new Vector3(0.033f, 0.0013f, 0.0029f), new Vector3(115f, -105f, 15f), false, false, false, 2, true)
                        },
                    }
                },
            }.ToDictionary(x => RAGE.Util.Joaat.Hash(x.Key), x => x.Value);

        public static Dictionary<AttachmentType, AttachmentData> Attachments = new Dictionary<AttachmentType, AttachmentData>()
        {
            { AttachmentType.PushVehicle, new AttachmentData(6286, new Vector3(0f, 0f, 0.35f), new Vector3(0f, 0f, 0f), false, true, true, 2, true) },
            { AttachmentType.PhoneSync, new AttachmentData(28422, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 30f), false, false, false, 2, true) },
            { AttachmentType.VehKey, new AttachmentData(6286, new Vector3(0.08f, 0.04f, -0.015f), new Vector3(175f, -115f, -90f), false, false, false, 2, true) },
            { AttachmentType.ParachuteSync, new AttachmentData(1_000_000 + 57717, new Vector3(0f, 0f, 3f), new Vector3(0f, 0f, 0f), false, false, false, 0, true) },
            { AttachmentType.WeaponLeftTight, new AttachmentData(58271, new Vector3(0.08f, 0.03f, -0.1f), new Vector3(-80.77f, 0f, 0f), false, false, false, 2, true) },
            { AttachmentType.WeaponRightTight, new AttachmentData(51826, new Vector3(0.02f, 0.06f, 0.1f), new Vector3(-100f, 0f, 0f), false, false, false, 2, true) },
            { AttachmentType.WeaponLeftBack, new AttachmentData(24818, new Vector3(-0.1f, -0.15f, 0.11f), new Vector3(-180f, 0f, 0f), false, false, false, 2, true) },
            { AttachmentType.WeaponRightBack, new AttachmentData(24818, new Vector3(-0.1f, -0.15f, -0.13f), new Vector3(0f, 0f, 3.5f), false, false, false, 2, true) },
            { AttachmentType.Carry, new AttachmentData(1_000_000 + 0, new Vector3(0.23f, 0.18f, 0.65f), new Vector3(0.5f, 0.5f, 15f), false, false, false, 2, true) },
            { AttachmentType.PiggyBack, new AttachmentData(0, new Vector3(0f, -0.07f, 0.45f), new Vector3(0f, 0f, 0f), false, false, false, 2, true) },
            { AttachmentType.Hostage, new AttachmentData(0, new Vector3(-0.24f, 0.11f, 0f), new Vector3(0.5f, 0.5f, 0f), false, false, false, 2, true) },
            { AttachmentType.VehicleTrunk, new AttachmentData(-1, new Vector3(0f, 0.5f, 0.4f), new Vector3(0f, 0f, 0f), false, false, false, 2, true) },
            { AttachmentType.TrailerObjOnBoat, new AttachmentData(20, new Vector3(0f, -1f, 0.25f), new Vector3(0f, 0f, 0f), false, true, false, 2, true) },
            { AttachmentType.TractorTrailFarmHarv, new AttachmentData(0, new Vector3(0f, -2.7f, 0f), new Vector3(0f, 0f, 0f), false, true, false, 2, true) },
            { AttachmentType.ItemFishingRodG, new AttachmentData(60309, new Vector3(0.01f, -0.01f, 0.03f), new Vector3(0.1f, 0f, 0f), false, false, false, 2, true) },
            { AttachmentType.ItemShovel, new AttachmentData(28422, new Vector3(0.05f, -0.03f, -0.9f), new Vector3(2.1f, -4.2f, 5f), false, false, false, 2, true) },
            { AttachmentType.ItemMetalDetector, new AttachmentData(18905, new Vector3(0.15f, 0.1f, 0f), new Vector3(270f, 90f, 80f), false, false, false, 2, true) },
            {
                AttachmentType.ItemFishG, new AttachmentData(int.MinValue,
                    null,
                    new Vector3(0f, 0f, 0f),
                    false,
                    false,
                    false,
                    2,
                    true,
                    async (args) =>
                    {
                        var gEntity = (MapObject)args[0];

                        await Streaming.RequestPtfx("core");

                        gEntity.SetData("PtfxHandle",
                            RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("water_splash_shark_wade", gEntity.Handle, 0f, 0f, 0f, 0f, 0f, 0f, 2.5f, false, false, false)
                        ); // water_splash_animal_wade
                    }
                )
            },
            {
                AttachmentType.ItemCigHand, new AttachmentData(64097,
                    new Vector3(0.02f, 0.02f, -0.008f),
                    new Vector3(100f, 0f, 100f),
                    false,
                    false,
                    false,
                    2,
                    true,
                    async (args) =>
                    {
                        var gEntity = (MapObject)args[0];

                        await Streaming.RequestPtfx("core");

                        gEntity.SetData("PtfxHandle",
                            RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle, -0.05f, 0f, 0f, 0f, 0f, 0f, 0.04f, false, false, false)
                        );
                    }
                )
            },
            {
                AttachmentType.ItemCig1Hand, new AttachmentData(64097,
                    new Vector3(0.02f, 0.02f + 0.0365f, -0.008f),
                    new Vector3(100f, 0f, -80f),
                    false,
                    false,
                    false,
                    2,
                    true,
                    async (args) =>
                    {
                        var gEntity = (MapObject)args[0];

                        await Streaming.RequestPtfx("core");

                        gEntity.SetData("PtfxHandle",
                            RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle, 0.125f, 0f, 0f, 0f, 0f, 0f, 0.05f, false, false, false)
                        );
                    }
                )
            },
            {
                AttachmentType.ItemCig2Hand, new AttachmentData(64097,
                    new Vector3(0.02f, 0.02f, -0.008f),
                    new Vector3(100f, 0f, -100f),
                    false,
                    false,
                    false,
                    2,
                    true,
                    async (args) =>
                    {
                        var gEntity = (MapObject)args[0];

                        await Streaming.RequestPtfx("core");

                        gEntity.SetData("PtfxHandle",
                            RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle, 0.05f, 0f, 0f, 0f, 0f, 0f, 0.075f, false, false, false)
                        );
                    }
                )
            },
            {
                AttachmentType.ItemCig3Hand, new AttachmentData(64097,
                    new Vector3(0.02f, 0.02f, -0.008f),
                    new Vector3(100f, 0f, 100f),
                    false,
                    false,
                    false,
                    2,
                    true,
                    async (args) =>
                    {
                        var gEntity = (MapObject)args[0];

                        await Streaming.RequestPtfx("core");

                        gEntity.SetData("PtfxHandle",
                            RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle, -0.09f, 0f, 0f, 0f, 0f, 0f, 0.06f, false, false, false)
                        );
                    }
                )
            },
            {
                AttachmentType.ItemCigMouth, new AttachmentData(47419,
                    new Vector3(0.015f, -0.009f, 0.003f),
                    new Vector3(55f, 0f, 110f),
                    false,
                    false,
                    false,
                    2,
                    true,
                    async (args) =>
                    {
                        var gEntity = (MapObject)args[0];

                        await Streaming.RequestPtfx("core");

                        gEntity.SetData("PtfxHandle",
                            RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle, -0.05f, 0f, 0f, 0f, 0f, 0f, 0.04f, false, false, false)
                        );
                    }
                )
            },
            {
                AttachmentType.ItemCig1Mouth, new AttachmentData(47419,
                    new Vector3(0.001f, 0.036f, 0.005f),
                    new Vector3(55f, 0f, -70f),
                    false,
                    false,
                    false,
                    2,
                    true,
                    async (args) =>
                    {
                        var gEntity = (MapObject)args[0];

                        await Streaming.RequestPtfx("core");

                        gEntity.SetData("PtfxHandle",
                            RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle, 0.125f, 0f, 0f, 0f, 0f, 0f, 0.05f, false, false, false)
                        );
                    }
                )
            },
            {
                AttachmentType.ItemCig2Mouth, new AttachmentData(47419,
                    new Vector3(0.01f, 0f, 0f),
                    new Vector3(50f, 0f, -80f),
                    false,
                    false,
                    false,
                    2,
                    true,
                    async (args) =>
                    {
                        var gEntity = (MapObject)args[0];

                        await Streaming.RequestPtfx("core");

                        gEntity.SetData("PtfxHandle",
                            RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle, 0.05f, 0f, 0f, 0f, 0f, 0f, 0.075f, false, false, false)
                        );
                    }
                )
            },
            {
                AttachmentType.ItemCig3Mouth, new AttachmentData(47419,
                    new Vector3(0.01f, 0f, 0f),
                    new Vector3(50f, 0f, 80f),
                    false,
                    false,
                    false,
                    2,
                    true,
                    async (args) =>
                    {
                        var gEntity = (MapObject)args[0];

                        await Streaming.RequestPtfx("core");

                        gEntity.SetData("PtfxHandle",
                            RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("exp_grd_bzgas_smoke", gEntity.Handle, -0.09f, 0f, 0f, 0f, 0f, 0f, 0.06f, false, false, false)
                        );
                    }
                )
            },
            { AttachmentType.ItemBandage, new AttachmentData(36029, new Vector3(-0.04f, 0f, -0.01f), new Vector3(160f, 0f, 90f), false, false, false, 2, true) },
            { AttachmentType.ItemMedKit, new AttachmentData(36029, new Vector3(0.03f, 0.01f, 0.12f), new Vector3(180f, -10f, 90f), false, false, false, 2, true) },
            { AttachmentType.ItemChips, new AttachmentData(28422, new Vector3(-0.04f, 0.02f, -0.04f), new Vector3(15f, 20f, 10f), false, false, false, 2, true) },
            { AttachmentType.ItemBurger, new AttachmentData(28422, new Vector3(-0.01f, -0.01f, 0f), new Vector3(20f, 0f, 0f), false, false, false, 2, true) },
            { AttachmentType.ItemHotdog, new AttachmentData(60309, new Vector3(0.05f, 0.02f, -0.01f), new Vector3(0f, 0f, 90f), false, false, false, 2, true) },
            { AttachmentType.ItemChocolate, new AttachmentData(28422, new Vector3(-0.01f, -0.01f, 0f), new Vector3(20f, 0f, 0f), false, false, false, 2, true) },
            { AttachmentType.ItemPizza, new AttachmentData(28422, new Vector3(-0.01f, -0.01f, 0f), new Vector3(20f, 0f, 0f), false, false, false, 2, true) },
            { AttachmentType.ItemBeer, new AttachmentData(28422, new Vector3(0.012f, 0.028f, -0.1f), new Vector3(5f, 0f, 0f), false, false, false, 2, true) },
            { AttachmentType.Cuffs, new AttachmentData(60309, new Vector3(-0.055f, 0.06f, 0.04f), new Vector3(265f, 155f, 80f), false, false, false, 0, true) },
            { AttachmentType.CableCuffs, new AttachmentData(60309, new Vector3(-0.055f, 0.06f, 0.04f), new Vector3(265f, 155f, 80f), false, false, false, 0, true) },
            { AttachmentType.EmsHealingBedFakeAttach, new AttachmentData(int.MinValue, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), false, false, false, 0, true) },
            {
                AttachmentType.PoliceEscort, new AttachmentData(1_000_000 + 11816, new Vector3(0.30f, 0.35f, 0f), new Vector3(0f, 0f, 0f), false, false, false, 2, true)
                {
                    DisableInteraction = 1,
                }
            },
            {
                AttachmentType.FarmPlantSmallShovel, new AttachmentData(28422,
                    new Vector3(0f, 0.01f, -0.03f),
                    new Vector3(0f, 0f, 0f),
                    false,
                    false,
                    false,
                    2,
                    true,
                    async (args) =>
                    {
                        var gEntity = (MapObject)args[0];

                        await Streaming.RequestPtfx("core");

                        gEntity.SetData("PtfxHandle",
                            RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("scrape_mud", gEntity.Handle, 0.25f, 0f, 0f, 0f, 0f, 0f, 0.25f, false, false, false)
                        );
                    }
                )
            }, // rot Y -180 prop_buck_spade_09

            { AttachmentType.FarmOrangeBoxCarry, new AttachmentData(28422, new Vector3(0f, -0.02f, -0.07f), new Vector3(0f, 320f, 90f), false, false, false, 2, true) },
            { AttachmentType.FarmMilkBucketCarry, new AttachmentData(28422, new Vector3(0.05f, 0f, -0.05f), new Vector3(0f, 0f, 0f), false, false, false, 2, true) },
            {
                AttachmentType.FarmWateringCan, new AttachmentData(28422,
                    new Vector3(0.09f, 0f, -0.2f),
                    new Vector3(0f, 25f, 0f),
                    false,
                    false,
                    false,
                    2,
                    true,
                    async (args) =>
                    {
                        var gEntity = (MapObject)args[0];

                        await Streaming.RequestPtfx("core");

                        gEntity.SetData("PtfxHandle",
                            RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("water_cannon_spray", gEntity.Handle, 0.35f, 0f, 0.25f, 0f, 0f, 0f, 0.15f, false, false, false)
                        );
                    }
                )
            }, // prop_wateringcan
        };

        private static Dictionary<AttachmentType, AttachmentType> SameActionsTypes = new Dictionary<AttachmentType, AttachmentType>()
        {
            { AttachmentType.ItemCig1Hand, AttachmentType.ItemCigHand },
            { AttachmentType.ItemCig2Hand, AttachmentType.ItemCigHand },
            { AttachmentType.ItemCig3Hand, AttachmentType.ItemCigHand },
            { AttachmentType.ItemCig1Mouth, AttachmentType.ItemCigMouth },
            { AttachmentType.ItemCig2Mouth, AttachmentType.ItemCigMouth },
            { AttachmentType.ItemCig3Mouth, AttachmentType.ItemCigMouth },
        };

        private static Dictionary<AttachmentType, (Action On, Action Off, Action Loop)?> TargetActions = new Dictionary<AttachmentType, (Action On, Action Off, Action Loop)?>()
        {
            {
                AttachmentType.PushVehicle, (new Action(() =>
                    {
                        var veh = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Vehicle;

                        if (veh == null)
                            return;

                        PushVehicle.On(true, veh);

                        Management.Weapons.Core.DisabledFiring = true;
                    }
                ), new Action(() =>
                    {
                        PushVehicle.Off(true);

                        Management.Weapons.Core.DisabledFiring = false;
                    }
                ), new Action(() =>
                    {
                        var veh = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Vehicle;

                        if (veh?.Exists != true ||
                            Utils.Game.Misc.AnyOnFootMovingControlJustPressed() ||
                            PlayerActions.IsAnyActionActive(false,
                                PlayerActions.Types.Knocked,
                                PlayerActions.Types.Frozen,
                                PlayerActions.Types.Cuffed,
                                PlayerActions.Types.Finger,
                                PlayerActions.Types.FastAnimation,
                                PlayerActions.Types.Scenario,
                                PlayerActions.Types.InVehicle,
                                PlayerActions.Types.InWater,
                                PlayerActions.Types.Shooting,
                                PlayerActions.Types.Reloading,
                                PlayerActions.Types.Climbing,
                                PlayerActions.Types.Falling,
                                PlayerActions.Types.Ragdoll,
                                PlayerActions.Types.Jumping,
                                PlayerActions.Types.NotOnFoot
                            ) ||
                            veh.GetIsEngineRunning() ||
                            veh.HasCollidedWithAnything() ||
                            Vector3.Distance(Player.LocalPlayer.Position, veh.GetCoords(false)) > Settings.App.Static.EntityInteractionMaxDistance)
                        {
                            if (Animations.Service.LastSent.IsSpam(500, false, false))
                                return;

                            Animations.Service.LastSent = World.Core.ServerTime;

                            PushVehicle.Off(false);
                        }
                        else
                        {
                            Graphics.DrawText(Locale.General.Animations.CancelTextPushVehicle,
                                0.5f,
                                0.95f,
                                255,
                                255,
                                255,
                                255,
                                0.45f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                false,
                                true
                            );
                        }
                    }
                ))
            },
            {
                AttachmentType.VehicleTrunk, (new Action(() =>
                    {
                        Management.Weapons.Core.DisabledFiring = true;
                    }
                ), new Action(() =>
                    {
                        Management.Weapons.Core.DisabledFiring = false;
                    }
                ), new Action(() =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var root = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Vehicle;

                        Input.Core.ExtraBind bind = Input.Core.Get(Input.Enums.BindTypes.CancelAnimation);

                        bool isForced = pData.IsKnocked || pData.IsCuffed;

                        if (root?.Exists != true || !isForced && bind.IsPressed)
                        {
                            if (Animations.Service.LastSent.IsSpam(500, false, false))
                                return;

                            Animations.Service.LastSent = World.Core.ServerTime;

                            Events.CallRemote("Players::StopInTrunk");
                        }
                        else
                        {
                            if (!isForced)
                                Graphics.DrawText(string.Format(Locale.General.Animations.CancelTextInTrunk, bind.GetKeyString()),
                                    0.5f,
                                    0.95f,
                                    255,
                                    255,
                                    255,
                                    255,
                                    0.45f,
                                    RAGE.Game.Font.ChaletComprimeCologne,
                                    false,
                                    true
                                );
                        }
                    }
                ))
            },
            {
                AttachmentType.Carry, (new Action(() =>
                    {
                        Management.Weapons.Core.DisabledFiring = true;
                    }
                ), new Action(() =>
                    {
                        Management.Weapons.Core.DisabledFiring = false;
                    }
                ), new Action(() =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var root = Player.LocalPlayer.GetData<Entity>("IsAttachedTo::Entity") as Player;

                        Input.Core.ExtraBind bind = Input.Core.Get(Input.Enums.BindTypes.CancelAnimation);

                        bool isForced = pData.IsKnocked || pData.IsCuffed;

                        if (root?.Exists != true || !isForced && bind.IsJustPressed)
                        {
                            if (Animations.Service.LastSent.IsSpam(500, false, false))
                                return;

                            Animations.Service.LastSent = World.Core.ServerTime;

                            Events.CallRemote("Players::StopCarry");
                        }
                        else
                        {
                            if (!isForced)
                                Graphics.DrawText(string.Format(Locale.General.Animations.CancelTextCarryB, bind.GetKeyString()),
                                    0.5f,
                                    0.95f,
                                    255,
                                    255,
                                    255,
                                    255,
                                    0.45f,
                                    RAGE.Game.Font.ChaletComprimeCologne,
                                    false,
                                    true
                                );
                        }
                    }
                ))
            },
            {
                AttachmentType.PoliceEscort, (() =>
                {
                }, () =>
                {
                    if (Player.LocalPlayer.GetData<bool>("POLICEESCORTEFLAG"))
                    {
                        Vector3 pos = Player.LocalPlayer.GetCoords(false);

                        Player.LocalPlayer.TaskGoStraightToCoord(pos.X, pos.Y, pos.Z, 1f, 1, Player.LocalPlayer.GetHeading(), 0f);

                        Player.LocalPlayer.ResetData("POLICEESCORTEFLAG");
                    }
                }, () =>
                {
                    Player rootPlayer = Utils.Game.Misc.GetPlayerByHandle(Player.LocalPlayer.GetAttachedTo(), true);

                    var speed = 0f;

                    if (rootPlayer?.Exists != true || rootPlayer == Player.LocalPlayer || (speed = rootPlayer.GetSpeed()) <= 0.8f)
                    {
                        if (Player.LocalPlayer.GetData<bool>("POLICEESCORTEFLAG"))
                        {
                            Vector3 pos = Player.LocalPlayer.GetCoords(false);

                            Player.LocalPlayer.TaskGoStraightToCoord(pos.X, pos.Y, pos.Z, 1f, 1, Player.LocalPlayer.GetHeading(), 0f);

                            Player.LocalPlayer.ResetData("POLICEESCORTEFLAG");
                        }

                        return;
                    }
                    else
                    {
                        float heading = rootPlayer.GetHeading();

                        Vector3 pos = Management.Camera.Service.GetFrontOf(Player.LocalPlayer.Position, heading, 1f);

                        Player.LocalPlayer.TaskGoStraightToCoord(pos.X, pos.Y, pos.Z, speed * 0.5f, -1, heading, 0f);

                        Player.LocalPlayer.SetData("POLICEESCORTEFLAG", true);
                    }
                })
            },
        };

        private static Dictionary<AttachmentType, (Action On, Action Off, Action Loop)?> RootActions = new Dictionary<AttachmentType, (Action On, Action Off, Action Loop)?>()
        {
            {
                AttachmentType.Carry, (new Action(() =>
                    {
                        Management.Weapons.Core.DisabledFiring = true;
                    }
                ), new Action(() =>
                    {
                        Management.Weapons.Core.DisabledFiring = false;
                    }
                ), new Action(() =>
                    {
                        Player target = Player.LocalPlayer.GetData<List<AttachmentEntity>>(AttachedEntitiesKey)
                                              .Where(x => x.Type == AttachmentType.Carry)
                                              .Select(x => Entities.Players.GetAtRemote(x.RemoteID))
                                              .FirstOrDefault();

                        Input.Core.ExtraBind bind = Input.Core.Get(Input.Enums.BindTypes.CancelAnimation);

                        if (target?.Exists != true || bind.IsJustPressed)
                        {
                            if (Animations.Service.LastSent.IsSpam(500, false, false))
                                return;

                            Animations.Service.LastSent = World.Core.ServerTime;

                            Events.CallRemote("Players::StopCarry");
                        }
                        else
                        {
                            Graphics.DrawText(string.Format(Locale.General.Animations.CancelTextCarryA, bind.GetKeyString()),
                                0.5f,
                                0.95f,
                                255,
                                255,
                                255,
                                255,
                                0.45f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                true,
                                true
                            );
                        }
                    }
                ))
            },
            {
                AttachmentType.ItemCigHand, (new Action(() =>
                    {
                        Management.Weapons.Core.DisabledFiring = true;

                        Player.LocalPlayer.SetData("Temp::Smoke::LastSent", World.Core.ServerTime);
                    }
                ), new Action(() =>
                    {
                        Management.Weapons.Core.DisabledFiring = false;

                        Player.LocalPlayer.ResetData("Temp::Smoke::LastSent");
                    }
                ), new Action(() =>
                    {
                        if (!Player.LocalPlayer.HasData("Smoke::Data::Puffs"))
                            return;

                        Input.Core.ExtraBind bind = Input.Core.Get(Input.Enums.BindTypes.CancelAnimation);

                        int puffs = Player.LocalPlayer.GetData<int>("Smoke::Data::Puffs");

                        if (bind.IsJustPressed || Player.LocalPlayer.IsInWater() || puffs == 0)
                        {
                            if (Animations.Service.LastSent.IsSpam(500, false, false))
                                return;

                            Animations.Service.LastSent = World.Core.ServerTime;

                            Events.CallRemote("Players::Smoke::Stop");
                        }
                        else
                        {
                            DateTime lastSent = Player.LocalPlayer.GetData<DateTime>("Temp::Smoke::LastSent");

                            // lmb - do puff
                            if (!UI.CEF.Cursor.IsVisible &&
                                RAGE.Game.Pad.IsDisabledControlJustPressed(0, 24) &&
                                !PlayerActions.IsAnyActionActive(false, PlayerActions.Types.Animation, PlayerActions.Types.FastAnimation, PlayerActions.Types.OtherAnimation))
                            {
                                if (!lastSent.IsSpam(1000, false, false))
                                {
                                    Events.CallRemote("Players::Smoke::Puff");

                                    Player.LocalPlayer.SetData("Temp::Smoke::LastSent", World.Core.ServerTime);
                                }
                            }
                            // alt - to mouth
                            else if ((!UI.CEF.Cursor.IsVisible && Input.Core.IsJustDown(VirtualKeys.LeftMenu) || Player.LocalPlayer.Vehicle != null) &&
                                     !PlayerActions.IsAnyActionActive(false, PlayerActions.Types.Animation, PlayerActions.Types.FastAnimation, PlayerActions.Types.OtherAnimation))
                            {
                                if (!lastSent.IsSpam(1000, false, false))
                                {
                                    Events.CallRemote("Players::Smoke::State");

                                    Player.LocalPlayer.SetData("Temp::Smoke::LastSent", World.Core.ServerTime);
                                }
                            }

                            Graphics.DrawText(string.Format(Locale.General.Animations.TextDoPuffSmoke, puffs),
                                0.5f,
                                0.90f,
                                255,
                                255,
                                255,
                                255,
                                0.45f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                false,
                                true
                            );
                            Graphics.DrawText(Locale.General.Animations.TextToMouthSmoke,
                                0.5f,
                                0.925f,
                                255,
                                255,
                                255,
                                255,
                                0.45f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                false,
                                true
                            );
                            Graphics.DrawText(string.Format(Locale.General.Animations.CancelTextSmoke, bind.GetKeyString()),
                                0.5f,
                                0.95f,
                                255,
                                255,
                                255,
                                255,
                                0.45f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                false,
                                true
                            );
                        }
                    }
                ))
            },
            {
                AttachmentType.ItemCigMouth, (new Action(() =>
                    {
                        Player.LocalPlayer.SetData("Temp::Smoke::LastSent", World.Core.ServerTime);
                    }
                ), new Action(() =>
                    {
                        Player.LocalPlayer.ResetData("Temp::Smoke::LastSent");
                    }
                ), new Action(() =>
                    {
                        Input.Core.ExtraBind bind = Input.Core.Get(Input.Enums.BindTypes.CancelAnimation);

                        if (bind.IsJustPressed || Player.LocalPlayer.IsInWater())
                        {
                            if (Animations.Service.LastSent.IsSpam(500, false, false))
                                return;

                            Animations.Service.LastSent = World.Core.ServerTime;

                            Events.CallRemote("Players::Smoke::Stop");
                        }
                        else
                        {
                            DateTime lastSent = Player.LocalPlayer.GetData<DateTime>("Temp::Smoke::LastSent");

                            if (Player.LocalPlayer.Vehicle == null)
                            {
                                // alt - to hand
                                if (!UI.CEF.Cursor.IsVisible && Input.Core.IsJustDown(VirtualKeys.LeftMenu))
                                    if (!lastSent.IsSpam(1000, false, true) &&
                                        !PlayerActions.IsAnyActionActive(false,
                                            PlayerActions.Types.Animation,
                                            PlayerActions.Types.FastAnimation,
                                            PlayerActions.Types.OtherAnimation
                                        ))
                                    {
                                        Events.CallRemote("Players::Smoke::State");

                                        Player.LocalPlayer.SetData("Temp::Smoke::LastSent", World.Core.ServerTime);
                                    }

                                Graphics.DrawText(Locale.General.Animations.TextToHandSmoke,
                                    0.5f,
                                    0.925f,
                                    255,
                                    255,
                                    255,
                                    255,
                                    0.45f,
                                    RAGE.Game.Font.ChaletComprimeCologne,
                                    false,
                                    true
                                );
                            }

                            Graphics.DrawText(string.Format(Locale.General.Animations.CancelTextSmoke, bind.GetKeyString()),
                                0.5f,
                                0.95f,
                                255,
                                255,
                                255,
                                255,
                                0.45f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                false,
                                true
                            );
                        }
                    }
                ))
            },
            {
                AttachmentType.FarmPlantSmallShovel, (null, null, new Action(() =>
                    {
                        Input.Core.ExtraBind bind = Input.Core.Get(Input.Enums.BindTypes.CancelAnimation);

                        if (bind.IsJustPressed)
                        {
                            if (Animations.Service.LastSent.IsSpam(500, false, true))
                                return;

                            Animations.Service.LastSent = World.Core.ServerTime;

                            Events.CallRemote("Job::FARM::SCP");
                        }
                        else
                        {
                            Graphics.DrawText(string.Format(Locale.General.Animations.JustStopText, bind.GetKeyString()),
                                0.5f,
                                0.95f,
                                255,
                                255,
                                255,
                                255,
                                0.45f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                false,
                                true
                            );
                        }
                    }
                ))
            },
            {
                AttachmentType.FarmWateringCan, (null, null, new Action(() =>
                    {
                        Input.Core.ExtraBind bind = Input.Core.Get(Input.Enums.BindTypes.CancelAnimation);

                        if (bind.IsJustPressed)
                        {
                            if (Animations.Service.LastSent.IsSpam(500, false, false))
                                return;

                            Animations.Service.LastSent = World.Core.ServerTime;

                            Events.CallRemote("Job::FARM::SOTP");
                        }
                        else
                        {
                            Graphics.DrawText(string.Format(Locale.General.Animations.JustStopText, bind.GetKeyString()),
                                0.5f,
                                0.95f,
                                255,
                                255,
                                255,
                                255,
                                0.45f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                false,
                                true
                            );
                        }
                    }
                ))
            },
            {
                AttachmentType.FarmOrangeBoxCarry, (new Action(() =>
                    {
                        Farm farmBusiness = (PlayerData.GetData(Player.LocalPlayer)?.CurrentJob as Farmer)?.FarmBusiness;

                        if (farmBusiness == null || farmBusiness.OrangeTreeBoxPositions == null)
                            return;

                        var markers = new List<Marker>();

                        foreach (Tuple<Vector3, ExtraColshape> x in farmBusiness.OrangeTreeBoxPositions)
                        {
                            markers.Add(new Marker(2,
                                    new Vector3(x.Item2.Position.X, x.Item2.Position.Y, x.Item2.Position.Z + 1f),
                                    1f,
                                    Vector3.Zero,
                                    Vector3.Zero,
                                    new RGBA(0, 0, 255, 125),
                                    true,
                                    Settings.App.Static.MainDimension
                                )
                            );
                        }

                        Vector3 closestOrangeBoxPos = farmBusiness.OrangeTreeBoxPositions.Select(x => x.Item1)
                                                                  .OrderBy(x => x.DistanceTo(Player.LocalPlayer.Position))
                                                                  .FirstOrDefault();

                        Player.LocalPlayer.SetData("JOBATFARM::FOBC::B",
                            new ExtraBlip(478, closestOrangeBoxPos, "Коробки с апельсинами", 1f, 21, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension)
                        );
                        Player.LocalPlayer.SetData("JOBATFARM::FOBC::MS", markers);

                        Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), "Отнесите коробку с апельсинами в место, отмеченное на карте");
                    }
                ), new Action(() =>
                    {
                        Player.LocalPlayer.GetData<ExtraBlip>("JOBATFARM::FOBC::B")?.Destroy();

                        Player.LocalPlayer.ResetData("JOBATFARM::FOBC::B");

                        List<Marker> markers = Player.LocalPlayer.GetData<List<Marker>>("JOBATFARM::FOBC::MS");

                        if (markers != null)
                            foreach (Marker x in markers)
                            {
                                x?.Destroy();
                            }

                        Player.LocalPlayer.ResetData("JOBATFARM::FOBC::MS");
                    }
                ), new Action(() =>
                    {
                        if (PlayerActions.IsAnyActionActive(false,
                                PlayerActions.Types.Ragdoll,
                                PlayerActions.Types.Falling,
                                PlayerActions.Types.IsSwimming,
                                PlayerActions.Types.Climbing,
                                PlayerActions.Types.Crawl,
                                PlayerActions.Types.InVehicle,
                                PlayerActions.Types.Reloading,
                                PlayerActions.Types.Shooting,
                                PlayerActions.Types.MeleeCombat
                            ))
                        {
                            if (Animations.Service.LastSent.IsSpam(500, false, false))
                                return;

                            Animations.Service.LastSent = World.Core.ServerTime;

                            Notification.ShowError("Вы уронили коробку с апельсинами!");

                            Events.CallRemote("Job::FARM::SOTP");
                        }
                    }
                ))
            },
            {
                AttachmentType.FarmMilkBucketCarry, (new Action(() =>
                    {
                        Farm farmBusiness = (PlayerData.GetData(Player.LocalPlayer)?.CurrentJob as Farmer)?.FarmBusiness;

                        if (farmBusiness == null || farmBusiness.CowBucketPositions == null)
                            return;

                        var markers = new List<Marker>();

                        foreach (Tuple<Vector3, ExtraColshape> x in farmBusiness.CowBucketPositions)
                        {
                            markers.Add(new Marker(2,
                                    new Vector3(x.Item2.Position.X, x.Item2.Position.Y, x.Item2.Position.Z + 1f),
                                    1f,
                                    Vector3.Zero,
                                    Vector3.Zero,
                                    new RGBA(0, 0, 255, 125),
                                    true,
                                    Settings.App.Static.MainDimension
                                )
                            );
                        }

                        Vector3 closestOrangeBoxPos = farmBusiness.CowBucketPositions.Select(x => x.Item1).OrderBy(x => x.DistanceTo(Player.LocalPlayer.Position)).FirstOrDefault();


                        Player.LocalPlayer.SetData("JOBATFARM::FOBC::B",
                            new ExtraBlip(478, closestOrangeBoxPos, "Вёдра с молоком", 1f, 21, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension)
                        );
                        Player.LocalPlayer.SetData("JOBATFARM::FOBC::MS", markers);

                        Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), "Отнесите ведро с молоком в место, отмеченное на карте");
                    }
                ), new Action(() =>
                    {
                        Player.LocalPlayer.GetData<ExtraBlip>("JOBATFARM::FOBC::B")?.Destroy();

                        Player.LocalPlayer.ResetData("JOBATFARM::FOBC::B");

                        List<Marker> markers = Player.LocalPlayer.GetData<List<Marker>>("JOBATFARM::FOBC::MS");

                        if (markers != null)
                            foreach (Marker x in markers)
                            {
                                x?.Destroy();
                            }

                        Player.LocalPlayer.ResetData("JOBATFARM::FOBC::MS");
                    }
                ), new Action(() =>
                    {
                        if (PlayerActions.IsAnyActionActive(false,
                                PlayerActions.Types.Ragdoll,
                                PlayerActions.Types.Falling,
                                PlayerActions.Types.IsSwimming,
                                PlayerActions.Types.Climbing,
                                PlayerActions.Types.Crawl,
                                PlayerActions.Types.InVehicle,
                                PlayerActions.Types.Reloading,
                                PlayerActions.Types.Shooting,
                                PlayerActions.Types.MeleeCombat
                            ))
                        {
                            if (Animations.Service.LastSent.IsSpam(500, false, false))
                                return;

                            Animations.Service.LastSent = World.Core.ServerTime;

                            Notification.ShowError("Вы уронили ведро с молоком!");

                            Events.CallRemote("Job::FARM::SCOWP");
                        }
                    }
                ))
            },
            {
                AttachmentType.EmsHealingBedFakeAttach, (null, () =>
                {
                    ExtraColshape.All.Where(x => x.Name == "ems_healing_bed").ToList().ForEach(x => x.Destroy());
                }, new Action(() =>
                    {
                        Input.Core.ExtraBind bind = Input.Core.Get(Input.Enums.BindTypes.CancelAnimation);

                        Graphics.DrawText(string.Format("Нажмите {0}, чтобы встать с койки", bind.GetKeyString()),
                            0.5f,
                            0.95f,
                            255,
                            255,
                            255,
                            255,
                            0.45f,
                            RAGE.Game.Font.ChaletComprimeCologne,
                            true,
                            true
                        );

                        if (Utils.Misc.CanShowCEF(true, true))
                            if (bind.IsJustPressed)
                                if (!Animations.Service.LastSent.IsSpam(500, false, false))
                                {
                                    Animations.Service.LastSent = World.Core.ServerTime;

                                    Events.CallRemote("EMS::BedFree");
                                }
                    }
                ))
            },
            {
                AttachmentType.Cuffs, (() =>
                {
                    Management.Weapons.Core.DisabledFiring = true;

                    Input.Core.Get(Input.Enums.BindTypes.Crawl)?.Disable();

                    Player.LocalPlayer.SetEnableHandcuffs(true);

                    Management.Interaction.Enabled = false;
                }, () =>
                {
                    Management.Weapons.Core.DisabledFiring = false;

                    Input.Core.Get(Input.Enums.BindTypes.Crawl)?.Enable();

                    Player.LocalPlayer.SetEnableHandcuffs(false);

                    Management.Interaction.Enabled = true;

                    if (LockPicking.CurrentContext == "POLICE_CUFFS_LOCKPICK")
                        LockPicking.Close();
                }, () =>
                {
                    if (Player.LocalPlayer.IsInAnyVehicle(false))
                        Main.DisableMoveRender();

                    decimal lockpickItemAmount = LockPicking.GetLockpickTotalAmount();

                    if (lockpickItemAmount > 0)
                    {
                        VirtualKeys key = VirtualKeys.Return;

                        if (Utils.Misc.CanShowCEF(true, true) && !Utils.Misc.IsAnyCefActive())
                        {
                            Graphics.DrawText(string.Format("Нажмите {0}, чтобы воспользоваться отмычкой (x{1})", Input.Core.GetKeyString(key), lockpickItemAmount),
                                0.5f,
                                0.95f,
                                255,
                                255,
                                255,
                                255,
                                0.45f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                true,
                                true
                            );

                            if (Input.Core.IsJustDown(key))
                                if (!Animations.Service.LastSent.IsSpam(500, false, false))
                                {
                                    Animations.Service.LastSent = World.Core.ServerTime;

                                    LockPicking.Show("POLICE_CUFFS_LOCKPICK",
                                        LockPicking.DurabilityDefault,
                                        LockPicking.GetLockpickingRandomTargetRotation(),
                                        LockPicking.MaxDeviationDefault,
                                        LockPicking.RotationDefault
                                    );
                                }
                        }
                    }
                    else
                    {
                        if (LockPicking.CurrentContext == "POLICE_CUFFS_LOCKPICK")
                        {
                            Notification.Show("Inventory::NoItem");

                            LockPicking.Close();
                        }
                    }
                })
            },
            {
                AttachmentType.PoliceEscort, (() =>
                {
                    Management.Weapons.Core.DisabledFiring = true;

                    Input.Core.Get(Input.Enums.BindTypes.Crouch)?.Disable();
                    Input.Core.Get(Input.Enums.BindTypes.Crawl)?.Disable();
                }, () =>
                {
                    Management.Weapons.Core.DisabledFiring = false;

                    Input.Core.Get(Input.Enums.BindTypes.Crouch)?.Enable();
                    Input.Core.Get(Input.Enums.BindTypes.Crawl)?.Enable();
                }, async () =>
                {
                    RAGE.Game.Pad.DisableControlAction(32, 21, true);
                    RAGE.Game.Pad.DisableControlAction(32, 22, true);

                    Input.Core.ExtraBind bind = Input.Core.Get(Input.Enums.BindTypes.CancelAnimation);

                    Graphics.DrawText(string.Format("Нажмите {0}, чтобы прекратить вести человека", bind.GetKeyString()),
                        0.5f,
                        0.95f,
                        255,
                        255,
                        255,
                        255,
                        0.45f,
                        RAGE.Game.Font.ChaletComprimeCologne,
                        true,
                        true
                    );

                    if (Utils.Misc.CanShowCEF(true, true))
                        if (bind.IsJustPressed)
                            if (!Animations.Service.LastSent.IsSpam(500, false, false))
                            {
                                Animations.Service.LastSent = World.Core.ServerTime;

                                await Events.CallRemoteProc("Police::Escort", null, false);
                            }
                })
            },
            {
                AttachmentType.PlayerResurrect, (() =>
                {
                    Management.Weapons.Core.DisabledFiring = true;

                    Input.Core.Get(Input.Enums.BindTypes.Crouch)?.Disable();
                    Input.Core.Get(Input.Enums.BindTypes.Crawl)?.Disable();

                    var taskKey = "ATTACH_PLAYER_RESURRECT_TASK";

                    AsyncTask.Methods.CancelPendingTask(taskKey);

                    AttachmentEntity attach = PlayerData.GetData(Player.LocalPlayer).AttachedEntities.Where(x => x.Type == AttachmentType.PlayerResurrect).FirstOrDefault();

                    if (attach == null)
                        return;

                    GameEntity targetEntity = Utils.Game.Misc.GetGameEntityAtRemoteId(attach.EntityType, attach.RemoteID);

                    if (targetEntity?.Exists == true)
                    {
                        Vector3 targetPos = RAGE.Game.Entity.GetEntityCoords(targetEntity.Handle, false);

                        Vector3 pos = Management.Camera.Service.GetFrontOf(RAGE.Game.Entity.GetEntityCoords(targetEntity.Handle, false), 90f, 0.5f);

                        if (Player.LocalPlayer.GetCoords(false).DistanceTo(pos) < 10f)
                            Player.LocalPlayer.SetCoordsNoOffset(pos.X, pos.Y, pos.Z, false, false, false);

                        Player.LocalPlayer.SetHeading(Geometry.GetRotationZToFacePointTo(pos, targetPos));
                    }

                    AsyncTask task = null;

                    task = new AsyncTask(async () =>
                        {
                            await RAGE.Game.Invoker.WaitAsync(int.Parse(attach.SyncData.Split('_')[0]));

                            if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                return;

                            Events.CallRemote("Player::ResurrectFinish");

                            AsyncTask.Methods.CancelPendingTask(taskKey);
                        },
                        0,
                        false,
                        0
                    );

                    AsyncTask.Methods.SetAsPending(task, taskKey);
                }, () =>
                {
                    Management.Weapons.Core.DisabledFiring = false;

                    Input.Core.Get(Input.Enums.BindTypes.Crouch)?.Enable();
                    Input.Core.Get(Input.Enums.BindTypes.Crawl)?.Enable();

                    AsyncTask.Methods.CancelPendingTask("ATTACH_PLAYER_RESURRECT_TASK");
                }, async () =>
                {
                })
            },
        };

        public Service()
        {
            Player.LocalPlayer.SetData(AttachedObjectsKey, new List<AttachmentObject>());
            Player.LocalPlayer.SetData(AttachedEntitiesKey, new List<AttachmentEntity>());

            Events.AddDataHandler(AttachedEntitiesKey,
                async (entity, value, oldValue) =>
                {
                    if (entity.IsLocal)
                        return;

                    if (!entity.HasData(AttachedEntitiesKey))
                        return;

                    List<AttachmentEntityNet> currentListEntitiesNet =
                        ((Newtonsoft.Json.Linq.JArray)oldValue)?.ToObject<List<AttachmentEntityNet>>() ?? new List<AttachmentEntityNet>();
                    List<AttachmentEntityNet> newListEntitiesNet = ((Newtonsoft.Json.Linq.JArray)value)?.ToObject<List<AttachmentEntityNet>>() ?? new List<AttachmentEntityNet>();

                    foreach (AttachmentEntityNet x in currentListEntitiesNet.Union(newListEntitiesNet))
                    {
                        if (newListEntitiesNet.Contains(x))
                        {
                            if (currentListEntitiesNet.Contains(x))
                                continue;
                            else
                                await AttachEntity(entity, x);
                        }
                        else
                        {
                            DetachEntity(entity, x.Type, x.Id, x.EntityType);
                        }
                    }
                }
            );

            Events.AddDataHandler(AttachedObjectsKey,
                async (entity, value, oldValue) =>
                {
                    if (entity.IsLocal)
                        return;

                    if (!entity.HasData(AttachedObjectsKey))
                        return;

                    List<AttachmentObjectNet> currentListObjectsNet =
                        ((Newtonsoft.Json.Linq.JArray)oldValue)?.ToObject<List<AttachmentObjectNet>>() ?? new List<AttachmentObjectNet>();
                    List<AttachmentObjectNet> newListObjectsNet = ((Newtonsoft.Json.Linq.JArray)value)?.ToObject<List<AttachmentObjectNet>>() ?? new List<AttachmentObjectNet>();

                    foreach (AttachmentObjectNet x in currentListObjectsNet.Union(newListObjectsNet))
                    {
                        if (newListObjectsNet.Contains(x))
                        {
                            if (currentListObjectsNet.Contains(x))
                                continue;
                            else
                                await AttachObject(entity, x);
                        }
                        else
                        {
                            DetachObject(entity, x.Type);
                        }
                    }
                }
            );
        }

        /*
            mp.game.streaming.requestNamedPtfxAsset('core');
            mp.game.graphics.setPtfxAssetNextCall('core');
            mp.game.graphics.startParticleFxLoopedOnEntity('water_cannon_spray', 2212114, 0.25, 0, 0, 0, 0, 0, 0.05, false, false, false);


            mp.game.graphics.removeParticleFxInRange(0, 0, 0, 1000000);
         */

        private static Dictionary<int, List<int>> StreamedAttachments { get; set; } = new Dictionary<int, List<int>>();

        public static bool IsTypeStaticObject(AttachmentType type)
        {
            return type >= AttachmentType.PedRingLeft3 && type < AttachmentType.VehKey;
        }

        public static bool IsTypeObjectInHand(AttachmentType type)
        {
            return type >= AttachmentType.VehKey && type < AttachmentType.VehicleTrailer;
        }

        public static void AddLocalAttachment(int fromHandle, int toHandle)
        {
            List<int> list = StreamedAttachments.GetValueOrDefault(toHandle);

            if (list == null)
            {
                list = new List<int>()
                {
                    fromHandle,
                };

                StreamedAttachments.Add(toHandle, list);
            }
            else
            {
                if (!list.Contains(fromHandle))
                    list.Add(fromHandle);
            }

            //Utils.ConsoleOutput($"Attached {fromHandle} to {toHandle}");
        }

        public static void RemoveLocalAttachment(int fromHandle, int toHandle)
        {
            List<int> list = StreamedAttachments.GetValueOrDefault(toHandle);

            if (list == null)
                return;

            if (list.Remove(fromHandle) && list.Count == 0)
                StreamedAttachments.Remove(toHandle);

            //Utils.ConsoleOutput($"Detached {fromHandle} from {toHandle}");
        }

        public static void DetachAllFromLocalEntity(int toHandle)
        {
            RAGE.Game.Entity.DetachEntity(toHandle, true, false);

            List<int> list = StreamedAttachments.GetValueOrDefault(toHandle);

            if (list == null)
                return;

            list.ForEach(x =>
                {
                    RAGE.Game.Entity.DetachEntity(x, true, false);
                }
            );

            StreamedAttachments.Remove(toHandle);
        }

        public static async System.Threading.Tasks.Task OnEntityStreamIn(Entity entity)
        {
            if (entity.IsLocal)
                return;

            List<AttachmentObjectNet> listObjectsNet = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey, null)?.ToObject<List<AttachmentObjectNet>>();

            if (listObjectsNet != null)
            {
                entity.SetData(AttachedObjectsKey, new List<AttachmentObject>());

                foreach (AttachmentObjectNet x in listObjectsNet)
                {
                    await AttachObject(entity, x);
                }
            }

            List<AttachmentEntityNet> listEntitiesNet = entity.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedEntitiesKey, null)?.ToObject<List<AttachmentEntityNet>>();

            if (listEntitiesNet != null)
            {
                entity.SetData(AttachedEntitiesKey, new List<AttachmentEntity>());

                foreach (AttachmentEntityNet x in listEntitiesNet)
                {
                    await AttachEntity(entity, x);
                }
            }
        }

        public static async System.Threading.Tasks.Task OnEntityStreamOut(Entity entity)
        {
            var gEntity = entity as GameEntity;

            if (entity is Vehicle veh)
            {
                veh.DetachFromTrailer();
                veh.DetachFromAnyTowTruck();
            }

            if (entity.IsLocal)
            {
                if (gEntity != null)
                    DetachAllFromLocalEntity(gEntity.Handle);

                return;
            }
            else
            {
                if (gEntity != null)
                    RAGE.Game.Entity.DetachEntity(gEntity.Handle, true, false);

                /*                var pData = Sync.Players.GetData(Player.LocalPlayer);

                                if (pData != null)
                                {
                                    if (pData.IsAttachedTo == entity)
                                    {
                                        AsyncTask.RunSlim(() =>
                                        {
                                            Events.CallRemote("atsdme");
                                        }, 2500);
                                    }
                                }*/
            }

            List<AttachmentObject> listObjects = entity.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (listObjects == null)
                return;

            foreach (AttachmentObject obj in listObjects.ToList())
            {
                DetachObject(entity, obj.Type);
            }

            listObjects.Clear();
            entity.ResetData(AttachedObjectsKey);

            List<AttachmentEntity> listEntities = entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);

            if (listEntities == null)
                return;

            foreach (AttachmentEntity obj in listEntities.ToList())
            {
                DetachEntity(entity, obj.Type, obj.RemoteID, obj.EntityType);
            }

            listEntities.Clear();
            entity.ResetData(AttachedEntitiesKey);
        }

        public static async System.Threading.Tasks.Task AttachEntity(Entity entity, AttachmentEntityNet attachmentNet)
        {
            GameEntity gTarget = null;
            var gEntity = (GameEntity)entity;

            if (gEntity == null)
                return;

            List<AttachmentEntity> list = entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);

            if (list == null)
                return;

            var aObj = new AttachmentEntity(attachmentNet);

            list.Add(aObj);

            while (true)
            {
                if (gEntity?.Exists != true || entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey)?.Contains(aObj) != true)
                    return;

                gTarget = Utils.Game.Misc.GetGameEntityAtRemoteId(attachmentNet.EntityType, attachmentNet.Id);

                if (gTarget?.Exists != true)
                {
                    await RAGE.Game.Invoker.WaitAsync(15);

                    continue;
                }

                break;
            }

            Vector3 positionBase = Vector3.Zero;

            AttachmentData props = Attachments.GetValueOrDefault(attachmentNet.Type);

            if (entity is Vehicle veh)
            {
                if (attachmentNet.Type == AttachmentType.VehicleTrailer)
                {
                }
                else
                {
                    entity.GetModelDimensions(out Vector3 min, out Vector3 max);

                    if (attachmentNet.Type == AttachmentType.PushVehicle)
                    {
                        if (attachmentNet.SyncData == "1")
                        {
                            positionBase.Y = max.Y;
                            //positionBase.Z = min.Z;

                            props.PositionOffset.Y = 0.35f;
                            props.Rotation.Z = 180f;
                        }
                        else
                        {
                            positionBase.Y = min.Y;
                            //positionBase.Z = max.Z;

                            props.PositionOffset.Y = -0.6f;
                            props.Rotation.Z = 0f;
                        }
                    }
                    else if (attachmentNet.Type == AttachmentType.VehicleTrunk)
                    {
                        positionBase.Y = -(max.Y - min.Y) / 2f;
                    }
                }
            }

            if (props != null)
            {
                void attachMethod() => RAGE.Game.Entity.AttachEntityToEntity(gTarget.Handle,
                    gEntity.Handle,
                    props.BoneID >= 1_000_000 ? props.BoneID - 1_000_000 : RAGE.Game.Ped.GetPedBoneIndex(gEntity.Handle, props.BoneID),
                    positionBase.X + props.PositionOffset.X,
                    positionBase.Y + props.PositionOffset.Y,
                    positionBase.Z + props.PositionOffset.Z,
                    props.Rotation.X,
                    props.Rotation.Y,
                    props.Rotation.Z,
                    false,
                    props.UseSoftPinning,
                    props.Collision,
                    props.IsPed,
                    props.RotationOrder,
                    props.FixedRot
                );

                gTarget.SetData<Action>("AttachMethod", attachMethod);

                attachMethod();
            }

            if (attachmentNet.Type == AttachmentType.VehicleTrailer)
            {
                RAGE.Game.Vehicle.AttachVehicleToTrailer(gEntity.Handle, gTarget.Handle, float.MaxValue);
            }
            else if (attachmentNet.Type == AttachmentType.VehicleTrailerObjBoat)
            {
                GameEntity trailerObj = gEntity.GetData<List<AttachmentObject>>(AttachedObjectsKey)
                                              ?.Where(x => x.Type == AttachmentType.TrailerObjOnBoat)
                                               .FirstOrDefault()
                                              ?.Object;

                if (trailerObj != null)
                    RAGE.Game.Vehicle.AttachVehicleToTrailer(gTarget.Handle, trailerObj.Handle, float.MaxValue);
            }

            aObj.WasAttached = true;

            if (gTarget == Player.LocalPlayer)
            {
                TargetAction(attachmentNet.Type, entity, true);

                if (props != null && (props.DisableInteraction == 1 || props.DisableInteraction == 2))
                    Management.Interaction.SetEntityAsDisabled(gEntity, true);
            }
            else if (gEntity == Player.LocalPlayer)
            {
                RootAction(attachmentNet.Type, gTarget, true);

                if (props != null && (props.DisableInteraction == 1 || props.DisableInteraction == 3))
                    Management.Interaction.SetEntityAsDisabled(gTarget, true);
            }
        }

        public static void DetachEntity(Entity entity, AttachmentType type, ushort remoteId, RAGE.Elements.Type eType)
        {
            var gEntity = (GameEntity)entity;

            if (gEntity == null)
                return;

            List<AttachmentEntity> list = entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);

            if (list == null)
                return;

            AttachmentEntity aObj = list.Where(x => x.RemoteID == remoteId && x.EntityType == eType && x.Type == type).FirstOrDefault();

            if (aObj == null)
                return;

            list.Remove(aObj);

            GameEntity gTarget = Utils.Game.Misc.GetGameEntityAtRemoteId(eType, remoteId);

            if (gTarget?.Exists == true)
            {
                AttachmentData props = Attachments.GetValueOrDefault(aObj.Type);

                if (props != null)
                {
                    RAGE.Game.Entity.DetachEntity(gTarget.Handle, true, props.Collision);

                    gTarget.ResetData("AttachMethod");
                }

                if (aObj.Type == AttachmentType.VehicleTrailer || aObj.Type == AttachmentType.VehicleTrailerObjBoat)
                    RAGE.Game.Vehicle.DetachVehicleFromTrailer(gTarget.Handle);
            }

            if (aObj.WasAttached)
            {
                if (gTarget == Player.LocalPlayer)
                {
                    TargetAction(aObj.Type, null, false);

                    Management.Interaction.SetEntityAsDisabled(gEntity, false);
                }
                else if (gEntity == Player.LocalPlayer)
                {
                    RootAction(aObj.Type, null, false);

                    Management.Interaction.SetEntityAsDisabled(gTarget, false);
                }
            }
        }

        public static async System.Threading.Tasks.Task AttachObject(Entity target, AttachmentObjectNet attachmentNet)
        {
            bool res = await Streaming.RequestModel(attachmentNet.Model);

            var gTarget = (GameEntity)target;

            if (gTarget == null)
                return;

            if (gTarget.Handle == Player.LocalPlayer.Handle)
                if (attachmentNet.Type == AttachmentType.PhoneSync || attachmentNet.Type == AttachmentType.ParachuteSync)
                    return;

            List<AttachmentObject> list = target.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (list == null)
                return;

            GameEntity gEntity = null;

            Vector3 positionBase = Vector3.Zero;

            if (attachmentNet.Type >= AttachmentType.WeaponRightTight && attachmentNet.Type <= AttachmentType.WeaponLeftBack)
            {
                await Streaming.RequestWeaponAsset(attachmentNet.Model);

                gEntity = new MapObject(RAGE.Game.Weapon.CreateWeaponObject(attachmentNet.Model, 0, target.Position.X, target.Position.Y, target.Position.Z, true, 0f, 0, 0, 0));

                if (attachmentNet.SyncData != null)
                    Management.Weapons.Core.UpdateWeaponObjectComponents(gEntity.Handle, attachmentNet.Model, attachmentNet.SyncData);
            }
            else if (attachmentNet.Type == AttachmentType.TrailerObjOnBoat)
            {
                Vector3 rot = RAGE.Game.Entity.GetEntityRotation(gTarget.Handle, 2);

                Vector3 pos = target.Position;

                RAGE.Game.Entity.SetEntityCoordsNoOffset(gTarget.Handle, pos.X, pos.Y, pos.Z + 5f, false, false, false);

                var veh = new Vehicle(attachmentNet.Model, pos, rot.Z, "", 255, true, 0, 0, target.Dimension);

                var targetVeh = target as Vehicle;

                var targetData = VehicleData.GetData(targetVeh);

                if (targetData != null)
                    if (targetData.Data.ID.StartsWith("seashark"))
                        positionBase.Z -= 0.5f;
                veh.StreamInCustomActionsAdd((entity) =>
                    {
                        var eVeh = entity as Vehicle;

                        eVeh.SetCanBeDamaged(false);
                        eVeh.SetCanBeVisiblyDamaged(false);
                        eVeh.SetCanBreak(false);
                        eVeh.SetDirtLevel(0f);
                        eVeh.SetDisablePetrolTankDamage(true);
                        eVeh.SetDisablePetrolTankFires(true);
                        eVeh.SetInvincible(true);

                        if (VehicleData.GetData(targetVeh)?.IsFrozen == true)
                            eVeh.FreezePosition(true);
                    }
                );

                gEntity = veh;

                gEntity.SetData("TrailerSync::Owner", targetVeh);
            }
            else if (attachmentNet.Type == AttachmentType.TractorTrailFarmHarv)
            {
                var veh = new Vehicle(attachmentNet.Model, target.Position, 0f, "", 255, true, 0, 0, target.Dimension);
                veh.StreamInCustomActionsAdd((entity) =>
                    {
                        var eVeh = entity as Vehicle;

                        eVeh.SetAutomaticallyAttaches(0, 0);

                        eVeh.SetCanBeDamaged(false);
                        eVeh.SetCanBeVisiblyDamaged(false);
                        eVeh.SetCanBreak(false);
                        eVeh.SetDirtLevel(0f);
                        eVeh.SetDisablePetrolTankDamage(true);
                        eVeh.SetDisablePetrolTankFires(true);
                        eVeh.SetInvincible(true);
                    }
                );

                gEntity = veh;
            }
            else
            {
                if (res)
                    gEntity = Streaming.CreateObjectNoOffsetImmediately(attachmentNet.Model, target.Position.X, target.Position.Y, target.Position.Z);
            }

            AttachmentData props = ModelDependentAttachments.GetValueOrDefault(attachmentNet.Model)?.GetValueOrDefault(attachmentNet.Type) ??
                                   Attachments.GetValueOrDefault(attachmentNet.Type);

            list.Add(new AttachmentObject(gEntity, attachmentNet));

            target.SetData(AttachedObjectsKey, list);

            if (props != null)
            {
                if (props.BoneID == int.MinValue)
                {
                    if (attachmentNet.Type == AttachmentType.ItemFishG)
                        if (gEntity != null)
                        {
                            string[] pos = attachmentNet.SyncData.Split('&');

                            RAGE.Game.Entity.SetEntityCoordsNoOffset(gEntity.Handle, float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]), false, false, false);

                            RAGE.Game.Entity.SetEntityVisible(gEntity.Handle, false, true);
                        }
                }
                else
                {
                    if (gEntity != null)
                    {
                        if (attachmentNet.Type == AttachmentType.TrailerObjOnBoat)
                        {
                            AddLocalAttachment(gTarget.Handle, gEntity.Handle);

                            RAGE.Game.Entity.AttachEntityToEntity(gTarget.Handle,
                                gEntity.Handle,
                                props.BoneID >= 1_000_000 ? props.BoneID - 1_000_000 : RAGE.Game.Ped.GetPedBoneIndex(gTarget.Handle, props.BoneID),
                                positionBase.X + props.PositionOffset.X,
                                positionBase.Y + props.PositionOffset.Y,
                                positionBase.Z + props.PositionOffset.Z,
                                props.Rotation.X,
                                props.Rotation.Y,
                                props.Rotation.Z,
                                false,
                                props.UseSoftPinning,
                                props.Collision,
                                props.IsPed,
                                props.RotationOrder,
                                props.FixedRot
                            );
                        }
                        else
                        {
                            RAGE.Game.Entity.AttachEntityToEntity(gEntity.Handle,
                                gTarget.Handle,
                                props.BoneID >= 1_000_000 ? props.BoneID - 1_000_000 : RAGE.Game.Ped.GetPedBoneIndex(gTarget.Handle, props.BoneID),
                                positionBase.X + props.PositionOffset.X,
                                positionBase.Y + props.PositionOffset.Y,
                                positionBase.Z + props.PositionOffset.Z,
                                props.Rotation.X,
                                props.Rotation.Y,
                                props.Rotation.Z,
                                false,
                                props.UseSoftPinning,
                                props.Collision,
                                props.IsPed,
                                props.RotationOrder,
                                props.FixedRot
                            );
                        }
                    }
                }

                if (gEntity != null)
                    props.EntityAction?.Invoke(new object[]
                        {
                            gEntity,
                        }
                    );
            }

            if (gTarget is Player tPlayer && tPlayer.Handle == Player.LocalPlayer.Handle)
                RootAction(attachmentNet.Type, gEntity, true);
        }

        public static void DetachObject(Entity target, AttachmentType type)
        {
            List<AttachmentObject> list = target.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (list == null)
                return;

            AttachmentObject item = list.Where(x => x.Type == type).FirstOrDefault();

            if (item == null)
                return;

            GameEntity gEntity = item.Object;
            var gTarget = target as GameEntity;

            if (gTarget == null)
                return;

            AttachmentData props = ModelDependentAttachments.GetValueOrDefault(item.Model)?.GetValueOrDefault(type) ?? Attachments.GetValueOrDefault(type);

            if (gEntity != null)
            {
                if (gEntity.HasData("PtfxHandle"))
                    RAGE.Game.Graphics.StopParticleFxLooped(gEntity.GetData<int>("PtfxHandle"), false);

                if (type == AttachmentType.TrailerObjOnBoat)
                {
                    RemoveLocalAttachment(gTarget.Handle, gEntity.Handle);

                    RAGE.Game.Entity.DetachEntity(gTarget.Handle, true, props?.Collision ?? false);

                    gEntity.Destroy();
                }
                else
                {
                    if (gEntity.IsLocal)
                    {
                        RAGE.Game.Entity.DetachEntity(gEntity.Handle, true, props?.Collision ?? false);

                        RAGE.Game.Entity.SetEntityAsMissionEntity(gEntity.Handle, false, false);

                        gEntity.Destroy();
                    }
                    else
                    {
                        RAGE.Game.Entity.DetachEntity(gEntity.Handle, true, props?.Collision ?? false);
                    }
                }
            }

            list.Remove(item);

            target.SetData(AttachedObjectsKey, list);

            if (gTarget is Player tPlayer && tPlayer.Handle == Player.LocalPlayer.Handle)
                RootAction(item.Type, null, false);
        }

        public static void ReattachObjects(Entity target)
        {
            List<AttachmentObject> list = target.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (list == null)
                return;

            foreach (AttachmentObject x in list.ToList())
            {
                if (x == null)
                    continue;

                DetachObject(target, x.Type);
            }

            AttachAllObjects(target);
        }

        public static void DetachAllObjects(Entity target)
        {
            List<AttachmentObject> list = target.GetData<List<AttachmentObject>>(AttachedObjectsKey);

            if (list == null)
                return;

            foreach (AttachmentObject x in list.ToList())
            {
                if (x == null)
                    continue;

                DetachObject(target, x.Type);
            }
        }

        public static async void AttachAllObjects(Entity target)
        {
            List<AttachmentObjectNet> listObjectsNet = target.GetSharedData<Newtonsoft.Json.Linq.JArray>(AttachedObjectsKey, null)?.ToObject<List<AttachmentObjectNet>>();

            if (listObjectsNet == null)
                return;

            foreach (AttachmentObjectNet x in listObjectsNet)
            {
                await AttachObject(target, x);
            }
        }

        public static List<AttachmentObject> GetEntityObjectAttachments(Entity entity)
        {
            return entity.GetData<List<AttachmentObject>>(AttachedObjectsKey);
        }

        public static List<AttachmentEntity> GetEntityEntityAttachments(Entity entity)
        {
            return entity.GetData<List<AttachmentEntity>>(AttachedEntitiesKey);
        }

        private static (Action On, Action Off, Action Loop)? GetTargetActions(AttachmentType type)
        {
            (Action On, Action Off, Action Loop)? action = TargetActions.GetValueOrDefault(type);

            if (action == null)
            {
                AttachmentType sType;

                if (SameActionsTypes.TryGetValue(type, out sType))
                    return TargetActions.GetValueOrDefault(sType);

                return null;
            }

            return action;
        }

        private static (Action On, Action Off, Action Loop)? GetRootActions(AttachmentType type)
        {
            (Action On, Action Off, Action Loop)? action = RootActions.GetValueOrDefault(type);

            if (action == null)
            {
                AttachmentType sType;

                if (SameActionsTypes.TryGetValue(type, out sType))
                    return RootActions.GetValueOrDefault(sType);

                return null;
            }

            return action;
        }

        public static void TargetAction(AttachmentType type, Entity root, bool attach)
        {
            var data = PlayerData.GetData(Player.LocalPlayer);

            if (data == null)
                return;

            data.IsAttachedTo = root;

            (Action On, Action Off, Action Loop)? actions = GetTargetActions(type);

            if (actions == null)
                return;

            if (attach)
            {
                data.IsAttachedTo = root;

                actions.Value.On?.Invoke();

                if (actions.Value.Loop != null)
                {
                    Main.Update -= actions.Value.Loop.Invoke;
                    Main.Update += actions.Value.Loop.Invoke;
                }
            }
            else
            {
                data.IsAttachedTo = null;

                if (actions.Value.Loop != null)
                    Main.Update -= actions.Value.Loop.Invoke;

                actions.Value.Off?.Invoke();
            }
        }

        public static void RootAction(AttachmentType type, Entity target, bool attach)
        {
            var data = PlayerData.GetData(Player.LocalPlayer);

            if (data == null)
                return;

            (Action On, Action Off, Action Loop)? actions = GetRootActions(type);

            if (actions == null)
                return;

            if (attach)
            {
                actions.Value.On?.Invoke();

                if (actions.Value.Loop != null)
                {
                    Main.Update -= actions.Value.Loop.Invoke;
                    Main.Update += actions.Value.Loop.Invoke;
                }
            }
            else
            {
                if (actions.Value.Loop != null)
                    Main.Update -= actions.Value.Loop.Invoke;

                actions.Value.Off?.Invoke();
            }
        }

        public static async System.Threading.Tasks.Task<GameEntity> AttachObjectSimpleLocal(uint hash, Entity target, AttachmentType type)
        {
            var gTarget = (GameEntity)target;

            if (gTarget == null)
                return null;

            GameEntity gEntity = null;

            Vector3 positionBase = Vector3.Zero;

            if (type >= AttachmentType.WeaponRightTight && type <= AttachmentType.WeaponLeftBack)
            {
                await Streaming.RequestWeaponAsset(hash);

                gEntity = new MapObject(RAGE.Game.Weapon.CreateWeaponObject(hash, 0, target.Position.X, target.Position.Y, target.Position.Z, true, 0f, 0, 0, 0));
            }
            else
            {
                gEntity = Streaming.CreateObjectNoOffsetImmediately(hash, target.Position.X, target.Position.Y, target.Position.Z);
            }

            if (gEntity == null)
                return null;

            AttachmentData props = ModelDependentAttachments.GetValueOrDefault(hash)?.GetValueOrDefault(type) ?? Attachments.GetValueOrDefault(type);

            if (props != null)
            {
                RAGE.Game.Entity.AttachEntityToEntity(gEntity.Handle,
                    gTarget.Handle,
                    RAGE.Game.Ped.GetPedBoneIndex(gTarget.Handle, props.BoneID),
                    positionBase.X + props.PositionOffset.X,
                    positionBase.Y + props.PositionOffset.Y,
                    positionBase.Z + props.PositionOffset.Z,
                    props.Rotation.X,
                    props.Rotation.Y,
                    props.Rotation.Z,
                    false,
                    props.UseSoftPinning,
                    props.Collision,
                    props.IsPed,
                    props.RotationOrder,
                    props.FixedRot
                );

                props.EntityAction?.Invoke(new object[]
                    {
                        gEntity,
                    }
                );
            }

            return gEntity;
        }
    }
}