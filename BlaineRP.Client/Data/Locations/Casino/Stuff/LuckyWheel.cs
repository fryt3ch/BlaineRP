using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Data
{
    public partial class Locations
    {
        public partial class Casino
        {
            public class LuckyWheel
            {
                public enum ZoneTypes : byte
                {
                    Cash_0 = 0,
                    Vehicle_0 = 1,
                    Mystery_0 = 2,
                    Clothes_0 = 3,
                    Chips_0 = 4,
                    Cash_1 = 5,
                    Mystery_1 = 6,
                    Clothes_1 = 7,
                    Mystery_2 = 8,
                    Chips_1 = 9,
                    Mystery_3 = 10,
                    Clothes_2 = 11,
                    Chips_3 = 12,
                    Cash_2 = 13,
                    Mystery_4 = 14,
                    Donate_0 = 15,
                    Chips_4 = 16,
                    Cash_3 = 17,
                    Mystery_5 = 18,
                    Clothes_3 = 19,
                }

                public MapObject WheelObj { get; set; }
                public MapObject BaseObj { get; set; }
                public MapObject ArrowObj { get; set; }
                public MapObject LightsObj { get; set; }

                public LuckyWheel(int CasinoId, int Id, float PosX, float PosY, float PosZ, float Heading)
                {
                    WheelObj = new MapObject(RAGE.Util.Joaat.Hash("vw_prop_vw_luckywheel_02a"), new Vector3(PosX, PosY, PosZ + 1.5f), new Vector3(0f, 0f, Heading), 255, Settings.App.Static.MainDimension)
                    {

                    };

                    BaseObj = new MapObject(RAGE.Util.Joaat.Hash("vw_prop_vw_luckywheel_01a"), new Vector3(PosX, PosY, PosZ), new Vector3(0f, 0f, Heading), 255, Settings.App.Static.MainDimension)
                    {

                    };

                    ArrowObj = new MapObject(RAGE.Util.Joaat.Hash("vw_prop_vw_jackpot_on"), new Vector3(PosX, PosY, PosZ), new Vector3(0f, 0f, Heading), 255, Settings.App.Static.MainDimension)
                    {
                        NotifyStreaming = true,
                    };
                    ArrowObj.StreamInCustomActionsAdd(OnLightObjStreamIn);

                    ArrowObj.SetHeading(Heading);

                    LightsObj = new MapObject(RAGE.Util.Joaat.Hash("vw_prop_vw_luckylight_on"), new Vector3(PosX, PosY, PosZ), new Vector3(0f, 0f, Heading), 255, Settings.App.Static.MainDimension)
                    {
                        NotifyStreaming = true,
                    };
                    LightsObj.StreamInCustomActionsAdd(OnLightObjStreamIn);
                }

                private static void OnLightObjStreamIn(Entity entity)
                {
                    var mObj = entity as MapObject;

                    if (mObj == null)
                        return;

                    mObj.SetLights(false);
                    mObj.SetLightColour(255, 0, 0);
                }

                public void Spin(int casinoId, int wheelId, Player player, ZoneTypes targetZoneType, float? resultOffset = null)
                {
                    var taskKey = $"CASINO_LUCKYWHEEL_{casinoId}_{wheelId}";

                    AsyncTask.Methods.CancelPendingTask(taskKey);

                    AsyncTask task = null;

                    task = new AsyncTask(async () =>
                    {
                        var startDate = Core.ServerTime;

                        var basePos = BaseObj.GetCoords(false);

                        var wheelRotation = WheelObj.GetRotation(0);

                        var targetPos = RAGE.Game.Object.GetObjectOffsetFromCoords(basePos.X, basePos.Y, basePos.Z, wheelRotation.Z, -0.9f, -0.8f, 1f);

                        if (player?.Exists == true)
                        {
                            player.Position = RAGE.Game.Object.GetObjectOffsetFromCoords(basePos.X, basePos.Y, basePos.Z, wheelRotation.Z, 0f, -2f, 1f);
                            player.SetHeading(wheelRotation.Z);

                            player.TaskGoStraightToCoord(targetPos.X, targetPos.Y, targetPos.Z, 1f, -1, wheelRotation.Z, 0f);
                        }

                        await Streaming.RequestAnimDict("anim_casino_a@amb@casino@games@lucky7wheel@male");

                        await RAGE.Game.Invoker.WaitAsync(2000);

                        if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                            return;

                        if (player?.Exists == true)
                        {
                            player.Position = targetPos;
                            player.SetHeading(wheelRotation.Z);

                            player.TaskPlayAnim("anim_casino_a@amb@casino@games@lucky7wheel@male", "enter_to_armraisedidle", 1f, 1f, 2000, 2, 0f, true, true, true);
                        }

                        await RAGE.Game.Invoker.WaitAsync(2000);

                        if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                            return;

                        if (player?.Exists == true)
                        {
                            player.TaskPlayAnim("anim_casino_a@amb@casino@games@lucky7wheel@male", "armraisedidle_to_spinningidle_high", 1f, 1f, 2000, 1, 0f, true, true, true);
                        }

                        await RAGE.Game.Invoker.WaitAsync(250);

                        await Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false, -1);

                        if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task) || WheelObj?.Exists != true)
                            return;

                        RAGE.Game.Audio.PlaySoundFromEntity(-1, "Spin_Start", WheelObj.Handle, "dlc_vw_casino_lucky_wheel_sounds", true, 1);

                        var j = 360f;

                        var win = ((byte)targetZoneType - 1) * 18;

                        var timeout = false;

                        for (int i = 0; i < 1100; i++)
                        {
                            WheelObj.SetRotation(wheelRotation.X, j, wheelRotation.Z, 0, false);

                            if (i < 50)
                                j -= 1.5f;
                            else if (i < 100)
                                j -= 2f;
                            else if (i < 150)
                                j -= 2.5f;
                            else if (i > 1060)
                                j -= 0.3f;
                            else if (i > 1030)
                                j -= 0.6f;
                            else if (i > 1000)
                                j -= 0.9f;
                            else if (i > 970)
                                j -= 1.2f;
                            else if (i > 940)
                                j -= 1.5f;
                            else if (i > 910)
                                j -= 1.8f;
                            else if (i > 880)
                                j -= 2.1f;
                            else if (i > 850)
                                j -= 2.4f;
                            else if (i > 820)
                                j -= 2.7f;
                            else
                                j -= 3f;

                            if (i == 850)
                            {
                                if (resultOffset is float f)
                                    j = f;
                                else
                                    j = (float)Utils.Misc.Random.Next(win - 4, win + 10);
                            }

                            if (!timeout)
                            {
                                if (Core.ServerTime.Subtract(startDate).TotalMilliseconds > 22_000)
                                    timeout = true;
                                else
                                    await RAGE.Game.Invoker.WaitAsync(10);
                            }

                            if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task) || WheelObj?.Exists != true)
                                return;
                        }

                        RAGE.Game.Audio.PlaySoundFromEntity(-1, "Win", WheelObj.Handle, "dlc_vw_casino_lucky_wheel_sounds", true, 1);

                        /*                    for (int i = 0; i < 4; i++)
                                            {
                                                LightsObj.SetLightColour(0, 255, 0);
                                                ArrowObj.SetLightColour(0, 255, 0);

                                                await RAGE.Game.Invoker.WaitAsync(500);

                                                LightsObj.SetLightColour(0, 0, 255);
                                                ArrowObj.SetLightColour(0, 0, 255);

                                                await RAGE.Game.Invoker.WaitAsync(500);

                                                LightsObj.SetLightColour(255, 0, 0);
                                                ArrowObj.SetLightColour(255, 0, 0);

                                                await RAGE.Game.Invoker.WaitAsync(500);
                                            }*/

                        AsyncTask.Methods.CancelPendingTask(taskKey);
                    }, 0, false, 0);

                    AsyncTask.Methods.SetAsPending(task, taskKey);
                }
            }
        }
    }
}
