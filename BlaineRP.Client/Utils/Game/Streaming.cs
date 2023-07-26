using System;
using System.Collections.Generic;
using RAGE.Elements;

namespace BlaineRP.Client.Utils.Game
{
    internal static class Streaming
    {
        public static MapObject CreateObjectNoOffsetImmediately(uint modelHash, float posX, float posY, float posZ)
        {
            if (!RequestModelNow(modelHash))
                return null;

            int handle = RAGE.Game.Object.CreateObjectNoOffset(modelHash, posX, posY, posZ, false, false, false);

            var mObj = new MapObject(handle)
            {
                Dimension = uint.MaxValue,
            };

            //RAGE.Game.Entity.SetEntityAsMissionEntity(handle, false, false);

            RAGE.Game.Streaming.SetModelAsNoLongerNeeded(modelHash);

            return mObj;
        }

        public static async System.Threading.Tasks.Task RequestAnimDict(string name)
        {
            if (RAGE.Game.Streaming.HasAnimDictLoaded(name))
                return;

            RAGE.Game.Streaming.RequestAnimDict(name);

            while (!RAGE.Game.Streaming.HasAnimDictLoaded(name))
            {
                await RAGE.Game.Invoker.WaitAsync(5);
            }
        }

        public static async System.Threading.Tasks.Task RequestClipSet(string name)
        {
            if (RAGE.Game.Streaming.HasClipSetLoaded(name))
                return;

            RAGE.Game.Streaming.RequestClipSet(name);

            while (!RAGE.Game.Streaming.HasClipSetLoaded(name))
            {
                await RAGE.Game.Invoker.WaitAsync(5);
            }
        }

        public static async System.Threading.Tasks.Task<bool> RequestModel(uint hash)
        {
            if (!RAGE.Game.Streaming.IsModelValid(hash))
                return false;

            if (RAGE.Game.Streaming.HasModelLoaded(hash))
                return true;

            RAGE.Game.Streaming.RequestModel(hash);

            while (!RAGE.Game.Streaming.HasModelLoaded(hash))
            {
                await RAGE.Game.Invoker.WaitAsync(5);
            }

            return true;
        }

        public static bool RequestModelNow(uint hash)
        {
            if (!RAGE.Game.Streaming.IsModelValid(hash))
                return false;

            if (RAGE.Game.Streaming.HasModelLoaded(hash))
                return true;

            RAGE.Game.Streaming.RequestModel(hash);

            while (!RAGE.Game.Streaming.HasModelLoaded(hash))
            {
                RAGE.Game.Invoker.Wait(0);
            }

            return true;
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
            {
                await RAGE.Game.Invoker.WaitAsync(5);
            }

            RAGE.Game.Graphics.UseParticleFxAssetNextCall(name);
        }

        public static async System.Threading.Tasks.Task<bool> RequestStreamedTextureDict(string dictName)
        {
            if (RAGE.Game.Graphics.HasStreamedTextureDictLoaded(dictName))
                return true;

            RAGE.Game.Graphics.RequestStreamedTextureDict(dictName, true);

            while (!RAGE.Game.Graphics.HasStreamedTextureDictLoaded(dictName))
            {
                await RAGE.Game.Invoker.WaitAsync(5);
            }

            return true;
        }

        public static async System.Threading.Tasks.Task RequestWeaponAsset(uint hash)
        {
            if (RAGE.Game.Weapon.HasWeaponAssetLoaded(hash))
                return;

            RAGE.Game.Weapon.RequestWeaponAsset(hash, 31, 0);

            while (!RAGE.Game.Weapon.HasWeaponAssetLoaded(hash))
            {
                await RAGE.Game.Invoker.WaitAsync(5);
            }
        }

        public static bool StreamInCustomActionsAdd(this Entity entity, Action<Entity> action)
        {
            HashSet<Action<Entity>> eHandler = entity.GetData<HashSet<Action<Entity>>>("ECA_SI");

            if (eHandler == null)
            {
                eHandler = new HashSet<Action<Entity>>()
                {
                    action,
                };

                entity.SetData("ECA_SI", eHandler);

                return true;
            }
            else
            {
                return eHandler.Add(action);
            }
        }

        public static HashSet<Action<Entity>> StreamInCustomActionsGet(this Entity entity)
        {
            return entity.GetData<HashSet<Action<Entity>>>("ECA_SI");
        }

        public static bool StreamInCustomActionsRemove(this Entity entity, Action<Entity> action)
        {
            return entity.GetData<HashSet<Action<Entity>>>("ECA_SI")?.Remove(action) ?? false;
        }

        public static bool StreamInCustomActionsReset(this Entity entity)
        {
            return entity.ResetData("ECA_SI");
        }

        public static bool StreamOutCustomActionsAdd(this Entity entity, Action<Entity> action)
        {
            HashSet<Action<Entity>> eHandler = entity.GetData<HashSet<Action<Entity>>>("ECA_SO");

            if (eHandler == null)
            {
                eHandler = new HashSet<Action<Entity>>()
                {
                    action,
                };

                entity.SetData("ECA_SO", eHandler);

                return true;
            }
            else
            {
                return eHandler.Add(action);
            }
        }

        public static HashSet<Action<Entity>> StreamOutCustomActionsGet(this Entity entity)
        {
            return entity.GetData<HashSet<Action<Entity>>>("ECA_SO");
        }

        public static bool StreamOutCustomActionsRemove(this Entity entity, Action<Entity> action)
        {
            return entity.GetData<HashSet<Action<Entity>>>("ECA_SO")?.Remove(action) ?? false;
        }

        public static bool StreamOutCustomActionsReset(this Entity entity)
        {
            return entity.ResetData("ECA_SO");
        }

        public static async System.Threading.Tasks.Task<bool> WaitIsLoaded(GameEntity gEntity)
        {
            await RAGE.Game.Invoker.WaitAsync(500);

            var counter = 0;

            while (true)
            {
                if (gEntity?.Exists != true || counter > 20)
                    return false;

                await RAGE.Game.Invoker.WaitAsync(25);

                if (gEntity.Handle > 0)
                    return true;

                counter++;
            }
        }
    }
}