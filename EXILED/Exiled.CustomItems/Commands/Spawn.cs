// -----------------------------------------------------------------------
// <copyright file="Spawn.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.Commands
{
    using System;

    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.CustomItems.API.Features;
    using Exiled.Permissions.Extensions;

    using UnityEngine;

    /// <summary>
    /// The command to spawn a specific item.
    /// </summary>
    internal sealed class Spawn : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "spawn";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "sp" };

        /// <inheritdoc/>
        public string Description { get; set; } = "Спавнит кастомный предмет.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customitems.spawn"))
            {
                response = "Не хватает прав!";
                return false;
            }

            if (arguments.Count < 2)
            {
                response = "spawn [Название/ID кастомного предмета] [Никнейм/SteamID игока]\nspawn [Название/ID кастомного предмета] [X] [Y] [Z]";
                return false;
            }

            if (!CustomItem.TryGet(arguments.At(0), out CustomItem? item))
            {
                response = $" {arguments.At(0)} is not a valid custom item.";
                return false;
            }

            Vector3 position;

            if (Player.Get(arguments.At(1)) is Player player)
            {
                if (player.IsDead)
                {
                    response = $"Игрок мертв!";
                    return false;
                }

                position = player.Position;
            }
            else if (arguments.Count > 3)
            {
                if (!float.TryParse(arguments.At(1), out float x) || !float.TryParse(arguments.At(2), out float y) || !float.TryParse(arguments.At(3), out float z))
                {
                    response = "Невозможно получить координату (попробуй писать через , а не .)";
                    return false;
                }

                position = new Vector3(x, y, z);
            }
            else
            {
                response = $"Невозможно найти локацию для спавна.";
                return false;
            }

            item?.Spawn(position);

            response = $"{item?.Name} ({item?.Type}) заспавнился на позиции {position}.";
            return true;
        }
    }
}
