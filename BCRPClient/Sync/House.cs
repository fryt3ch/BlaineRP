using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using static BCRPClient.Additional.Camera;

namespace BCRPClient.Sync
{
    class House : Events.Script
    {
        public static int CurrentHouse;

        private static bool HasGarage;

        private static List<Additional.ExtraColshape> TempColshapes;
        private static List<Blip> TempBlips;

        private static List<(int Handle, Utils.Colour Colour, bool State)> Lights;
        private static List<(string Model, Vector3 Position, bool IsLocked)> Doors;

        private static Utils.Colour DefaultLightColour;

        public class Style
        {
            /// <summary>Типы планировки</summary>
            public enum Types
            {
                Room2_1 = 0, Room2_2,
            }

            public Types Type { get; private set; }
            public Vector3 EnterancePos { get; private set; }
            public Vector3 Position { get; private set; }

            public (string Model, Vector3 Position)[] Lights { get; set; }
            public (string Model, Vector3 Position)[] Doors { get; set; }

            /// <summary>Словарь планировок</summary>
            private static Dictionary<Style.Types, Style> All = new Dictionary<Style.Types, Style>()
            {
                {
                    Types.Room2_1, new Style(Types.Room2_1, new Vector3(70f, 70f, -10f), new Vector3(67.955511f, 70.03592f, -10f),
                        new (string, Vector3)[]
                        {
                            ("brp_p_light_3_1", new Vector3(0f, 0f, 3.976f)),
                            ("brp_p_light_3_1", new Vector3(0f, -5.349999f, 3.976f)),
                            ("brp_p_light_3_1", new Vector3(5.400002f, -6.099998f, 3.976f)),
                            ("brp_p_light_3_1", new Vector3(5.641205f, 4.258736f, 3.976f)),
                            ("brp_p_light_3_1", new Vector3(3.800003f, -0.75f, 3.976f)),
                            ("brp_p_light_2_1", new Vector3(6.75f, -0.75f, 3.95f))
                        },
                        new (string, Vector3)[]
                        {
                            ("v_ilev_ra_door3", new Vector3(-0.6799995f, -1.300002f, 1.152f)),
                            ("v_ilev_ra_door3", new Vector3(2.550006f, -5.550001f, 1.152f)),
                            ("v_ilev_ra_door3", new Vector3(3.25f, -2.800003f, 1.152f)),
                            ("v_ilev_ra_door3", new Vector3(4.850007f, -0.2000022f, 1.152f)),
                            ("v_ilev_ra_door3", new Vector3(3.249999f, 1.3f, 1.152f)),
                        })
                },
                {
                    Types.Room2_2, new Style(Types.Room2_2, new Vector3(70f, 70f, -20f), new Vector3(67.955511f, 70.03592f, -20f),
                        new (string, Vector3)[]
                        {
                            ("brp_p_light_3_1", new Vector3(0f, 0f, 3.976f)),
                            ("brp_p_light_3_1", new Vector3(0f, -5.349999f, 3.976f)),
                            ("brp_p_light_3_1", new Vector3(5.400002f, -6.099998f, 3.976f)),
                            ("brp_p_light_3_1", new Vector3(5.641205f, 4.258736f, 3.976f)),
                            ("brp_p_light_3_1", new Vector3(3.800003f, -0.75f, 3.976f)),
                            ("brp_p_light_2_1", new Vector3(6.75f, -0.75f, 3.95f))
                        },
                        new (string, Vector3)[]
                        {
                            ("v_ilev_ra_door3", new Vector3(-0.6799995f, -1.300002f, 1.152f)),
                            ("v_ilev_ra_door3", new Vector3(2.550006f, -5.550001f, 1.152f)),
                            ("v_ilev_ra_door3", new Vector3(3.25f, -2.800003f, 1.152f)),
                            ("v_ilev_ra_door3", new Vector3(4.850007f, -0.2000022f, 1.152f)),
                            ("v_ilev_ra_door3", new Vector3(3.249999f, 1.3f, 1.152f)),
                        })
                },
            };

            public static Style Get(Types type) => All[type];

            public Style(Types Type, Vector3 Position, Vector3 EnterancePos, (string Model, Vector3 Position)[] Lights, (string Model, Vector3 Position)[] Doors)
            {
                this.Type = Type;

                this.Position = Position;
                this.EnterancePos = EnterancePos;

                this.Lights = Lights.Select(x => (x.Model, Position + x.Position)).ToArray();
                this.Doors = Doors.Select(x => (x.Model, Position + x.Position)).ToArray();
            }
        }

