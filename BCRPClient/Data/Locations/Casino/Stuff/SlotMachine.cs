using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public partial class Casino
        {
            public class SlotMachine
            {
                public enum ModelTypes : byte
                {
                    vw_prop_casino_slot_01a,
                    vw_prop_casino_slot_02a,
                    vw_prop_casino_slot_03a,
                    vw_prop_casino_slot_04a,
                    vw_prop_casino_slot_05a,
                    vw_prop_casino_slot_06a,
                    vw_prop_casino_slot_07a,
                    vw_prop_casino_slot_08a,
                }

                public enum ReelIconTypes : byte
                {
                    Seven = 0,
                    Grape = 1,
                    Watermelon = 2,
                    Microphone = 3,
                    Bell = 5,
                    Cherry = 6,
                    Superstar = 13,
                }

                public ModelTypes ModelType { get; set; }

                public Vector3 Position { get; set; }

                public MapObject MachineObj { get; set; }

                public MapObject[] Reels { get; set; }

                public SlotMachine(int CasinoId, int Id, ModelTypes ModelType, float PosX, float PosY, float PosZ, float Heading)
                {
                    this.ModelType = ModelType;

                    this.Position = new Vector3(PosX, PosY, PosZ);
                }

                public async void Spin(byte resultA, byte resultB, byte resultC)
                {
                    await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_01", false, -1);
                    await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_02", false, -1);
                    await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_03", false, -1);

                    if (MachineObj?.Exists != true)
                        return;

                    if (Reels != null)
                    {
                        for (int i = 0; i < Reels.Length; i++)
                        {
                            Reels[i]?.Destroy();

                            Reels[i] = null;
                        }
                    }

                    Reels = new MapObject[3];

                    var machinePos = MachineObj.GetCoords(false);
                    var machineHeading = MachineObj.GetHeading();

                    var rotation = new Vector3(0f, 0f, machineHeading);

                    var rModelStr = ModelType.ToString();

                    var reelsModel = RAGE.Util.Joaat.Hash($"{rModelStr}_reels");
                    var reelsModelB = RAGE.Util.Joaat.Hash(rModelStr.Substring(0, rModelStr.Length - 1) + "b_reels");

                    await Utils.RequestModel(reelsModel);
                    await Utils.RequestModel(reelsModelB);

                    for (int i = 0; i < Reels.Length; i++)
                    {
                        var pos = RAGE.Game.Object.GetObjectOffsetFromCoords(machinePos.X, machinePos.Y, machinePos.Z, machineHeading, i == 0 ? -0.115f : i == 1 ? 0f : 0.125f, 0.04f, 1.1f);

                        Reels[i] = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(reelsModelB, pos.X, pos.Y, pos.Z, false, false, false))
                        {
                            Dimension = uint.MaxValue,
                        };

                        Reels[i].SetRotation(rotation.X + Utils.Random.Next(0, 360) - 180f, rotation.Y, rotation.Z, 0, false);
                    }

                    var soundSetName = GetSoundSetName(ModelType);

/*                    var animDict0 = Player.LocalPlayer.IsMale() ? "anim_casino_a@amb@casino@games@slots@male" : "anim_casino_a@amb@casino@games@slots@female";

                    await Utils.RequestAnimDict(animDict0);

                    Player.LocalPlayer.TaskPlayAnim(animDict0, "press_spin_b", 8f, 0f, -1, 16, 0f, false, false, false);*/

                    RAGE.Game.Audio.PlaySoundFromEntity(-1, "start_spin", MachineObj.Handle, soundSetName, true, 20);
                    RAGE.Game.Audio.PlaySoundFromEntity(-1, "spinning", MachineObj.Handle, soundSetName, true, 20);

                    var checkPoints = new int[] { 180, 240, 300 };
                    var results = new byte[] { resultA, resultB, resultC };

                    for (int i = 1; i < 300 + 1; i++)
                    {
                        for (int j = 0; j < Reels.Length; j++)
                        {
                            var rot = Reels[j].GetRotation(0);

                            var checkPoint = checkPoints[j];

                            if (i < checkPoint)
                            {
                                Reels[j].SetRotation(rot.X + Utils.Random.Next(40, 100) / 10, rotation.Y, rotation.Z, 0, false);
                            }
                            else if (i == checkPoint)
                            {
                                var pos = Reels[j].GetCoords(false);

                                Reels[j].Destroy();

                                Reels[j] = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(reelsModel, pos.X, pos.Y, pos.Z, false, false, false))
                                {
                                    Dimension = uint.MaxValue,
                                };

                                Reels[j].SetRotation(results[j] * 22.5f - 180f, rotation.Y, rotation.Z, 0, false);

                                Utils.ConsoleOutput(RAGE.Util.Json.Serialize(Reels[j].GetRotation(0)));

                                RAGE.Game.Audio.PlaySoundFrontend(-1, "wheel_stop_clunk", soundSetName, false);
                            }
                        }

                        await RAGE.Game.Invoker.WaitAsync(13);
                    }
                }

                private static string GetSoundSetName(ModelTypes modelType)
                {
                    if (modelType == ModelTypes.vw_prop_casino_slot_01a)
                        return "dlc_vw_casino_slot_machine_ak_npc_sounds";
                    else if (modelType == ModelTypes.vw_prop_casino_slot_02a)
                        return "dlc_vw_casino_slot_machine_ir_npc_sounds";
                    else if (modelType == ModelTypes.vw_prop_casino_slot_03a)
                        return "dlc_vw_casino_slot_machine_rsr_npc_sounds";
                    else if (modelType == ModelTypes.vw_prop_casino_slot_04a)
                        return "dlc_vw_casino_slot_machine_fs_npc_sounds";
                    else if (modelType == ModelTypes.vw_prop_casino_slot_05a)
                        return "dlc_vw_casino_slot_machine_ds_npc_sounds";
                    else if (modelType == ModelTypes.vw_prop_casino_slot_06a)
                        return "dlc_vw_casino_slot_machine_kd_npc_sounds";
                    else if (modelType == ModelTypes.vw_prop_casino_slot_07a)
                        return "dlc_vw_casino_slot_machine_td_npc_sounds";

                    return "dlc_vw_casino_slot_machine_hz_npc_sounds";
                }
            }
        }
    }
}
