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
            public class LuckyWheel
            {
                public MapObject WheelObj { get; set; }
                public MapObject BaseObj { get; set; }
                public MapObject ArrowObj { get; set; }
                public MapObject LightsObj { get; set; }

                public LuckyWheel(int CasinoId, int Id, float PosX, float PosY, float PosZ, float Heading)
                {
                    WheelObj = new MapObject(RAGE.Util.Joaat.Hash("vw_prop_vw_luckywheel_02a"), new Vector3(PosX, PosY, PosZ + 1.5f), new Vector3(0f, 0f, Heading), 255, Settings.MAIN_DIMENSION)
                    {

                    };

                    BaseObj = new MapObject(RAGE.Util.Joaat.Hash("vw_prop_vw_luckywheel_01a"), new Vector3(PosX, PosY, PosZ), new Vector3(0f, 0f, Heading), 255, Settings.MAIN_DIMENSION)
                    {

                    };

                    ArrowObj = new MapObject(RAGE.Util.Joaat.Hash("vw_prop_vw_jackpot_on"), new Vector3(PosX, PosY, PosZ), new Vector3(0f, 0f, Heading), 255, Settings.MAIN_DIMENSION)
                    {
                        NotifyStreaming = true,
                    };

                    ArrowObj.SetStreamInCustomAction(OnLightObjStreamIn);

                    ArrowObj.SetHeading(Heading);

                    LightsObj = new MapObject(RAGE.Util.Joaat.Hash("vw_prop_vw_luckylight_on"), new Vector3(PosX, PosY, PosZ), new Vector3(0f, 0f, Heading), 255, Settings.MAIN_DIMENSION)
                    {
                        NotifyStreaming = true,
                    };

                    LightsObj.SetStreamInCustomAction(OnLightObjStreamIn);
                }

                private static void OnLightObjStreamIn(Entity entity)
                {
                    var mObj = entity as MapObject;

                    if (mObj == null)
                        return;

                    mObj.SetLights(false);
                    mObj.SetLightColour(255, 0, 0);
                }

                public async void Spin(Player player, byte targetZone)
                {
                    var basePos = BaseObj.GetCoords(false);

                    var wheelRotation = WheelObj.GetRotation(2);

                    var targetPos = RAGE.Game.Object.GetObjectOffsetFromCoords(basePos.X, basePos.Y, basePos.Z, wheelRotation.Z, -0.9f, -0.8f, 1f);

                    Player.LocalPlayer.Position = RAGE.Game.Object.GetObjectOffsetFromCoords(basePos.X, basePos.Y, basePos.Z, wheelRotation.Z, 0f, -2f, 1f);
                    Player.LocalPlayer.SetHeading(wheelRotation.Z);

                    player.TaskGoStraightToCoord(targetPos.X, targetPos.Y, targetPos.Z, 1f, -1, wheelRotation.Z, 0f);

                    await Utils.RequestAnimDict("anim_casino_a@amb@casino@games@lucky7wheel@male");

                    await RAGE.Game.Invoker.WaitAsync(2000);

                    Player.LocalPlayer.Position = targetPos;
                    Player.LocalPlayer.SetHeading(wheelRotation.Z);

                    player.TaskPlayAnim("anim_casino_a@amb@casino@games@lucky7wheel@male", "enter_to_armraisedidle", 1f, 1f, 2000, 2, 0f, true, true, true);

                    await RAGE.Game.Invoker.WaitAsync(2000);

                    player.TaskPlayAnim("anim_casino_a@amb@casino@games@lucky7wheel@male", "armraisedidle_to_spinningidle_high", 1f, 1f, 2000, 1, 0f, true, true, true);

                    await RAGE.Game.Invoker.WaitAsync(250);

                    await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false, -1);

                    // 0-19, 1-vehicle
                    if (WheelObj?.Exists != true)
                        return;

                    RAGE.Game.Audio.PlaySoundFromEntity(-1, "Spin_Start", WheelObj.Handle, "dlc_vw_casino_lucky_wheel_sounds", true, 1);

                    float j = 360;

                    var win = (targetZone - 1) * 18;

                    for (int i = 0; i < 1100; i++)
                    {
                        WheelObj.SetRotation(wheelRotation.X, j, wheelRotation.Z, 2, false);

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
                            j = (float)Utils.Random.Next(win - 4, win + 10);
                        }

                        await RAGE.Game.Invoker.WaitAsync(0);
                    }

                    RAGE.Game.Audio.PlaySoundFromEntity(-1, "Win", WheelObj.Handle, "dlc_vw_casino_lucky_wheel_sounds", true, 1);

                    for (int i = 0; i < 4; i++)
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
                    }
                }
            }
        }
    }
}
