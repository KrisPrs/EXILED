// -----------------------------------------------------------------------
// <copyright file="RoomPrefabs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using API.Enums;
    using API.Features;
    using API.Features.Pools;

    using CommandSystem;

    using Exiled.Permissions.Extensions;

    using RemoteAdmin;

    using UnityEngine;

    /// <summary>
    /// Отладочная команда: показывает содержимое <see cref="Room.RoomPrefabs"/>.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class RoomPrefabs : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "roomprefabs";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "rprefabs", "rpf" };

        /// <inheritdoc/>
        public string Description { get; set; } = "Показывает префабы Хуберта, найденные в комнате. Использование: roomprefabs [RoomType|all]";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            const string perm = "ee.roomprefabs";

            if (!sender.CheckPermission(perm) && sender is PlayerCommandSender playerSender && !playerSender.FullPermissions)
            {
                response = $"Нет прав \"{perm}\".";
                return false;
            }

            if (arguments.Count > 0 && string.Equals(arguments.At(0), "all", StringComparison.OrdinalIgnoreCase))
            {
                response = BuildSummary();
                return true;
            }

            Room room;

            if (arguments.Count > 0)
            {
                if (!Enum.TryParse(arguments.At(0), true, out RoomType roomType))
                {
                    response = $"Неизвестный RoomType \"{arguments.At(0)}\". Использование: roomprefabs [RoomType|all]";
                    return false;
                }

                room = Room.Get(roomType);

                if (room is null)
                {
                    response = $"Комната типа {roomType} на карте не найдена.";
                    return false;
                }
            }
            else
            {
                Player player = sender is PlayerCommandSender playerCommandSender ? Player.Get(playerCommandSender) : null;

                if (player is null)
                {
                    response = "Из консоли нужно указать RoomType или all.";
                    return false;
                }

                room = player.CurrentRoom;

                if (room is null)
                {
                    response = "Не удалось определить текущую комнату.";
                    return false;
                }
            }

            response = BuildRoomReport(room);
            return true;
        }

        private static string BuildSummary()
        {
            StringBuilder sb = StringBuilderPool.Pool.Get();

            int roomsWithPrefabs = 0;
            int totalObjects = 0;

            sb.AppendLine();

            foreach (Room room in Room.List.OrderBy(r => r.Zone).ThenBy(r => r.Type))
            {
                int count = room.RoomPrefabs.Values.Sum(list => list.Count);

                if (count == 0)
                    continue;

                roomsWithPrefabs++;
                totalObjects += count;

                sb.Append(room.Type).Append(" (").Append(room.Name).Append("): ключей ")
                    .Append(room.RoomPrefabs.Count).Append(", объектов ").Append(count).AppendLine();
            }

            sb.Append("Комнат всего: ").Append(Room.List.Count)
                .Append(", с префабами: ").Append(roomsWithPrefabs)
                .Append(", объектов всего: ").Append(totalObjects);

            string result = sb.ToString();
            StringBuilderPool.Pool.Return(sb);
            return result;
        }

        private static string BuildRoomReport(Room room)
        {
            StringBuilder sb = StringBuilderPool.Pool.Get();

            sb.AppendLine()
                .Append(room.Type).Append(" (").Append(room.Name).Append("), зона ").Append(room.Zone)
                .Append(": ключей ").Append(room.RoomPrefabs.Count).AppendLine();

            if (room.RoomPrefabs.Count == 0)
            {
                sb.Append("Префабы из списка не найдены. Дочерних объектов в комнате: ").Append(room.GetChildren().Count());
            }

            foreach (KeyValuePair<string, List<GameObject>> pair in room.RoomPrefabs.OrderBy(p => p.Key))
            {
                sb.Append(pair.Key).Append(": ").Append(pair.Value.Count).AppendLine();

                foreach (GameObject obj in pair.Value)
                {
                    if (obj == null)
                    {
                        sb.Append("\t(уничтожен)").AppendLine();
                        continue;
                    }

                    Vector3 local = obj.transform.localPosition;

                    sb.Append('\t').Append(obj.name)
                        .Append(obj.activeInHierarchy ? " [активен]" : " [выключен]")
                        .Append(" local=").Append(local.ToString("F2"))
                        .Append(" parent=").Append(obj.transform.parent != null ? obj.transform.parent.name : "-")
                        .AppendLine();
                }
            }

            string result = sb.ToString().TrimEnd();
            StringBuilderPool.Pool.Return(sb);
            return result;
        }
    }
}
