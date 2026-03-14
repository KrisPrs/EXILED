// -----------------------------------------------------------------------
// <copyright file="SpawningRagdollEventArgs.cs" company="ExMod Team">
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
    /// Contains all information before spawning a player ragdoll.
    /// </summary>
    public class SpawningRagdollEventArgs : IPlayerEvent, IDeniableEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpawningRagdollEventArgs" /> class.
        /// </summary>
        /// <param name="info">
        /// <inheritdoc cref="Info" />
        /// </param>
        /// <param name="isAllowed">
        /// <inheritdoc cref="IsAllowed" />
        /// </param>
        public SpawningRagdollEventArgs(RagdollData info, bool isAllowed = true)
        {
            this.Info = info;
            this.Player = Player.Get(info.OwnerHub);
            this.Scale = this.Player.Scale;
            this.IsAllowed = isAllowed;
        }

        /// <summary>
        /// Gets or sets the spawning position of the ragdoll.
        /// </summary>
        public Vector3 Position
        {
            get => this.Info.StartRelativePosition.Position;
            set => this.Info = new RagdollData(this.Player.ReferenceHub, this.DamageHandlerBase, this.Role, new(value), this.Info.StartRelativeRotation, this.Scale, this.Nickname, this.CreationTime);
        }

        /// <summary>
        /// Gets or sets the ragdoll's rotation.
        /// </summary>
        public Quaternion Rotation
        {
            get => WaypointBase.GetWorldRotation(this.Info.StartRelativePosition.WaypointId, this.Info.StartRelativeRotation);
            set => this.Info = new RagdollData(this.Player.ReferenceHub, this.DamageHandlerBase, this.Role, this.Info.StartRelativePosition, WaypointBase.GetWorldRotation(this.Info.StartRelativePosition.WaypointId, value), this.Scale, this.Nickname, this.CreationTime);
        }

        /// <summary>
        /// Gets or sets the ragdoll's scale with RagdollData.
        /// </summary>
        public Vector3 Scale
        {
            get => this.Info.Scale;
            set => this.Info = new RagdollData(this.Player.ReferenceHub, this.DamageHandlerBase, this.Role, this.Info.StartRelativePosition, this.Info.StartRelativeRotation, Vector3.Scale(value, RagdollManager.GetDefaultScale(this.Role)), this.Nickname, this.CreationTime);
        }

        /// <summary>
        /// Gets or sets the ragdoll's scale with GameObject.
        /// </summary>
        public Vector3 RagdollScale { get; set; } = Vector3.one;

        /// <summary>
        /// Gets or sets the ragdoll's <see cref="RoleTypeId" />.
        /// </summary>
        public RoleTypeId Role
        {
            get => this.Info.RoleType;
            set => this.Info = new RagdollData(this.Player.ReferenceHub, this.DamageHandlerBase, value, this.Info.StartRelativePosition, this.Info.StartRelativeRotation, this.Scale, this.Nickname, this.CreationTime);
        }

        /// <summary>
        /// Gets the ragdoll's creation time.
        /// </summary>
        public double CreationTime => this.Info.CreationTime;

        /// <summary>
        /// Gets or sets the ragdoll's nickname.
        /// </summary>
        public string Nickname
        {
            get => this.Info.Nickname;
            set => this.Info = new RagdollData(this.Player.ReferenceHub, this.DamageHandlerBase, this.Role, this.Info.StartRelativePosition, this.Info.StartRelativeRotation, this.Scale, value, this.CreationTime);
        }

        /// <summary>
        /// Gets or sets the ragdoll's <see cref="RagdollData" />.
        /// </summary>
        public RagdollData Info { get; set; }

        /// <summary>
        /// Gets or sets the ragdoll's <see cref="PlayerStatsSystem.DamageHandlerBase" />.
        /// </summary>
        public DamageHandlerBase DamageHandlerBase
        {
            get => this.Info.Handler;
            set => this.Info = new RagdollData(this.Player.ReferenceHub, value, this.Role, this.Info.StartRelativePosition, this.Info.StartRelativeRotation, this.Scale, this.Nickname, this.CreationTime);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ragdoll can be spawned.
        /// </summary>
        public bool IsAllowed { get; set; }

        /// <summary>
        /// Gets the <see cref="Player">Owner</see> of the ragdoll.
        /// </summary>
        public Player Player { get; }
    }
}