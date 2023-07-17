﻿using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;

namespace BCRPClient
{
    class Interaction
    {
        private static bool _Enabled = false;

        public static bool Enabled { get => _Enabled; set { if (!_Enabled && value) { GameEvents.Render -= Render; GameEvents.Render += Render; _Enabled = value; } else if (_Enabled && !value) { GameEvents.Render -= Render; _Enabled = value; CurrentEntity = null; } } }

        public static bool EnabledVisual { get; set; }

        public static RAGE.Elements.Entity CurrentEntity { get; set; }

        private static HashSet<Entity> DisabledEntities { get; } = new HashSet<Entity>();


        private static void Render()
        {
            var entity = Player.LocalPlayer.Vehicle ?? Utils.GetEntityPlayerLookAt(Settings.ENTITY_INTERACTION_MAX_DISTANCE_RENDER);

            if (entity == null)
            {
                CurrentEntity = null;

                return;
            }

            if (DisabledEntities.Contains(entity))
                return;

            float x = 0f, y = 0f;

            if (entity.Type == RAGE.Elements.Type.Player || entity.Type == RAGE.Elements.Type.Vehicle)
            {
                if ((entity.Type == RAGE.Elements.Type.Vehicle && entity.IsLocal) || !entity.GetScreenPosition(ref x, ref y))
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
                        var furnData = entity.GetData<Data.Furniture>("Furniture");

                        if (furnData != null)
                        {
                            if (EnabledVisual)
                                Utils.DrawText(furnData.Name, x, y - NameTags.Interval, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                        }
                    }
                    else if (entity.HasData("CustomText"))
                    {
                        if (EnabledVisual)
                        {
                            var ctAction = entity.GetData<Action<float, float>>("CustomText");

                            if (ctAction != null)
                            {
                                ctAction.Invoke(x, y);
                            }
                        }
                    }
                }
                else
                {
                    CurrentEntity = entity;

                    if (mObj.GetSharedData<int>("IOG", -1) == 1)
                    {
                        var iogData = Sync.World.ItemOnGround.GetItemOnGroundObject(mObj);

                        if (iogData != null)
                        {
                            if (EnabledVisual)
                                Utils.DrawText(iogData.Name, x, y - NameTags.Interval, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                        }
                    }
                }
            }

            if (!EnabledVisual)
                return;

            Utils.DrawText(KeyBinds.Binds[KeyBinds.Types.Interaction].GetKeyString(), x, y, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
        }

        public static bool SetEntityAsDisabled(Entity entity, bool state)
        {
            if (state)
            {
                if (DisabledEntities.Add(entity))
                {
                    if (CurrentEntity == entity)
                        CurrentEntity = null;

                    CEF.Interaction.CloseMenu();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return DisabledEntities.Remove(entity);
            }
        }
    }
}
