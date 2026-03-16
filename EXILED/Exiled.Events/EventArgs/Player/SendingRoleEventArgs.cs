// -----------------------------------------------------------------------
// <copyright file="SendingRoleEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Interfaces;
    using PlayerRoles;

    /// <summary>
    /// Contains all information before a <see cref="Player"/>'s role is sent to a client.
    /// </summary>
    public class SendingRoleEventArgs : IPlayerEvent, IExiledEvent
    {
        private RoleTypeId roleTypeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendingRoleEventArgs"/> class.
        /// </summary>
        /// <param name="player">The player whose role is being sent.</param>
        /// <param name="target">The player receiving the role information.</param>
        /// <param name="roleType">The role type being sent.</param>
        public SendingRoleEventArgs(Player player, Player target, RoleTypeId roleType)
        {
            this.Player = player;
            this.Target = target;
            this.roleTypeId = roleType;
        }

        /// <summary>
        /// Gets the <see cref="Player"/> on whose behalf the role change request is sent.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// Gets the <see cref="Player"/> to whom the request is sent.
        /// </summary>
        /// <remarks>This is never null when event is invoked from the transpiler.</remarks>
        public Player Target { get; }

        /// <summary>
        /// Gets or sets the <see cref="RoleTypeId"/> that is sent to the <see cref="Target"/>.
        /// </summary>
        /// <remarks>Checks value by player's <see cref="API.Features.Roles.Role.CheckAppearanceCompatibility(RoleTypeId)"/>.</remarks>
        public RoleTypeId RoleType
        {
            get => this.roleTypeId;
            set
            {
                if (this.Player?.Role != null && !this.Player.Role.CheckAppearanceCompatibility(value))
                    return;

                this.roleTypeId = value;
            }
        }
    }
}