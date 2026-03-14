// -----------------------------------------------------------------------
// <copyright file="SpawnedRagdollEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using API.Features;

    using Interfaces;

    using PlayerRoles;
    using PlayerRoles.Ragdolls;
    using PlayerStatsSystem;
    using RelativePositioning;
    using UnityEngine;

    /// <summary>
    /// Contains all information after spawning a player ragdoll.
    /// </summary>
    public class SpawnedRagdollEventArgs : IPlayerEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpawnedRagdollEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        /// <inheritdoc cref="Player" />
        /// </param>
        /// <param name="ragdoll">
        /// <inheritdoc cref="Ragdoll" />
        /// </param>
        /// <param name="info">
        /// <inheritdoc cref="Info" />
        /// </param>
        /// <param name="damageHandlerBase">
        /// <inheritdoc cref="DamageHandlerBase" />
        /// </param>
        public SpawnedRagdollEventArgs(Player player, Ragdoll ragdoll, RagdollData info, DamageHandlerBase damageHandlerBase)
        {
            this.Player = player;
            this.Ragdoll = ragdoll;
            this.Info = info;
            this.DamageHandlerBase = damageHandlerBase;
        }

        /// <summary>
        /// Gets the ragdoll's position.
        /// </summary>
        public Vector3 Position => this.Info.StartRelativePosition.Position;

        /// <summary>
        /// Gets the ragdoll's rotation.
        /// </summary>
        public Quaternion Rotation => WaypointBase.GetWorldRotation(this.Info.StartRelativePosition.WaypointId, this.Info.StartRelativeRotation);

        /// <summary>
        /// Gets the ragdoll's <see cref="RoleTypeId" />.
        /// </summary>
        public RoleTypeId Role => this.Info.RoleType;

        /// <summary>
        /// Gets the ragdoll's creation time.
        /// </summary>
        public double CreationTime => this.Info.CreationTime;

        /// <summary>
        /// Gets the ragdoll's nickname.
        /// </summary>
        public string Nickname => this.Info.Nickname;

        /// <summary>
        /// Gets the ragdoll's <see cref="RagdollData" />.
        /// </summary>
        public RagdollData Info { get; }

        /// <summary>
        /// Gets the ragdoll's <see cref="PlayerStatsSystem.DamageHandlerBase" />.
        /// </summary>
        public DamageHandlerBase DamageHandlerBase { get; }

        /// <summary>
        /// Gets the spawned <see cref="API.Features.Ragdoll"/>.
        /// </summary>
        public Ragdoll Ragdoll { get; }

        /// <summary>
        /// Gets the <see cref="Player">Owner</see> of the ragdoll.
        /// </summary>
        public Player Player { get; }
    }
}