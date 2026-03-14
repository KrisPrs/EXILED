// -----------------------------------------------------------------------
// <copyright file="PlayingVoiceEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Scp939
{
    using API.Features;
    using Exiled.API.Features.Roles;
    using Interfaces;

    /// <summary>
    /// Contains all information before SCP-939 plays a stolen player's voice.
    /// </summary>
    public class PlayingVoiceEventArgs : IScp939Event, IDeniableEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayingVoiceEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        /// <inheritdoc cref="Player" />
        /// </param>
        /// <param name="stolen">
        /// The player who's voice was stolen.
        /// </param>
        public PlayingVoiceEventArgs(ReferenceHub player, ReferenceHub stolen)
        {
            this.Player = Player.Get(player);
            this.Scp939 = this.Player.Role.As<Scp939Role>();
            this.Stolen = Player.Get(stolen);
        }

        /// <summary>
        /// Gets or sets a value indicating whether SCP-939 can play the stolen voice.
        /// </summary>
        public bool IsAllowed { get; set; } = true;

        /// <summary>
        /// Gets the players who's voice was stolen.
        /// </summary>
        public Player Stolen { get; }

        /// <summary>
        /// Gets the player who's controlling SCP-939.
        /// </summary>
        public Player Player { get; }

        /// <inheritdoc/>
        public Scp939Role Scp939 { get; }
    }
}