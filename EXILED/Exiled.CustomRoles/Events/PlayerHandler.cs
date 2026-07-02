// -----------------------------------------------------------------------
// <copyright file="PlayerHandler.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Events
{
    using System.Collections.Generic;
    using System.Linq;

    using API;
    using API.Features;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using Exiled.API.Features.Spawn;
    using Exiled.Events.EventArgs.Player;
    using FLXLib.Extensions;
    using MEC;
    using PlayerRoles;
    using UnityEngine;

    /// <summary>
    ///     Event Handlers for the CustomRole API.
    /// </summary>
    public class PlayerHandler
    {
        /// <summary>
        ///     SessionVariable key.
        /// </summary>
        public const string LastCustomRoleKey = "LastCustomRole";

        /// <summary>
        ///     SessionVariable key.
        /// </summary>
        private readonly Dictionary<(Player spectator, Player target), string> sentSpectatorNames = new();

        /// <inheritdoc cref="Exiled.Events.Handlers.Server.WaitingForPlayers" />
        public void OnWaitingForPlayers()
        {
            Extensions.InternalPlayerToCustomRoles.Clear();
            Extensions.ToChangeRolePlayers.Clear();
            Extensions.AssignInventoryPlayers.Clear();
            sentSpectatorNames.Clear();
        }

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.ChangingRole" />
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Reason is SpawnReason.Destroyed or SpawnReason.None)
                return;

            if (ev.Player.TryGetCustomRole(out CustomRole customRole))
                ev.Player.SessionVariables[LastCustomRoleKey] = customRole;
            else
                ev.Player.SessionVariables.Remove(LastCustomRoleKey);
        }

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.Spawning" />
        public void OnSpawned(SpawnedEventArgs ev)
        {
            Log.Debug($"Spawned {ev.Player.Nickname}");
            if (Extensions.ToChangeRolePlayers.TryGetValue(ev.Player, out CustomRole cr))
            {
                Log.Debug($"Player with role {ev.Player.Role.Type} going to custom role {cr.Name} ({cr.Id})");
                if (cr.SpawnProperties.IsAny && !ev.SpawnFlags.HasFlag(RoleSpawnFlags.UseSpawnpoint))
                    ev.Player.Position = cr.SpawnProperties.GetRandomPoint() + (Vector3.up * 1.5f);

                cr.AddProperties(ev.Player, ev.Reason, Extensions.AssignInventoryPlayers.Remove(ev.Player));
                cr.RoleAdded(ev.Player);

                Extensions.ToChangeRolePlayers.Remove(ev.Player);
            }

            foreach (Player player in Player.List)
            {
                if (player == ev.Player)
                    continue;

                if (player.Role?.RoleAppearances.Count > 0 || player.Role?.TeamAppearances.Count > 0 ||
                    player.Role?.IndividualAppearances.Count > 0)
                    player.Role.UpdateAppearanceFor(ev.Player);
            }
        }

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.SendingRole" />
        public void OnSendingRole(SendingRoleEventArgs ev)
        {
            if (ev.Target == null)
                return;

            if (ev.Player.IsDead && ev.Target.TryGetCustomRole(out CustomRole role))
            {
                string spectatorText = role.GetSpectatorText(ev.Target);
                (Player Player, Player Target) key = (ev.Player, ev.Target);

                if (!sentSpectatorNames.TryGetValue(key, out string sentText) || sentText != spectatorText)
                {
                    ev.Player.SetDispayNicknameForTargetOnly(ev.Target, spectatorText);
                    sentSpectatorNames[key] = spectatorText;
                    Log.Debug($"[Name sync] Sent name of {ev.Target.Nickname} to {ev.Player.Nickname}");
                }
            }
            else if (ev.Target.IsDead && ev.Player.TryGetCustomRole(out role))
            {
                string spectatorText = role.GetSpectatorText(ev.Player);
                (Player Target, Player Player) key = (ev.Target, ev.Player);

                if (!sentSpectatorNames.TryGetValue(key, out string sentText) || sentText != spectatorText)
                {
                    ev.Target.SetDispayNicknameForTargetOnly(ev.Player, spectatorText);
                    sentSpectatorNames[key] = spectatorText;
                    Log.Debug($"[Name sync] Sent name of {ev.Player.Nickname} to {ev.Target.Nickname}");
                }
            }
            else
            {
                (Player Player, Player Target) key1 = (ev.Player, ev.Target);
                (Player Target, Player Player) key2 = (ev.Target, ev.Player);

                if (sentSpectatorNames.Remove(key1))
                {
                    ev.Target.SetDispayNicknameForTargetOnly(ev.Player, ev.Player.CustomName);
                    Log.Debug($"[Name sync] Name reset for {ev.Player.Nickname} of {ev.Target.Nickname}.");
                }

                if (sentSpectatorNames.Remove(key2))
                {
                    ev.Player.SetDispayNicknameForTargetOnly(ev.Target, ev.Target.CustomName);
                    Log.Debug($"[Name sync] Name reset for {ev.Target.Nickname} of {ev.Player.Nickname}.");
                }
            }
        }

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.ChangedNickname" />
        public void OnChangedNickname(ChangedNicknameEventArgs ev) =>
            Timing.CallDelayed(0.1f, () =>
            {
                if (ev.Player is { IsConnected: true } && ev.Player.TryGetCustomRole(out CustomRole role))
                {
                    foreach (Player player in Player.List)
                    {
                        if (!player.IsDead)
                            continue;

                        player.SetDispayNicknameForTargetOnly(ev.Player, role.GetSpectatorText(ev.Player));
                        Log.Debug($"[Name sync] [{nameof(this.OnChangedNickname)}] Sent name of {ev.Player.Nickname} to {player.Nickname}");
                    }
                }
            });
    }
}
