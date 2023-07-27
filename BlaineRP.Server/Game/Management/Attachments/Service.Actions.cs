using System;
using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Management.Animations;
using BlaineRP.Server.Sync;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.Attachments
{
    public partial class Service
    {
        private static Dictionary<AttachmentType, AttachmentType> SameActionsTypes = new Dictionary<AttachmentType, AttachmentType>()
        {
            { AttachmentType.ItemCig1Hand, AttachmentType.ItemCigHand },
            { AttachmentType.ItemCig2Hand, AttachmentType.ItemCigHand },
            { AttachmentType.ItemCig3Hand, AttachmentType.ItemCigHand },

            { AttachmentType.ItemCig1Mouth, AttachmentType.ItemCigMouth },
            { AttachmentType.ItemCig2Mouth, AttachmentType.ItemCigMouth },
            { AttachmentType.ItemCig3Mouth, AttachmentType.ItemCigMouth },
        };

        private static Dictionary<AttachmentType, Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>> Actions =
            new Dictionary<AttachmentType, Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>>()
            {
                {
                    AttachmentType.TrailerObjOnBoat, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Vehicle veh)
                                {
                                    var attachedTo = veh.GetEntityIsAttachedTo();

                                    if (attachedTo != null)
                                        attachedTo.DetachEntity(veh);
                                }
                            }
                        }
                    }
                },

                {
                    AttachmentType.PushVehicle, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (target is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    if (root is Vehicle veh)
                                    {
                                        var vData = veh.GetMainData();

                                        if (vData == null)
                                            return;

                                        var baseSpeed = Properties.Settings.Static.PUSHING_VEHICLE_STRENGTH_MIN;
                                        var strengthCoef = pData.Info.Skills[SkillTypes.Strength] /
                                                           (float)Properties.Settings.Static.PlayerMaxSkills.GetValueOrDefault(SkillTypes.Strength);

                                        if (syncData == "1")
                                        {
                                            baseSpeed = -baseSpeed;
                                            strengthCoef = -strengthCoef;
                                        }

                                        vData.ForcedSpeed = baseSpeed +
                                                            strengthCoef *
                                                            (Properties.Settings.Static.PUSHING_VEHICLE_STRENGTH_MAX - Properties.Settings.Static.PUSHING_VEHICLE_STRENGTH_MIN);

                                        pData.PlayAnim(GeneralType.PushingVehicle);
                                    }
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (target is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData != null)
                                    {
                                        if (pData.GeneralAnim == GeneralType.PushingVehicle)
                                            pData.StopGeneralAnim();
                                    }
                                }

                                if (root is Vehicle veh)
                                {
                                    var vData = veh.GetMainData();

                                    if (vData != null)
                                    {
                                        vData.ForcedSpeed = 0f;
                                    }
                                }
                            }
                        }
                    }
                },

                {
                    AttachmentType.VehicleTrunk, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (target is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    pData.PlayAnim(GeneralType.LieInTrunk);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (target is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    if (pData.GeneralAnim == GeneralType.LieInTrunk)
                                    {
                                        if (pData.IsCuffed)
                                        {
                                            pData.PlayAnim(GeneralType.CuffedStatic0);
                                        }
                                        else
                                        {
                                            pData.StopGeneralAnim();
                                        }
                                    }
                                }
                            }
                        }
                    }
                },

                {
                    AttachmentType.Carry, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (target is Player tPlayer)
                                {
                                    if (root is Player player)
                                    {
                                        var pData = player.GetMainData();

                                        if (pData == null)
                                            return;

                                        var tData = tPlayer.GetMainData();

                                        if (tData == null)
                                            return;

                                        pData.PlayAnim(GeneralType.CarryA);
                                        tData.PlayAnim(GeneralType.CarryB);
                                    }
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData != null)
                                    {
                                        if (pData.GeneralAnim == GeneralType.CarryA)
                                            pData.StopGeneralAnim();
                                    }
                                }

                                if (target is Player tPlayer)
                                {
                                    var tData = tPlayer.GetMainData();

                                    if (tData != null)
                                    {
                                        if (tData.GeneralAnim == GeneralType.CarryB)
                                        {
                                            if (tData.IsCuffed)
                                            {
                                                tData.PlayAnim(GeneralType.CuffedStatic0);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },

                {
                    AttachmentType.ItemCigMouth, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (args.Length == 0)
                                    return;

                                int maxTime = (int)args[0];
                                int maxPuffs = (int)args[1];

                                if (root is Player player)
                                {
                                    player.TriggerEvent("Player::Smoke::Start", maxTime, maxPuffs);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (args.Length == 0)
                                {
                                    if (root is Player player)
                                    {
                                        player.TriggerEvent("Player::Smoke::Stop");
                                    }
                                }
                            }
                        }
                    }
                },

                {
                    AttachmentType.ItemCigHand, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (args.Length == 0)
                                    return;

                                int maxTime = (int)args[0];
                                int maxPuffs = (int)args[1];

                                if (root is Player player)
                                {
                                    player.TriggerEvent("Player::Smoke::Start", maxTime, maxPuffs);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (args.Length == 0)
                                {
                                    if (root is Player player)
                                    {
                                        player.TriggerEvent("Player::Smoke::Stop");
                                    }
                                }
                            }
                        }
                    }
                },

                {
                    AttachmentType.ItemFishingRodG, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    player.TriggerEvent("MG::F::S", args);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    player.TriggerEvent("MG::F::S");
                                }
                            }
                        }
                    }
                },

                {
                    AttachmentType.ItemFishG, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    player.TriggerEvent("MG::F::S", args);
                                }
                            }
                        },
                    }
                },

                {
                    AttachmentType.ItemShovel, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    player.TriggerEvent("MG::SHOV::S", args);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    player.TriggerEvent("MG::SHOV::S");
                                }
                            }
                        },
                    }
                },

                {
                    AttachmentType.FarmPlantSmallShovel, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    pData.PlayAnim(GeneralType.FarmPlantSmallShovelProcess0);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    var farmerJob = pData.CurrentJob as Game.Jobs.Farmer;

                                    if (farmerJob == null)
                                        return;

                                    int cFieldIdx;
                                    byte cRow, cCol;

                                    if (Game.Jobs.Farmer.TryGetPlayerCurrentCropInfo(pData, out cFieldIdx, out cRow, out cCol))
                                        Game.Jobs.Farmer.ResetPlayerCurrentCropInfo(pData);

                                    pData.StopGeneralAnim();
                                }
                            }
                        },
                    }
                },

                {
                    AttachmentType.FarmWateringCan, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    pData.PlayAnim(GeneralType.WateringCan0);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    var farmerJob = pData.CurrentJob as Game.Jobs.Farmer;

                                    if (farmerJob == null)
                                        return;

                                    int idx;

                                    if (Game.Jobs.Farmer.TryGetPlayerCurrentOrangeTreeInfo(pData, out idx))
                                        Game.Jobs.Farmer.ResetPlayerCurrentOrangeTreeInfo(pData);

                                    if (pData.GeneralAnim == GeneralType.WateringCan0)
                                        pData.StopGeneralAnim();
                                }
                            }
                        },
                    }
                },

                {
                    AttachmentType.FarmOrangeBoxCarry, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    pData.PlayAnim(GeneralType.BoxCarry0);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    var farmerJob = pData.CurrentJob as Game.Jobs.Farmer;

                                    if (farmerJob == null)
                                        return;

                                    int idx;

                                    if (Game.Jobs.Farmer.TryGetPlayerCurrentOrangeTreeInfo(pData, out idx))
                                        Game.Jobs.Farmer.ResetPlayerCurrentOrangeTreeInfo(pData);

                                    if (pData.GeneralAnim == GeneralType.BoxCarry0)
                                        pData.StopGeneralAnim();
                                }
                            }
                        },
                    }
                },

                {
                    AttachmentType.FarmMilkBucketCarry, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    if (pData.CrouchOn)
                                        pData.CrouchOn = false;

                                    pData.PlayAnim(GeneralType.BucketCarryOneHand0);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    var farmerJob = pData.CurrentJob as Game.Jobs.Farmer;

                                    if (farmerJob == null)
                                        return;

                                    int idx;

                                    if (Game.Jobs.Farmer.TryGetPlayerCurrentCowInfo(pData, out idx))
                                        Game.Jobs.Farmer.ResetPlayerCurrentCowInfo(pData);

                                    if (pData.GeneralAnim == GeneralType.BucketCarryOneHand0)
                                        pData.StopGeneralAnim();
                                }
                            }
                        },
                    }
                },

                {
                    AttachmentType.EmsHealingBedFakeAttach, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    Game.Fractions.EMS ems;
                                    int bedIdx;

                                    if (Game.Fractions.EMS.TryGetCurrentPlayerBed(pData, out ems, out bedIdx))
                                    {
                                        ems.SetBedAsFree(bedIdx);
                                    }

                                    if (pData.GeneralAnim == GeneralType.BedLie0)
                                        pData.StopGeneralAnim();
                                }
                            }
                        },
                    }
                },

                {
                    AttachmentType.Cuffs, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    if (player.GetEntityIsAttachedTo() is Entity entityAttachedTo)
                                    {
                                        entityAttachedTo.DetachEntity(player);
                                    }

                                    pData.StopUseCurrentItem();

                                    pData.UnequipActiveWeapon();

                                    pData.Player.DetachAllEntities();
                                    pData.Player.DetachAllObjectsInHand();

                                    pData.PlayAnim(GeneralType.CuffedStatic0);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    if (player.GetEntityIsAttachedTo() is Entity entityAttachedTo)
                                    {
                                        entityAttachedTo.DetachEntity(player);
                                    }

                                    if (pData.GeneralAnim == GeneralType.CuffedStatic0)
                                        pData.StopGeneralAnim();
                                }
                            }
                        },
                    }
                },

                {
                    AttachmentType.PoliceEscort, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    pData.PlayAnim(GeneralType.PoliceEscort0);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    if (pData.GeneralAnim == GeneralType.PoliceEscort0)
                                        pData.StopGeneralAnim();
                                }
                            }
                        },
                    }
                },

                {
                    AttachmentType.PlayerResurrect, new Dictionary<bool, Action<Entity, Entity, AttachmentType, string, object[]>>()
                    {
                        {
                            true, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    pData.PlayAnim(GeneralType.MedicalRevive);
                                }
                            }
                        },

                        {
                            false, (root, target, type, syncData, args) =>
                            {
                                if (root is Player player)
                                {
                                    var pData = player.GetMainData();

                                    if (pData == null)
                                        return;

                                    if (pData.GeneralAnim == GeneralType.MedicalRevive)
                                        pData.StopGeneralAnim();
                                }
                            }
                        },
                    }
                },
            };
    }
}