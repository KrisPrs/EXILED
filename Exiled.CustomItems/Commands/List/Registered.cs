// -----------------------------------------------------------------------
// <copyright file="Registered.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.Commands.List
{
    using System;
    using System.Linq;
    using System.Text;

    using CommandSystem;

    using Exiled.CustomItems.API.Features;
    using Exiled.Permissions.Extensions;

    using NorthwoodLib.Pools;

    /// <inheritdoc/>
    internal sealed class Registered : ICommand
    {
        private Registered()
        {
        }

        /// <summary>
        /// Gets the command instance.
        /// </summary>
        public static Registered Instance { get; } = new();

        /// <inheritdoc/>
        public string Command { get; } = "registered";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "r", "reg" };

        /// <inheritdoc/>
        public string Description { get; } = "Получает все зарегистрированные кастомные предметы.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customitems.list.registered"))
            {
                response = "Не хватает прав!";
                return false;
            }

            if (CustomItem.Registered.Count == 0)
            {
                response = "На сервере нет кастомных предметов.";
                return false;
            }

            StringBuilder message = StringBuilderPool.Shared.Rent().AppendLine();

            message.Append("[Кастомные предметы (").Append(CustomItem.Registered.Count).AppendLine(")]");

            foreach (CustomItem customItem in CustomItem.Registered.OrderBy(item => item.Id))
                message.Append('[').Append(customItem.Id).Append(". ").Append(customItem.Name).Append(" (").Append(customItem.Type).Append(')').AppendLine("]");

            response = StringBuilderPool.Shared.ToStringReturn(message);
            return true;
        }
    }
}