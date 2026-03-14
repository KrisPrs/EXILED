// -----------------------------------------------------------------------
// <copyright file="FlashGrenade.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Items
{
    using Exiled.API.Enums;
    using Exiled.API.Features.Pickups;
    using Exiled.API.Features.Pickups.Projectiles;

    using InventorySystem.Items;
    using InventorySystem.Items.Pickups;
    using InventorySystem.Items.ThrowableProjectiles;

    using UnityEngine;

    using Object = UnityEngine.Object;

    /// <summary>
    /// A wrapper class for <see cref="FlashbangGrenade"/>.
    /// </summary>
    public class FlashGrenade : Throwable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlashGrenade"/> class.
        /// </summary>
        /// <param name="itemBase">The base <see cref="ThrowableItem"/> class.</param>
        public FlashGrenade(ThrowableItem itemBase)
            : base(itemBase) =>
            this.Projectile = (FlashbangProjectile)((Throwable)this).Projectile;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashGrenade"/> class, as well as a new flash grenade item.
        /// </summary>
        /// <param name="player">The owner of the grenade. Leave <see langword="null"/> for no owner.</param>
        /// <remarks>The player parameter will always need to be defined if this grenade is custom using Exiled.CustomItems.</remarks>
        internal FlashGrenade(Player player = null)
            : this((ThrowableItem)(player ?? Server.Host).Inventory.CreateItemInstance(new(ItemType.GrenadeFlash, 0), true))
        {
        }

        /// <summary>
        /// Gets a <see cref="FlashbangProjectile"/> to change grenade properties.
        /// </summary>
        public new FlashbangProjectile Projectile { get; }

        /// <summary>
        /// Gets or sets the minimum duration of player can take the effect.
        /// </summary>
        public float MinimalDurationEffect
        {
            get => this.Projectile.MinimalDurationEffect;
            set => this.Projectile.MinimalDurationEffect = value;
        }

        /// <summary>
        /// Gets or sets the additional duration of the <see cref="EffectType.Blurred"/> effect.
        /// </summary>
        public float AdditionalBlurredEffect
        {
            get => this.Projectile.AdditionalBlurredEffect;
            set => this.Projectile.AdditionalBlurredEffect = value;
        }

        /// <summary>
        /// Gets or sets the how mush the flash grenade going to be intensified when explode at <see cref="RoomType.Surface"/>.
        /// </summary>
        public float SurfaceDistanceIntensifier
        {
            get => this.Projectile.SurfaceDistanceIntensifier;
            set => this.Projectile.SurfaceDistanceIntensifier = value;
        }

        /// <summary>
        /// Gets or sets how long the fuse will last.
        /// </summary>
        public float FuseTime
        {
            get => this.Projectile.FuseTime;
            set => this.Projectile.FuseTime = value;
        }

        /// <summary>
        /// Spawns an active grenade on the map at the specified location.
        /// </summary>
        /// <param name="position">The location to spawn the grenade.</param>
        /// <param name="owner">Optional: The <see cref="Player"/> owner of the grenade.</param>
        /// <returns>Spawned <see cref="FlashbangProjectile">grenade</see>.</returns>
        public FlashbangProjectile SpawnActive(Vector3 position, Player owner = null)
        {
#if DEBUG
            Log.Debug($"Spawning active grenade: {this.FuseTime}");
#endif
            ItemPickupBase ipb = Object.Instantiate(this.Projectile.Base, position, Quaternion.identity);

            ipb.Info = new PickupSyncInfo(this.Type, this.Weight, ItemSerialGenerator.GenerateNext());

            FlashbangProjectile grenade = Pickup.Get<FlashbangProjectile>(ipb);

            grenade.Base.gameObject.SetActive(true);

            grenade.MinimalDurationEffect = this.MinimalDurationEffect;
            grenade.AdditionalBlurredEffect = this.AdditionalBlurredEffect;
            grenade.SurfaceDistanceIntensifier = this.SurfaceDistanceIntensifier;
            grenade.FuseTime = this.FuseTime;

            grenade.PreviousOwner = owner ?? Server.Host;

            grenade.Spawn();

            grenade.Base.ServerActivate();

            return grenade;
        }

        /// <summary>
        /// Clones current <see cref="FlashGrenade"/> object.
        /// </summary>
        /// <returns> New <see cref="FlashGrenade"/> object. </returns>
        public override Item Clone() => new FlashGrenade()
        {
            MinimalDurationEffect = this.MinimalDurationEffect,
            AdditionalBlurredEffect = this.AdditionalBlurredEffect,
            SurfaceDistanceIntensifier = this.SurfaceDistanceIntensifier,
            FuseTime = this.FuseTime,
            Repickable = this.Repickable,
            PinPullTime = this.PinPullTime,
        };

        /// <summary>
        /// Returns the FlashGrenade in a human readable format.
        /// </summary>
        /// <returns>A string containing FlashGrenade-related data.</returns>
        public override string ToString() => $"{this.Type} ({this.Serial}) [{this.Weight}] *{this.Scale}* |{this.FuseTime}|";

        /// <inheritdoc/>
        internal override void ReadPickupInfoBefore(Pickup pickup)
        {
            base.ReadPickupInfoBefore(pickup);
            if (pickup is FlashGrenadePickup flashGrenadePickup)
            {
                this.MinimalDurationEffect = flashGrenadePickup.MinimalDurationEffect;
                this.AdditionalBlurredEffect = flashGrenadePickup.AdditionalBlurredEffect;
                this.SurfaceDistanceIntensifier = flashGrenadePickup.SurfaceDistanceIntensifier;
                this.FuseTime = flashGrenadePickup.FuseTime;
            }
        }
    }
}
