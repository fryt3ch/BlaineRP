using System;
using System.Collections.Generic;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Components;
using BlaineRP.Client.Game.EntitiesData.Enums;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using Core = BlaineRP.Client.Game.World.Core;

namespace BlaineRP.Client.Game.Fractions
{
    public abstract partial class Fraction
    {
        public static string NoFractionStr = "Отсутствует";

        private static Dictionary<uint, string> PermissionNames = new Dictionary<uint, string>()
        {
            { 0, "Доступ к складу (даже если закрыт)" },
            { 1, "Доступ к созданию предметов (даже если закрыто)" },
            { 2, "Приглашать новых сотрудников" },
            { 3, "Понижать сотрудников в должности (ниже своего ранга)" },
            { 4, "Повышать сотрудников в должности (ниже своего ранга)" },
            { 5, "Увольнять сотрудников сотрудников (ниже своего ранга)" },
            { 6, "Респавнить фракционный транспорт" },
            { 7, "Использовать чат фракции | /f, /r" },
            { 8, "Мут чата фракции сотрудников (ниже своего ранга) | /mutef, /unmutef" },

            { 9, "Использовать чат департамента | /d" },

            { 10, "Заключать в Следственный изолятор" },
            { 11, "Выпускать из Следственного изолятора" },
            { 12, "Изменять время заключения (только для Следственного изолятора)" },

            { 13, "Заключать в Федеральную тюрьму" },

            { 14, "Выписывать штрафы" },
            { 15, "Лишать лицензии (кроме адвокатской и на право владения бизнесом)" },

            { 16, "Поиск граждан в Базе данных (Служебный планшет)" },
            { 17, "Добавлять и исполнять ориентировки (Служебный планшет)" },
            { 18, "Отправлять экстренные коды (Служебный планшет, коды 0-1-2)" },
            { 19, "Завершать активные вызовы (Служебный планшет, код 3)" },
            { 20, "Устанавливать GPS-трекеры на транспорт" },
            { 21, "Отключать активные GPS-трекеры на транспорте" },

            { 22, "Лечить" },
            { 23, "Лечить наркозависимость" },
            { 24, "Выдавать медицинские карты" },

            { 25, "Модерировать объявления" },
            { 26, "Выходить в эфир" },

            { 100, "Изымать предметы при обыске" },

            { 10_000, "Заправка транспорта за счёт фракции" },
        };

        public static string GetFractionPermissionName(uint id) => PermissionNames.GetValueOrDefault(id);

        public static Dictionary<uint, MemberData> AllMembers { get; set; }

        public static NewsData NewsData { get; set; }

        public static Dictionary<uint, VehicleData> AllVehicles { get; set; }

        public static Dictionary<FractionTypes, Fraction> All { get; private set; } = new Dictionary<FractionTypes, Fraction>();

        public static Fraction Get(FractionTypes type) => All.GetValueOrDefault(type);

        public byte MaxRank { get; private set; }

        public uint LeaderCID => Utils.Convert.ToUInt32(Core.GetSharedData<object>($"FRAC::L_{(int)Type}", 0));

        public uint Materials => Utils.Convert.ToUInt32(Core.GetSharedData<object>($"FRAC::M_{(int)Type}", 0));

        public bool StorageLocked => Core.GetSharedData<bool>($"FRAC::SL_{(int)Type}", false);
        public bool CreationWorkbenchLocked => Core.GetSharedData<bool>($"FRAC::CWL_{(int)Type}", false);

        public ExtraLabel[] StorageTextInfos { get; set; }
        public ExtraLabel[] CreationWorkbenchTextInfos { get; set; }

        public FractionTypes Type { get; set; }

        public string Name { get; set; }

        public uint StorageContainerId { get; set; }

        public Dictionary<string, uint> CreationWorkbenchPrices { get; set; }

        public MetaFlagTypes MetaFlags { get; set; }

