// -----------------------------------------------------------------------
// <copyright file="Usable.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Items
{
    using Exiled.API.Extensions;
    using Exiled.API.Features.Pickups;
    using Exiled.API.Interfaces;

    using InventorySystem;
    using InventorySystem.Items;
    using InventorySystem.Items.Pickups;
    using InventorySystem.Items.Usables;

    using UnityEngine;

    /// <summary>
    /// A wrapper class for <see cref="UsableItem"/>.
    /// </summary>
    public class Usable : Item, IWrapper<UsableItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Usable"/> class.
        /// </summary>
        /// <param name="itemBase">The base <see cref="UsableItem"/> class.</param>
        public Usable(UsableItem itemBase)
            : base(itemBase) =>
            this.Base = itemBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="Usable"/> class.
        /// </summary>
        /// <param name="type">The <see cref="ItemType"/> of the usable item.</param>
        internal Usable(ItemType type)
            : this((UsableItem)Server.Host.Inventory.CreateItemInstance(new(type, 0), false))
        {
        }

        /// <summary>
        /// Gets the <see cref="UsableItem"/> that this class is encapsulating.
        /// </summary>
        public new UsableItem Base { get; }

        /// <summary>
        /// Gets a value indicating whether this item is equippable.
        /// </summary>
        public bool Equippable => this.Base.AllowEquip;

        /// <summary>
        /// Gets a value indicating whether this item is holsterable.
        /// </summary>
        public bool Holsterable => this.Base.AllowHolster;

        /// <summary>
        /// Gets or sets the weight of the item.
        /// </summary>
        public new float Weight
        {
            get => this.Base._weight;
            set => this.Base._weight = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the item is currently being used.
        /// </summary>
        public bool IsUsing
        {
            get => this.Base.IsUsing;
            set => UsableItemsController.ServerEmulateMessage(this.Serial, value ? StatusMessage.StatusType.Start : StatusMessage.StatusType.Cancel);
        }

        /// <summary>
        /// Gets or sets how long it takes to use the item.
        /// </summary>
        public float UseTime
        {
            get => this.Base.UseTime;
            set => this.Base.UseTime = value;
        }

        /// <summary>
        /// Gets or sets how long after using starts a player has to cancel using the item.
        /// </summary>
        public float MaxCancellableTime
        {
            get => this.Base.MaxCancellableTime;
            set => this.Base.MaxCancellableTime = value;
        }

        /// <summary>
        /// Gets or sets the cooldown between repeated uses of this item.
        /// </summary>
        public float RemainingCooldown
        {
            get => UsableItemsController.GlobalItemCooldowns.TryGetValue(this.Serial, out float value) ? value : -1;
            set => UsableItemsController.GlobalItemCooldowns[this.Serial] = Time.timeSinceLevelLoad + value;
        }

        /// <summary>
        /// Gets all the cooldown between uses of this item.
        /// </summary>
        public float PlayerGetCooldown => UsableItemsController.GetCooldown(this.Serial, this.Base, UsableItemsController.GetHandler(this.Base.Owner));

        /// <summary>
        /// Creates the <see cref="Pickup"/> that based on this <see cref="Item"/>.
        /// </summary>
        /// <param name="position">The location to spawn the item.</param>
        /// <param name="rotation">The rotation of the item.</param>
        /// <param name="spawn">Whether the <see cref="Pickup"/> should be initially spawned.</param>
        /// <returns>The created <see cref="Pickup"/>.</returns>
        public override Pickup CreatePickup(Vector3 position, Quaternion rotation = default, bool spawn = true)
        {
            PickupSyncInfo info = new(this.Type, this.Weight, this.Serial);

            ItemPickupBase ipb = InventoryExtensions.ServerCreatePickup(this.Base, info, position, rotation);

            Pickup pickup = Pickup.Get(ipb);

            if (spawn)
                pickup.Spawn();

            return pickup;
        }

        /// <summary>
        /// Uses the item.
        /// </summary>
        /// <param name="owner">Target <see cref="Player"/> to use an <see cref="Usable"/>.</param>
        /// <exception cref="System.InvalidOperationException">The <see cref="Item.Owner"/> of the item cannot be <see langword="null"/>.</exception>
        public virtual void Use(Player owner = null)
        {
            Player oldOwner = this.Owner;
            owner ??= this.Owner;

            if (owner is null)
                throw new System.InvalidOperationException("The Owner of the item cannot be null.");

            this.Base.Owner = owner.ReferenceHub;
            this.Base.ServerOnUsingCompleted();

            typeof(UsableItemsController).InvokeStaticEvent(nameof(UsableItemsController.ServerOnUsingCompleted), new object[] { owner.ReferenceHub, this.Base });

            this.Base.Owner = oldOwner.ReferenceHub;
        }

        /// <inheritdoc/>
        internal override void ReadPickupInfoBefore(Pickup pickup)
        {
            base.ReadPickupInfoBefore(pickup);
            if (pickup is UsablePickup usablePickup)
            {
                this.UseTime = usablePickup.UseTime;
                this.MaxCancellableTime = usablePickup.MaxCancellableTime;
            }
        }
    }
}
