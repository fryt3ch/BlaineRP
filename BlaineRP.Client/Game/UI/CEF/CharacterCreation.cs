using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.RAGE.Ui.Cursor;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Items;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;
using Core = BlaineRP.Client.Game.Management.Camera.Core;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class CharacterCreation
    {
        public static DateTime LastSent;

        public static bool IsActive { get => Browser.IsActive(Browser.IntTypes.CharacterCreation); }

        private static Core.StateTypes[] AllowedCameraStates = new Core.StateTypes[] { Core.StateTypes.Head, Core.StateTypes.Body, Core.StateTypes.Legs, Core.StateTypes.Foots, Core.StateTypes.WholePed };

        private static string[] _maleNames => "Dario,Deante,Alton,Estevan,Kody,Louis,Joey,Braiden,Kennedy,Sabastian,Tony,Briar,Osvaldo,Corbin,Winston,Shamar,Allan,Layton,Jaleel,Rogelio,Bronson,Tracy,Jaylan,Derek,Corey,Domenic,Jevon,Nathen,Kane,Floyd,Drake,Kristofer,Auston,Ramon,Gordon,Patrick,Ethen,Tyriq,Dashaun,Reginald,Mikael,Tristin,Reilly,Jase,Christopher,Braeden,Cain,Ronaldo,Norman,Bradly,Conor,Kole,Ryan,Mustafa,Martin,Tyson,Jalil,Lloyd,Treyvon,Jordon,David,Morgan,Rowan,Lincoln,Abdullah,Jonathan,Dejon,Mohammad,Trever,Cecil,Alvaro,Deonte,Mauricio,Leslie,Daquan,Tommy,Garrison,Khalil,Eli,Leonard,Brycen,Ladarius,Bailey,Armando,Cannon,Korey,Geoffrey,Mikel,Kasey,Enrique,Darryl,Tyrese,Esteban,Jonathon,Zackary,Kavon,Desmond,Rodney,Desean,Marquis,Spencer,Malachi,Rocco,Brooks,Gerard,Clarence,Miguelangel,Dequan,Micah,Jabari,Andres,Tahj,Rahul,Jayson,Lamar,Caleb,Isidro,Lucas,Ismael,Marcos,Alden,Maxim,Albert,Billy,Bryant,Jan,Draven,Gannon,Korbin,Donald,Stefan,Syed,Aiden,Troy,Grant,Aman,Rashad,Alek,Deion,Trinity,Edgardo,Rickey,Angelo,Rafael,Javon,William,Roman,Coy,Arturo,Guillermo".Split(',');
        private static string[] _femaleNames => "Makaela,Adriana,Aniya,Maryann,Nayeli,Alex,Janiya,Camila,Paulina,Zaria,Samira,Audrey,Yasmin,Pilar,Veronica,Tristin,Iris,Angie,Laken,Leyla,Maia,Shay,Tatyana,Jaliyah,Shelbi,Corrine,Alanis,Miracle,Aiyana,Tayler,Susannah,Eliza,Helen,Devyn,Nicolette,Kassidy,Griselda,Janessa,Joy,Cayla,Rubi,Hillary,Chassidy,Liza,Lyndsey,Dajah,Savana,Danielle,Lilliana,Imari,Emerald,Tiffani,Tyra,Kaylyn,Jasmyne,Bobbi,Ann,Infant,Robin,Nathalie,Gwendolyn,Allie,Hazel,Jailyn,Darby,Anabel,Justine,Gina,Susanna,Carina,Katlyn,Chaya,Haven,Mindy,Lily,Jennifer,Lilian,Macie,Klarissa,Gia,Whitney,Izabella,Tionna,Kayley,Camille,Misty,Karlee,Brooklyn,Kyndall,Kelsi,Charity,Keri,Susana,Karla,Jewel,Kristal,Karissa,Kiana,Gloria,Danica,Francesca,Elaina,Jayden,Jacinda,Nicolle,Cathy,Jessika,Jada,Lucinda,Tyasia,Daria,Meg,Desire,Halley,Beth,Jazmyn,Ayesha,Kristy,Flor,Nyla,Fatima,Kalie,Sandra,Evelin,Georgina,Alaina,Notnamed,Terri,Sasha,Carrie,Melanie,Keyara,Galilea,Lucero,Silvia,Yesica,Sadie,Shyanne,Breanne,Sheyla,Shreya,Rhonda,Colette,Leila,Katharine,Jala,Cynthia,Jesse,Nikole,Linnea".Split(',');
        private static string[] _surnames => "Zimmer,Cable,Edwards,Neumann,Royer,Beers,Marr,Daigle,Turner,Baldwin,Poore,Roldan,Hadley,Benton,Wetzel,Good,Walsh,Carlson,Low,Mcghee,Corcoran,Dozier,Krueger,Jaeger,Reyes,Stroud,Ricks,Gallegos,Bartels,Ridgeway,Gill,Estep,Graham,Burks,Nance,Norris,Patten,Holmes,Locke,Mancuso,Huerta,Cordell,Schiller,Oh,Snyder,Staples,Morgan,Hand,Newberry,Gallant,Turpin,Bermudez,Mallory,Garber,Robles,Spears,Corbin,Maxwell,Mott,Mulligan,Lowry,Whitworth,Fulmer,Heredia,Vang,Dixon,Alonzo,Muse,Watts,Hennessy,Savage,Borden,Sam,Loy,Mojica,Singletary,Noble,Wolff,Evers,Guillen,Muir,Mason,Correa,Emerson,Reedy,Braswell,Zhao,Hinds,Shipp,Ruffin,Land,Jacobson,Stamper,Solorzano,Ly,Garza,Canada,Colburn,High,Light,Woodruff,Jacoby,Schwab,Kenny,Lindsey,Ngo,Cramer,Chin,Cepeda,Ochoa,Mears,Victor,Ferguson,Kirk,Felder,Quigley,Price,Browne,Atkinson,Mancini,Robertson,Alley,Israel,Polanco,Lane,Heinrich,Chow,Herr,Morris,Llamas,Woods,Ceja,Davenport,Ware,Ryder,Swain,Sepulveda,Hastings,Flowers,Fair,Decker,Winslow,Jewell,Ortega,Lauer,Root,Spaulding,Ragland,Embry,Bateman".Split(',');

        private static Game.Data.Customization.Customization.HeadBlend _headBlend;
        private static Dictionary<int, Game.Data.Customization.Customization.HeadOverlay> _headOverlays;
        private static float[] _faceFeatures;
        private static Game.Data.Customization.Customization.HairStyle _hairStyle;
        private static byte _eyeColor;

        private static bool _sex;

        private static int _currentCameraStateNum;

        private static Dictionary<bool, string[][]> _defaultClothes;
        private static string[] _clothes;

        private static List<int> _tempBinds;

        private static float _defaultHeading;
        private static RAGE.Ui.Cursor.Vector2 _lastCursorPos;
        private static AsyncTask _cursorTask;

        private static void SetDefaultCustomization()
        {
            _headBlend.SetFather(0);
            _headBlend.SetMother(21);
            _headBlend.ShapeMix = _sex ? 0.5f : 0f; _headBlend.SkinMix = 0.5f;

            _eyeColor = 0;
            _hairStyle.Id = 0; _hairStyle.Overlay = 0; _hairStyle.Color = 0; _hairStyle.Color2 = 0;

            for (int i = 0; i < 20; i++)
                _faceFeatures[i] = 0;

            for (int i = 0; i < 13; i++)
            {
                _headOverlays[i].Color = 0;
                _headOverlays[i].SecondaryColor = 0;
                _headOverlays[i].Opacity = 1;
                _headOverlays[i].Index = 255;
            }

            _headOverlays[4].Opacity = 0.5f;
            _headOverlays[5].Opacity = 0.5f;
            _headOverlays[8].Opacity = 0.5f;

            _headOverlays[12].Opacity = 0;

            Player.LocalPlayer.ClearFacialDecorations();
        }

        public CharacterCreation()
        {
            Events.Add("CharacterCreation::StartNew", async (args) => { await StartNew(); });
            Events.Add("CharacterCreation::Close", async (args) => { await Close(); });

            Events.Add("CharacterCreation::Create", async (args) =>
            {
                var name = (string)args[0];
                var surname = (string)args[1];
                var age = (string)args[2];

                if (!Utils.Misc.IsNameValid(name))
                {
                    CEF.Notification.ShowError(Locale.Notifications.CharacterCreation.WrongName);

                    return;
                }

                if (!Utils.Misc.IsNameValid(surname))
                {
                    CEF.Notification.ShowError(Locale.Notifications.CharacterCreation.WrongSurname);

                    return;
                }

                int numAge = 0;

                if (age.Length < 1 || !int.TryParse(age, out numAge) || numAge < 18 || numAge > 99)
                {
                    CEF.Notification.ShowError(Locale.Notifications.CharacterCreation.WrongAge);

                    return;
                }

                var approveContext = $"CharacterCreation_{name}_{surname}_{age}";
                var approveTime = 5_000;

                if (CEF.Notification.HasApproveTimedOut(approveContext, Game.World.Core.ServerTime, approveTime))
                {
                    if (LastSent.IsSpam(1_500, false, true))
                        return;

                    LastSent = Game.World.Core.ServerTime;

                    CEF.Notification.SetCurrentApproveContext(approveContext, Game.World.Core.ServerTime);

                    CEF.Notification.Show(Notification.Types.Question, Locale.Get("NOTIFICATION_HEADER_APPROVE"), Locale.Notifications.CharacterCreation.PressAgainToCreate, approveTime);
                }
                else
                {
                    CEF.Notification.ClearAll();

                    CEF.Notification.SetCurrentApproveContext(null, DateTime.MinValue);

                    var res = await Events.CallRemoteProc("CharacterCreation::Create", name, surname, age, _sex, _eyeColor, RAGE.Util.Json.Serialize(_headBlend), RAGE.Util.Json.Serialize(_headOverlays), RAGE.Util.Json.Serialize(_hairStyle), RAGE.Util.Json.Serialize(_faceFeatures), RAGE.Util.Json.Serialize(_clothes));
                }
            });

            Events.Add("CharacterCreation::OnExit", (object[] args) =>
            {
                var approveContext = "CharacterCreationEXIT";
                var approveTime = 5_000;

                if (CEF.Notification.HasApproveTimedOut(approveContext, Game.World.Core.ServerTime, approveTime))
                {
                    if (LastSent.IsSpam(1_000, false, true))
                        return;

                    LastSent = Game.World.Core.ServerTime;

                    CEF.Notification.SetCurrentApproveContext(approveContext, Game.World.Core.ServerTime);

                    CEF.Notification.Show(Notification.Types.Question, Locale.Get("NOTIFICATION_HEADER_APPROVE"), Locale.Notifications.CharacterCreation.PressAgainToExit, approveTime);
                }
                else
                {
                    CEF.Notification.ClearAll();

                    CEF.Notification.SetCurrentApproveContext(null, DateTime.MinValue);

                    Events.CallRemote("CharacterCreation::Exit");
                }
            });

            Events.Add("CharacterCreation::SetSex", async (args) =>
            {
                if (LastSent.IsSpam(1000, false, false))
                    return;

                var newSex = (bool)args[0];

                if (newSex == _sex)
                    return;


                LastSent = Game.World.Core.ServerTime;

                var res = (int)await Events.CallRemoteProc("CharacterCreation::SetSex", _sex);

                if (res == 0)
                {
                    CEF.Notification.ShowErrorDefault();
                }
                else if (res == 255)
                {
                    _sex = newSex;

                    if (IsActive)
                        Browser.Window.ExecuteJs("ChCreate.sexApply", _sex ? "boy" : "girl");

                    Events.CallRemote("CharacterCreation::SetSex", _sex);

                    LastSent = Game.World.Core.ServerTime;

                    SetDefaultCustomization();
                    Game.Data.Customization.Clothes.UndressAll();

                    _clothes[0] = null;
                    _clothes[1] = null;
                    _clothes[2] = null;
                    _clothes[3] = null;
                }
            });

            Events.Add("CharacterCreation::GetRandomName", (object[] args) =>
            {
                if (!IsActive)
                    return;

                var r = Utils.Misc.Random;

                Browser.Window.ExecuteJs("ChCreate.nameSet", _sex ? _maleNames[r.Next(0, _maleNames.Length - 1)] : _femaleNames[r.Next(0, _maleNames.Length - 1)], _surnames[r.Next(0, _surnames.Length - 1)]);
            });

            #region Parents
            Events.Add("CharacterCreation::Update::Parents", (object[] args) =>
            {
                var parent = (string)args[1];

                if (parent == "father")
                    _headBlend.SetFather((byte)(int)args[0]);
                else if (parent == "mother")
                    _headBlend.SetMother((byte)(int)args[0]);
                else if (parent == "mix-0")
                    _headBlend.ShapeMix = RAGE.Util.Json.Deserialize<float>((string)args[0]);
                else if (parent == "mix-1")
                    _headBlend.SkinMix = RAGE.Util.Json.Deserialize<float>((string)args[0]);

                Player.LocalPlayer.SetHeadBlendData(_headBlend.GetMother(), _headBlend.GetFather(), 0, _headBlend.GetMother(), _headBlend.GetFather(), 0, _headBlend.ShapeMix, _headBlend.SkinMix, 0, true);
            });
            #endregion

            #region Face Feature
            Events.Add("CharacterCreation::Update::FaceFeature", (object[] args) =>
            {
                var id = (int)args[1];
                var value = RAGE.Util.Json.Deserialize<float>((string)args[0]);

                if (id < 0 || id > _faceFeatures.Length)
                    return;

                if (value < -1f)
                    value = -1f;
                else if (value > 1f)
                    value = 1f;

                Player.LocalPlayer.SetFaceFeature(id, value);

                _faceFeatures[id] = value;
            });
            #endregion

            #region Head Overlay
            Events.Add("CharacterCreation::Update::HeadOverlay", (object[] args) =>
            {
                var id = (int)args[1];
                var value = (byte)(int)args[0];

                if (id < 0 || id > 12)
                    return;

                if (value == 0)
                    value = 255;
                else
                    value--;

                Player.LocalPlayer.SetHeadOverlay(id, value, _headOverlays[id].Opacity);

                _headOverlays[id].Index = (byte)value;
            });

            Events.Add("CharacterCreation::Update::HeadOverlayOpacity", (object[] args) =>
            {
                var id = (int)args[1];
                var value = RAGE.Util.Json.Deserialize<float>((string)args[0]);

                if (id < 0 || id > 12)
                    return;

                if (value < 0f)
                    value = 0f;
                else if (value > 1f)
                    value = 1f;

                Player.LocalPlayer.SetHeadOverlay(id, _headOverlays[id].Index, value);

                _headOverlays[id].Opacity = value;
            });

            Events.Add("CharacterCreation::Update::HeadOverlayColor", (object[] args) =>
            {
                var id = (int)args[1];
                var value = (byte)(int)args[0];

                if (id < 0 || id > 12)
                    return;

                if (value < 0)
                    value = 0;
                else if (value > 63)
                    value = 63;

                Player.LocalPlayer.SetHeadOverlayColor(id, id == 5 || id == 8 ? 2 : 1, value, value);

                _headOverlays[id].Color = value;
                _headOverlays[id].SecondaryColor = value;
            });
            #endregion

            #region Hair
            Events.Add("CharacterCreation::Update::Hair", (object[] args) =>
            {
                int id = (int)args[0];
                int value = Game.Data.Customization.Customization.GetHair(_sex, id);

                if (_hairStyle.Id == id)
                    return;

                Player.LocalPlayer.SetComponentVariation(2, value, 0, 0);

                _hairStyle.Id = id;

                if (IsActive)
                    Browser.Window.ExecuteJs("ChCreate.setHairFuzz", Game.Data.Customization.Customization.GetDefaultHairOverlayId(_sex, (int)args[0]));
            });

            Events.Add("CharacterCreation::Update::HairOverlay", (object[] args) =>
            {
                var value = (byte)(int)args[0];

                if (value < 0)
                    value = 0;

                _hairStyle.Overlay = value;

                var overlay = Game.Data.Customization.Customization.GetHairOverlay(_sex, value);

                Player.LocalPlayer.ClearFacialDecorations();

                if (overlay != null)
                    Player.LocalPlayer.SetFacialDecoration(overlay.Collection, overlay.Overlay);
            });

            Events.Add("CharacterCreation::Update::HairColor", (object[] args) =>
            {
                var value = (byte)(int)args[0];
                var isFirst = (bool)args[1];

                if (value < 0)
                    value = 0;
                else if (value > 63)
                    value = 63;

                Player.LocalPlayer.SetHairColor(isFirst ? value : _hairStyle.Color, value);

                if (isFirst)
                    _hairStyle.Color = value;

                _hairStyle.Color2 = value;

                if (Game.Data.Customization.Customization.GetHairOverlay(_sex, _hairStyle.Overlay) is Game.Data.Customization.Customization.HairOverlay overlay)
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

                _eyeColor = value;
            });
            #endregion

            #region Clothes
            Events.Add("CharacterCreation::Update::Clothes", (object[] args) =>
            {
                var type = (int)args[1];
                var value = (int)args[0];

                if (value != -1)
                {
                    if (type == 1)
                    {
                        Game.Data.Customization.Clothes.Unwear(typeof(Under));
                        Game.Data.Customization.Clothes.Unwear(typeof(Top));
                    }

                    var clothes = _defaultClothes[_sex][type][value];

                    _clothes[type] = clothes;

                    Game.Data.Customization.Clothes.Wear(clothes, 0);
                }
                else
                {
                    _clothes[type] = null;

                    if (type == 0)
                        Game.Data.Customization.Clothes.Unwear(typeof(Hat));
                    else if (type == 1)
                    {
                        Game.Data.Customization.Clothes.Unwear(typeof(Under));
                        Game.Data.Customization.Clothes.Unwear(typeof(Top));
                    }
                    else if (type == 2)
                        Game.Data.Customization.Clothes.Unwear(typeof(Pants));
                    else
                        Game.Data.Customization.Clothes.Unwear(typeof(Shoes));
                }
            });
            #endregion
        }

        public static async System.Threading.Tasks.Task Show()
        {
            CEF.Audio.StopAuthPlaylist();

            Browser.Switch(Browser.IntTypes.CharacterSelection, false);

            if (IsActive)
            {
                Browser.Render(Browser.IntTypes.CharacterCreation, false);
            }

            await Browser.Render(Browser.IntTypes.CharacterCreation, true, true);

            Browser.Window.ExecuteJs("ChCreate.draw", true);

            Browser.Window.ExecuteJs("ChCreate.sexApply", "boy");

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task Close()
        {
            foreach (var bind in _tempBinds)
                Input.Core.Unbind(bind);

            _tempBinds = null;

            _defaultClothes = null;

            _headBlend = null;
            _headOverlays = null;
            _faceFeatures = null;
            _hairStyle = null;
            _clothes = null;

            _cursorTask = null;
            _lastCursorPos = null;

            await Browser.Render(Browser.IntTypes.CharacterCreation, false);

            Cursor.Show(false, false);

            Main.Render -= ClearTasksRender;

            Core.Disable();

            Player.LocalPlayer.ResetData("TempClothes::Under");
            Player.LocalPlayer.ResetData("TempClothes::Hat");
            Player.LocalPlayer.ResetData("TempClothes::Top");
            Player.LocalPlayer.ResetData("TempClothes::Gloves");
        }

        public static async System.Threading.Tasks.Task StartNew()
        {
            _currentCameraStateNum = 0;

            LastSent = Game.World.Core.ServerTime;

            _tempBinds = new List<int>();

            _defaultClothes = new Dictionary<bool, string[][]>()
            {
                { true, new string[][] { new string[] { "hat_m_0", "hat_m_8", "hat_m_39" }, new string[] { "top_m_0", "under_m_2", "under_m_5" }, new string[] { "pants_m_0", "pants_m_2", "pants_m_11" }, new string[] { "shoes_m_5", "shoes_m_1", "shoes_m_3" } } },
                { false, new string[][] { new string[] { "hat_f_1", "hat_f_13", "hat_f_6" }, new string[] { "top_f_5", "under_f_2", "under_f_1" }, new string[] { "pants_f_0", "pants_f_4", "pants_f_7" }, new string[] { "shoes_f_0", "shoes_f_2", "shoes_f_5" } } },
            };

            _clothes = new string[] { null, null, null, null };

            _sex = true;

            _headBlend = new Game.Data.Customization.Customization.HeadBlend();
            _hairStyle = new Game.Data.Customization.Customization.HairStyle();
            _headOverlays = new Dictionary<int, Game.Data.Customization.Customization.HeadOverlay>();

            for (int i = 0; i < 13; i++)
                _headOverlays.Add(i, new Game.Data.Customization.Customization.HeadOverlay());

            _faceFeatures = new float[20];

            SetDefaultCustomization();
            Game.Data.Customization.Clothes.UndressAll();

            _cursorTask = null;

            Main.Render -= ClearTasksRender;
            Main.Render += ClearTasksRender;

            CEF.Notification.ShowHint(Locale.Notifications.CharacterCreation.CtrlMovePed, true, 5000);

            _defaultHeading = Player.LocalPlayer.GetHeading();

            await Show();

            Core.Enable(Core.StateTypes.Head, Player.LocalPlayer, Player.LocalPlayer, 0);

            _tempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.Control, true, () =>
            {
                if (_cursorTask != null)
                    return;

                _lastCursorPos = RAGE.Ui.Cursor.Position;

                _cursorTask = new AsyncTask(() => OnTickMouse(), 10, true);
                _cursorTask.Run();
            }));

            _tempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.Control, false, () =>
            {
                if (_cursorTask == null)
                    return;

                _cursorTask.Cancel();

                _cursorTask = null;
            }));

            _tempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.V, true, () =>
            {
                ChangeView(++_currentCameraStateNum);
            }));
        }

        private static void ChangeView(int camStateNum)
        {
            if (camStateNum >= AllowedCameraStates.Length || AllowedCameraStates.Length < camStateNum)
                camStateNum = 0;

            Player.LocalPlayer.SetHeading(_defaultHeading);

            Core.FromState(AllowedCameraStates[camStateNum], Player.LocalPlayer, Player.LocalPlayer, -1);

            _currentCameraStateNum = camStateNum;
        }

        public static void ClearTasksRender()
        {
            Player.LocalPlayer.ClearTasks();
        }

        public static void OnTickMouse()
        {
            var curPos = RAGE.Ui.Cursor.Position;
            var dist = curPos.Distance(_lastCursorPos);
            var newHeading = Player.LocalPlayer.GetHeading();

            if (curPos.X > _lastCursorPos.X)
                newHeading += dist / 10;
            else if (curPos.X < _lastCursorPos.X)
                newHeading -= dist / 10;
            else if (curPos.X == _lastCursorPos.X)
            {
                if (curPos.X == 0)
                    newHeading -= 5;
                else if (curPos.X + 10 >= Main.ScreenResolution.X)
                    newHeading += 5;
            }

            if (RAGE.Game.Pad.GetDisabledControlNormal(0, 241) == 1f)
            {
                Core.Fov -= 1;
            }
            else if (RAGE.Game.Pad.GetDisabledControlNormal(0, 242) == 1f)
            {
                Core.Fov += 1;
            }

            if (newHeading > 360f)
                newHeading = 0f;
            else if (newHeading < 0f)
                newHeading = 360f;

            Player.LocalPlayer.SetHeading(newHeading);

            _lastCursorPos = curPos;
        }
    }
}
