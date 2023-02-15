﻿using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    class CharacterCreation : Events.Script
    {
        public static DateTime LastSent;
        public static DateTime LastExitRequested;
        public static DateTime LastCreateRequested;

        public static bool IsActive { get => Browser.IsActive(Browser.IntTypes.CharacterCreation); }

        private static Additional.Camera.StateTypes[] AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.Head, Additional.Camera.StateTypes.Body, Additional.Camera.StateTypes.Legs, Additional.Camera.StateTypes.Foots, Additional.Camera.StateTypes.WholePed };

        #region Temp Variables
        private static string[] MaleNames;
        private static string[] FemaleNames;
        private static string[] Surnames;

        private static Data.Customization.HeadBlend HeadBlend;
        private static Dictionary<int, Data.Customization.HeadOverlay> HeadOverlays;
        private static float[] FaceFeatures;
        private static Data.Customization.HairStyle HairStyle;
        private static byte EyeColor;

        private static bool Sex;

        private static int CurrentCameraStateNum;

        private static Dictionary<bool, string[][]> DefaultClothes;
        private static string[] Clothes;

        private static List<int> TempBinds;

        private static float DefaultHeading;
        private static RAGE.Ui.Cursor.Vector2 LastCursorPos;
        private static AsyncTask CursorTask;
        #endregion

        #region Defaulter
        private static void SetDefaultCustomization()
        {
            HeadBlend.SetFather(0);
            HeadBlend.SetMother(21);
            HeadBlend.ShapeMix = Sex ? 0.5f : 0f; HeadBlend.SkinMix = 0.5f;

            EyeColor = 0;
            HairStyle.Id = 0; HairStyle.Overlay = 0; HairStyle.Color = 0; HairStyle.Color2 = 0;

            for (int i = 0; i < 20; i++)
                FaceFeatures[i] = 0;

            for (int i = 0; i < 13; i++)
            {
                HeadOverlays[i].Color = 0;
                HeadOverlays[i].SecondaryColor = 0;
                HeadOverlays[i].Opacity = 1;
                HeadOverlays[i].Index = 255;
            }

            HeadOverlays[4].Opacity = 0.5f;
            HeadOverlays[5].Opacity = 0.5f;
            HeadOverlays[8].Opacity = 0.5f;

            HeadOverlays[12].Opacity = 0;

            Player.LocalPlayer.ClearFacialDecorations();
        }
        #endregion

        public CharacterCreation()
        {
            #region Events
            Events.Add("CharacterCreation::StartNew", async (object[] args) => { await StartNew(); });
            Events.Add("CharacterCreation::Close", async (object[] args) => { await Close(); });

            #region Create
            Events.Add("CharacterCreation::Create", (object[] args) =>
            {
                var name = (string)args[0];
                var surname = (string)args[1];
                var age = (string)args[2];

                if (!Utils.IsNameValid(name))
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.CharacterCreation.WrongName);

                    return;
                }

                if (!Utils.IsNameValid(surname))
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.CharacterCreation.WrongSurname);

                    return;
                }

                int numAge = 0;

                if (age.Length < 1 || !int.TryParse(age, out numAge) || numAge < 18 || numAge > 99)
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.CharacterCreation.WrongAge);

                    return;
                }

                if (DateTime.Now.Subtract(LastCreateRequested).TotalMilliseconds > 5000)
                {
                    CEF.Notification.Show(Notification.Types.Question, Locale.Notifications.ApproveHeader, Locale.Notifications.CharacterCreation.PressAgainToCreate, 5000);

                    LastCreateRequested = DateTime.Now;

                    return;
                }

                if (LastSent.IsSpam(5000, false, false))
                    return;

                Events.CallRemote("CharacterCreation::Create", name, surname, age, Sex, EyeColor, RAGE.Util.Json.Serialize(HeadBlend), RAGE.Util.Json.Serialize(HeadOverlays), RAGE.Util.Json.Serialize(HairStyle), RAGE.Util.Json.Serialize(FaceFeatures), RAGE.Util.Json.Serialize(Clothes));

                LastSent = DateTime.Now;
            });
            #endregion

            #region Exit
            Events.Add("CharacterCreation::OnExit", (object[] args) =>
            {
                if (DateTime.Now.Subtract(LastExitRequested).TotalMilliseconds > 5000)
                {
                    CEF.Notification.Show(Notification.Types.Question, Locale.Notifications.ApproveHeader, Locale.Notifications.CharacterCreation.PressAgainToExit, 5000);

                    LastExitRequested = DateTime.Now;
                }
                else
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    Events.CallRemote("CharacterCreation::Exit");

                    LastSent = DateTime.Now;
                }
            });
            #endregion

            #region Set Sex
            Events.Add("CharacterCreation::SetSex", (object[] args) =>
            {
                if (LastSent.IsSpam(1000, false, false))
                    return;

                var newSex = (bool)args[0];

                if (newSex == Sex)
                    return;

                Sex = newSex;

                if (IsActive)
                    Browser.Window.ExecuteJs("ChCreate.sexApply", Sex ? "boy" : "girl");

                Events.CallRemote("CharacterCreation::SetSex", Sex);

                LastSent = DateTime.Now;

                (new AsyncTask(() =>
                {
                    SetDefaultCustomization();
                    Data.Clothes.UndressAll();

                    Clothes[0] = null;
                    Clothes[1] = null;
                    Clothes[2] = null;
                    Clothes[3] = null;
                }, 250)).Run();
            });
            #endregion

            #region Get Random Name
            Events.Add("CharacterCreation::GetRandomName", (object[] args) =>
            {
                if (!IsActive)
                    return;

                Random r = new Random();

                Browser.Window.ExecuteJs("ChCreate.nameSet", Sex ? MaleNames[r.Next(0, MaleNames.Length - 1)] : FemaleNames[r.Next(0, MaleNames.Length - 1)], Surnames[r.Next(0, Surnames.Length - 1)]);
            });
            #endregion

            #region Updaters
            #region Parents
            Events.Add("CharacterCreation::Update::Parents", (object[] args) =>
            {
                string parent = (string)args[0];

                if (parent == "father")
                    HeadBlend.SetFather((byte)(int)args[1]);
                else if (parent == "mother")
                    HeadBlend.SetMother((byte)(int)args[1]);
                else if (parent == "mix-0")
                    HeadBlend.ShapeMix = RAGE.Util.Json.Deserialize<float>((string)args[1]);
                else if (parent == "mix-1")
                    HeadBlend.SkinMix = RAGE.Util.Json.Deserialize<float>((string)args[1]);

                Player.LocalPlayer.SetHeadBlendData(HeadBlend.GetMother(), HeadBlend.GetFather(), 0, HeadBlend.GetMother(), HeadBlend.GetFather(), 0, HeadBlend.ShapeMix, HeadBlend.SkinMix, 0, true);
            });
            #endregion

            #region Face Feature
            Events.Add("CharacterCreation::Update::FaceFeature", (object[] args) =>
            {
                int id = (int)args[0];
                float value = RAGE.Util.Json.Deserialize<float>((string)args[1]);

                if (id < 0 || id > FaceFeatures.Length)
                    return;

                if (value < -1f)
                    value = -1f;
                else if (value > 1f)
                    value = 1f;

                Player.LocalPlayer.SetFaceFeature(id, value);

                FaceFeatures[id] = value;
            });
            #endregion

            #region Head Overlay
            Events.Add("CharacterCreation::Update::HeadOverlay", (object[] args) =>
            {
                int id = (int)args[0];
                byte value = (byte)(int)args[1];

                if (id < 0 || id > 12)
                    return;

                if (value == 0)
                    value = 255;
                else
                    value--;

                Player.LocalPlayer.SetHeadOverlay(id, value, HeadOverlays[id].Opacity);

                HeadOverlays[id].Index = (byte)value;
            });

            Events.Add("CharacterCreation::Update::HeadOverlayOpacity", (object[] args) =>
            {
                int id = (int)args[0];
                float value = RAGE.Util.Json.Deserialize<float>((string)args[1]);

                if (id < 0 || id > 12)
                    return;

                if (value < 0f)
                    value = 0f;
                else if (value > 1f)
                    value = 1f;

                Player.LocalPlayer.SetHeadOverlay(id, HeadOverlays[id].Index, value);

                HeadOverlays[id].Opacity = value;
            });

            Events.Add("CharacterCreation::Update::HeadOverlayColor", (object[] args) =>
            {
                int id = (int)args[0];
                byte value = (byte)(int)args[1];

                if (id < 0 || id > 12)
                    return;

                if (value < 0)
                    value = 0;
                else if (value > 63)
                    value = 63;

                Player.LocalPlayer.SetHeadOverlayColor(id, id == 5 || id == 8 ? 2 : 1, value, value);

                HeadOverlays[id].Color = value;
                HeadOverlays[id].SecondaryColor = value;
            });
            #endregion

            #region Hair
            Events.Add("CharacterCreation::Update::Hair", (object[] args) =>
            {
                int id = (int)args[0];
                int value = Data.Customization.GetHair(Sex, id);

                if (HairStyle.Id == id)
                    return;

                Player.LocalPlayer.SetComponentVariation(2, value, 0, 0);

                HairStyle.Id = id;

                if (IsActive)
                    Browser.Window.ExecuteJs("ChCreate.setHairFuzz", Data.Customization.GetDefaultHairOverlayId(Sex, (int)args[0]));
            });

            Events.Add("CharacterCreation::Update::HairOverlay", (object[] args) =>
            {
                byte value = (byte)(int)args[0];

                if (value < 0)
                    value = 0;

                HairStyle.Overlay = value;

                var overlay = Data.Customization.GetHairOverlay(Sex, value);

                Player.LocalPlayer.ClearFacialDecorations();

                if (overlay != null)
                    Player.LocalPlayer.SetFacialDecoration(overlay.Collection, overlay.Overlay);
            });

            Events.Add("CharacterCreation::Update::HairColor", (object[] args) =>
            {
                byte value = (byte)(int)args[0];
                bool isFirst = (bool)args[1];

                if (value < 0)
                    value = 0;
                else if (value > 63)
                    value = 63;

                Player.LocalPlayer.SetHairColor(isFirst ? value : HairStyle.Color, value);

                if (isFirst)
                    HairStyle.Color = value;

                HairStyle.Color2 = value;

                if (Data.Customization.GetHairOverlay(Sex, HairStyle.Overlay) is Data.Customization.HairOverlay overlay)
                {
                    Player.LocalPlayer.ClearFacialDecorations();

                    Player.LocalPlayer.SetFacialDecoration(overlay.Collection, overlay.Overlay);
                }
            });
            #endregion

            #region Eye Color
            Events.Add("CharacterCreation::Update::EyeColor", (object[] args) =>
            {
                byte value = (byte)(int)args[0];

                if (value < 0)
                    value = 0;
                else if (value > 31)
                    value = 31;

                Player.LocalPlayer.SetEyeColor(value);

                EyeColor = value;
            });
            #endregion

            #region Clothes
            Events.Add("CharacterCreation::Update::Clothes", (object[] args) =>
            {
                int type = (int)args[0];
                int value = (int)args[1];

                if (value != -1)
                {
                    if (type == 1)
                    {
                        Data.Clothes.Unwear(typeof(Data.Items.Under));
                        Data.Clothes.Unwear(typeof(Data.Items.Top));
                    }

                    var clothes = DefaultClothes[Sex][type][value];

                    Clothes[type] = clothes;

                    Data.Clothes.Wear(clothes, 0);
                }
                else
                {
                    Clothes[type] = null;

                    if (type == 0)
                        Data.Clothes.Unwear(typeof(Data.Items.Hat));
                    else if (type == 1)
                    {
                        Data.Clothes.Unwear(typeof(Data.Items.Under));
                        Data.Clothes.Unwear(typeof(Data.Items.Top));
                    }
                    else if (type == 2)
                        Data.Clothes.Unwear(typeof(Data.Items.Pants));
                    else
                        Data.Clothes.Unwear(typeof(Data.Items.Shoes));
                }
            });
            #endregion
            #endregion
            #endregion
        }

        public static async System.Threading.Tasks.Task Show()
        {
            if (IsActive)
            {
                Browser.Render(Browser.IntTypes.CharacterCreation, false);
            }

            await Browser.Render(Browser.IntTypes.CharacterCreation, true, true);

            Browser.Window.ExecuteJs("ChCreate.sexApply", "boy");

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task Close()
        {
            foreach (var bind in TempBinds)
                KeyBinds.Unbind(bind);

            TempBinds = null;

            DefaultClothes = null;
            MaleNames = null;
            FemaleNames = null;
            Surnames = null;

            HeadBlend = null;
            HeadOverlays = null;
            FaceFeatures = null;
            HairStyle = null;
            Clothes = null;

            CursorTask = null;
            LastCursorPos = null;

            await Browser.Render(Browser.IntTypes.CharacterCreation, false);

            Cursor.Show(false, false);

            GameEvents.Render -= ClearTasksRender;

            Additional.Camera.Disable();

            Player.LocalPlayer.ResetData("TempClothes::Under");
            Player.LocalPlayer.ResetData("TempClothes::Hat");
            Player.LocalPlayer.ResetData("TempClothes::Top");
            Player.LocalPlayer.ResetData("TempClothes::Gloves");
        }

        #region Start New
        public static async System.Threading.Tasks.Task StartNew()
        {
            CurrentCameraStateNum = 0;

            LastSent = DateTime.Now;

            TempBinds = new List<int>();

            #region Default Data (+ Random Names)
            MaleNames = "Dario,Deante,Alton,Estevan,Kody,Louis,Joey,Braiden,Kennedy,Sabastian,Tony,Briar,Osvaldo,Corbin,Winston,Shamar,Allan,Layton,Jaleel,Rogelio,Bronson,Tracy,Jaylan,Derek,Corey,Domenic,Jevon,Nathen,Kane,Floyd,Drake,Kristofer,Auston,Ramon,Gordon,Patrick,Ethen,Tyriq,Dashaun,Reginald,Mikael,Tristin,Reilly,Jase,Christopher,Braeden,Cain,Ronaldo,Norman,Bradly,Conor,Kole,Ryan,Mustafa,Martin,Tyson,Jalil,Lloyd,Treyvon,Jordon,David,Morgan,Rowan,Lincoln,Abdullah,Jonathan,Dejon,Mohammad,Trever,Cecil,Alvaro,Deonte,Mauricio,Leslie,Daquan,Tommy,Garrison,Khalil,Eli,Leonard,Brycen,Ladarius,Bailey,Armando,Cannon,Korey,Geoffrey,Mikel,Kasey,Enrique,Darryl,Tyrese,Esteban,Jonathon,Zackary,Kavon,Desmond,Rodney,Desean,Marquis,Spencer,Malachi,Rocco,Brooks,Gerard,Clarence,Miguelangel,Dequan,Micah,Jabari,Andres,Tahj,Rahul,Jayson,Lamar,Caleb,Isidro,Lucas,Ismael,Marcos,Alden,Maxim,Albert,Billy,Bryant,Jan,Draven,Gannon,Korbin,Donald,Stefan,Syed,Aiden,Troy,Grant,Aman,Rashad,Alek,Deion,Trinity,Edgardo,Rickey,Angelo,Rafael,Javon,William,Roman,Coy,Arturo,Guillermo".Split(',');
            FemaleNames = "Makaela,Adriana,Aniya,Maryann,Nayeli,Alex,Janiya,Camila,Paulina,Zaria,Samira,Audrey,Yasmin,Pilar,Veronica,Tristin,Iris,Angie,Laken,Leyla,Maia,Shay,Tatyana,Jaliyah,Shelbi,Corrine,Alanis,Miracle,Aiyana,Tayler,Susannah,Eliza,Helen,Devyn,Nicolette,Kassidy,Griselda,Janessa,Joy,Cayla,Rubi,Hillary,Chassidy,Liza,Lyndsey,Dajah,Savana,Danielle,Lilliana,Imari,Emerald,Tiffani,Tyra,Kaylyn,Jasmyne,Bobbi,Ann,Infant,Robin,Nathalie,Gwendolyn,Allie,Hazel,Jailyn,Darby,Anabel,Justine,Gina,Susanna,Carina,Katlyn,Chaya,Haven,Mindy,Lily,Jennifer,Lilian,Macie,Klarissa,Gia,Whitney,Izabella,Tionna,Kayley,Camille,Misty,Karlee,Brooklyn,Kyndall,Kelsi,Charity,Keri,Susana,Karla,Jewel,Kristal,Karissa,Kiana,Gloria,Danica,Francesca,Elaina,Jayden,Jacinda,Nicolle,Cathy,Jessika,Jada,Lucinda,Tyasia,Daria,Meg,Desire,Halley,Beth,Jazmyn,Ayesha,Kristy,Flor,Nyla,Fatima,Kalie,Sandra,Evelin,Georgina,Alaina,Notnamed,Terri,Sasha,Carrie,Melanie,Keyara,Galilea,Lucero,Silvia,Yesica,Sadie,Shyanne,Breanne,Sheyla,Shreya,Rhonda,Colette,Leila,Katharine,Jala,Cynthia,Jesse,Nikole,Linnea".Split(',');
            Surnames = "Zimmer,Cable,Edwards,Neumann,Royer,Beers,Marr,Daigle,Turner,Baldwin,Poore,Roldan,Hadley,Benton,Wetzel,Good,Walsh,Carlson,Low,Mcghee,Corcoran,Dozier,Krueger,Jaeger,Reyes,Stroud,Ricks,Gallegos,Bartels,Ridgeway,Gill,Estep,Graham,Burks,Nance,Norris,Patten,Holmes,Locke,Mancuso,Huerta,Cordell,Schiller,Oh,Snyder,Staples,Morgan,Hand,Newberry,Gallant,Turpin,Bermudez,Mallory,Garber,Robles,Spears,Corbin,Maxwell,Mott,Mulligan,Lowry,Whitworth,Fulmer,Heredia,Vang,Dixon,Alonzo,Muse,Watts,Hennessy,Savage,Borden,Sam,Loy,Mojica,Singletary,Noble,Wolff,Evers,Guillen,Muir,Mason,Correa,Emerson,Reedy,Braswell,Zhao,Hinds,Shipp,Ruffin,Land,Jacobson,Stamper,Solorzano,Ly,Garza,Canada,Colburn,High,Light,Woodruff,Jacoby,Schwab,Kenny,Lindsey,Ngo,Cramer,Chin,Cepeda,Ochoa,Mears,Victor,Ferguson,Kirk,Felder,Quigley,Price,Browne,Atkinson,Mancini,Robertson,Alley,Israel,Polanco,Lane,Heinrich,Chow,Herr,Morris,Llamas,Woods,Ceja,Davenport,Ware,Ryder,Swain,Sepulveda,Hastings,Flowers,Fair,Decker,Winslow,Jewell,Ortega,Lauer,Root,Spaulding,Ragland,Embry,Bateman".Split(',');

            DefaultClothes = new Dictionary<bool, string[][]>()
            {
                { true, new string[][] { new string[] { "hat_m_0", "hat_m_8", "hat_m_39" }, new string[] { "top_m_0", "under_m_2", "under_m_5" }, new string[] { "pants_m_0", "pants_m_2", "pants_m_11" }, new string[] { "shoes_m_5", "shoes_m_1", "shoes_m_3" } } },
                { false, new string[][] { new string[] { "hat_f_1", "hat_f_13", "hat_f_6" }, new string[] { "top_f_5", "under_f_2", "under_f_1" }, new string[] { "pants_f_0", "pants_f_4", "pants_f_7" }, new string[] { "shoes_f_0", "shoes_f_2", "shoes_f_5" } } },
            };

            Clothes = new string[] { null, null, null, null };
            #endregion

            LastExitRequested = DateTime.MinValue;
            LastCreateRequested = DateTime.MinValue;

            Sex = true;

            HeadBlend = new Data.Customization.HeadBlend();
            HairStyle = new Data.Customization.HairStyle();
            HeadOverlays = new Dictionary<int, Data.Customization.HeadOverlay>();

            for (int i = 0; i < 13; i++)
                HeadOverlays.Add(i, new Data.Customization.HeadOverlay());

            FaceFeatures = new float[20];

            SetDefaultCustomization();
            Data.Clothes.UndressAll();

            CursorTask = null;

            GameEvents.Render -= ClearTasksRender;
            GameEvents.Render += ClearTasksRender;

            CEF.Notification.ShowHint(Locale.Notifications.CharacterCreation.CtrlMovePed, true, 5000);

            DefaultHeading = Player.LocalPlayer.GetHeading();

            await Show();

            Additional.Camera.Enable(Additional.Camera.StateTypes.Head, Player.LocalPlayer, Player.LocalPlayer, 0);

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Control, true, () =>
            {
                if (CursorTask != null)
                    return;

                LastCursorPos = RAGE.Ui.Cursor.Position;

                CursorTask = new AsyncTask(() => OnTickMouse(), 10, true);
                CursorTask.Run();
            }));

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Control, false, () =>
            {
                if (CursorTask == null)
                    return;

                CursorTask.Cancel();

                CursorTask = null;
            }));

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.V, true, () =>
            {
                ChangeView(++CurrentCameraStateNum);
            }));
        }
        #endregion

        #region Stuff
        private static void ChangeView(int camStateNum)
        {
            if (camStateNum >= AllowedCameraStates.Length || AllowedCameraStates.Length < camStateNum)
                camStateNum = 0;

            Player.LocalPlayer.SetHeading(DefaultHeading);

            Additional.Camera.FromState(AllowedCameraStates[camStateNum], Player.LocalPlayer, Player.LocalPlayer, -1);

            CurrentCameraStateNum = camStateNum;
        }

        public static void ClearTasksRender()
        {
            Player.LocalPlayer.ClearTasks();
        }

        public static void OnTickMouse()
        {
            var curPos = RAGE.Ui.Cursor.Position;
            var dist = curPos.Distance(LastCursorPos);
            var newHeading = Player.LocalPlayer.GetHeading();

            if (curPos.X > LastCursorPos.X)
                newHeading += dist / 10;
            else if (curPos.X < LastCursorPos.X)
                newHeading -= dist / 10;
            else if (curPos.X == LastCursorPos.X)
            {
                if (curPos.X == 0)
                    newHeading -= 5;
                else if (curPos.X + 10 >= GameEvents.ScreenResolution.X)
                    newHeading += 5;
            }

            if (RAGE.Game.Pad.GetDisabledControlNormal(0, 241) == 1f)
            {
                Additional.Camera.Fov -= 1;
            }
            else if (RAGE.Game.Pad.GetDisabledControlNormal(0, 242) == 1f)
            {
                Additional.Camera.Fov += 1;
            }

            if (newHeading > 360f)
                newHeading = 0f;
            else if (newHeading < 0f)
                newHeading = 360f;

            Player.LocalPlayer.SetHeading(newHeading);

            LastCursorPos = curPos;
        }
        #endregion
    }
}
