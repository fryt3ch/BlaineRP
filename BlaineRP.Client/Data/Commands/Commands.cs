using Newtonsoft.Json;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlaineRP.Client.Data
{
    [Script(int.MaxValue)]
    public partial class Commands
    {
        private static DateTime LastSent;

        private static List<Instance> All { get; set; } = new List<Instance>();

        [AttributeUsage(AttributeTargets.Method)]
        /// <summary>Класс, служащий для хранения информации о команде</summary>
        private class CommandAttribute : Attribute
        {
            /// <summary>Основное название команды</summary>
            public string Name { get; set; }
            /// <summary>Псевдонимы</summary>
            public string[] Aliases { get; set; }
            /// <summary>Доступна ли эта команда только администраторам?</summary>
            public bool AdminOnly { get; set; }
            /// <summary>Описание команды</summary>
            public string Description { get; set; }

            /// <summary>Информация о новой команде<br/><br/>Параметры метода, который содержит этот аттрибут, должны быть IConvertable!<br/>В противном случае, команда не будет загружена</summary>
            /// <param name="Name">Название</param>
            /// <param name="AdminOnly">Доступна ли только администраторам?</param>
            /// <param name="Aliases">Псевдонимы</param>
            public CommandAttribute(string Name, bool AdminOnly, string Description, params string[] Aliases)
            {
                this.Name = Name;
                this.AdminOnly = AdminOnly;
                this.Aliases = Aliases;

                this.Description = Description;
            }
        }

        /// <summary>Класс, служащий для хранения информации о команде и её методе</summary>
        private class Instance
        {
            /// <summary>Данные команды</summary>
            public CommandAttribute Attribute { get; }

            /// <summary>Параметры команды</summary>
            public ParameterInfo[] Parameters => MethodInfo.GetParameters();

            /// <summary>Данные метода команды</summary>
            public MethodInfo MethodInfo { get; }

            public Instance(MethodInfo MethodInfo, CommandAttribute Attribute)
            {
                this.Attribute = Attribute;

                this.MethodInfo = MethodInfo;
            }
        }

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

            if (inst == null || inst.Attribute.AdminOnly && (Sync.Players.GetData(Player.LocalPlayer)?.AdminLevel ?? -1) < 0)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Commands.Header, Locale.Notifications.Commands.NotFound);

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
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Commands.Header, string.Format(Locale.Notifications.Commands.WrongUsing, $"/{inst.Attribute.Name} {string.Join(", ", inst.Parameters.Select(x => x.HasDefaultValue ? x.Name.ToUpper() + "?" : x.Name.ToUpper()))}"));

                return;
            }
            else
                inst.MethodInfo.Invoke(null, newArgs.Length > 0 ? newArgs : null);
        }

        public static void CallRemote(string cmdId, params object[] args) => Events.CallRemote("Cmd::Exec", cmdId, string.Join('&', args));

        public Commands()
        {
            foreach (var method in typeof(Commands).GetMethods().Where(x => x.IsStatic))
            {
                var attr = method.GetCustomAttribute<CommandAttribute>();

                if (attr == null)
                    continue;

                All.Add(new Instance(method, attr));
            }
        }
    }
}
