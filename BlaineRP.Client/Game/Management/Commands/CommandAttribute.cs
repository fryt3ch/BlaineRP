using System;

namespace BlaineRP.Client.Game.Management.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    /// <summary>Класс, служащий для хранения информации о команде</summary>
    internal class CommandAttribute : Attribute
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
        /// <param name="name">Название</param>
        /// <param name="adminOnly">Доступна ли только администраторам?</param>
        /// <param name="aliases">Псевдонимы</param>
        public CommandAttribute(string name, bool adminOnly, string description, params string[] aliases)
        {
            Name = name;
            AdminOnly = adminOnly;
            Aliases = aliases;

            Description = description;
        }
    }
}