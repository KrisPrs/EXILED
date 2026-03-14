// -----------------------------------------------------------------------
// <copyright file="PlacingTantrumEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Scp173
{
    using System;

    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Interfaces;

    using Hazards;
    using PlayerRoles.Subroutines;

    using Scp173Role = API.Features.Roles.Scp173Role;

    /// <summary>
    /// Contains all information before the tantrum is placed.
    /// </summary>
    public class PlacingTantrumEventArgs : IScp173Event, IDeniableEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlacingTantrumEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        /// <inheritdoc cref="Player" />
        /// </param>
        /// <param name="tantrumHazard">
        /// <inheritdoc cref="TantrumHazard" />
        /// </param>
        /// <param name="cooldown">
        /// <inheritdoc cref="Cooldown" />
        /// </param>
        /// <param name="isAllowed">
        /// <inheritdoc cref="IsAllowed" />
        /// </param>
        public PlacingTantrumEventArgs(Player player, TantrumEnvironmentalHazard tantrumHazard, AbilityCooldown cooldown, bool isAllowed = true)
        {
            this.Player = player;
            this.Scp173 = this.Player.Role.As<Scp173Role>();
#pragma warning disable CS0618
            this.TantrumHazard = tantrumHazard;
#pragma warning restore CS0618
            this.Cooldown = cooldown;
            this.IsAllowed = isAllowed;
        }

        /// <summary>
        /// Gets the player's <see cref="Scp173Role" /> instance.
        /// </summary>
        public Scp173Role Scp173 { get; }

        /// <summary>
        /// Gets the <see cref="TantrumEnvironmentalHazard" />.
        /// </summary>
        [Obsolete("This propperty is always null")]
        public TantrumEnvironmentalHazard TantrumHazard { get; }

        /// <summary>
        /// Gets the tantrum <see cref="AbilityCooldown"/>.
        /// </summary>
        public AbilityCooldown Cooldown { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the tantrum can be placed.
        /// </summary>
        public bool IsAllowed { get; set; }

        /// <summary>
        /// Gets the player who's placing the tantrum.
        /// </summary>
        public Player Player { get; }
    }
}