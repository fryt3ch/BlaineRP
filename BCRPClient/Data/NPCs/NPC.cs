using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BCRPClient.Data
{
    public class NPC : Events.Script
    {
        public static NPC CurrentNPC { get; set; }

        private static Dictionary<Ped, NPC> AllNPCs = new Dictionary<Ped, NPC>();

        public static NPC GetData(string id) => AllNPCs.Where(x => x.Value.Id == id).Select(x => x.Value).FirstOrDefault();

        public static NPC GetData(Ped ped) => ped == null ? null : AllNPCs.GetValueOrDefault(ped);

        public static DateTime LastSent;

        private static int EscBindIdx { get; set; } = -1;

        public enum Types
        {
            Static = -1,

            Quest = 0,
            Talkable,
        }

        public enum PedAnimationTypes
        {
            /// <summary>Ничего</summary>
            None = 0,

            /// <summary>Анимация прерывается при диалоге с игроком, потом - продолжается</summary>
            /// <remarks>Если анимация была запущена во время диалога с игроком, то она будет проигрываться!</remarks>
            InterruptOnDialogue,
            /// <summary>Анимация прерывается при диалоге с игроком, потом - продолжается</summary>
            ///<remarks>Если анимация была запущена во время диалога с игроком, то она НЕ будет проигрываться!</remarks>
            DontPlayOnDialogue,
        }

        public string SubName { get; set; }

        public string Id { get; set; }

        public Types Type { get; set; }

        public Ped Ped { get; private set; }

        private bool _Invincible { get; set; }

        public bool Invincible { get => _Invincible; set { _Invincible = value; Ped.SetInvincible(value); } }

        public PedAnimationTypes AnimationType { get => Ped.GetData<PedAnimationTypes>("AnimType"); set => Ped.SetData("AnimType", value); }

        public Sync.Animations.Animation DefaultAnimation { get => Ped.GetData<Sync.Animations.Animation>("DefaultAnim"); set { if (value != null) { Ped.SetData("DefaultAnim", value); } else { Ped.ResetData("DefaultAnim"); } } }

        public Sync.Animations.Animation CurrentAnimation { get => Ped.GetData<Sync.Animations.Animation>("CurrentAnim") ?? DefaultAnimation; set { if (value != null) { Ped.SetData("CurrentAnim", value); if (IsStreamed) Sync.Animations.Play(Ped, value, -1); } else { Ped.ResetData("CurrentAnim"); if (IsStreamed) Sync.Animations.Stop(Ped); } } }

        public float DefaultHeading { get; set; }

        public string Name { get; set; }

        public string DefaultDialogueId { get; set; }

        public Dialogue CurrentDialogue { get => Ped.GetData<Dialogue>("CurrentDialogue"); set { if (value != null) { Ped.SetData("CurrentDialogue", value); } else { Ped.ResetData("CurrentDialogue"); } } }

        public List<Dialogue.LastInfo> LastDialogues { get => Ped.GetData<List<Dialogue.LastInfo>>("LastDialogues"); set { if (value != null) { Ped.SetData("LastDialogues", value); } else { Ped.ResetData("LastDialogues"); } } }

        public Additional.Cylinder Colshape { get; set; }

        public bool IsStreamed => RAGE.Elements.Entities.Peds.Streamed.Contains(Ped);

        public Additional.ExtraBlip Blip { get => Ped.GetData<Additional.ExtraBlip>("Blip"); set { if (value == null) Ped.ResetData("Blip"); else Ped.SetData("Blip", value); } }

        public object Data { get; set; }

        private Dictionary<string, object> TempDialogueData { get => Player.LocalPlayer.GetData<Dictionary<string, object>>($"NPC::{Id}::TDD"); set { if (value == null) Player.LocalPlayer.ResetData($"NPC::{Id}::TDD"); else Player.LocalPlayer.SetData($"NPC::{Id}::TDD", value); } }

        public NPC(string Id, string Name, Types Type, uint Model, Vector3 Position, float Heading = 0f, uint Dimension = 0)
        {
            this.Id = Id;

            Ped = new Ped(Model, Position, Heading, Dimension);

            this.Type = Type;

            this.DefaultHeading = Heading;
            this.Name = Name;

            this._Invincible = true;

            if (Type == Types.Talkable)
            {
                this.Colshape = new Additional.Cylinder(new Vector3(Position.X, Position.Y, Position.Z - 1f), 2f, 2f, false, new Utils.Colour(255, 0, 0, 255), Dimension, null)
                {
                    ActionType = Additional.ExtraColshape.ActionTypes.NpcDialogue,
                    InteractionType = Additional.ExtraColshape.InteractionTypes.NpcDialogue,

                    Data = this,
                };
            }

            AllNPCs.Add(Ped, this);
        }

        public NPC(string Id, string Name, Types Type, string Model, Vector3 Position, float Heading, uint Dimension = 0) : this(Id, Name, Type, RAGE.Util.Joaat.Hash(Model), Position, Heading, Dimension) { }

        public bool SellerNpcRequestEnterBusiness()
        {
            if (NPC.CurrentNPC.Data is Data.Locations.Business businessData)
            {
                Events.CallRemote("Business::Enter", businessData.Id);

                return true;
            }

            return false;
        }

        public static async System.Threading.Tasks.Task OnPedStreamIn(Ped ped)
        {
            var data = GetData(ped);

            if (data == null)
                return;

            ped.FreezePosition(true);

            ped.SetProofs(true, true, true, true, true, true, true, true);

            data.Ped.SetHeading(data.DefaultHeading);

            if (data.CurrentAnimation is Sync.Animations.Animation curAnim)
            {
                Sync.Animations.Play(ped, curAnim, -1);
            }
        }

        public static async System.Threading.Tasks.Task OnPedStreamOut(Ped ped)
        {
            var data = GetData(ped);

            if (data == null)
            {
                ped.ClearTasksImmediately();
            }
            else
            {

            }
        }

        public NPC()
        {
            LastSent = DateTime.MinValue;

            GameEvents.Render += () =>
            {
                float screenX = 0f, screenY = 0f;

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                foreach (var x in Utils.GetPedsOnScreen(5))
                {
                    var data = GetData(x);

                    if (data == null)
                        continue;

                    var pos = x.GetRealPosition();

                    if (Vector3.Distance(pos, Player.LocalPlayer.Position) > 10f)
                        continue;

                    if (Settings.Other.DebugLabels && pData.AdminLevel > -1)
                    {
                        if (Utils.GetScreenCoordFromWorldCoord(pos, ref screenX, ref screenY))
                        {
                            Utils.DrawText($"ID: {data.Id} | Type: {data.Type}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                            Utils.DrawText($"Data: {data.Data}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                        }
                    }

                    pos.Z += 1.1f;

                    if (!Utils.GetScreenCoordFromWorldCoord(pos, ref screenX, ref screenY))
                        continue;

                    Utils.DrawText(data.Name, screenX, screenY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);

                    if (data.SubName != null)
                        Utils.DrawText(Locale.Get(data.SubName), screenX, screenY += NameTags.Interval / 2f, 255, 215, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                }
            };
        }

        public void CallRemote(string actionName, params object[] args) => Events.CallRemote("NPC::Action", Id, actionName, string.Join('&', args));

        public async System.Threading.Tasks.Task<object> CallRemoteProc(string actionName, params object[] args) => await Events.CallRemoteProc("NPC::Proc", Id, actionName, string.Join('&', args));

        public void Interact(bool state = true)
        {

        }

        public void SetTempDialogueData(string key, object value)
        {
            if (TempDialogueData == null)
                return;

            if (!TempDialogueData.TryAdd(key, value))
                TempDialogueData[key] = value;
        }

        public bool ResetTempDialogueData(string key) => TempDialogueData?.Remove(key) == true;

        public T GetTempDialogueData<T>(string key)
        {
            var data = TempDialogueData?.GetValueOrDefault(key);

            if (data is T dataT)
                return dataT;

            return default;
        }

        public void SwitchDialogue(bool state = true)
        {
            if (state)
            {
                if (CurrentNPC != null)
                    return;

                TempDialogueData = new Dictionary<string, object>();

                LastDialogues = new List<Dialogue.LastInfo>();

                CEF.Notification.SetOnTop(true);

                CurrentNPC = this;

                var pedPos = Ped.GetRealPosition();
                var playerPos = Player.LocalPlayer.GetRealPosition();

                var t = Utils.RadiansToDegrees((float)Math.Atan2(pedPos.Y - playerPos.Y, pedPos.X - playerPos.X)) - 90f;

                Player.LocalPlayer.SetHeading(t);
                Ped.SetHeading(t + 180f);

                Player.LocalPlayer.SetVisible(false, false);

                Additional.Camera.Enable(Additional.Camera.StateTypes.NpcTalk, Ped, Ped, -1, null, null, new Vector3(0f, 0f, Ped.GetBoneCoords(31086, 0f, 0f, 0f).Z - pedPos.Z));

                var playerHeadCoord = Additional.Camera.Position;

                playerHeadCoord.Z -= 0.1f;

                Ped.TaskLookAtCoord2(playerHeadCoord.X, playerHeadCoord.Y, playerHeadCoord.Z, -1, 2048, 3);

                BindEsc();
            }
            else
            {
                if (CurrentNPC == null)
                    return;

                CEF.Notification.SetOnTop(false);

                if (LastDialogues is List<Dialogue.LastInfo> lastDialogues)
                {
                    lastDialogues.Clear();

                    LastDialogues = null;
                }

                Ped.TaskClearLookAt();

                UnbindEsc();

                Additional.Camera.Disable(750);

                Player.LocalPlayer.SetVisible(true, false);

                Ped.SetHeading(DefaultHeading);

                TempDialogueData?.Clear();

                TempDialogueData = null;

                CurrentNPC = null;

                CurrentDialogue = null;

                CEF.NPC.Close();
            }
        }

        public void ShowDialogue(string dialogueId, bool saveAsLast = true, object[] args = null, params object[] textArgs)
        {
            if (dialogueId == null)
                return;

            var dialogue = Dialogue.Get(dialogueId);

            if (dialogue == null)
                return;

            if (saveAsLast)
            {
                var dgInfo = new Dialogue.LastInfo(dialogue, saveAsLast, args, textArgs);

                if (LastDialogues is List<Dialogue.LastInfo> lastDialogues)
                    lastDialogues.Add(dgInfo);
                else
                    LastDialogues = new List<Dialogue.LastInfo>() { dgInfo };
            }

            CurrentDialogue = dialogue;

            dialogue.Show(this, args, textArgs);
        }

        public static bool UnbindEsc()
        {
            if (EscBindIdx >= 0)
            {
                KeyBinds.Unbind(EscBindIdx);

                EscBindIdx = -1;

                return true;
            }

            return false;
        }

        public static bool BindEsc()
        {
            if (EscBindIdx >= 0)
                return false;

            EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => NPC.CurrentNPC?.SwitchDialogue(false));

            return true;
        }

        public void Destroy()
        {
            if (Ped == null)
                return;

            if (AllNPCs.Remove(Ped))
            {
                Ped.Destroy();

                Ped = null;

                if (CurrentNPC == this)
                {
                    SwitchDialogue(false);
                }
            }
        }

        public string GetDisplayName()
        {
            if (Name != null && Name.Length > 0)
                return Name;

            if (SubName != null)
                return Locale.Get(SubName);

            return null;
        }
    }
}
