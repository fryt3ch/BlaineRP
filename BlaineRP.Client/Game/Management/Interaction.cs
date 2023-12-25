using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Utils.Game;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management
{
    internal class Interaction
    {
        private static bool _enabled = false;

        private static readonly HashSet<Entity> _disabledEntities = new HashSet<Entity>();

        public static bool Enabled
        {
            get => _enabled;
            set
            {
                if (!_enabled && value)
                {
                    Main.Render -= Render;
                    Main.Render += Render;
                    _enabled = value;
                }
                else if (_enabled && !value)
                {
                    Main.Render -= Render;
                    _enabled = value;
                    CurrentEntity = null;
                }
            }
        }

        public static bool EnabledVisual { get; set; }

        public static Entity CurrentEntity { get; set; }


        private static void Render()
        {
            Entity entity = Player.LocalPlayer.Vehicle ?? Raycast.GetEntityPedLookAt(Player.LocalPlayer, Settings.App.Static.EntityInteractionMaxDistance);

            if (entity == null)
            {
                CurrentEntity = null;

                return;
            }

            if (_disabledEntities.Contains(entity))
                return;

            float x = 0f, y = 0f;

            if (entity.Type == RAGE.Elements.Type.Player || entity.Type == RAGE.Elements.Type.Vehicle)
            {
                if (entity.Type == RAGE.Elements.Type.Vehicle && entity.IsLocal || !entity.GetScreenPosition(ref x, ref y))
                {
                    CurrentEntity = null;

                    return;
                }

                CurrentEntity = entity;
            }
            else if (entity.Type == RAGE.Elements.Type.Ped)
            {
                if (!entity.HasData("ECA_INT"))
                {
                    CurrentEntity = null;

                    return;
                }

                CurrentEntity = entity;
            }
            else if (entity is MapObject mObj)
            {
                if (!entity.HasData("Interactive"))
                {
                    CurrentEntity = null;

                    return;
                }

                if (!entity.GetScreenPosition(ref x, ref y))
                {
                    CurrentEntity = null;

                    return;
                }

                if (entity.IsLocal)
                {
                    CurrentEntity = entity;

                    if (entity.HasData("Furniture"))
                    {
                        Furniture furnData = entity.GetData<Furniture>("Furniture");

                        if (furnData != null)
                            if (EnabledVisual)
                                Graphics.DrawText(furnData.Name, x, y - NameTags.Interval, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                    }
                    else if (entity.HasData("CustomText"))
                    {
                        if (EnabledVisual)
                        {
                            Action<float, float> ctAction = entity.GetData<Action<float, float>>("CustomText");

                            if (ctAction != null)
                                ctAction.Invoke(x, y);
                        }
                    }
                }
                else
                {
                    CurrentEntity = entity;

                    if (mObj.GetSharedData<int>("IOG", -1) == 1)
                    {
                        var iogData = ItemOnGround.GetItemOnGroundObject(mObj);

                        if (iogData != null)
                            if (EnabledVisual)
                                Graphics.DrawText(iogData.Name, x, y - NameTags.Interval, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                    }
                }
            }

            if (!EnabledVisual)
                return;

            if (CurrentEntity == Player.LocalPlayer.Vehicle)
                return;

            Graphics.DrawText(Input.Core.Get(BindTypes.Interaction).GetKeyString(), x, y, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
        }

        public static bool SetEntityAsDisabled(Entity entity, bool state)
        {
            if (state)
            {
                if (_disabledEntities.Add(entity))
                {
                    if (CurrentEntity == entity)
                        CurrentEntity = null;

                    UI.CEF.Interaction.CloseMenu();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return _disabledEntities.Remove(entity);
            }
        }
    }
}