        public Fraction(FractionTypes type, string name, uint storageContainerId, string containerPositionsStr, string creationWorkbenchPositionsStr, byte maxRank, Dictionary<string, uint> creationWorkbenchPrices, uint metaFlags)
        {
            Type = type;

            Name = name;

            StorageContainerId = storageContainerId;

            MaxRank = maxRank;

            CreationWorkbenchPrices = creationWorkbenchPrices;

            MetaFlags = (MetaFlagTypes)metaFlags;

            var contPoses = RAGE.Util.Json.Deserialize<Vector4[]>(containerPositionsStr);
            var wbPoses = RAGE.Util.Json.Deserialize<Vector4[]>(creationWorkbenchPositionsStr);

            var contTextInfos = new List<ExtraLabel>();
            var wbTextInfos = new List<ExtraLabel>();

            for (int i = 0; i < contPoses.Length; i++)
            {
                var contPos = contPoses[i];

                var containerCs = new Cylinder(contPos.Position, contPos.RotationZ, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    InteractionType = InteractionTypes.ContainerInteract,

                    ActionType = ActionTypes.ContainerInteract,

                    Data = storageContainerId,
                };

                var containerTextLabel = new ExtraLabel(new Vector3(contPos.X, contPos.Y, contPos.Z + 1f), "Склад", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };

                var containerInfoTextLabel = new ExtraLabel(new Vector3(contPos.X, contPos.Y, contPos.Z + 0.8f), "", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };

                contTextInfos.Add(containerInfoTextLabel);
            }

            StorageTextInfos = contTextInfos.ToArray();

            for (int i = 0; i < wbPoses.Length; i++)
            {
                var wbPos = wbPoses[i];

                var creationWorkbenchCs = new Cylinder(wbPos.Position, wbPos.RotationZ, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    InteractionType = InteractionTypes.FractionCreationWorkbenchInteract,

                    ActionType = ActionTypes.FractionInteract,

                    Data = $"{(int)type}_{i}",
                };

                var creationWorkbenchTextLabel = new ExtraLabel(new Vector3(wbPos.X, wbPos.Y, wbPos.Z + 1f), "Создание предметов", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };

                var wbTextInfoLabel = new ExtraLabel(new Vector3(wbPos.X, wbPos.Y, wbPos.Z + 0.8f), "", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };

                wbTextInfos.Add(wbTextInfoLabel);
            }

            CreationWorkbenchTextInfos = wbTextInfos.ToArray();

            All.Add(type, this);

            for (int i = 0; i < maxRank + 1; i++)
                Core.AddDataHandler($"FRAC::RN_{(int)type}_{i}", OnRankNameChanged);

            Core.AddDataHandler($"FRAC::SL_{(int)type}", OnStorageLockedChanged);
            Core.AddDataHandler($"FRAC::CWL_{(int)type}", OnCreationWorkbenchLockedChanged);
            Core.AddDataHandler($"FRAC::M_{(int)type}", OnMaterialsChanged);
        }

        private static void OnMaterialsChanged(string key, object value, object oldValue)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var kData = key.Split('_');

            var fType = (FractionTypes)int.Parse(kData[1]);

            if (pData.Fraction != fType)
                return;

            var amount = Utils.Convert.ToDecimal(value);

            if (MaterialWorkbench.CurrentType == MaterialWorkbench.Types.Fraction)
            {
                MaterialWorkbench.UpdateMateriaslBalance(amount);
            }
            else
            {
                FractionMenu.UpdateInfoLine(3, amount);
            }
        }

        public static void OnCreationWorkbenchLockedChanged(string key, object value, object oldValue)
        {
            var state = value as bool? ?? false;

            var kData = key.Split('_');

            var fType = (FractionTypes)int.Parse(kData[1]);

            var fData = Get(fType);

            if (state)
            {
                foreach (var x in fData.CreationWorkbenchTextInfos)
                {
                    x.Text = "[Закрыто]";

                    x.Color = new RGBA(255, 0, 0, 255);
                }
            }
            else
            {
                foreach (var x in fData.CreationWorkbenchTextInfos)
                {
                    x.Text = "[Открыто]";

                    x.Color = new RGBA(0, 255, 0, 255);
                }
            }

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.Fraction != fType)
                return;

            if (!state && MaterialWorkbench.CurrentType == MaterialWorkbench.Types.Fraction)
                MaterialWorkbench.Close();

