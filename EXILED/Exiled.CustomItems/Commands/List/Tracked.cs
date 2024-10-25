// -----------------------------------------------------------------------
// <copyright file="Tracked.cs" company="Exiled Team">
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

    using Exiled.API.Features;
    using Exiled.API.Features.Pools;
    using Exiled.CustomItems.API.Features;
    using Exiled.Permissions.Extensions;

    using RemoteAdmin;

    /// <inheritdoc/>
    internal sealed class Tracked : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "insideinventories";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "ii", "inside", "inv", "inventories" };

        /// <inheritdoc/>
        public string Description { get; set; } = "Получает все предметы которые лежат в инвенторях игроков.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customitems.list.insideinventories") && sender is PlayerCommandSender playerSender && !playerSender.FullPermissions)
            {
                response = "Не хватает прав!";
                return false;
            }

            StringBuilder message = StringBuilderPool.Pool.Get();

            int count = 0;

            foreach (CustomItem customItem in CustomItem.Registered)
            {
                if (customItem.TrackedSerials.Count == 0)
                    continue;

                message.AppendLine()
                    .Append('[').Append(customItem.Id).Append(". ").Append(customItem.Name).Append(" (").Append(customItem.Type).Append(')')
                    .Append(" {").Append(customItem.TrackedSerials.Count).AppendLine("}]").AppendLine();

                count += customItem.TrackedSerials.Count;

                foreach (int insideInventory in customItem.TrackedSerials)
                {
                    Player owner = Player.List.FirstOrDefault(player => player.Inventory.UserInventory.Items.Any(item => item.Key == insideInventory));

                    message.Append(insideInventory).Append(". ");

                    if (owner is null)
                        message.AppendLine("Никто");
                    else
                        message.Append(owner.Nickname).Append(" (").Append(owner.UserId).Append(") (").Append(owner.Id).Append(") [").Append(owner.Role).AppendLine("]");
                }
            }

            if (message.Length == 0)
                message.Append("Кастомные предметы не найдены.");
            else
                message.Insert(0, Environment.NewLine + "[Custom items inside inventories (" + count + ")]" + Environment.NewLine);

            response = StringBuilderPool.Pool.ToStringReturn(message);
            return true;
        }
    }
}
