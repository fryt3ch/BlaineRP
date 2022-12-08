using BCRPClient.Sync;
using Newtonsoft.Json;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static BCRPClient.Additional.Camera;

namespace BCRPClient
{
    public class Utils
    {
        private static DateTime LastConsoleMsg = DateTime.Now;

        public enum ScreenTextFontTypes
        {
            ChaletLondon = 0, HouseScript = 1, Monospace = 2, CharletComprimeColonge = 4, Pricedown = 7
        }

        public static bool IsGameWindowFocused { get => RAGE.Ui.Windows.Focused; }

        #region Colours
        public class Colour
        {
            [JsonIgnore]
            /// <summary>Красный</summary>
            public byte Red { get; set; }

            [JsonIgnore]
            /// <summary>Зеленый</summary>
            public byte Green { get; set; }

            [JsonIgnore]
            /// <summary>Синий</summary>
            public byte Blue { get; set; }

            [JsonIgnore]
            /// <summary>Непрозрачность</summary>
            public byte Alpha { get; set; }

            [JsonProperty(PropertyName = "H")]
            public string HEX => $"#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}";

            public Colour(byte Red, byte Green, byte Blue, byte Alpha = 255)
            {
                this.Red = Red;
                this.Green = Green;
                this.Blue = Blue;

                this.Alpha = Alpha;
            }

