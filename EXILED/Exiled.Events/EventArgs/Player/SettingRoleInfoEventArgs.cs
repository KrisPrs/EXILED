// -----------------------------------------------------------------------
// <copyright file="SettingRoleInfoEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;
using UnityEngine;

/// <summary>
/// Contains all information before a player custom role info setted.
/// </summary>
public class SettingRoleInfoEventArgs
{
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingRoleInfoEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        /// The <see cref="Player"/> that custom role info is sets.
        /// </param>
        /// <param name="scale">
        /// <see cref="Vector3"/> scale is near to be set.
        /// </param>
        /// <param name="customInfo">
        /// <see cref="string"/> custom info is near to be set.
        /// </param>
        public SettingRoleInfoEventArgs(Player player, Vector3 scale, string customInfo)
        {
            this.Player = player;
            this.Scale = scale;
            this.CustomInfo = customInfo;
        }

        /// <summary>
        /// Gets the player whose custom role info is near to be set.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// Gets or sets the scale that is near to be set.
        /// </summary>
        public Vector3 Scale { get; set; }

        /// <summary>
        /// Gets or sets the custom role thas is near to be set.
        /// </summary>
        public string CustomInfo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the custom info setting can be set.
        /// </summary>
        public bool IsAllowed { get; set; } = true;
}