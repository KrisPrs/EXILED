// -----------------------------------------------------------------------
// <copyright file="Info.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Commands.Admin
{
    using System;
    using System.Text;

    using API.Features;

    using CommandSystem;

    using Exiled.API.Features.Pools;

    using Permissions.Extensions;

    /// <summary>
    ///     The command to view info about a specific role.
    /// </summary>
    internal sealed class Info : ICommand
    {
        /// <inheritdoc />
        public string Command { get; } = "info";

        /// <inheritdoc />
        public string[] Aliases { get; } = { "i" };

        /// <inheritdoc />
        public string Description { get; set; } = "Информация про кастомную роль.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customroles.info"))
            {
                response = "Не хватает прав!";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "info [Название/ID кастомной роли]";
                return false;
            }

            if ((!(uint.TryParse(arguments.At(0), out uint id) && CustomRole.TryGet(id, out CustomRole? role)) && !CustomRole.TryGet(arguments.At(0), out role)) || role is null)
            {
                response = $"{arguments.At(0)} не идентификатор кастомной роли.";
                return false;
            }

            StringBuilder builder = StringBuilderPool.Pool.Get().AppendLine();

            builder.Append("<color=#E6AC00>-</color> <color=#00D639>").Append(role.Name)
                .Append("</color> <color=#05C4E8>(").Append(role.Id).Append(")</color>")
                .Append("- ").AppendLine(role.Description)
                .AppendLine(role.Role.ToString())
                .Append("- Здоровье: ").AppendLine(role.MaxHealth.ToString()).AppendLine();

            response = StringBuilderPool.Pool.ToStringReturn(builder);
            return true;
        }
    }
}
