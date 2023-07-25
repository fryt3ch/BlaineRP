using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.UI.CEF;
using Newtonsoft.Json;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Commands
{
    [Script(int.MaxValue)]
    public partial class Core
    {
        private static DateTime LastSent;

        private static List<CommandInstance> All { get; set; } = new List<CommandInstance>();

        private static bool LastArgDeserializedSucces { get; set; }

        private static JsonSerializerSettings SerializeSettingsExecute { get; set; } = new JsonSerializerSettings()
        {
            Error = (sender, args) => { LastArgDeserializedSucces = false; args.ErrorContext.Handled = true; },
            MissingMemberHandling = MissingMemberHandling.Error
        };

        /// <summary>Метод для выполнения команды</summary>
        /// <param name="cmdName">Название команды (может быть основным либо одним из псевдонимов)</param>
        /// <param name="args">Аргументы</param>
        public static void Execute(string cmdName, params string[] args)
        {
            cmdName = cmdName.ToLower();

            var inst = All.Where(x => x.Attribute.Name == cmdName || x.Attribute.Aliases.Contains(cmdName)).FirstOrDefault();

            if (inst == null || inst.Attribute.AdminOnly && (PlayerData.GetData(Player.LocalPlayer)?.AdminLevel ?? -1) < 0)
            {
                Notification.Show(Notification.Types.Error, Locale.Notifications.Commands.Header, Locale.Notifications.Commands.NotFound);

                return;
            }

            var correct = true;

            var newArgs = new object[inst.Parameters.Length];

            for (int i = 0; i < inst.Parameters.Length; i++)
            {
                if (i < args.Length)
                {
                    if (inst.Parameters[i].ParameterType == typeof(string))
                    {
                        newArgs[i] = args[i];
                    }
                    else
                    {
                        if (inst.Parameters[i].ParameterType == typeof(float) || inst.Parameters[i].ParameterType == typeof(float?))
                        {
                            args[i] = args[i].Replace("f", "");
                        }

                        LastArgDeserializedSucces = true;

                        var newArg = JsonConvert.DeserializeObject(args[i], inst.Parameters[i].ParameterType, SerializeSettingsExecute);

                        if (LastArgDeserializedSucces)
                        {
                            newArgs[i] = newArg;
                        }
                        else
                        {
                            correct = false;

                            break;
                        }
                    }
                }
                else if (inst.Parameters[i].HasDefaultValue)
                    newArgs[i] = inst.Parameters[i].DefaultValue;
                else
                {
                    correct = false;

                    break;
                }
            }

            if (!correct)
            {
                Notification.Show(Notification.Types.Error, Locale.Notifications.Commands.Header, string.Format(Locale.Notifications.Commands.WrongUsing, $"/{inst.Attribute.Name} {string.Join(", ", inst.Parameters.Select(x => x.HasDefaultValue ? x.Name.ToUpper() + "?" : x.Name.ToUpper()))}"));

                return;
            }
            else
                inst.MethodInfo.Invoke(null, newArgs.Length > 0 ? newArgs : null);
        }

        public static void CallRemote(string cmdId, params object[] args) => Events.CallRemote("Cmd::Exec", cmdId, string.Join('&', args));

        public Core()
        {
            foreach (var method in typeof(Core).GetMethods().Where(x => x.IsStatic))
            {
                var attr = method.GetCustomAttribute<CommandAttribute>();

                if (attr == null)
                    continue;

                All.Add(new CommandInstance(method, attr));
            }
        }
    }
}
