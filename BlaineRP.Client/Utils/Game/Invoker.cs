﻿using RAGE;
using System.Linq;

namespace BlaineRP.Client.Utils.Game
{
    internal static class Invoker
    {
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

            return Additional.Storage.LastData != null ? RAGE.Util.Json.Deserialize<float>(Additional.Storage.LastData) : 0f;
        }

        public static async System.Threading.Tasks.Task<float> InvokeFloatViaJs(RAGE.Game.Natives hash, params object[] args) => await InvokeFloatViaJs((ulong)hash, args);

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

            return Additional.Storage.LastData != null ? RAGE.Util.Json.Deserialize<T>(Additional.Storage.LastData) : default;
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

            return Additional.Storage.LastData != null ? RAGE.Util.Json.Deserialize<T>(Additional.Storage.LastData) : default;
        }
    }
}
