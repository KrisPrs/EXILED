// -----------------------------------------------------------------------
// <copyright file="Scp244Pickup.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Pickups
{
    using System;

    using Exiled.API.Features.DamageHandlers;
    using Exiled.API.Features.Items;
    using Exiled.API.Interfaces;
    using InventorySystem.Items;
    using InventorySystem.Items.Usables.Scp244;

    using UnityEngine;

    /// <summary>
    /// A wrapper class for a SCP-244 pickup.
    /// </summary>
    public class Scp244Pickup : UsablePickup, IWrapper<Scp244DeployablePickup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Scp244Pickup"/> class.
        /// </summary>
        /// <param name="pickupBase">The base <see cref="Scp244DeployablePickup"/> class.</param>
        internal Scp244Pickup(Scp244DeployablePickup pickupBase)
            : base(pickupBase) =>
            this.Base = pickupBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scp244Pickup"/> class.
        /// </summary>
        /// <param name="type">The <see cref="ItemType"/> of the pickup.</param>
        internal Scp244Pickup(ItemType type)
            : base(type) =>
            this.Base = (Scp244DeployablePickup)((Pickup)this).Base;

        /// <summary>
        /// Gets the <see cref="Scp244DeployablePickup"/> that this class is encapsulating.
        /// </summary>
        public new Scp244DeployablePickup Base { get; }

        /// <summary>
        /// Gets the amount of time this Scp244 has been on the ground.
        /// </summary>
        public TimeSpan Lifetime => this.Base._lifeTime.Elapsed;

        /// <summary>
        /// Gets the speed of <see cref="Scp244Pickup"/>'s too grow.
        /// </summary>
        public float GrowSpeed => this.Base.GrowSpeed;

        /// <summary>
        /// Gets the time for the sphere to finish their expansion.
        /// </summary>
        public float TimeToGrow => this.Base.TimeToGrow;

        /// <summary>
        /// Gets the current size effect of the Scp244's Hypothermia.
        /// </summary>
        public float CurrentDiameter => this.Base.CurrentDiameter;

        /// <summary>
        /// Gets or sets the current size percent of the Scp244's Hypothermia.
        /// </summary>
        public float CurrentSizePercent
        {
            get => this.Base.CurrentSizePercent;
            set => this.Base.CurrentSizePercent = value;
        }

        /// <summary>
        /// Gets or sets the maximum diameter within which SCP-244's hypothermia effect is dealt.
        /// </summary>
        /// <remarks>This does not prevent visual effects.</remarks>
        public float MaxDiameter
        {
            get => this.Base.MaxDiameter;
            set => this.Base.MaxDiameter = value;
        }

        /// <summary>
        /// Gets or sets the Scp244's remaining health.
        /// </summary>
        public float Health
        {
            get => this.Base._health;
            set => this.Base._health = value;
        }

        /// <summary>
        /// Gets a value indicating whether this Scp244 is breakable.
        /// </summary>
        public bool IsBreakable => this.Base.State is Scp244State.Idle or Scp244State.Active;

        /// <summary>
        /// Gets a value indicating whether this Scp244 is broken.
        /// </summary>
        public bool IsBroken => this.Base.State is Scp244State.Destroyed;

        /// <summary>
        /// Gets or sets the <see cref="Scp244State"/>.
        /// </summary>
        public Scp244State State
        {
            get => this.Base.State;
            set => this.Base.State = value;
        }

        /// <summary>
        /// Gets or sets the activation angle, where 1 is a minimum, and -1 it's a maximum activation angle.
        /// </summary>
        public float ActivationDot
        {
            get => this.Base._activationDot;
            set => this.Base._activationDot = value;
        }

        /// <summary>
        /// Damages the Scp244Pickup.
        /// </summary>
        /// <param name="handler">The <see cref="DamageHandler"/> used to deal damage.</param>
        /// <returns><see langword="true"/> if the the damage has been deal; otherwise, <see langword="false"/>.</returns>
        public bool Damage(DamageHandler handler) => this.Base.Damage(handler.Damage, handler, Vector3.zero);

        /// <summary>
        /// Returns the Scp244Pickup in a human readable format.
        /// </summary>
        /// <returns>A string containing Scp244Pickup related data.</returns>
        public override string ToString() => $"{this.Type} ({this.Serial}) [{this.Weight}] *{this.Scale}* |{this.Health}| -{this.State}- ={this.CurrentSizePercent}=";

        /// <inheritdoc/>
        internal override void ReadItemInfo(Item item)
        {
            base.ReadItemInfo(item);
            if (item is Scp244 scp244)
            {
                this.ActivationDot = scp244.ActivationDot;
                this.MaxDiameter = scp244.MaxDiameter;
                this.Health = scp244.Health;
            }
        }
    }
}