        public House()
        {
            DefaultLightColour = new Utils.Colour(255, 187, 96);

            CurrentHouse = -1;
            HasGarage = false;

            TempColshapes = new List<Additional.ExtraColshape>();
            TempBlips = new List<Blip>();

            Lights = new List<(int Handle, Utils.Colour Color, bool State)>();
            Doors = new List<(string Model, Vector3 Position, bool IsLocked)>();

            Events.Add("House::Enter", (object[] args) =>
            {
                Additional.ExtraColshape.InteractionColshapesAllowed = false;

                Additional.SkyCamera.FadeScreen(true);

                CurrentHouse = (int)args[0];
                Style.Types sType = (Style.Types)(int)args[1];

                var dimension = RAGE.Util.Json.Deserialize<uint>((string)args[2]);

                (bool[] Doors, (Utils.Colour Colour, bool State)[] Lights) states = RAGE.Util.Json.Deserialize<(bool[], (Utils.Colour, bool)[])>((string)args[3]);

                var style = Style.Get(sType);

                if (style == null)
                    return;

                var exitCs = new Additional.Cylinder(style.EnterancePos, 1f, 2f, false, Utils.RedColor, dimension);

                exitCs.InteractionType = Additional.ExtraColshape.InteractionTypes.HouseExit;
                exitCs.ActionType = Additional.ExtraColshape.ActionTypes.HouseExit;

                TempColshapes.Add(exitCs);

                TempBlips.Add(new RAGE.Elements.Blip(40, style.EnterancePos, "Выход", 0.75f, 1, 255, 0, false, 0, 0, dimension));

                Lights.Clear();
                Doors.Clear();

                (new AsyncTask(() =>
                {
                    for (int i = 0; i < style.Doors.Length; i++)
                    {
                        var x = style.Doors[i];

                        Doors.Add((x.Model, x.Position, states.Doors[i]));

                        Sync.DoorSystem.ToggleLock(x.Model, x.Position, states.Doors[i]);

                        if (states.Doors[i])
                        {
                            var dBlip = new Blip(255, x.Position, "Закрытая дверь", 0.75f, 1, 255, 0, false, 0, 0, dimension);

                            dBlip.SetData("DoorIdx", i);

                            TempBlips.Add(dBlip);
                        }

                        var cs = new Additional.Cylinder(new Vector3(x.Position.X, x.Position.Y, x.Position.Z - 1f), 1f, 2f, false, Utils.RedColor, dimension);

                        cs.InteractionType = Additional.ExtraColshape.InteractionTypes.DoorLock;
                        cs.ActionType = Additional.ExtraColshape.ActionTypes.HouseDoorLock;

                        cs.Data = i; // doorIdx

                        TempColshapes.Add(cs);
                    }

                    for (int i = 0; i < style.Lights.Length; i++)
                    {
                        var x = style.Lights[i];

                        var handle = RAGE.Game.Object.GetClosestObjectOfType(x.Position.X, x.Position.Y, x.Position.Z, 1f, RAGE.Util.Joaat.Hash(x.Model), false, true, true);

                        if (handle <= 0)
                            continue;

                        Lights.Add((handle, states.Lights[i].Colour, states.Lights[i].State));

                        RAGE.Game.Entity.SetEntityLights(handle, !states.Lights[i].State);

                        RAGE.Game.Invoker.Invoke(0x5F048334B4A4E774, handle, true, states.Lights[i].Colour.Red, states.Lights[i].Colour.Green, states.Lights[i].Colour.Blue);
                    }
                }, 500, false, 0)).Run();

                (new AsyncTask(() =>
                {
                    Additional.SkyCamera.FadeScreen(false);

                    Additional.ExtraColshape.InteractionColshapesAllowed = true;
                }, 1500)).Run();

/*                if (style.IsCustom)
                {

                }
                else
                {
                    foreach (var x in style.LockerPositions)
                    {
                        var lockerCs = Additional.ExtraColshape.CreateTube(x, 1f, 2f, Player.LocalPlayer.Dimension, Utils.RedColor, 125, false);

                        lockerCs.SetData("ID", contIds.Locker);

                        lockerCs.OnEnter += (cancel) =>
                        {
                            if (!Sync.World.ColshapesAllowed)
                                return;

                            Player.LocalPlayer.SetData("CurrentContainer", lockerCs.GetData<uint>("ID"));
                            Player.LocalPlayer.SetData("CurrentContainer::Type", CEF.Inventory.ContainerTypes.Locker);

                            CEF.HUD.InteractionAction = OpenContainer;

                            CEF.HUD.SwitchInteractionText(true, Locale.Interaction.Locker);
                        };

                        lockerCs.OnExit += (cancel) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentContainer");
                            Player.LocalPlayer.ResetData("CurrentContainer::Type");

                            CEF.HUD.InteractionAction = null;

                            CEF.HUD.SwitchInteractionText(false);
                        };

                        TempColshapes.Add(lockerCs);

                        TempBlips.Add(new RAGE.Elements.Blip(568, x, "Шкаф", 0.75f, 5, 255, 0, false, 0, 0, Player.LocalPlayer.Dimension));
                    }

                    foreach (var x in style.WardrobePositions)
                    {
                        var wardrobeCs = Additional.ExtraColshape.CreateTube(x, 1f, 2f, Player.LocalPlayer.Dimension, Utils.RedColor, 125, false);

                        wardrobeCs.SetData("ID", contIds.Wardrobe);

                        wardrobeCs.OnEnter += (cancel) =>
                        {
                            if (!Sync.World.ColshapesAllowed)
                                return;

                            Player.LocalPlayer.SetData("CurrentContainer", wardrobeCs.GetData<uint>("ID"));
                            Player.LocalPlayer.SetData("CurrentContainer::Type", CEF.Inventory.ContainerTypes.Wardrobe);

                            CEF.HUD.InteractionAction = OpenContainer;

                            CEF.HUD.SwitchInteractionText(true, Locale.Interaction.Wardrobe);
                        };

                        wardrobeCs.OnExit += (cancel) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentContainer");
                            Player.LocalPlayer.ResetData("CurrentContainer::Type");

                            CEF.HUD.InteractionAction = null;

                            CEF.HUD.SwitchInteractionText(false);
                        };

                        TempColshapes.Add(wardrobeCs);

                        TempBlips.Add(new RAGE.Elements.Blip(366, x, "Гардероб", 0.75f, 4, 255, 0, false, 0, 0, Player.LocalPlayer.Dimension));
                    }

                    foreach (var x in style.FridgePositions)
                    {
                        var fridgeCs = Additional.ExtraColshape.CreateTube(x, 1f, 2f, Player.LocalPlayer.Dimension, Utils.RedColor, 125, false);

                        fridgeCs.SetData("ID", contIds.Wardrobe);

                        fridgeCs.OnEnter += (cancel) =>
                        {
                            if (!Sync.World.ColshapesAllowed)
                                return;

                            Player.LocalPlayer.SetData("CurrentContainer", fridgeCs.GetData<uint>("ID"));
                            Player.LocalPlayer.SetData("CurrentContainer::Type", CEF.Inventory.ContainerTypes.Fridge);

                            CEF.HUD.InteractionAction = OpenContainer;

                            CEF.HUD.SwitchInteractionText(true, Locale.Interaction.Fridge);
                        };

                        fridgeCs.OnExit += (cancel) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentContainer");
                            Player.LocalPlayer.ResetData("CurrentContainer::Type");

                            CEF.HUD.InteractionAction = null;

                            CEF.HUD.SwitchInteractionText(false);
                        };

                        TempColshapes.Add(fridgeCs);

                        TempBlips.Add(new RAGE.Elements.Blip(827, x, "Холодильник", 0.75f, 3, 255, 0, false, 0, 0, Player.LocalPlayer.Dimension));
                    }
                }*/
            });

            Events.Add("House::Exit", (object[] args) =>
            {
                Additional.ExtraColshape.InteractionColshapesAllowed = false;

                Additional.SkyCamera.FadeScreen(true);

                (new AsyncTask(() =>
                {
                    Additional.SkyCamera.FadeScreen(false);

                    Additional.ExtraColshape.InteractionColshapesAllowed = true;
                }, 1500)).Run();

                foreach (var x in TempColshapes)
                    x?.Delete();

                TempColshapes.Clear();

                foreach (var x in TempBlips)
                    x.Destroy();

                TempBlips.Clear();

                Doors.Clear(); Lights.Clear();
            });
        }

