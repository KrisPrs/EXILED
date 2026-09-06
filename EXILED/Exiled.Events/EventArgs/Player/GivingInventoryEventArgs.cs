// -----------------------------------------------------------------------
// <copyright file="GivingInventoryEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using System.Collections.Generic;

    using Exiled.API.Features;

    /// <summary>
    ///     Contains all the information before giving a CustomRoles inventory preset to a player.
    ///     Allows external plugins to edit the resolved inventory (e.g. to guarantee specific items) before it's
    ///     actually rolled and given.
    /// </summary>
    public class GivingInventoryEventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GivingInventoryEventArgs" /> class.
        /// </summary>
        /// <param name="player"><inheritdoc cref="Player" /></param>
        /// <param name="roleId"><inheritdoc cref="RoleId" /></param>
        /// <param name="inventory"><inheritdoc cref="Inventory" /></param>
        public GivingInventoryEventArgs(Player player, uint roleId, List<Dictionary<string, short>> inventory)
        {
            this.Player = player;
            this.RoleId = roleId;
            this.Inventory = inventory;
        }

        /// <summary>
        ///     Gets the player who is about to receive the inventory preset.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        ///     Gets the CustomRole whose inventory is about to be given.
        /// </summary>
        public uint RoleId { get; }

        /// <summary>
        ///     Gets or sets a mutable clone of Inventory. Any changes made here (chances,
        ///     removed/added entries, etc.) will directly affect the outcome of the item rolling process, and won't
        ///     touch the original Inventory config.
        /// </summary>
        public List<Dictionary<string, short>> Inventory { get; set; }
    }
}