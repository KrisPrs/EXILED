// -----------------------------------------------------------------------
// <copyright file="Scp018Projectile.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Pickups.Projectiles
{
    using System;
    using System.Reflection;

    using Exiled.API.Features.Items;
    using Exiled.API.Interfaces;
    using HarmonyLib;

    using InventorySystem.Items.ThrowableProjectiles;

    using BaseScp018Projectile = InventorySystem.Items.ThrowableProjectiles.Scp018Projectile;

    /// <summary>
    /// A wrapper class for Scp018Projectile.
    /// </summary>
    public class Scp018Projectile : TimeGrenadeProjectile, IWrapper<BaseScp018Projectile>
    {
        private static FieldInfo maxVelocityField;
        private static FieldInfo velocityPerBounceField;

        private float? maxVelocity;
        private float? velocityPerBounce;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scp018Projectile"/> class.
        /// </summary>
        /// <param name="pickupBase">The base <see cref="BaseScp018Projectile"/> class.</param>
        public Scp018Projectile(BaseScp018Projectile pickupBase)
            : base(pickupBase) =>
            this.Base = pickupBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scp018Projectile"/> class.
        /// </summary>
        internal Scp018Projectile()
            : base(ItemType.SCP018) =>
            this.Base = (BaseScp018Projectile)((Pickup)this).Base;

        /// <summary>
        /// Gets the <see cref="ExplosionGrenade"/> that this class is encapsulating.
        /// </summary>
        public new BaseScp018Projectile Base { get; }

        /// <summary>
        /// Gets the pickup's PhysicsModule.
        /// </summary>
        public new Scp018Physics PhysicsModule => this.Base.PhysicsModule as Scp018Physics;

        /// <summary>
        /// Gets a value indicating whether current Scp018 instance is projectile-typed, or normal pickup.
        /// </summary>
        public bool IsProjectile => this.Base.PhysicsModule is Scp018Physics;

        /// <summary>
        /// Gets or sets the pickup's max velocity.
        /// </summary>
        public float MaxVelocity
        {
            get => this.IsProjectile ? this.PhysicsModule._maxVel : 0.0f;
            set
            {
                if (this.IsProjectile)
                {
                    maxVelocityField ??= AccessTools.Field(typeof(Scp018Physics), nameof(Scp018Physics._maxVel));
                    maxVelocityField.SetValue(this.PhysicsModule, value);
                }

                this.maxVelocity = value;
            }
        }

        /// <summary>
        /// Gets or sets the pickup's velocity per bounce.
        /// </summary>
        public float VelocityPerBounce
        {
            get => this.IsProjectile ? this.PhysicsModule._velPerBounce : 0.0f;
            set
            {
                if (this.IsProjectile)
                {
                    velocityPerBounceField ??= AccessTools.Field(typeof(Scp018Physics), nameof(Scp018Physics._velPerBounce));
                    velocityPerBounceField.SetValue(this.PhysicsModule, value);
                    return;
                }

                this.velocityPerBounce = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether SCP-018 can injure teammates.
        /// </summary>
        public bool IgnoreFriendlyFire => this.Base.IgnoreFriendlyFire;

        /// <summary>
        /// Gets or sets the time for SCP-018 not to ignore the friendly fire.
        /// </summary>
        public float FriendlyFireTime
        {
            get => this.Base._friendlyFireTime;
            set => this.Base._friendlyFireTime = value;
        }

        /// <summary>
        /// Gets the current damage of SCP-018.
        /// </summary>
        public float Damage => this.Base.CurrentDamage;

        /// <summary>
        /// Returns the Scp018Pickup in a human readable format.
        /// </summary>
        /// <returns>A string containing Scp018Pickup-related data.</returns>
        public override string ToString() => $"{this.Type} ({this.Serial}) [{this.Weight}] *{this.Scale}* |{this.Position}| -{this.Damage}- ={this.IgnoreFriendlyFire}=";

        /// <inheritdoc/>
        public override void Explode()
        {
            if (!this.IsProjectile)
                this.Base.SetupModule();
            base.Explode();
        }

        /// <inheritdoc/>
        public override void Activate()
        {
            if (!this.IsProjectile)
                this.Base.SetupModule();
            base.Activate();
        }

        /// <inheritdoc/>
        internal override void ReadThrowableItemInfo(Throwable throwable)
        {
            base.ReadThrowableItemInfo(throwable);
            if (throwable is Scp018 scp018)
            {
                this.FriendlyFireTime = scp018.FriendlyFireTime;
            }
        }

        /// <summary>
        /// Setups scp018 projectile.
        /// </summary>
        internal void SetupProjectile()
        {
            if (this.velocityPerBounce.HasValue)
                this.VelocityPerBounce = this.velocityPerBounce.Value;
            if (this.maxVelocity.HasValue)
                this.MaxVelocity = this.maxVelocity.Value;
        }
    }
}
