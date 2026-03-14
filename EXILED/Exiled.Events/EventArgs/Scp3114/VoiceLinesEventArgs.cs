// -----------------------------------------------------------------------
// <copyright file="VoiceLinesEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Scp3114
{
    using API.Features;
    using Exiled.API.Features.Roles;
    using Interfaces;

    using static PlayerRoles.PlayableScps.Scp3114.Scp3114VoiceLines;

    /// <summary>
    /// Contains all information prior to sending voiceline SCP-3114.
    /// </summary>
    public class VoiceLinesEventArgs : IScp3114Event, IDeniableEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceLinesEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        /// <inheritdoc cref="Player" />
        /// </param>
        /// <param name="voiceLine">
        /// <inheritdoc cref="VoiceLine" />
        /// </param>
        /// <param name="isAllowed">
        /// <inheritdoc cref="IsAllowed" />
        /// </param>
        public VoiceLinesEventArgs(ReferenceHub player, VoiceLinesDefinition voiceLine, bool isAllowed = true)
        {
            this.Player = Player.Get(player);
            this.Scp3114 = this.Player.Role.As<Scp3114Role>();
            this.VoiceLine = voiceLine;
            this.IsAllowed = isAllowed;
        }

        /// <inheritdoc/>
        public Player Player { get; }

        /// <inheritdoc/>
        public Scp3114Role Scp3114 { get; }

        /// <summary>
        /// Gets or sets the <see cref="VoiceLinesDefinition" />.
        /// </summary>
        public VoiceLinesDefinition VoiceLine { get; set; }

        /// <inheritdoc/>
        public bool IsAllowed { get; set; }
    }
}