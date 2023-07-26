using System.Reflection;

namespace BlaineRP.Client.Game.Management.Commands
{
    /// <summary>Класс, служащий для хранения информации о команде и её методе</summary>
    internal class CommandInstance
    {
        public CommandInstance(MethodInfo methodInfo, CommandAttribute attribute)
        {
            Attribute = attribute;

            MethodInfo = methodInfo;
        }

        /// <summary>Данные команды</summary>
        public CommandAttribute Attribute { get; }

        /// <summary>Параметры команды</summary>
        public ParameterInfo[] Parameters => MethodInfo.GetParameters();

        /// <summary>Данные метода команды</summary>
        public MethodInfo MethodInfo { get; }
    }
}