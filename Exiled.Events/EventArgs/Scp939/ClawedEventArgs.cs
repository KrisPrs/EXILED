﻿// -----------------------------------------------------------------------
// <copyright file="ClawedEventArgs.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Scp939
{
    using API.Features;
    using Exiled.Events.EventArgs.Interfaces;

    /// <summary>
    /// Contains all information after SCP-939 attacks.
    /// </summary>
    public class ClawedEventArgs : IPlayerEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClawedEventArgs"/> class.
        /// </summary>
        /// <param name="player"><inheritdoc cref="Player"/></param>
        public ClawedEventArgs(Player player)
        {
            Player = player;
        }

        /// <summary>
        /// Gets the SCP-939.
        /// </summary>
        public Player Player { get; }
    }
}