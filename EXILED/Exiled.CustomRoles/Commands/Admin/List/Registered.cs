// -----------------------------------------------------------------------
// <copyright file="Registered.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Commands.Admin.List
{
    using System;
    using System.Linq;
    using System.Text;

    using API.Features;

    using CommandSystem;

    using Exiled.API.Features.Pools;

    using Permissions.Extensions;

    /// <inheritdoc />
    internal sealed class Registered : ICommand
    {
        private Registered()
        {
        }

        /// <summary>
        ///     Gets the command instance.
        /// </summary>
        public static Registered Instance { get; } = new();

        /// <inheritdoc />
        public string Command { get; } = "registered";

        /// <inheritdoc />
        public string[] Aliases { get; } = { "r", "reg" };

<<<<<<<< HEAD:Exiled.CustomRoles/Commands/Admin/List/Registered.cs
        /// <inheritdoc />
        public string Description { get; } = "Список всех кастомных ролей.";

        /// <inheritdoc />
        public bool SanitizeResponse { get; } = false;

========
>>>>>>>> dev:EXILED/Exiled.CustomRoles/Commands/List/Registered.cs
        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customroles.list.registered"))
            {
                response = "Не хватает прав!";
                return false;
            }

            if (CustomRole.Registered.Count == 0)
            {
                response = "На сервере нет кастомных ролей.";
                return false;
            }

            StringBuilder builder = StringBuilderPool.Pool.Get().AppendLine();

            builder.Append("[Кастомные роли (").Append(CustomRole.Registered.Count).AppendLine(")]");

            foreach (CustomRole role in CustomRole.Registered.OrderBy(r => r.Id))
                builder.Append('[').Append(role.Id).Append(". ").Append(role.Name).Append(" (").Append(role.Role).Append(')').AppendLine("]");

            response = StringBuilderPool.Pool.ToStringReturn(builder);
            return true;
        }
    }
}