        public static bool Exit()
        {
            if (Additional.ExtraColshape.LastSent.IsSpam(1000, false, false))
                return false;

            Events.CallRemote("House::Exit");

            Additional.ExtraColshape.LastSent = DateTime.Now;

            return true;
        }

        public static bool DoorLock()
        {
            if (Additional.ExtraColshape.LastSent.IsSpam(1000, false, false))
                return false;

            if (!Player.LocalPlayer.HasData("House::CurrentDoorIdx"))
                return true;

            var idx = Player.LocalPlayer.GetData<int>("House::CurrentDoorIdx");
            var door = Doors[idx];

            Sync.DoorSystem.ToggleLock(door.Model, door.Position, !door.IsLocked);

            door.IsLocked = !door.IsLocked;
            Doors[idx] = door;

            if (door.IsLocked)
            {
                var dBlip = new Blip(255, door.Position, "Закрытая дверь", 0.75f, 1, 255, 0, false, 0, 0, Player.LocalPlayer.Dimension);

                dBlip.SetData("DoorIdx", idx);

                TempBlips.Add(dBlip);
            }
            else
            {
                for (int i = 0; i < TempBlips.Count; i++)
                    if (TempBlips[i].HasData("DoorIdx") && TempBlips[i].GetData<int>("DoorIdx") == idx)
                    {
                        TempBlips[i].Destroy();

                        TempBlips.Remove(TempBlips[i]);
                    }
            }

            return true;
        }

        public static bool OpenContainer()
        {
            if (Additional.ExtraColshape.LastSent.IsSpam(1000, false, false))
                return false;

            if (!Player.LocalPlayer.HasData("CurrentContainer"))
                return false;

            CEF.Inventory.Show(CEF.Inventory.Types.Container);

            Additional.ExtraColshape.LastSent = DateTime.Now;

            return true;
        }
    }
}
