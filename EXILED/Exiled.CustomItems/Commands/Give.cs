// -----------------------------------------------------------------------
// <copyright file="Give.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CommandSystem;

    using Exiled.API.Features;
    using Exiled.CustomItems.API.Features;
    using Exiled.Permissions.Extensions;

    using RemoteAdmin;

    /// <summary>
    /// The command to give a player an item.
    /// </summary>
    internal sealed class Give : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "give";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "g" };

        /// <inheritdoc/>
        public string Description { get; set; } = "Дает кастомный предмет.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customitems.give"))
            {
                response = "Не хватает прав!";
                return false;
            }

            if (arguments.Count == 0)
            {
                response = "give <Название/ID кастомного предмета> [Никнейм/ID/SteamID игрока или */all для выдачи всем]";
                return false;
            }

            if (!CustomItem.TryGet(arguments.At(0), out CustomItem? item))
            {
                response = $"Кастомный предмет {arguments.At(0)} не найден!";
                return false;
            }

            if (arguments.Count == 1)
            {
                if (sender is PlayerCommandSender playerCommandSender)
                {
                    Player player = Player.Get(playerCommandSender.SenderId);

                    if (!CheckEligible(player))
                    {
                        response = "Вы не можете получить кастомный предмет!";
                        return false;
                    }

                    item?.Give(player);
                    response = $"{item?.Name} дан игроку {player.Nickname} ({player.UserId})";
                    return true;
                }

                response = "Игрок не найден.";
                return false;
            }

            string identifier = string.Join(" ", arguments.Skip(1));

            switch (identifier)
            {
                case "*":
                case "all":
                    List<Player> eligiblePlayers = Player.List.Where(CheckEligible).ToList();
                    foreach (Player ply in eligiblePlayers)
                        item?.Give(ply);

                    response = $"Кастомный предмет {item?.Name} дан ({eligiblePlayers.Count} игрокам)";
                    return true;
                default:
                    if (Player.Get(identifier) is not { } player)
                    {
                        response = $"Невозможно найти игрока: {identifier}.";
                        return false;
                    }

                    if (!CheckEligible(player))
                    {
                        response = "Игрок не можете получить кастомный предмет!";
                        return false;
                    }

                    item?.Give(player);
                    response = $"{item?.Name} дан игроку {player.Nickname} ({player.UserId})";
                    return true;
            }
        }

        /// <summary>
        /// Checks if the player is eligible to receive custom items.
        /// </summary>
        private bool CheckEligible(Player player) => player.IsAlive && !player.IsCuffed && (player.Items.Count < 8);
    }
}
