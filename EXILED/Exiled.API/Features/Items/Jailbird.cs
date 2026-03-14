// -----------------------------------------------------------------------
// <copyright file="Jailbird.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Items
{
    using System;

    using Exiled.API.Features.Pickups;
    using Exiled.API.Interfaces;
    using InventorySystem.Items;
    using InventorySystem.Items.Autosync;
    using InventorySystem.Items.Jailbird;
    using Mirror;
    using UnityEngine;

    using JailbirdPickup = Pickups.JailbirdPickup;

    /// <summary>
    /// A wrapped class for <see cref="JailbirdItem"/>.
    /// </summary>
    public class Jailbird : Item, IWrapper<JailbirdItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Jailbird"/> class.
        /// </summary>
        /// <param name="itemBase">The base <see cref="JailbirdItem"/> class.</param>
        public Jailbird(JailbirdItem itemBase)
            : base(itemBase) =>
            this.Base = itemBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="Jailbird"/> class, as well as a new Jailbird item.
        /// </summary>
        internal Jailbird()
            : this((JailbirdItem)Server.Host.Inventory.CreateItemInstance(new(ItemType.Jailbird, 0), false))
        {
        }

        /// <summary>
        /// Gets the <see cref="JailbirdItem"/> that this class is encapsulating.
        /// </summary>
        public new JailbirdItem Base { get; }

        /// <summary>
        /// Gets or sets the amount of damage dealt with a Jailbird melee hit.
        /// </summary>
        public float MeleeDamage
        {
            get => this.Base.MeleeDamage;
            set => this.Base.MeleeDamage = value;
        }

        /// <summary>
        /// Gets or sets the amount of damage dealt with a Jailbird charge hit.
        /// </summary>
        public float ChargeDamage
        {
            get => this.Base._chargeDamage;
            set => this.Base._chargeDamage = value;
        }

        /// <summary>
        /// Gets or sets the amount of time in seconds that the <see cref="CustomPlayerEffects.Flashed"/> effect will be applied on being hit.
        /// </summary>
        public float FlashDuration
        {
            get => this.Base._flashedDuration;
            set => this.Base._flashedDuration = value;
        }

        /// <summary>
        /// Gets or sets the amount of time in seconds that the <see cref="CustomPlayerEffects.Concussed"/> effect will be applied on being hit.
        /// </summary>
        public float ConcussionDuration
        {
            get => this.Base._concussionDuration;
            set => this.Base._concussionDuration = value;
        }

        /// <summary>
        /// Gets or sets the radius of the Jailbird's hit radius.
        /// </summary>
        public float Radius
        {
            get => this.Base._hitregRadius;
            set => this.Base._hitregRadius = value;
        }

        /// <summary>
        /// Gets or sets the total amount of damage dealt with the Jailbird.
        /// </summary>
        public float TotalDamageDealt
        {
            get => this.Base.TotalMeleeDamageDealt;
            set
            {
                this.Base.TotalMeleeDamageDealt = value;
                this.Base._deterioration.RecheckUsage();
            }
        }

        /// <summary>
        /// Gets or sets the number of times the item has been charged and used.
        /// </summary>
        public int TotalCharges
        {
            get => this.Base.TotalChargesPerformed;
            set
            {
                this.Base.TotalChargesPerformed = value;
                this.Base._deterioration.RecheckUsage();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="JailbirdWearState"/> for this item.
        /// </summary>
        public JailbirdWearState WearState
        {
            get => this.Base._deterioration.WearState;
            set
            {
                this.TotalDamageDealt = this.GetDamage(value);
                this.TotalCharges = this.GetCharge(value);
                this.Base._deterioration.RecheckUsage();
            }
        }

        /// <summary>
        /// Calculates the damage corresponding to a given <see cref="JailbirdWearState"/>.
        /// </summary>
        /// <param name="wearState">The wear state to calculate damage for.</param>
        /// <returns>The amount of damage associated with the specified wear state.</returns>
        public float GetDamage(JailbirdWearState wearState)
        {
            foreach (Keyframe keyframe in this.Base._deterioration._damageToWearState.keys)
            {
                if (this.Base._deterioration.FloatToState(keyframe.value) == wearState)
                    return keyframe.time;
            }

            throw new Exception("Wear state not found in damage to wear state mapping.");
        }

        /// <summary>
        /// Gets the charge needed to reach a specific <see cref="JailbirdWearState"/>.
        /// </summary>
        /// <param name="wearState">The desired wear state to calculate the charge for.</param>
        /// <returns>The charge value required to achieve the specified wear state.</returns>
        public int GetCharge(JailbirdWearState wearState) => (int)wearState;

        /// <summary>
        /// Breaks the Jailbird.
        /// </summary>
        public void Break()
        {
            this.WearState = JailbirdWearState.Broken;
            ItemIdentifier identifier = new(this.Base);
            using (new AutosyncRpc(identifier, out NetworkWriter networkWriter))
            {
                networkWriter.WriteByte(0);
                networkWriter.WriteByte((byte)JailbirdWearState.Broken);
            }

            using (new AutosyncRpc(identifier, out NetworkWriter networkWriter2))
            {
                networkWriter2.WriteByte(1);
            }
        }

        /// <summary>
        /// Clones current <see cref="Jailbird"/> object.
        /// </summary>
        /// <returns> New <see cref="Jailbird"/> object. </returns>
        public override Item Clone() => new Jailbird()
        {
            MeleeDamage = this.MeleeDamage,
            ChargeDamage = this.ChargeDamage,
            TotalDamageDealt = this.TotalDamageDealt,
            TotalCharges = this.TotalCharges,
        };

        /// <summary>
        /// Returns the JailBird in a human readable format.
        /// </summary>
        /// <returns>A string containing JailBird-related data.</returns>
        public override string ToString() => $"{this.Type} ({this.Serial}) [{this.Weight}] *{this.Scale}*";

        /// <inheritdoc/>
        internal override void ReadPickupInfoBefore(Pickup pickup)
        {
            base.ReadPickupInfoBefore(pickup);
            if (pickup is JailbirdPickup jailbirdPickup)
            {
                this.MeleeDamage = jailbirdPickup.MeleeDamage;
                this.ChargeDamage = jailbirdPickup.ChargeDamage;
                this.FlashDuration = jailbirdPickup.FlashDuration;
                this.ConcussionDuration = jailbirdPickup.ConcussionDuration;
                this.Radius = jailbirdPickup.Radius;
            }
        }
    }
}