            [JsonConstructor]
            public Colour(string HEX)
            {
                this.Red = byte.Parse(HEX.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                this.Green = byte.Parse(HEX.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                this.Blue = byte.Parse(HEX.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

                if (HEX.Length == 6)
                    this.Alpha = byte.Parse(HEX.Substring(7, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                else
                    this.Alpha = 255;
            }

            public Color ToSystemColour() => Color.FromArgb(Alpha, Red, Green, Blue);

            public static Colour FromSystemColour(Color colour) => new Colour(colour.R, colour.G, colour.B, colour.A);
        }

        public static Colour WhiteColor = new Colour(255, 255, 255);
        public static Colour BlackColor = new Colour(0, 0, 0);
        public static Colour RedColor = new Colour(255, 0, 0);
        public static Colour BlueColor = new Colour(0, 0, 255);
        public static Colour GreenColor = new Colour(0, 255, 0);
        public static Colour YellowColor = new Colour(255, 255, 0);
        #endregion

        public class Vector4
        {
            [JsonProperty(PropertyName = "P")]
            public Vector3 Position { get; set; }

            [JsonProperty(PropertyName = "RZ")]
            public float RotationZ { get; set; }

            [JsonIgnore]
            public float X => Position.X;

            [JsonIgnore]
            public float Y => Position.Y;

            [JsonIgnore]
            public float Z => Position.Z;

            public Vector4(float X, float Y, float Z, float RotationZ)
            {
                this.Position = new Vector3(X, Y, Z);

                this.RotationZ = RotationZ;
            }

            public Vector4(Vector3 Position, float RotationZ) : this(Position.X, Position.Y, Position.Z, RotationZ) { }

            public Vector4() { }
        }

        public const uint MP_MALE_MODEL = 0x705E61F2;

        public const uint MP_FEMALE_MODEL = 0x9C9EFFD8;

        /// <summary>Вектор для ненужных данных</summary>
        /// <remarks>Использовать везде, где необходимо передавать вектор в качестве параметра, но его изменения нам не нужны</remarks>
        private static Vector3 GarbageVector = new Vector3(0f, 0f, 0f);

        public static RAGE.Elements.Vehicle GetVehicleByHandle(int handle, bool streamedOnly = true) => streamedOnly ? RAGE.Elements.Entities.Vehicles.Streamed.Where(x => x.Handle == handle).FirstOrDefault() : RAGE.Elements.Entities.Vehicles.All.Where(x => x.Handle == handle).FirstOrDefault();

        public static RAGE.Elements.Player GetPlayerByHandle(int handle, bool streamedOnly = true) => streamedOnly ? RAGE.Elements.Entities.Players.Streamed.Where(x => x.Handle == handle).FirstOrDefault() : RAGE.Elements.Entities.Players.All.Where(x => x.Handle == handle).FirstOrDefault();

        public static RAGE.Elements.Ped GetPedByHandle(int handle, bool streamedOnly = true) => streamedOnly ? RAGE.Elements.Entities.Peds.Streamed.Where(x => x.Handle == handle).FirstOrDefault() : RAGE.Elements.Entities.Peds.All.Where(x => x.Handle == handle).FirstOrDefault();

        public static RAGE.Elements.MapObject GetMapObjectByHandle(int handle, bool streamedOnly = true) => streamedOnly ? RAGE.Elements.Entities.Objects.Streamed.Where(x => x.Handle == handle).FirstOrDefault() : RAGE.Elements.Entities.Objects.All.Where(x => x.Handle == handle).FirstOrDefault();

        public static RAGE.Elements.Vehicle GetVehicleByRemoteId(int id, bool streamedOnly = true) => streamedOnly ? RAGE.Elements.Entities.Vehicles.Streamed.Where(x => x.RemoteId == id).FirstOrDefault() : RAGE.Elements.Entities.Vehicles.All.Where(x => x.RemoteId == id).FirstOrDefault();

        public static RAGE.Elements.Player GetPlayerByRemoteId(int id, bool streamedOnly = true) => streamedOnly ? RAGE.Elements.Entities.Players.Streamed.Where(x => x.RemoteId == id).FirstOrDefault() : RAGE.Elements.Entities.Players.All.Where(x => x.RemoteId == id).FirstOrDefault();

        public static bool IsEntityStreamed(RAGE.Elements.Entity entity)
        {
            if (entity is Player player)
            {
                if (player.Handle == Player.LocalPlayer.Handle)
                    return true;

                return RAGE.Elements.Entities.Players.Streamed.Contains(entity);
            }

            if (entity.Type == RAGE.Elements.Type.Vehicle)
                return RAGE.Elements.Entities.Vehicles.Streamed.Contains(entity);

            if (entity.Type == RAGE.Elements.Type.Ped)
                return RAGE.Elements.Entities.Peds.Streamed.Contains(entity);

            return false;
        }

        public static RAGE.Elements.GameEntity GetGameEntity(RAGE.Elements.Entity entity)
        {
            if (entity == null)
                return null;

            switch (entity.Type)
            {
                case RAGE.Elements.Type.Ped: return RAGE.Elements.Entities.Peds.GetAt(entity.Id);
                case RAGE.Elements.Type.Player: return RAGE.Elements.Entities.Players.GetAt(entity.Id);
                case RAGE.Elements.Type.Vehicle: return RAGE.Elements.Entities.Vehicles.GetAt(entity.Id);
                case RAGE.Elements.Type.Object: return RAGE.Elements.Entities.Objects.GetAt(entity.Id);
            }

            return null;
        }

        public static Vector3 GetBonePositionOfEntity(GameEntity entity, object boneId)
        {
            if (entity is Player player)
            {
                return player.GetBoneCoords((int)boneId, 0f, 0f, 0f);
            }
            else if (entity is Ped ped)
            {
                return ped.GetBoneCoords((int)boneId, 0f, 0f, 0f);
            }
            else if (entity is Vehicle vehicle)
            {
                return vehicle.GetWorldPositionOfBone(vehicle.GetBoneIndexByName((string)boneId));
            }

            return null;
        }

        public static RAGE.Elements.GameEntity GetGameEntityAtRemoteId(RAGE.Elements.Type type, int remoteId)
        {
            switch (type)
            {
                case RAGE.Elements.Type.Ped: return RAGE.Elements.Entities.Peds.GetAtRemote((ushort)remoteId);
                case RAGE.Elements.Type.Player: return RAGE.Elements.Entities.Players.GetAtRemote((ushort)remoteId);
                case RAGE.Elements.Type.Vehicle: return RAGE.Elements.Entities.Vehicles.GetAtRemote((ushort)remoteId);
                case RAGE.Elements.Type.Object: return RAGE.Elements.Entities.Objects.GetAtRemote((ushort)remoteId);
            }

            return null;
        }

        public static List<Ped> GetPedsOnScreen(int maxCount = 5) => RAGE.Elements.Entities.Peds.Streamed.Where(x => x.IsOnScreen()).OrderBy(x => Vector3.Distance(x.Position, Player.LocalPlayer.Position)).Take(maxCount).ToList();
        public static List<Vehicle> GetVehiclesOnScreen(int maxCount = 5) => RAGE.Elements.Entities.Vehicles.Streamed.Where(x => x.IsOnScreen()).OrderBy(x => Vector3.Distance(x.Position, Player.LocalPlayer.Position)).Take(maxCount).ToList();

        public static RAGE.Elements.Vehicle GetClosestVehicle(Vector3 position, float radius)
        {
            float minDistance = radius;

            RAGE.Elements.Vehicle vehicle = null;

            for (int i = 0; i < RAGE.Elements.Entities.Vehicles.Streamed.Count; i++)
            {
                var veh = RAGE.Elements.Entities.Vehicles.Streamed[i];

                if (veh == null)
                    continue;

                float distance = Vector3.Distance(position, veh.Position);

                if (distance <= radius && minDistance >= distance)
                {
                    vehicle = veh;
                    minDistance = distance;
                }
            }

            return vehicle;
        }

        public static Entity GetEntityByRaycast(Vector3 startPos, Vector3 endPos, int ignoreHandle = 0, int flags = 14)
        {
            //RAGE.Game.Graphics.DrawLine(startPos.X, startPos.Y, startPos.Z, endPos.X, endPos.Y, endPos.Z, 255, 0, 0, 255);

            int hit = -1, endEntity = -1;

            //int result = RAGE.Game.Shapetest.GetShapeTestResult(RAGE.Game.Shapetest.StartShapeTestRay(startPos.X, startPos.Y, startPos.Z, endPos.X, endPos.Y, endPos.Z, 31, ignoreHandle, 4), ref hit, GarbageVector, GarbageVector, ref endEntity);

            int result = RAGE.Game.Shapetest.GetShapeTestResult(RAGE.Game.Shapetest.StartShapeTestCapsule(startPos.X, startPos.Y, startPos.Z, endPos.X, endPos.Y, endPos.Z, 0.25f, flags, ignoreHandle, 4), ref hit, GarbageVector, GarbageVector, ref endEntity);

            if (result != 2 || endEntity <= 0)
                return null;

            var type = RAGE.Game.Entity.GetEntityType(endEntity);

            if (type <= 0)
                return null;

            // Ped
            if (type == 1)
            {
                Entity entity = GetPlayerByHandle(endEntity, true);

                if (entity != null)
                    return entity;

                entity = GetPedByHandle(endEntity, true);

                return entity;
            }
            // Vehicle
            else if (type == 2)
            {
                return GetVehicleByHandle(endEntity, true);
            }
            // Object
            else if (type == 3)
            {
                return GetMapObjectByHandle(endEntity, false);
            }

            return null;
        }

        public static (bool hit, int handle) TestRaycast(Vector3 startPos, Vector3 endPos, int ignoreHandle = 0, int flags = 14)
        {
            int hit = -1, endEntity = -1;

            int result = RAGE.Game.Shapetest.GetShapeTestResult(RAGE.Game.Shapetest.StartShapeTestRay(startPos.X, startPos.Y, startPos.Z, endPos.X, endPos.Y, endPos.Z, flags, ignoreHandle, 0), ref hit, GarbageVector, GarbageVector, ref endEntity);

            if (result != 2)
                return (false, -1);

            return (hit == 1, endEntity);
        }

        public static Entity GetEntityPlayerLookAt(float distance)
        {
            var headCoord = RAGE.Elements.Player.LocalPlayer.GetBoneCoords(12844, 0f, 0f, 0f);
            var screenCenterCoord = headCoord.MinimizeDistance(GetWorldCoordFromScreenCoord(0.5f, 0.5f), distance);

            if (Settings.Other.RaytraceEnabled)
                RAGE.Game.Graphics.DrawLine(headCoord.X, headCoord.Y, headCoord.Z, screenCenterCoord.X, screenCenterCoord.Y, screenCenterCoord.Z, 255, 0, 0, 255);

            return GetEntityByRaycast(headCoord, screenCenterCoord, Player.LocalPlayer.Handle, 31);
        }
        public static Entity GetEntityPlayerPointsAt(float distance)
        {
            var fingerCoord = RAGE.Elements.Player.LocalPlayer.GetBoneCoords(26613, 0f, 0f, 0f);
            var screenCenterCoord = fingerCoord.MinimizeDistance(GetWorldCoordFromScreenCoord(0.5f, 0.5f), distance);

            if (Settings.Other.RaytraceEnabled)
                RAGE.Game.Graphics.DrawLine(fingerCoord.X, fingerCoord.Y, fingerCoord.Z, screenCenterCoord.X, screenCenterCoord.Y, screenCenterCoord.Z, 0, 255, 0, 255);

            return GetEntityByRaycast(fingerCoord, screenCenterCoord, Player.LocalPlayer.Handle, 31);
        }

        public static bool PlayerInFrontOfVehicle(RAGE.Elements.Vehicle vehicle, float radius = 2f)
        {
            var leftFront = vehicle.GetWorldPositionOfBone(vehicle.GetBoneIndexByName("suspension_lf"));
            var rightFront = vehicle.GetWorldPositionOfBone(vehicle.GetBoneIndexByName("suspension_rf"));

            var leftBack = vehicle.GetWorldPositionOfBone(vehicle.GetBoneIndexByName("suspension_lr"));
            var rightBack = vehicle.GetWorldPositionOfBone(vehicle.GetBoneIndexByName("suspension_rr"));

            var playerPos = Player.LocalPlayer.Position;

            if (Vector3.Distance(playerPos, leftFront) <= radius || Vector3.Distance(playerPos, rightFront) <= radius)
                return true;
            else if (Vector3.Distance(playerPos, leftBack) <= radius || Vector3.Distance(playerPos, rightBack) <= radius)
                return false;

            return false;
        }

        public static bool AnyOnFootMovingControlPressed()
        {
            if (RAGE.Game.Pad.IsControlPressed(0, 32) || RAGE.Game.Pad.IsControlPressed(0, 33) || RAGE.Game.Pad.IsControlPressed(0, 34) || RAGE.Game.Pad.IsControlPressed(0, 35) || RAGE.Game.Pad.IsControlPressed(0, 22) || RAGE.Game.Pad.IsControlPressed(0, 44) || RAGE.Game.Pad.IsControlPressed(0, 75))
                return true;

            return false;
        }

        public static bool IsPlayerFamiliar(Player player, bool fractionToo = true)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);
            var tData = Sync.Players.GetData(player);

            if (pData == null || tData == null)
                return false;

            if (pData.CID == tData.CID)
                return true;

            if (fractionToo)
            {
                return pData.Familiars.Contains(tData.CID) || pData.Fraction == tData.Fraction && pData.Fraction != Sync.Players.FractionTypes.None;
            }
            else
            {
                return pData.Familiars.Contains(tData.CID);
            }
        }

        public static string GetPlayerName(Player player, bool familiarOnly = true, bool dontMask = true, bool includeId = false)
        {
            var pData = Sync.Players.GetData(player);

            if (pData == null)
                return includeId ? Locale.General.Players.MaleNameDefault + string.Format(Locale.General.Players.Id, player.RemoteId) : Locale.General.Players.MaleNameDefault;

            string name = familiarOnly ? (player.IsFamilliar() && (dontMask || !pData.Masked) ? player.Name : pData.Sex ? Locale.General.Players.MaleNameDefault : Locale.General.Players.FemaleNameDefault) : player.Name;

            if (includeId)
                return name + " " + string.Format(Locale.General.Players.Id, player.RemoteId);
            else
                return name;
        }

        public static bool IsCar(Vehicle vehicle)
        {
            int type = vehicle.GetClass();

            if (type == 8 || type == 13 || type == 14 || type == 15 || type == 16)
                return false;

            return true;
        }

        public static bool IsBike(Vehicle vehicle) => vehicle.GetClass() == 8;

        public static bool IsBoat(Vehicle vehicle) => vehicle.GetClass() == 14;
        public static bool IsHelicopter(Vehicle vehicle) => vehicle.GetClass() == 15;
        public static bool IsPlane(Vehicle vehicle) => vehicle.GetClass() == 16;

        public static List<T> ConvertJArrayToList<T>(Newtonsoft.Json.Linq.JArray jArray) => jArray.ToObject<List<T>>();
        public static Dictionary<T1, T2> ConvertJArrayToDictionary<T1, T2>(Newtonsoft.Json.Linq.JArray jArray) => jArray.ToObject<Dictionary<T1, T2>>();

        private static Regex MailPattern = new Regex(@"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$", RegexOptions.Compiled);
        private static Regex LoginPattern = new Regex(@"^(?=.*[a-zA-Z0-9])[0-9a-zA-Z!@#$%^&*]{6,12}$", RegexOptions.Compiled);
        private static Regex NamePattern = new Regex(@"^[A-Z]{1}[a-zA-Z]{1,9}$", RegexOptions.Compiled);
        private static Regex PasswordPattern = new Regex(@"^(?=.*[a-zA-Z0-9])[0-9a-zA-Z!@#$%^&*]{6,64}$", RegexOptions.Compiled);

        public static bool IsMailValid(string str) => MailPattern.IsMatch(str);
        public static bool IsLoginValid(string str) => LoginPattern.IsMatch(str);
        public static bool IsPasswordValid(string str) => PasswordPattern.IsMatch(str);
        public static bool IsNameValid(string str) => NamePattern.IsMatch(str);

        public static async System.Threading.Tasks.Task RequestAnimDict(string name)
        {
            if (RAGE.Game.Streaming.HasAnimDictLoaded(name))
                return;

            RAGE.Game.Streaming.RequestAnimDict(name);

            while (!RAGE.Game.Streaming.HasAnimDictLoaded(name))
                await RAGE.Game.Invoker.WaitAsync(25);
        }

        public static async System.Threading.Tasks.Task RequestClipSet(string name)
        {
            if (RAGE.Game.Streaming.HasClipSetLoaded(name))
                return;

            RAGE.Game.Streaming.RequestClipSet(name);

            while (!RAGE.Game.Streaming.HasClipSetLoaded(name))
                await RAGE.Game.Invoker.WaitAsync(25);
        }

        public static async System.Threading.Tasks.Task RequestModel(uint hash)
        {
            if (RAGE.Game.Streaming.HasModelLoaded(hash))
                return;

            RAGE.Game.Streaming.RequestModel(hash);

            while (!RAGE.Game.Streaming.HasModelLoaded(hash))
                await RAGE.Game.Invoker.WaitAsync(25);
        }

        public static async System.Threading.Tasks.Task RequestPtfx(string name)
        {
            if (RAGE.Game.Streaming.HasNamedPtfxAssetLoaded(name))
            {
                RAGE.Game.Graphics.UseParticleFxAssetNextCall(name);

                return;
            }

            RAGE.Game.Streaming.RequestNamedPtfxAsset(name);

            while (!RAGE.Game.Streaming.HasNamedPtfxAssetLoaded(name))
                await RAGE.Game.Invoker.WaitAsync(25);

            RAGE.Game.Graphics.UseParticleFxAssetNextCall(name);
        }

        /// <summary>Метод проверяет, активен ли у локального игрока вид от первого лица</summary>
        public static bool IsFirstPersonActive() => RAGE.Game.Cam.GetFollowPedCamViewMode() == 4;

        public static bool CanShowCEF(bool checkCursor = true, bool checkPause = true) => (checkCursor ? !CEF.Cursor.IsVisible : true) && (checkPause ? !RAGE.Game.Ui.IsPauseMenuActive() : true);

        /// <summary>Получить локальное время на ПК</summary>
        public static DateTime GetLocalTime() => DateTime.Now;
        /// <summary>Получить серверное время</summary>
        public static DateTime GetServerTime() => DateTime.UtcNow.AddHours(3);

        public static void ConsoleOutput(object obj, bool line = true)
        {
            if (line)
                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
            else
                RAGE.Ui.Console.Log(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
        }

        public static void ConsoleOutputLimited(object obj, bool line = true, int ms = 2000)
        {
            if (DateTime.Now.Subtract(LastConsoleMsg).TotalMilliseconds < ms)
                return;

            LastConsoleMsg = DateTime.Now;

            if (line)
                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
            else
                RAGE.Ui.Console.Log(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
        }

        public static bool IsAnyCefActive(bool checkChatInput = true) => (checkChatInput && CEF.Chat.InputVisible) || CEF.Browser.IsAnyCEFActive;

        /// <summary>Получить handle блипа метки на карте</summary>
        /// <returns>0, если не найдена</returns>
        public static int GetWaypointBlip()
        {
            if (RAGE.Game.Ui.IsWaypointActive())
            {
                var blipIterator = RAGE.Game.Invoker.Invoke<int>(0x186E5D252FA50E7D); // GetWaypointBlipEnumId

                var firstBlip = RAGE.Game.Ui.GetFirstBlipInfoId(blipIterator);
                var nextBlip = RAGE.Game.Ui.GetNextBlipInfoId(blipIterator);

                for (int i = firstBlip; RAGE.Game.Ui.DoesBlipExist(i); i = nextBlip)
                {
                    if (RAGE.Game.Ui.GetBlipInfoIdType(i) == 4)
                    {
                        return i;
                    }
                }
            }

            return 0;
        }

        /// <summary>Получить координаты текущей метки на карте</summary>
        /// <returns>null, если метка не стоит</returns>
        public static Vector3 GetWaypointPosition()
        {
            var blip = GetWaypointBlip();

            if (blip == 0)
                return null;

            return RAGE.Game.Ui.GetBlipInfoIdCoord(blip);
        }

        public static Vector3 RotatePoint(Vector3 point, Vector3 originPoint, float angle)
        {
            angle = (float)(angle * Math.PI / 180);

            float x = point.X, y = point.Y;
            float cos = (float)Math.Cos(angle), sin = (float)Math.Sin(angle);

            point.X = cos * (x - originPoint.X) - sin * (y - originPoint.Y) + originPoint.X;
            point.Y = sin * (x - originPoint.X) + cos * (y - originPoint.Y) + originPoint.Y;

            return point;
        }

        /// <summary>Метод для замены символа \n в строке на тег /br</summary>
        /// <param name="text"></param>
        public static string ReplaceNewLineHtml(string text) => text.Replace("\n", "</br>");

        public enum Actions
        {
            Knocked = 0, Frozen,
            InVehicle,
            InWater, HasWeapon, Crouch, Crawl, Shooting, Climbing,
            Cuffed, Falling, Jumping, Ragdoll, Scenario, OtherAnimation, Animation, FastAnimation, PushingVehicle, OnFoot, Reloading, Finger,
            HasItemInHands, IsAttachedTo,
        }

        private static Dictionary<Actions, Func<bool>> ActionsFuncs = new Dictionary<Actions, Func<bool>>()
        {
            { Actions.Knocked, () => Sync.Players.GetData(Player.LocalPlayer)?.IsKnocked ?? false },

            { Actions.Frozen, () => Sync.Players.GetData(Player.LocalPlayer)?.IsFrozen ?? false},

            { Actions.Crouch, () => Crouch.Toggled },

            { Actions.Crawl, () => Crawl.Toggled },

            { Actions.Finger, () => Finger.Toggled },

            { Actions.PushingVehicle, () => PushVehicle.Toggled },

            {
                Actions.OtherAnimation, () =>
                {
                    var data = Sync.Players.GetData(Player.LocalPlayer);

                    if (data == null)
                        return true;

                    return data.OtherAnim != Animations.OtherTypes.None;
                }
            },

            {
                Actions.Animation, () =>
                {
                    var data = Sync.Players.GetData(Player.LocalPlayer);

                    if (data == null)
                        return true;

                    return data.GeneralAnim != Animations.GeneralTypes.None;
                }
            },

            {
                Actions.FastAnimation, () =>
                {
                    var data = Sync.Players.GetData(Player.LocalPlayer);

                    if (data == null)
                        return true;

                    return data.FastAnim != Animations.FastTypes.None;
                }
            },

            { Actions.Scenario, () => false },

            { Actions.InVehicle, () => Player.LocalPlayer.IsInAnyVehicle(true) || Player.LocalPlayer.IsInAnyVehicle(false) },

            { Actions.InWater, () => Player.LocalPlayer.IsInWater() || Player.LocalPlayer.IsDiving() },

            { Actions.HasWeapon, () => Player.LocalPlayer.HasWeapon() },

            { Actions.Shooting, () => Player.LocalPlayer.IsShooting() },

            { Actions.Cuffed, () => Player.LocalPlayer.IsCuffed() },

            { Actions.Climbing, () => Player.LocalPlayer.IsClimbing() },

            { Actions.Falling, () => Player.LocalPlayer.IsFalling() || Player.LocalPlayer.IsJumpingOutOfVehicle() || Player.LocalPlayer.IsInParachuteFreeFall() },

            { Actions.Jumping, () => Player.LocalPlayer.IsJumping() },

            { Actions.Ragdoll, () => Player.LocalPlayer.IsRagdoll() },

            { Actions.OnFoot, () => !Player.LocalPlayer.IsOnFoot() },

            { Actions.Reloading, () => WeaponSystem.Reloading },

            { Actions.IsAttachedTo, () => Player.LocalPlayer.GetAttachedTo() > 0 },

            { Actions.HasItemInHands, () => Player.LocalPlayer.GetData<List<Sync.AttachSystem.AttachmentObject>>(Sync.AttachSystem.AttachedObjectsKey)?.Where(x => !Sync.AttachSystem.StaticObjectsTypes.Contains(x.Type)).Any() ?? false },
        };

        /// <summary>Метод для проверки, может ли локальный игрок делать что-либо в данный момент</summary>
        /// <returns>Возврвает true, есле выполняются следующие условия, false - в противном случае</returns>
        public static bool CanDoSomething(params Actions[] actions)
        {
            /*          
                var atc = new Utils.Actions[]
                {
                    Utils.Actions.Knocked,
                    Utils.Actions.Frozen,
                    Utils.Actions.Cuffed,

                    Utils.Actions.Crouch,
                    Utils.Actions.Crawl,
                    Utils.Actions.Finger,
                    Utils.Actions.PushingVehicle,

                    Utils.Actions.Animation,
                    Utils.Actions.FastAnimation,
                    Utils.Actions.CustomAnimation,
                    Utils.Actions.Scenario,
                    Utils.Actions.CustomScenario,
                            
                    Utils.Actions.IsAttachedTo,
                    Utils.Actions.HasItemInHands,

                    Utils.Actions.InVehicle,
                    Utils.Actions.InWater,
                    Utils.Actions.Shooting, Utils.Actions.Reloading, Utils.Actions.HasWeapon,
                    Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.OnFoot,
                };
            */

            foreach (var x in actions)
                if (ActionsFuncs[x].Invoke())
                    return false;

            return true;
        }

        /// <summary>Метод для исполнения кода в JS версии RAGE</summary>
        /// <param name="code">Код</param>
        public static void JsEval(string code) => Events.CallLocal("RAGE::Eval", code);

        /// <summary>Метод для исполнения кода в JS версии RAGE</summary>
        /// <remarks>Код, который выполнит данная версия метода, обязан возвращать значение! Для этого в коде необходимо завести переменную returnValue и присваивать ей значение</remarks>
        /// <param name="code">Код</param>
        public static async System.Threading.Tasks.Task<T> JsEval<T>(string code)
        {
            Additional.Storage.LastData = null;
            Additional.Storage.GotData = false;

            Events.CallLocal("RAGE::Eval", code + "mp.events.callLocal(\"Storage::Temp\", JSON.stringify(returnValue));");

            while (!Additional.Storage.GotData)
                await RAGE.Game.Invoker.WaitAsync(25);

            return Additional.Storage.LastData != null ? RAGE.Util.Json.Deserialize<T>(Additional.Storage.LastData) : default(T);
        }

        /// <summary>Метод для исполнения кода в JS версии RAGE</summary>
        /// <remarks>Данный метод отличается от обычного наличием приема параметров, которые сериализируются в JSON строки</remarks>
        /// <param name="function">Название функции</param>
        /// <param name="args">Аргументы</param>
        public static void JsEval(string function, params object[] args) => Events.CallLocal("RAGE::Eval", $"{function}({string.Join(", ", args.Select(x => RAGE.Util.Json.Serialize(x)))});");

        /// <summary>Метод для исполнения кода в JS версии RAGE</summary>
        /// <remarks>Данный метод отличается от обычного наличием приема параметров, которые сериализируются в JSON строки</remarks>
        /// <param name="function">Название функции</param>
        /// <param name="args">Аргументы</param>
        public static async System.Threading.Tasks.Task<T> JsEval<T>(string function, params object[] args)
        {
            Additional.Storage.LastData = null;
            Additional.Storage.GotData = false;

            Events.CallLocal("RAGE::Eval", $"mp.events.callLocal(\"Storage::Temp\", JSON.stringify({function}({string.Join(", ", args.Select(x => RAGE.Util.Json.Serialize(x)))})));");

            while (!Additional.Storage.GotData)
                await RAGE.Game.Invoker.WaitAsync(25);

            return Additional.Storage.LastData != null ? RAGE.Util.Json.Deserialize<T>(Additional.Storage.LastData) : default(T);
        }

        /// <summary>Метод для исполнения нативных функций через JS версию RAGE</summary>
        /// <remarks>Использовать в случае неработоспособности некоторых нативных функций через C# версию RAGE</remarks>
        /// <param name="hash">Хэш функции</param>
        /// <param name="args">Аргументы</param>
        public static void InvokeViaJs(ulong hash, params object[] args)
        {
            if (args.Length > 0)
                Events.CallLocal("RAGE::Eval", $"mp.game.invoke('{string.Format("0x{0:X}", hash)}', {string.Join(", ", args.Select(x => RAGE.Util.Json.Serialize(x)))});");
            else
                Events.CallLocal("RAGE::Eval", $"mp.game.invoke('{string.Format("0x{0:X}", hash)}');");
        }

        /// <inheritdoc cref="InvokeViaJs(ulong, object[])"></inheritdoc>
        public static void InvokeViaJs(RAGE.Game.Natives hash, params object[] args) => InvokeViaJs((ulong)hash, args);

        public static async System.Threading.Tasks.Task<float> InvokeFloatViaJs(ulong hash, params object[] args)
        {
            Additional.Storage.LastData = null;
            Additional.Storage.GotData = false;

            if (args.Length > 0)
                Events.CallLocal("RAGE::Eval", $"mp.events.callLocal(\"Storage::Temp\", JSON.stringify(mp.game.invokeFloat('{string.Format("0x{0:X}", hash)}', {string.Join(", ", args.Select(x => RAGE.Util.Json.Serialize(x)))})));");
            else
                Events.CallLocal("RAGE::Eval", $"mp.events.callLocal(\"Storage::Temp\", JSON.stringify(mp.game.invokeFloat('{string.Format("0x{0:X}", hash)}')));");

            while (!Additional.Storage.GotData)
                await RAGE.Game.Invoker.WaitAsync(25);

            Utils.ConsoleOutputLimited(Additional.Storage.LastData);

            return Additional.Storage.LastData != null ? RAGE.Util.Json.Deserialize<float>(Additional.Storage.LastData) : 0f;
        }

        public static async System.Threading.Tasks.Task<float> InvokeFloatViaJs(RAGE.Game.Natives hash, params object[] args) => await InvokeFloatViaJs((ulong)hash, args);

        /// <summary>Функция для отрисовки текста на экране</summary>
        /// <remarks>Вызов необходимо осуществлять каждый кадр!</remarks>
        /// <param name="text">Текст</param>
        /// <param name="x">Координата по оси X (от 0 до 1)</param>
        /// <param name="y">Координата по оси X (от 0 до 1)</param>
        /// <param name="red">R</param>
        /// <param name="green">G</param>
        /// <param name="blue">B</param>
        /// <param name="alpha">Непрозрачность</param>
        /// <param name="scale">Масштаб</param>
        /// <param name="fontType">Шрифт</param>
        /// <param name="outline">Обводка</param>
        public static void DrawText(string text, float x, float y, byte red = 255, byte green = 255, byte blue = 255, byte alpha = 255, float scale = 0.4f, ScreenTextFontTypes fontType = ScreenTextFontTypes.CharletComprimeColonge, bool outline = true, bool center = true)
        {
            RAGE.Game.Ui.SetTextFont((int)fontType);
            RAGE.Game.Ui.SetTextCentre(center);
            RAGE.Game.Ui.SetTextColour(red, green, blue, alpha);
            RAGE.Game.Ui.SetTextScale(scale, scale);
            RAGE.Game.Ui.SetTextWrap(0f, 1f);

            if (outline)
                RAGE.Game.Ui.SetTextOutline();

            RAGE.Game.Ui.BeginTextCommandDisplayText("STRING");

            RAGE.Game.Ui.AddTextComponentSubstringPlayerName(text);

            RAGE.Game.Ui.EndTextCommandDisplayText(x, y, 0);
        }

        //public static void DrawText(string text, float x, float y, byte red = 255, byte green = 255, byte blue = 255, byte alpha = 255, float scale = 0.4f, ScreenTextFontTypes fontType = ScreenTextFontTypes.Pricedown, bool outline = true) => JsEval($"mp.game.graphics.drawText('{text}', [{x.ToString(Settings.CultureInfo)}, {y.ToString(Settings.CultureInfo)}], {{ font: {(int)fontType}, color: [{red}, {green}, {blue}, {alpha}], scale: [{scale.ToString(Settings.CultureInfo)}, {scale.ToString(Settings.CultureInfo)}], outline: {(outline ? "true" : "false")} }});");

        /// <summary>Метод для перезагрузки голосового чата со стороны RAGE</summary>
        public static void ReloadVoiceChat() => JsEval("try { mp.voiceChat.cleanupAndReload(true, false, false) } catch {} try { mp.voiceChat.cleanupAndReload(false, false, true) } catch {} try { mp.voiceChat.cleanupAndReload(true, true, true) } catch {}");

        /// <summary>Метод для получения координат, на которые смотрит игровая камера игрока</summary>
        /// <param name="distance">Дистанция</param>
        public static Vector3 GetCoordsFromCamera(float distance)
        {
            var coord = RAGE.Game.Cam.GetGameplayCamCoord();

            var rotation = RAGE.Game.Cam.GetGameplayCamRot(0);

            var tX = rotation.X * 0.0174532924f;
            var tZ = rotation.Z * 0.0174532924f;

            var num = (float)Math.Abs(Math.Cos(tX)) + distance;

            return new Vector3(coord.X + (float)(-Math.Sin(tZ)) * num, coord.Y + (float)Math.Cos(tZ) * num, coord.Z + (float)Math.Sin(tX) * 8.0f);
        }

        public static Vector3 GetWorldCoordFromScreenCoord(float x, float y, float maxDistance = 100f) => GetWorldCoordFromScreenCoord(RAGE.Game.Cam.GetGameplayCamCoord(), RAGE.Game.Cam.GetGameplayCamRot(0), x, y, maxDistance);

        /// <summary>Метод для преобразования координаты на экране в игровую координату</summary>
        /// <param name="camPos">Позиция камеры</param>
        /// <param name="camRot">Вектор вращения камеры</param>
        /// <param name="coord">2D координата на экране (коэфициенты! например, при X = 960, а Y = 1080, а текущее разрешение 1920x1080 - передавать X = 0.5, Y = 1</param>
        /// <param name="maxDistance">Максимальная дистанция</param>
        public static Vector3 GetWorldCoordFromScreenCoord(Vector3 camPos, Vector3 camRot, float x, float y, float maxDistance = 100f)
        {
            var camForward = RotationToDirection(camRot);

            var rotUp = camRot + new Vector3(maxDistance, 0, 0);
            var rotDown = camRot + new Vector3(-maxDistance, 0, 0);
            var rotLeft = camRot + new Vector3(0, 0, -maxDistance);
            var rotRight = camRot + new Vector3(0, 0, maxDistance);

            var camRight = RotationToDirection(rotRight) - RotationToDirection(rotLeft);
            var camUp = RotationToDirection(rotUp) - RotationToDirection(rotDown);

            var rollRad = -DegreesToRadians(camRot.Y);

            var camRightRoll = camRight * (float)Math.Cos(rollRad) - camUp * (float)Math.Sin(rollRad);
            var camUpRoll = camRight * (float)Math.Sin(rollRad) + camUp * (float)Math.Cos(rollRad);

            var point3D = camPos + camForward * maxDistance + camRightRoll + camUpRoll;

            float point2d_x = 0, point2d_y = 0;

            if (!GetScreenCoordFromWorldCoord(point3D, ref point2d_x, ref point2d_y))
                return camPos + camForward * maxDistance;

            var point3DZero = camPos + camForward * maxDistance;

            float point2d_zero_x = 0, point2d_zero_y = 0;

            if (!GetScreenCoordFromWorldCoord(point3DZero, ref point2d_zero_x, ref point2d_zero_y))
                return camPos + camForward * maxDistance;

            const double eps = 0.001;

            if (Math.Abs(point2d_x - point2d_zero_x) < eps || Math.Abs(point2d_y - point2d_zero_y) < eps)
                return camPos + camForward * maxDistance;

            var scaleX = (x - point2d_zero_x) / (point2d_x - point2d_zero_x);
            var scaleY = (y - point2d_zero_y) / (point2d_y - point2d_zero_y);

            return camPos + camForward * maxDistance + camRightRoll * scaleX + camUpRoll * scaleY;
        }

        /// <summary>Метод для полуечения противоположного угла</summary>
        /// <param name="angle">Угол</param>
        public static float GetOppositeAngle(float angle) => (angle + 180) % 360;

        /// <summary>Метод для преобразования вектора поворота в вектор направления</summary>
        /// <param name="rotation">Вектор вращения</param>
        /// <returns></returns>
        public static Vector3 RotationToDirection(Vector3 rotation)
        {
            var z = DegreesToRadians(rotation.Z);
            var x = DegreesToRadians(rotation.X);

            var num = Math.Abs(Math.Cos(x));

            return new Vector3((float)(-Math.Sin(z) * num), (float)(Math.Cos(z) * num), (float)Math.Sin(x));
        }

        /// <summary>Метод для преобразования градусов в радианы</summary>
        /// <param name="degrees">Градусы</param>
        public static float DegreesToRadians(float degrees) => (float)(Math.PI / 180f) * degrees;

        /// <summary>Метод для преобразования радиан в градусы</summary>
        /// <param name="radians">Радианы</param>
        public static float RadiansToDegrees(float radians) => (float)(180f / Math.PI) * radians;

        /// <summary>Получить позицию на экране точки в игровом пространстве</summary>
        /// <remarks>Лучше не использовать для рендера, при каждом вызове создает объект класса Vector2</remarks>
        /// <param name="pos">Позиция</param>
        /// <returns>Vector2 - успешно, null - в противном случае</returns>
        public static RAGE.Ui.Cursor.Vector2 GetScreenCoordFromWorldCoord(Vector3 pos)
        {
            RAGE.Ui.Cursor.Vector2 result = new RAGE.Ui.Cursor.Vector2(0f, 0f);

            if (!RAGE.Game.Graphics.GetScreenCoordFromWorldCoord(pos.X, pos.Y, pos.Z, ref result.X, ref result.Y))
                return null;

            return result;
        }

        /// <summary>Получить позицию на экране точки в игровом пространстве</summary>
        /// <remarks>Облегченная версия метода, использовать для рендера</remarks>
        /// <param name="pos">Позиция</param>
        /// <returns>true - успешно, false - в противном случае</returns>
        public static bool GetScreenCoordFromWorldCoord(Vector3 pos, ref float x, ref float y) => RAGE.Game.Graphics.GetScreenCoordFromWorldCoord(pos.X, pos.Y, pos.Z, ref x, ref y);

        public static void DrawSphere(Vector3 pos, float radius, byte r, byte g, byte b, float opacity) => RAGE.Game.Invoker.Invoke(0x799017F9E3B10112, pos.X, pos.Y, pos.Z, radius, r, g, b, opacity);

        public static string GetStreetName(Vector3 pos)
        {
            int streetNameHash = 0, crossingRoadNameHash = 0;

            RAGE.Game.Pathfind.GetStreetNameAtCoord(pos.X, pos.Y, pos.Z, ref streetNameHash, ref crossingRoadNameHash);

            return RAGE.Game.Ui.GetStreetNameFromHashKey((uint)streetNameHash) ?? "null";
        }

        public static float GetGroundZCoord(Vector3 pos, bool ignoreWater = false)
        {
            float z = pos.Z;

            if (RAGE.Game.Misc.GetGroundZFor3dCoord(pos.X, pos.Y, pos.Z, ref z, ignoreWater))
                return z;

            return pos.Z;
        }

        public static void ResetGameplayCameraRotation()
        {
            RAGE.Game.Cam.SetGameplayCamRelativeHeading(0f);

            RAGE.Game.Invoker.Invoke(0x48608C3464F58AB4, 0f, 0f, 0f);
        }
    }

    public static class Extensions
    {
        public static async System.Threading.Tasks.Task<bool> WaitIsLoaded(this GameEntity gEntity)
        {
            await RAGE.Game.Invoker.WaitAsync(500);

            int handle = -1;

            do
            {
                if (gEntity?.Exists != true)
                    return false;

                await RAGE.Game.Invoker.WaitAsync(25);

                handle = gEntity?.Handle ?? -1;
            }
            while (handle <= 0);

            return true;
        }

        /// <summary>Получить серверную информацию сущности</summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="entity">Сущность</param>
        /// <param name="key">Ключ</param>
        /// <param name="otherwise">Возвращаемая информация в случае отсутствия у сущности таковой</param>
        /// <return>Информация о сущности по ключу (если таковой нет - T otherwise)</return>
        public static T GetSharedData<T>(this Entity entity, string key, T otherwise = default(T)) => (T)(entity.GetSharedData(key) ?? otherwise);

        /// <summary>Проверить, имеет ли сущность информацию</summary>
        /// <param name="entity">Сущность</param>
        /// <param name="key">Ключ</param>
        /// <return>true / false</return>
        public static bool HasSharedData(this Entity entity, string key) => entity.GetSharedData(key) != null;

        public static float GetSpeedKm(this Vehicle vehicle) => vehicle.GetSpeed() * 3.6f;

        /// <summary>Получить настоящий уровень здоровья игрока</summary>
        /// <remarks>Метод отнимает 100 от игрового уровня здоровья</remarks>
        /// <param name="player">Игрок</param>
        /// <returns>Значение от 0 до int.MaxValue - 100</returns>
        public static int GetRealHealth(this Player player)
        {
            var hp = player.GetHealth() - 100;

            return hp < 0 ? 0 : hp;
        }

        /// <summary>Устанавливает настоящий уровень здоровья игроку</summary>
        /// <remarks>Метод прибавляет 100 к передаваемому значению</remarks>
        /// <param name="player">Игрок</param>
        /// <param name="value">Значение</param>
        public static void SetRealHealth(this Player player, int value)
        {
            if (value <= 0)
                player.SetHealth(0);
            else
                player.SetHealth(100 + value);
        }

        /// <summary>Проверка DateTime на спам</summary>
        /// <param name="dt">DateTime</param>
        /// <param name="timeout">Таймаут</param>
        /// <param name="updateTime">Обновить ли переданный DateTime на актуальный?</param>
        /// <param name="notify">Уведомить ли игрока о том, чтобы он подождал?</param>
        public static bool IsSpam(this ref DateTime dt, int timeout = 500, bool updateTime = false, bool notify = false) => CEF.Notification.SpamCheck(ref dt, timeout, updateTime, notify);

        public static bool IsFamilliar(this Player player, bool fractionToo = true) => Utils.IsPlayerFamiliar(player, fractionToo);

        public static string GetName(this Player player, bool familiarOnly = true, bool dontMask = true, bool includeId = false) => Utils.GetPlayerName(player, familiarOnly, dontMask, includeId);

        public static bool IsEntityNear(this Entity entity, float maxDistance) => Player.LocalPlayer.Dimension == entity.Dimension && Vector3.Distance(Player.LocalPlayer.Position, entity.Position) <= maxDistance;

        public static bool IsStreamed(this MapObject obj) => RAGE.Elements.Entities.Objects.Streamed.Contains(obj);

        /// <summary>Конвертировать строку HEX в Color</summary>
        /// <param name="colour"></param>
        public static Utils.Colour ToColour(this string colour) => new Utils.Colour(colour);

        /// <summary>Найти расстояние между двумя точками в 2D пространстве</summary>
        /// <param name="pos1">Точка 1</param>
        /// <param name="pos2">Точка 2</param>
        public static float Distance(this RAGE.Ui.Cursor.Vector2 pos1, RAGE.Ui.Cursor.Vector2 pos2) => (float)Math.Sqrt((float)Math.Pow(pos1.X - pos2.X, 2) + (float)Math.Pow(pos1.Y - pos2.Y, 2));

        /// <summary>Найти середину между двумя точками в 3D пространстве</summary>
        /// <param name="pos1">Точка 1</param>
        /// <param name="pos2">Точка 2</param>
        public static Vector3 GetMiddle(this Vector3 pos1, Vector3 pos2) => new Vector3((pos1.X + pos2.X) / 2f, (pos1.Y + pos2.Y) / 2f, (pos1.Z + pos2.Z) / 2f);

        /// <summary>Уменьшить расстояние между двумя точками до максимально возможной</summary>
        /// <param name="pos1">Точка 1 (источник)</param>
        /// <param name="pos2">Точка 2 (конечная)</param>
        /// <return>Измененная (если требуется) точка 2 (конечная), которая будет ближе к точке 1 (источник)</return>
        public static Vector3 MinimizeDistance(this Vector3 pos1, Vector3 pos2, float maxDistance)
        {
            float distance = pos1.DistanceTo(pos2);

            if (distance > maxDistance)
            {
                var vector = pos2 - pos1;

                pos2 = pos1 + (vector * (maxDistance / (distance - maxDistance)));
            }

            return pos2;
        }

        /// <summary>Найти расстояние между двумя точками в 3D пространстве</summary>
        /// <remarks>Игнорирует ось Z</remarks>
        /// <param name="pos1">Точка 1</param>
        /// <param name="pos2">Точка 2</param>
        public static float DistanceIgnoreZ(this Vector3 pos1, Vector3 pos2) => (float)Math.Sqrt((float)Math.Pow(pos1.X - pos2.X, 2) + (float)Math.Pow(pos1.Y - pos2.Y, 2));

        /// <summary>Выполнить функцию в окне браузера</summary>
        /// <param name="window">Окно</param>
        /// <param name="function">Функция</param>
        /// <param name="args">Аргументы (если передается массив или IEnumerable и для передачи подразумевается именно массив, то нужно дополнительно обернуть его в new object[] { arr }</param>
        public static void ExecuteJs(this RAGE.Ui.HtmlWindow window, string function, params object[] args) => window?.ExecuteJs($"{function}({string.Join(", ", args.Select(x => RAGE.Util.Json.Serialize(x)))});");

        /// <summary>Выполнить кэшируемую функцию в окне браузера</summary>
        /// <param name="window">Окно</param>
        /// <param name="function">Функция</param>
        /// <param name="args">Аргументы (если передается массив или IEnumerable и для передачи подразумевается именно массив, то нужно дополнительно обернуть его в new object[] { arr }</param>
        /// <remarks>Использовать только для ОДИНАКОВЫХ функций, которые часто выполняются и содержат ОДИНАКОВЫЕ аргументы, в противном случае использовать обычный ExecuteJs</remarks>
        public static void ExecuteCachedJs(this RAGE.Ui.HtmlWindow window, string function, params object[] args) => window?.ExecuteCachedJs($"{function}({string.Join(", ", args.Select(x => RAGE.Util.Json.Serialize(x)))});");

        /// <summary>Проверка, держит ли игрок в руках оружие (игнорируется кулак и телефон)</summary>
        /// <param name="player"></param>
        public static bool HasWeapon(this RAGE.Elements.Player player)
        {
            var weapon = player.GetSelectedWeapon();

            return weapon != Sync.WeaponSystem.UnarmedHash && weapon != Sync.WeaponSystem.MobileHash;
        }

        public static (string Name, string Surname) GetNameSurname(this Player player)
        {
            if (player == null || (player.Name?.Length ?? 0) < 2)
                return ("null", "null");

            var name = player.Name.Split(' ').ToArray();

            if (name.Length >= 2)
                return (name[0], name[1]);
            else
                return ("null", "null");
        }

        /// <summary>Метод для получения пределов размера модели сущности</summary>
        /// <remarks>Необходимо посчитать (Maximum - Minimum) для получения размера модели, где Y - величина модели в длину. Если нужно получить сразу размер модели - использовать GetModelSize()</remarks>
        /// <param name="entity">Сущность</param>
        /// <returns>Vector3 минимума и Vector3 максимума</returns>
        public static (Vector3 Minimum, Vector3 Maximum) GetModelDimensions(this Entity entity)
        {
            Vector3 min = new Vector3(0f, 0f, 0f);
            Vector3 max = new Vector3(0f, 0f, 0f);

            RAGE.Game.Misc.GetModelDimensions(entity.Model, min, max);

            return (min, max);
        }

        /// <summary>Метод для получения размера модели сущности</summary>
        /// <param name="entity">Сущность</param>
        /// <returns>Vector3, где X - размер в ширину, Y - в длину, Z - в высоту</returns>
        public static Vector3 GetModelSize(this Entity entity)
        {
            var dims = entity.GetModelDimensions();

            return dims.Maximum - dims.Minimum;
        }

        /// <summary>Метод для получения радиуса модели сущности</summary>
        /// <remarks>Фактически, получает максимальное значение из размеров по осям, полученных через GetModelSize</remarks>
        /// <param name="entity">Сущность</param>
        /// <returns>Радиус сущности</returns>
        public static float GetModelRange(this Entity entity)
        {
            var size = entity.GetModelSize();

            return size.X >= size.Y && size.X >= size.Z ? size.X : (size.Y >= size.X && size.Y >= size.Z ? size.Y : size.Z);
        }

        /// <summary>Метод для получения позиции сущности на экране</summary>
        /// <param name="entity">Сущность</param>
        /// <returns>Vector2 - успешно, null - в противном случае</returns>
        public static RAGE.Ui.Cursor.Vector2 GetScreenPosition(this Entity entity) => Utils.GetScreenCoordFromWorldCoord(entity.GetRealPosition());

        /// <summary>Метод для получения позиции сущности на экране</summary>
        /// <remarks>Облегченная версия метода, использовать для рендера</remarks>
        /// <param name="entity">Сущность</param>
        /// <returns>true - успешно, false - в противном случае</returns>
        public static bool GetScreenPosition(this Entity entity, ref float x, ref float y) => Utils.GetScreenCoordFromWorldCoord(entity.GetRealPosition(), ref x, ref y);

        public static Vector3 GetRealPosition(this Entity entity)
        {
            if (entity is Player player)
            {
                return player.GetCoords(false);
            }
            else if (entity is Vehicle vehicle)
            {
                return vehicle.GetCoords(false);
            }
            else if (entity is Ped ped)
            {
                return ped.GetCoords(false);
            }
            else if (entity is MapObject obj)
            {
                return obj.GetCoords(false);
            }

            return entity.Position;
        }

        public static void SetName(this Blip blip, string name)
        {
            if (blip == null)
                return;

            blip.SetData("Name", name);

            RAGE.Game.Ui.BeginTextCommandSetBlipName("STRING");

            RAGE.Game.Ui.AddTextComponentSubstringPlayerName(name);

            RAGE.Game.Ui.EndTextCommandSetBlipName(blip.Handle);
        }

        public static string GetName(this Blip blip) => blip.GetData<string>("Name");

        public static bool GetSex(this Player player) => player.Model == Utils.MP_MALE_MODEL;

        public static uint ToUInt32(this int value)
        {
            unchecked
            {
                return (uint)value;
            }
        }

        public static uint? ToUInt32(this int? value)
        {
            if (value == null)
                return null;

            unchecked
            {
                return (uint)value.Value;
            }
        }

        public static void SetLightColour(this MapObject mObj, Utils.Colour rgb) => RAGE.Game.Invoker.Invoke(0x5F048334B4A4E774, mObj.Handle, true, rgb.Red, rgb.Green, rgb.Blue);

        public static Utils.Colour GetNeonColour(this Vehicle veh)
        {
            int r = 0, g = 0, b = 0;

            veh.GetNeonLightsColour(ref r, ref g, ref b);

            return new Utils.Colour((byte)r, (byte)g, (byte)b);
        }

        public static Utils.Colour GetPrimaryColour(this Vehicle veh)
        {
            int r = 0, g = 0, b = 0;

            veh.GetCustomPrimaryColour(ref r, ref g, ref b);

            return new Utils.Colour((byte)r, (byte)g, (byte)b);
        }

        public static Utils.Colour GetSecondaryColour(this Vehicle veh)
        {
            int r = 0, g = 0, b = 0;

            veh.GetCustomSecondaryColour(ref r, ref g, ref b);

            return new Utils.Colour((byte)r, (byte)g, (byte)b);
        }

        public static int GetColourType(this Vehicle veh)
        {
            int t = 0, a = 0;

            veh.GetModColor1(ref t, ref a, ref a);

            return t;
        }

        public static void SetColourType(this Vehicle veh, int type)
        {
            int t = 0, a = 0;

            veh.GetExtraColours(ref t, ref a);

            veh.SetModColor1(type, 0, 0);

            veh.SetModColor2(type, 0);

            veh.SetExtraColours(t, a);
        }

        public static int? GetPearlColour(this Vehicle veh)
        {
            int t = 0, a = 0;

            veh.GetExtraColours(ref t, ref a);

            return t == 0 ? null : (int?)t;
        }

        public static int? GetWheelsColour(this Vehicle veh)
        {
            int t = 0, a = 0;

            veh.GetExtraColours(ref a, ref t);

            return t == 0 ? null : (int?)t;
        }

        public static void SetPearlColour(this Vehicle veh, int colour) => veh.SetExtraColours(colour, veh.GetWheelsColour() ?? 0);

        public static void SetWheelsColour(this Vehicle veh, int colour) => veh.SetExtraColours(veh.GetPearlColour() ?? 0, colour);

        public static void SetNeonEnabled(this Vehicle veh, bool state)
        {
            for (int i = 0; i < 4; i++)
                veh.SetNeonLightEnabled(i, state);
        }

        public static Utils.Colour GetTyreSmokeColour(this Vehicle veh)
        {
            int r = 0, g = 0, b = 0;

            veh.GetTyreSmokeColor(ref r, ref g, ref b);

            return new Utils.Colour((byte)r, (byte)g, (byte)b);
        }

        public static int? GetXenonColour(this Vehicle veh)
        {
            if (!veh.IsToggleModOn(22))
                return null;

            var colour = RAGE.Game.Invoker.Invoke<int>(0x3DFF319A831E0CDB, veh.Handle);

            if (colour == 255)
                return -1;

            return colour;
        }

        public static void SetXenonColour(this Vehicle veh, int? colour)
        {
            if (colour is int col)
            {
                veh.ToggleMod(22, true);

                RAGE.Game.Invoker.Invoke(0xE41033B25D003A07, veh.Handle, col);
            }
            else
            {
                veh.ToggleMod(22, false);
            }
        }

        public static void SetWheels(this Vehicle veh, int type, int num, bool front = true)
        {
            veh.SetWheelType(type);

            veh.SetMod(front ? 23 : 25, num, false);
        }

        public static string GetNumberplateText(this Vehicle veh) => veh.GetNumberPlateText()?.Replace(" ", "");

        public static void TaskTempAction(this Vehicle veh, int action, int time)
        {
            var driverPed = veh.GetPedInSeat(-1, 0);

            if (driverPed < 0)
                return;

            RAGE.Game.Invoker.Invoke(RAGE.Game.Natives.TaskVehicleTempAction, driverPed, veh.Handle, action, time);
        }

        public static int GetScriptTaskStatus(this PedBase ped, uint taskHash) => RAGE.Game.Invoker.Invoke<int>(RAGE.Game.Natives.GetScriptTaskStatus, ped.Handle, (int)taskHash);
    }
}