            FractionMenu.SetLockButtonState("workbench", state);
        }

        public static void OnStorageLockedChanged(string key, object value, object oldValue)
        {
            var state = value as bool? ?? false;

            var kData = key.Split('_');

            var fType = (FractionTypes)int.Parse(kData[1]);

            var fData = Get(fType);

            if (state)
            {
                foreach (var x in fData.StorageTextInfos)
                {
                    x.Text = "[Закрыт]";

                    x.Color = new RGBA(255, 0, 0, 255);
                }
            }
            else
            {
                foreach (var x in fData.StorageTextInfos)
                {
                    x.Text = "[Открыт]";

                    x.Color = new RGBA(0, 255, 0, 255);
                }
            }

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.Fraction != fType)
                return;

            FractionMenu.SetLockButtonState("storage", state);
        }

        private static void OnRankNameChanged(string key, object value, object oldValue)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var kData = key.Split('_');

            var fType = (FractionTypes)int.Parse(kData[1]);

            if (pData.Fraction != fType)
                return;

            var rank = byte.Parse(kData[2]);

            FractionMenu.UpdateRankName((byte)(rank + 1), (string)value ?? "null");
        }

        public static async void ShowFractionMenu()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var curFrac = pData.CurrentFraction;

            if (curFrac == null)
                return;

            var fMenuServerData = ((string)await RAGE.Events.CallRemoteProc("Fraction::GMSD"))?.Split('&');

            if (fMenuServerData == null)
                return;

            var balance = decimal.Parse(fMenuServerData[0]);

            FractionMenu.Show(curFrac.Type, NewsData, AllMembers, AllVehicles, balance, AllMembers.GetValueOrDefault(pData.CID)?.Rank ?? 0);
        }

        public virtual void OnStartMembership(params object[] args)
        {
            CurrentData = new Dictionary<string, object>();

            NewsData = RAGE.Util.Json.Deserialize<NewsData>((string)args[0]);

            AllVehicles = new Dictionary<uint, VehicleData>();

            AllMembers = new Dictionary<uint, MemberData>();

            foreach (var x in ((JArray)args[1]).ToObject<List<string>>())
            {
                var data = x.Split('&');

                AllVehicles.Add(uint.Parse(data[0]), new VehicleData() { Numberplate = data[1], MinRank = byte.Parse(data[2]) });
            }

            foreach (var x in ((JArray)args[2]).ToObject<List<string>>())
            {
                var data = x.Split('&');

                AllMembers.Add(uint.Parse(data[0]), new MemberData() { Name = data[1], Rank = byte.Parse(data[2]), IsOnline = data[3] == "1", SubStatus = byte.Parse(data[4]), LastSeenDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(data[5])).DateTime });
            }

            HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.Fraction_Menu);

            Menu.SetFraction(Type);

            Interaction.CharacterInteractionInfo.AddAction("char_job", "fraction_invite", (entity) => { var player = entity as Player; if (player == null) return; PlayerInvite(player); });
            Interaction.CharacterInteractionInfo.AddAction("documents", "fraction_docs", (entity) => { var player = entity as Player; if (player == null) return; PlayerShowDocs(player); });
        }

        public virtual void OnEndMembership()
        {
            if (CurrentData != null)
            {
                CurrentData.Clear();

                CurrentData = null;
            }

            HUD.Menu.UpdateCurrentTypes(false, HUD.Menu.Types.Fraction_Menu);

            Menu.SetFraction(FractionTypes.None);

            FractionMenu.Close();

            Interaction.CloseMenu();

            if (MaterialWorkbench.CurrentType == MaterialWorkbench.Types.Fraction)
                MaterialWorkbench.Close();

            AllMembers = null;
            NewsData = null;
            AllVehicles = null;
        }

        public string GetRankName(byte rank) => Core.GetSharedData<string>($"FRAC::RN_{(int)Type}_{rank}", "null");

        private Dictionary<string, object> CurrentData { get; set; }

        public void SetCurrentData(string key, object data)
        {
            if (CurrentData == null)
                return;

            if (!CurrentData.TryAdd(key, data))
                CurrentData[key] = data;
        }

        public T GetCurrentData<T>(string key)
        {
            var data = CurrentData?.GetValueOrDefault(key);

            if (data is T dataT)
                return dataT;

            return default;
        }

        public bool ResetCurrentData(string key) => CurrentData.Remove(key);

        public void PlayerInvite(Player player)
        {
            var tData = PlayerData.GetData(player);

            if (tData == null)
                return;

            if (tData.Fraction != FractionTypes.None)
            {
                if (tData.Fraction == Type)
                {
                    Notification.ShowError(Locale.Get("FRACTION_INV_E_1"));
                }
                else
                {
                    Notification.ShowError(Locale.Get("FRACTION_INV_E_0"));
                }

                return;
            }

            Offers.Request(player, OfferTypes.InviteFraction, null);
        }

        public void PlayerShowDocs(Player player)
        {
            if (!MetaFlags.HasFlag(MetaFlagTypes.MembersHaveDocs))
                return;

            Offers.Request(player, OfferTypes.ShowFractionDocs, null);
        }
    }
}
