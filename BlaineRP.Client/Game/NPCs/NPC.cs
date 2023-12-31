﻿using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.NPCs.Dialogues;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.NPCs
{
    [Script(int.MaxValue)]
    public class NPC
    {
        public enum PedAnimationTypes
        {
            /// <summary>Ничего</summary>
            None = 0,

            /// <summary>Анимация прерывается при диалоге с игроком, потом - продолжается</summary>
            /// <remarks>Если анимация была запущена во время диалога с игроком, то она будет проигрываться!</remarks>
            InterruptOnDialogue,

            /// <summary>Анимация прерывается при диалоге с игроком, потом - продолжается</summary>
            /// <remarks>Если анимация была запущена во время диалога с игроком, то она НЕ будет проигрываться!</remarks>
            DontPlayOnDialogue,
        }

        public enum Types
        {
            Static = -1,

            Quest = 0,
            Talkable,
        }

        private static Dictionary<Ped, NPC> AllNPCs = new Dictionary<Ped, NPC>();

        public static DateTime LastSent;

        public NPC(string Id, string Name, Types Type, uint Model, Vector3 Position, float Heading = 0f, uint Dimension = 0)
        {
            this.Id = Id;

            Ped = new Ped(Model, Position, Heading, Dimension);

            this.Type = Type;

            DefaultHeading = Heading;
            this.Name = Name;

            _Invincible = true;

            if (Type == Types.Talkable)
                Colshape = new Cylinder(new Vector3(Position.X, Position.Y, Position.Z - 1f), 2f, 2f, false, new Colour(255, 0, 0, 255), Dimension, null)
                {
                    ActionType = ActionTypes.NpcDialogue,
                    InteractionType = InteractionTypes.NpcDialogue,
                    Data = this,
                };

            AllNPCs.Add(Ped, this);
        }

        public NPC(string Id, string Name, Types Type, string Model, Vector3 Position, float Heading, uint Dimension = 0) : this(Id,
            Name,
            Type,
            RAGE.Util.Joaat.Hash(Model),
            Position,
            Heading,
            Dimension
        )
        {
        }

        public NPC()
        {
            LastSent = DateTime.MinValue;

            Main.Render += () =>
            {
                float screenX = 0f, screenY = 0f;

                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                foreach (Ped x in Utils.Game.Misc.GetPedsOnScreen(5))
                {
                    NPC data = GetData(x);

                    if (data == null)
                        continue;

                    Vector3 pos = x.GetRealPosition();

                    if (Vector3.Distance(pos, Player.LocalPlayer.Position) > 10f)
                        continue;

                    if (Settings.User.Other.DebugLabels && pData.AdminLevel > -1)
                        if (Graphics.GetScreenCoordFromWorldCoord(pos, ref screenX, ref screenY))
                        {
                            Graphics.DrawText($"ID: {data.Id} | Type: {data.Type}",
                                screenX,
                                screenY += NameTags.Interval / 2f,
                                255,
                                255,
                                255,
                                255,
                                0.4f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                true
                            );
                            Graphics.DrawText($"Data: {data.Data}",
                                screenX,
                                screenY += NameTags.Interval / 2f,
                                255,
                                255,
                                255,
                                255,
                                0.4f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                true
                            );
                        }

                    pos.Z += 1.1f;

                    if (!Graphics.GetScreenCoordFromWorldCoord(pos, ref screenX, ref screenY))
                        continue;

                    Graphics.DrawText(data.Name, screenX, screenY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);

                    if (data.SubName != null)
                        Graphics.DrawText(Locale.Get(data.SubName), screenX, screenY += NameTags.Interval / 2f, 255, 215, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                }
            };
        }

        public static NPC CurrentNPC { get; set; }

        private static int EscBindIdx { get; set; } = -1;

        public string SubName { get; set; }

        public string Id { get; set; }

        public Types Type { get; set; }

        public Ped Ped { get; private set; }

        private bool _Invincible { get; set; }

        public bool Invincible
        {
            get => _Invincible;
            set
            {
                _Invincible = value;
                Ped.SetInvincible(value);
            }
        }

        public PedAnimationTypes AnimationType
        {
            get => Ped.GetData<PedAnimationTypes>("AnimType");
            set => Ped.SetData("AnimType", value);
        }

        public Animation DefaultAnimation
        {
            get => Ped.GetData<Animation>("DefaultAnim");
            set
            {
                if (value != null)
                    Ped.SetData("DefaultAnim", value);
                else
                    Ped.ResetData("DefaultAnim");
            }
        }

        public Animation CurrentAnimation
        {
            get => Ped.GetData<Animation>("CurrentAnim") ?? DefaultAnimation;
            set
            {
                if (value != null)
                {
                    Ped.SetData("CurrentAnim", value);
                    if (IsStreamed)
                        Service.Play(Ped, value, -1);
                }
                else
                {
                    Ped.ResetData("CurrentAnim");
                    if (IsStreamed)
                        Service.Stop(Ped);
                }
            }
        }

        public float DefaultHeading { get; set; }

        public string Name { get; set; }

        public string DefaultDialogueId { get; set; }

        public Dialogue CurrentDialogue
        {
            get => Ped.GetData<Dialogue>("CurrentDialogue");
            set
            {
                if (value != null)
                    Ped.SetData("CurrentDialogue", value);
                else
                    Ped.ResetData("CurrentDialogue");
            }
        }

        public List<Dialogue.LastInfo> LastDialogues
        {
            get => Ped.GetData<List<Dialogue.LastInfo>>("LastDialogues");
            set
            {
                if (value != null)
                    Ped.SetData("LastDialogues", value);
                else
                    Ped.ResetData("LastDialogues");
            }
        }

        public Cylinder Colshape { get; set; }

        public bool IsStreamed => Entities.Peds.Streamed.Contains(Ped);

        public ExtraBlip Blip
        {
            get => Ped.GetData<ExtraBlip>("Blip");
            set
            {
                if (value == null)
                    Ped.ResetData("Blip");
                else
                    Ped.SetData("Blip", value);
            }
        }

        public object Data { get; set; }

        private Dictionary<string, object> TempDialogueData
        {
            get => Player.LocalPlayer.GetData<Dictionary<string, object>>($"NPC::{Id}::TDD");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData($"NPC::{Id}::TDD");
                else
                    Player.LocalPlayer.SetData($"NPC::{Id}::TDD", value);
            }
        }

        public static NPC GetData(string id)
        {
            return AllNPCs.Where(x => x.Value.Id == id).Select(x => x.Value).FirstOrDefault();
        }

        public static NPC GetData(Ped ped)
        {
            return ped == null ? null : AllNPCs.GetValueOrDefault(ped);
        }

        public bool SellerNpcRequestEnterBusiness()
        {
            if (CurrentNPC.Data is Business businessData)
            {
                Events.CallRemote("Business::Enter", businessData.Id);

                return true;
            }

            return false;
        }

        public static async System.Threading.Tasks.Task OnPedStreamIn(Ped ped)
        {
            NPC data = GetData(ped);

            if (data == null)
                return;

            ped.FreezePosition(true);

            ped.SetProofs(true, true, true, true, true, true, true, true);

            data.Ped.SetHeading(data.DefaultHeading);

            if (data.CurrentAnimation is Animation curAnim)
                Service.Play(ped, curAnim, -1);
        }

        public static async System.Threading.Tasks.Task OnPedStreamOut(Ped ped)
        {
            NPC data = GetData(ped);

            if (data == null)
            {
                ped.ClearTasksImmediately();
            }
            else
            {
            }
        }

        public void CallRemote(string actionName, params object[] args)
        {
            Events.CallRemote("NPC::Action", Id, actionName, string.Join('&', args));
        }

        public async System.Threading.Tasks.Task<object> CallRemoteProc(string actionName, params object[] args)
        {
            return await Events.CallRemoteProc("NPC::Proc", Id, actionName, string.Join('&', args));
        }

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

        public bool ResetTempDialogueData(string key)
        {
            return TempDialogueData?.Remove(key) == true;
        }

        public T GetTempDialogueData<T>(string key)
        {
            object data = TempDialogueData?.GetValueOrDefault(key);

            if (data is T dataT)
                return dataT;

            return default(T);
        }

        public void SwitchDialogue(bool state = true)
        {
            if (state)
            {
                if (CurrentNPC != null)
                    return;

                TempDialogueData = new Dictionary<string, object>();

                LastDialogues = new List<Dialogue.LastInfo>();

                Notification.SetOnTop(true);

                CurrentNPC = this;

                Vector3 pedPos = Ped.GetRealPosition();
                Vector3 playerPos = Player.LocalPlayer.GetRealPosition();

                float t = Geometry.RadiansToDegrees((float)System.Math.Atan2(pedPos.Y - playerPos.Y, pedPos.X - playerPos.X)) - 90f;

                Player.LocalPlayer.SetHeading(t);
                Ped.SetHeading(t + 180f);

                Player.LocalPlayer.SetVisible(false, false);

                Management.Camera.Service.Enable(Management.Camera.Service.StateTypes.NpcTalk,
                    Ped,
                    Ped,
                    -1,
                    null,
                    null,
                    new Vector3(0f, 0f, Ped.GetBoneCoords(31086, 0f, 0f, 0f).Z - pedPos.Z)
                );

                Vector3 playerHeadCoord = Management.Camera.Service.Position;

                playerHeadCoord.Z -= 0.1f;

                Ped.TaskLookAtCoord2(playerHeadCoord.X, playerHeadCoord.Y, playerHeadCoord.Z, -1, 2048, 3);

                BindEsc();
            }
            else
            {
                if (CurrentNPC == null)
                    return;

                Notification.SetOnTop(false);

                if (LastDialogues is List<Dialogue.LastInfo> lastDialogues)
                {
                    lastDialogues.Clear();

                    LastDialogues = null;
                }

                Ped.TaskClearLookAt();

                UnbindEsc();

                Management.Camera.Service.Disable(750);

                Player.LocalPlayer.SetVisible(true, false);

                Ped.SetHeading(DefaultHeading);

                TempDialogueData?.Clear();

                TempDialogueData = null;

                CurrentNPC = null;

                CurrentDialogue = null;

                UI.CEF.NPC.Close();
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
                    LastDialogues = new List<Dialogue.LastInfo>()
                    {
                        dgInfo,
                    };
            }

            CurrentDialogue = dialogue;

            dialogue.Show(this, args, textArgs);
        }

        public static bool UnbindEsc()
        {
            if (EscBindIdx >= 0)
            {
                Input.Core.Unbind(EscBindIdx);

                EscBindIdx = -1;

                return true;
            }

            return false;
        }

        public static bool BindEsc()
        {
            if (EscBindIdx >= 0)
                return false;

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CurrentNPC?.SwitchDialogue(false));

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
                    SwitchDialogue(false);
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