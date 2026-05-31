// -----------------------------------------------------------------------
// <copyright file="ChangingRoleEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using API.Enums;
    using API.Features;
    using Exiled.API.Extensions;
    using Exiled.API.Features.Pools;
    using Interfaces;
    using InventorySystem;
    using PlayerRoles;

    /// <summary>
    /// Contains all information before a player's <see cref="RoleTypeId" /> changes.
    /// </summary>
    public class ChangingRoleEventArgs : IPlayerEvent, IDeniableEvent
    {
        private RoleTypeId newRole;
        private SpawnReason reason;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangingRoleEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        /// <inheritdoc cref="Player" />
        /// </param>
        /// <param name="newRole">
        /// <inheritdoc cref="NewRole" />
        /// </param>
        /// <param name="reason">
        /// <inheritdoc cref="Reason" />
        /// </param>
        /// <param name="spawnFlags">
        /// <inheritdoc cref="SpawnFlags" />
        /// </param>
        public ChangingRoleEventArgs(Player player, RoleTypeId newRole, RoleChangeReason reason, RoleSpawnFlags spawnFlags)
        {
            this.Player = player;
            this.NewRole = newRole;
            this.reason = (SpawnReason)reason;
            this.SpawnFlags = spawnFlags;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ChangingRoleEventArgs"/> class.
        /// </summary>
        ~ChangingRoleEventArgs()
        {
            ListPool<ItemType>.Pool.Return(this.Items);
            DictionaryPool<ItemType, ushort>.Pool.Return(this.Ammo);
        }

        /// <summary>
        /// Gets the player whose <see cref="RoleTypeId" /> is changing.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// Gets or sets the new player's role.
        /// </summary>
        public RoleTypeId NewRole
        {
            get => this.newRole;
            set
            {
                if (this.reason == SpawnReason.Destroyed)
                {
                    Log.Error($"Tried to change NewRole for Destroyed!\n{new StackTrace()}");
                    return;
                }

                InventoryRoleInfo inventory = value.GetInventory();

                this.Items.Clear();
                this.Ammo.Clear();

                foreach (ItemType itemType in inventory.Items)
                    this.Items.Add(itemType);

                foreach (KeyValuePair<ItemType, ushort> ammoPair in inventory.Ammo)
                    this.Ammo.Add(ammoPair.Key, ammoPair.Value);

                this.newRole = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current event is safe to do some actions with player.
        /// </summary>
        public bool IsSafe => this.NewRole != RoleTypeId.Destroyed && this.Reason != SpawnReason.Destroyed;

        /// <summary>
        /// Gets base items that the player will receive.
        /// </summary>
        public List<ItemType> Items { get; } = ListPool<ItemType>.Pool.Get();

        /// <summary>
        /// Gets the base ammo values for the new role.
        /// </summary>
        public Dictionary<ItemType, ushort> Ammo { get; } = DictionaryPool<ItemType, ushort>.Pool.Get();

        /// <summary>
        /// Gets or sets a value indicating whether the inventory will be preserved.
        /// </summary>
        public bool ShouldPreserveInventory
        {
            get => !this.SpawnFlags.HasFlag(RoleSpawnFlags.AssignInventory);
            set => this.SpawnFlags = this.SpawnFlags.ModifyFlags(!value, RoleSpawnFlags.AssignInventory);
        }

        /// <summary>
        /// Gets or sets the reason for their class change.
        /// </summary>
        public SpawnReason Reason
        {
            get => this.reason;

            set
            {
                if (this.reason == SpawnReason.Destroyed)
                {
                    Log.Error("Tried to change Destroyed reason!");
                    return;
                }

                this.reason = value;
            }
        }

        /// <summary>
        /// Gets or sets the spawn flags for their class change.
        /// </summary>
        public RoleSpawnFlags SpawnFlags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the event can continue.
        /// </summary>
        public bool IsAllowed { get; set; } = true;
    }
}
