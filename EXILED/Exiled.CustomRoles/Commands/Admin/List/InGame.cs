// -----------------------------------------------------------------------
// <copyright file="InGame.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Commands.Admin.List
{
    using System;
    using System.Text;

    using API.Features;

    using CommandSystem;

    using Exiled.API.Features;

    using NorthwoodLib.Pools;

    using Permissions.Extensions;

    /// <inheritdoc />
    internal sealed class InGame : ICommand
    {
        /// <inheritdoc />
        public string Command { get; } = "ingame";

        /// <inheritdoc />
        public string[] Aliases { get; } = { "ig", "alife" };

        /// <inheritdoc />
        public string Description { get; set; } = "Получает все кастомные роли которые сейчас учавствуют в раунде.";

        /// <inheritdoc />
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customroles.list.ingame"))
            {
                response = "Не хватает прав!";
                return false;
            }

            StringBuilder message = StringBuilderPool.Shared.Rent();

            int count = 0;

            foreach (CustomRole customRole in CustomRole.Registered)
            {
                if (customRole.TrackedPlayers.Count == 0)
                    continue;

                message.AppendLine()
                    .Append('[').Append(customRole.Id).Append(". ").Append(customRole.Name).Append(" (").Append(customRole.Role).Append(')')
                    .Append(" {").Append(customRole.TrackedPlayers.Count).AppendLine("}]").AppendLine();

                count += customRole.TrackedPlayers.Count;

                foreach (Player owner in customRole.TrackedPlayers)
                {
                    message.Append(owner.Nickname).Append(" (").Append(owner.UserId).Append(") (").Append(owner.Id).Append(") [").Append(owner.Role.Type).AppendLine("]");
                }
            }

            if (message.Length == 0)
                message.Append("Кастомные роли не найдены.");
            else
                message.Insert(0, Environment.NewLine + "[Текущие живые кастомные роли: (" + count + ")]" + Environment.NewLine);

            response = StringBuilderPool.Shared.ToStringReturn(message);
            return true;
        }
    }
}