// -----------------------------------------------------------------------
// <copyright file="Npc.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features
{
#nullable enable
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using CommandSystem;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features.Components;
    using Exiled.API.Features.Roles;
    using Footprinting;
    using MEC;
    using Mirror;
    using PlayerRoles;
    using UnityEngine;

    using Object = UnityEngine.Object;

    /// <summary>
    /// Wrapper class for handling NPC players.
    /// </summary>
    public class Npc : Player
    {
        /// <inheritdoc cref="Player" />
        public Npc(ReferenceHub referenceHub)
            : base(referenceHub)
        {
        }

        /// <inheritdoc cref="Player" />
        public Npc(GameObject gameObject)
            : base(gameObject)
        {
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey, TValue}"/> containing all <see cref="Npc"/>'s on the server.
        /// </summary>
        public static new Dictionary<GameObject, Npc> Dictionary { get; } = new(Server.MaxPlayerCount, new ReferenceHub.GameObjectComparer());

        /// <summary>
        /// Gets a list of all <see cref="Npc"/>'s on the server.
        /// </summary>
        public static new IReadOnlyCollection<Npc> List => Dictionary.Values;

        /// <summary>
        /// Gets or sets the player's position.
        /// </summary>
        public override Vector3 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                if (Role is FpcRole fpcRole)
                    fpcRole.ClientRelativePosition = new(value);
            }
        }

        /// <summary>
        /// Retrieves the NPC associated with the specified ReferenceHub.
        /// </summary>
        /// <param name="rHub">The ReferenceHub to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the ReferenceHub, or <c>null</c> if not found.</returns>
        public static new Npc? Get(ReferenceHub rHub) => Player.Get(rHub) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the GameObject, or <c>null</c> if not found.</returns>
        public static new Npc? Get(GameObject gameObject) => Player.Get(gameObject) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified user ID.
        /// </summary>
        /// <param name="userId">The user ID to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the user ID, or <c>null</c> if not found.</returns>
        public static new Npc? Get(string userId) => Player.Get(userId) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified ID.
        /// </summary>
        /// <param name="id">The ID to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the ID, or <c>null</c> if not found.</returns>
        public static new Npc? Get(int id) => Player.Get(id) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified ICommandSender.
        /// </summary>
        /// <param name="sender">The ICommandSender to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the ICommandSender, or <c>null</c> if not found.</returns>
        public static new Npc? Get(ICommandSender sender) => Player.Get(sender) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified Footprint.
        /// </summary>
        /// <param name="footprint">The Footprint to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the Footprint, or <c>null</c> if not found.</returns>
        public static new Npc? Get(Footprint footprint) => Player.Get(footprint) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified CommandSender.
        /// </summary>
        /// <param name="sender">The CommandSender to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the CommandSender, or <c>null</c> if not found.</returns>
        public static new Npc? Get(CommandSender sender) => Player.Get(sender) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified Collider.
        /// </summary>
        /// <param name="collider">The Collider to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the Collider, or <c>null</c> if not found.</returns>
        public static new Npc? Get(Collider collider) => Player.Get(collider) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified net ID.
        /// </summary>
        /// <param name="netId">The net ID to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the net ID, or <c>null</c> if not found.</returns>
        public static new Npc? Get(uint netId) => Player.Get(netId) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified NetworkConnection.
        /// </summary>
        /// <param name="conn">The NetworkConnection to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the NetworkConnection, or <c>null</c> if not found.</returns>
        public static new Npc? Get(NetworkConnection conn) => Player.Get(conn) as Npc;

        /// <summary>
        /// Spawns an NPC based on the given parameters.
        /// </summary>
        /// <param name="name">The name of the NPC.</param>
        /// <param name="role">The RoleTypeId of the NPC.</param>
        /// <param name="position">The position to spawn the NPC.</param>
        /// <returns>The <see cref="Npc"/> spawned.</returns>
        public static Npc Spawn(string name, RoleTypeId role, Vector3? position = null) =>
            Spawn(name, role, 0, string.Empty, position);

        /// <summary>
        /// Spawns an NPC based on the given parameters.
        /// </summary>
        /// <param name="name">The name of the NPC.</param>
        /// <param name="role">The RoleTypeId of the NPC.</param>
        /// <param name="id">The player ID of the NPC.</param>
        /// <param name="userId">The userID of the NPC.</param>
        /// <param name="position">The position to spawn the NPC.</param>
        /// <returns>The <see cref="Npc"/> spawned.</returns>
        public static Npc Spawn(string name, RoleTypeId role, int id = 0, string userId = "", Vector3? position = null)
            => Spawn(name, role, SpawnReason.RoundStart, RoleSpawnFlags.All, userId, position);

        /// <summary>
        /// Spawns an NPC based on the given parameters.
        /// </summary>
        /// <param name="name">The name of the NPC.</param>
        /// <param name="role">The RoleTypeId of the NPC.</param>
        /// <param name="reason">The reason for set role.</param>
        /// <param name="roleFlags">The role flags for set role.</param>
        /// <param name="userId">The userID of the NPC.</param>
        /// <param name="position">The position to spawn the NPC.</param>
        /// <returns>The <see cref="Npc"/> spawned.</returns>
        public static Npc Spawn(string name, RoleTypeId role, SpawnReason reason = SpawnReason.RoundStart, RoleSpawnFlags roleFlags = RoleSpawnFlags.All, string userId = "", Vector3? position = null)
        {
            GameObject newObject = Object.Instantiate(NetworkManager.singleton.playerPrefab);
            Npc npc = new(newObject)
            {
                IsNPC = true,
            };

            FakeConnection fakeConnection = new(npc.Id);
            NetworkServer.AddPlayerForConnection(fakeConnection, newObject);

            npc.ReferenceHub.roleManager.InitializeNewRole(RoleTypeId.None, RoleChangeReason.None);
            Dictionary.Add(newObject, npc);

            try
            {
                npc.ReferenceHub.authManager._hub = npc.ReferenceHub;
                npc.ReferenceHub.authManager.UserId = string.IsNullOrEmpty(userId) ? $"Dummy{npc.Id}@localhost" : userId;
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            npc.ReferenceHub.nicknameSync._hub = npc.ReferenceHub;
            npc.ReferenceHub.nicknameSync.SetNick(name);

            if (position is not null)
            {
                roleFlags &= ~RoleSpawnFlags.UseSpawnpoint;
            }

            npc.ReferenceHub.characterClassManager._hub = npc.ReferenceHub;

            Timing.CallDelayed(Timing.WaitForOneFrame, () =>
            {
                npc.Role.Set(role, reason, roleFlags);
                if (position is not null)
                {
                    npc.Position = position.Value;
                }
            });

            return npc;
        }

        /// <summary>
        /// Destroys the NPC.
        /// </summary>
        public void Destroy()
        {
            NetworkConnectionToClient conn = ReferenceHub.connectionToClient;
            CustomNetworkManager.TypedSingleton.OnServerDisconnect(conn);
            Dictionary.Remove(GameObject);
            Object.Destroy(GameObject);
        }
    }
}
