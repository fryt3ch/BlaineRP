using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Data
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

                public static uint MinBet { get; set; }
                public static uint MaxBet { get; set; }

                public static uint JackpotMinValue { get; set; }

                public static SlotMachine CurrentMachine { get; set; }

                public ModelTypes ModelType { get; set; }

                public Vector3 Position { get; set; }

                public MapObject MachineObj { get; set; }

                public MapObject[] Reels { get; set; }

                public static int SoundId { get; set; } = -1;

                public SlotMachine(int CasinoId, int Id, ModelTypes ModelType, float PosX, float PosY, float PosZ, float Heading)
                {
                    this.ModelType = ModelType;

                    this.MachineObj = new MapObject(RAGE.Util.Joaat.Hash(ModelType.ToString()), new Vector3(PosX, PosY, PosZ), new Vector3(0f, 0f, Heading), 255, Settings.App.Static.MainDimension);
                }

                public void Spin(int casinoId, int machineId, ReelIconTypes resultA, ReelIconTypes resultB, ReelIconTypes resultC, decimal jackpot)
                {
                    var taskKey = $"CASINO_SLOTMACHINE_{casinoId}_{machineId}";

                    AsyncTask task = null;

                    task = new AsyncTask(async () =>
                    {
                        await Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_01", false, -1);
                        await Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_02", false, -1);
                        await Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_03", false, -1);

                        var rModelStr = ModelType.ToString();

                        var reelsModel = RAGE.Util.Joaat.Hash($"{rModelStr}_reels");
                        var reelsModelB = RAGE.Util.Joaat.Hash(rModelStr.Substring(0, rModelStr.Length - 1) + "b_reels");

                        await Streaming.RequestModel(reelsModel);
                        await Streaming.RequestModel(reelsModelB);

                        if (MachineObj?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                            return;

                        var machineObjHandle = MachineObj.Handle;

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

                        for (int i = 0; i < Reels.Length; i++)
                        {
                            var pos = RAGE.Game.Object.GetObjectOffsetFromCoords(machinePos.X, machinePos.Y, machinePos.Z, machineHeading, i == 0 ? -0.115f : i == 1 ? 0f : 0.125f, 0.04f, 1.1f);

                            Reels[i] = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(reelsModelB, pos.X, pos.Y, pos.Z, false, false, false))
                            {
                                Dimension = uint.MaxValue,
                            };

                            Reels[i].SetRotation(rotation.X + Utils.Misc.Random.Next(0, 360) - 180f, rotation.Y, rotation.Z, 0, false);
                        }

                        var soundSetName = GetSoundSetName(ModelType);

                        //RAGE.Game.Audio.PlaySoundFromEntity(-1, "place_bet", machineObjHandle, soundSetName, true, 0);

                        /*                    var animDict0 = Player.LocalPlayer.IsMale() ? "anim_casino_a@amb@casino@games@slots@male" : "anim_casino_a@amb@casino@games@slots@female";

                                            await Utils.RequestAnimDict(animDict0);

                                            Player.LocalPlayer.TaskPlayAnim(animDict0, "press_spin_b", 8f, 0f, -1, 16, 0f, false, false, false);*/

                        if (Data.Minigames.Casino.Casino.SoundOn)
                        {
                            RAGE.Game.Audio.PlaySoundFromEntity(SoundId, "start_spin", machineObjHandle, soundSetName, true, 0);
                            RAGE.Game.Audio.PlaySoundFromEntity(SoundId, "spinning", machineObjHandle, soundSetName, true, 0);
                        }

                        var checkPoints = new int[] { 180, 240, 300 };
                        var results = new byte[] { (byte)resultA, (byte)resultB, (byte)resultC };

                        var startDate = Sync.World.ServerTime;
                        var endDate = Sync.World.ServerTime.AddMilliseconds(5_500);

                        for (int i = 1; i < 300 + 1; i++)
                        {
                            for (int j = 0; j < Reels.Length; j++)
                            {
                                var rot = Reels[j].GetRotation(0);

                                var checkPoint = checkPoints[j];

                                if (i < checkPoint)
                                {
                                    Reels[j].SetRotation(rot.X + Utils.Misc.Random.Next(40, 100) / 10, rotation.Y, rotation.Z, 0, false);
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

                                    if (Data.Minigames.Casino.Casino.SoundOn)
                                        RAGE.Game.Audio.PlaySoundFromEntity(SoundId, "wheel_stop_clunk", machineObjHandle, soundSetName, true, 0);
                                }
                            }

                            if (Sync.World.ServerTime <= endDate)
                            {
                                await RAGE.Game.Invoker.WaitAsync(13);

                                if (MachineObj?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                    return;
                            }
                        }

                        if (resultA == resultB && resultA == resultC)
                        {
                            if (resultA == ReelIconTypes.Seven)
                            {
                                if (Data.Minigames.Casino.Casino.SoundOn)
                                    RAGE.Game.Audio.PlaySoundFromEntity(SoundId, "jackpot", machineObjHandle, soundSetName, true, 0);
                            }
                            if (resultA == ReelIconTypes.Grape || resultA == ReelIconTypes.Cherry || resultA == ReelIconTypes.Watermelon)
                            {
                                if (Data.Minigames.Casino.Casino.SoundOn)
                                    RAGE.Game.Audio.PlaySoundFromEntity(SoundId, "small_win", machineObjHandle, soundSetName, true, 0);
                            }
                            else
                            {
                                if (Data.Minigames.Casino.Casino.SoundOn)
                                    RAGE.Game.Audio.PlaySoundFromEntity(SoundId, "big_win", machineObjHandle, soundSetName, true, 0);
                            }
                        }
                        else
                        {
                            if (Data.Minigames.Casino.Casino.SoundOn)
                                RAGE.Game.Audio.PlaySoundFromEntity(SoundId, "no_win", machineObjHandle, soundSetName, true, 0);
                        }

                        if (CurrentMachine == this && Data.Minigames.Casino.Casino.CurrentType == Minigames.Casino.Casino.Types.SlotMachine)
                        {
                            Data.Minigames.Casino.Casino.UpdateStatus(GetJackpotString(jackpot));
                        }

                        AsyncTask.Methods.CancelPendingTask(taskKey);
                    }, 0, false, 0);

                    AsyncTask.Methods.SetAsPending(task, taskKey);
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

                public async void PlayGreetingSound()
                {
                    await Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_01", false, -1);
                    await Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_02", false, -1);
                    await Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_03", false, -1);

                    if (MachineObj?.Exists == true)
                    {
                        RAGE.Game.Audio.PlaySoundFromEntity(SoundId, "welcome_stinger", MachineObj.Handle, GetSoundSetName(ModelType), true, 0);
                    }
                }

                public static string GetJackpotString(decimal currentJackpot)
                {
                    var baseStr = Locale.Get("GEN_CHIPS_1", currentJackpot);

                    if (currentJackpot < JackpotMinValue)
                        return $"&#9940; {baseStr}";
                    else
                        return $"&#9989; {baseStr}";
                }
            }
        }
    }
}
