// -----------------------------------------------------------------------
// <copyright file="CustomGrenade.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------
namespace Exiled.CustomItems.API.Features
{
    using System;

    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.API.Features.Pickups;
    using Exiled.API.Features.Pickups.Projectiles;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Map;
    using Exiled.Events.EventArgs.Player;
    using Footprinting;
    using InventorySystem.Items;
    using InventorySystem.Items.Pickups;
    using InventorySystem.Items.ThrowableProjectiles;
    using Mirror;
    using UnityEngine;

    using Map = Exiled.Events.Handlers.Map;
    using Object = UnityEngine.Object;

    /// <summary>
    ///     The Custom Grenade base class.
    /// </summary>
    public abstract class CustomGrenade : CustomItem
    {
        /// <summary>
        ///     Gets or sets the <see cref="ItemType" /> to use for this item.
        /// </summary>
        public override ItemType Type
        {
            get => base.Type;
            set
            {
                if (!value.IsThrowable() && value != ItemType.None)
                    throw new ArgumentOutOfRangeException("Type", value, "Invalid grenade type.");

                base.Type = value;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the grenade should explode immediately when contacting any surface.
        /// </summary>
        public abstract bool ExplodeOnCollision { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating how long grenate will not detonate when contacting any surface is you use
        ///     <see cref="ExplodeOnCollision" />.
        /// </summary>
        public virtual float ExplodeOnCollisionFuseTime { get; set; } = 0.15f;

        /// <summary>
        ///     Gets or sets a value indicating how long the grenade's fuse time should be.
        /// </summary>
        public abstract float FuseTime { get; set; }

        /// <inheritdoc/>
        public override Item CreateItem()
        {
            Item item = base.CreateItem();

            if (item is Throwable { Projectile: TimeGrenadeProjectile timeProjectile })
            {
                timeProjectile.FuseTime = this.FuseTime;
            }

            return item;
        }

        /// <inheritdoc/>
        public override Item CreateItem(Player owner)
        {
            Item item = base.CreateItem(owner);

            if (item is Throwable { Projectile: TimeGrenadeProjectile timeProjectile })
            {
                timeProjectile.FuseTime = this.FuseTime;
            }

            return item;
        }

        /// <summary>
        /// Throw the CustomGrenade object.
        /// </summary>
        /// <param name="position">The <see cref="Vector3"/>position to throw at.</param>
        /// <param name="player">The <see cref="Player"/> to count as the thrower of the grenade.</param>
        /// <param name="weight">The <see cref="float"/>Weight of the Grenade.</param>
        /// <param name="fuseTime">The <see cref="float"/>FuseTime of the grenade.</param>
        /// <param name="grenadeType">The <see cref="ItemType"/>of the grenade to spawn.</param>
        /// <returns>The <see cref="Pickup"/> spawned.</returns>
        public virtual Pickup Throw(Vector3 position, Player? player = null, float weight = 1f, float fuseTime = 3f, ItemType grenadeType = ItemType.GrenadeHE)
        {
            player ??= Server.Host;

            player.Role.Is(out FpcRole fpcRole);
            Vector3 velocity = fpcRole.FirstPersonController.FpcModule.Motor.Velocity;

            // Само наличие grenadeType здесь ранее - является как минимум ошибкой разраба
            Throwable throwable = (Throwable)CreateItem(player);

            ThrownProjectile thrownProjectile = Object.Instantiate(throwable.Base.Projectile, position, throwable.Owner.CameraTransform.rotation);

            PickupSyncInfo newInfo = new()
            {
                ItemId = throwable.Type,
                Locked = !throwable.Base._repickupable,
                Serial = throwable.Serial,
                WeightKg = weight,
            };

            if (thrownProjectile is TimeGrenade time)
                time._fuseTime = fuseTime;

            thrownProjectile.NetworkInfo = newInfo;
            thrownProjectile.PreviousOwner = new Footprint(throwable.Owner.ReferenceHub);
            NetworkServer.Spawn(thrownProjectile.gameObject);
            thrownProjectile.InfoReceivedHook(default, newInfo);
            if (thrownProjectile.TryGetComponent(out Rigidbody component))
                throwable.Base.PropelBody(component, throwable.Base.FullThrowSettings.StartTorque, ThrowableNetworkHandler.GetLimitedVelocity(velocity));

            thrownProjectile.ServerActivate();
            return Pickup.Get(thrownProjectile);
        }

        /// <summary>
        ///     Checks to see if the grenade is a custom grenade.
        /// </summary>
        /// <param name="grenade">The <see cref="Projectile">grenade</see> to check.</param>
        /// <returns>True if it is a custom grenade.</returns>
        public virtual bool Check(Projectile grenade) => grenade != null && this.TrackedSerials.Contains(grenade.Serial);

        /// <inheritdoc />
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ThrowingRequest += this.OnInternalThrowingRequest;
            Exiled.Events.Handlers.Player.ThrownProjectile += this.OnInternalThrownProjectile;
            Map.ExplodingGrenade += this.OnInternalExplodingGrenade;
            Map.ChangedIntoGrenade += this.OnInternalChangedIntoGrenade;

            base.SubscribeEvents();
        }

        /// <inheritdoc />
        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ThrowingRequest -= this.OnInternalThrowingRequest;
            Exiled.Events.Handlers.Player.ThrownProjectile -= this.OnInternalThrownProjectile;
            Map.ExplodingGrenade -= this.OnInternalExplodingGrenade;
            Map.ChangedIntoGrenade -= this.OnInternalChangedIntoGrenade;

            base.UnsubscribeEvents();
        }

        /// <summary>
        ///     Handles tracking thrown requests by custom grenades.
        /// </summary>
        /// <param name="ev"><see cref="ThrowingRequestEventArgs" />.</param>
        protected virtual void OnThrowingRequest(ThrowingRequestEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles tracking thrown custom grenades.
        /// </summary>
        /// <param name="ev"><see cref="ThrownProjectileEventArgs" />.</param>
        protected virtual void OnThrownProjectile(ThrownProjectileEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles tracking exploded custom grenades.
        /// </summary>
        /// <param name="ev"><see cref="ExplodingGrenadeEventArgs" />.</param>
        protected virtual void OnExploding(ExplodingGrenadeEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles the tracking of custom grenade pickups that are changed into live grenades by a frag grenade explosion.
        /// </summary>
        /// <param name="ev"><see cref="ChangedIntoGrenadeEventArgs" />.</param>
        protected virtual void OnChangedIntoGrenade(ChangedIntoGrenadeEventArgs ev)
        {
        }

        private void OnInternalThrowingRequest(ThrowingRequestEventArgs ev)
        {
            if (!this.Check(ev.Player.CurrentItem))
                return;

            Log.Debug($"{ev.Player.Nickname} is requesting throw of {this.Name}!");

            this.OnThrowingRequest(ev);
        }

        private void OnInternalThrownProjectile(ThrownProjectileEventArgs ev)
        {
            if (!this.Check(ev.Throwable))
                return;

            this.OnThrownProjectile(ev);
            if (ev.Player == null)
            {
                Log.Error($"CustomGrenade::OnInternalThrownProjectile player is null {ev.Projectile}");
            }
            else
            {
                Log.Debug($"{ev.Player.Nickname} has thrown a {this.Name} ({this.FuseTime}) {ev.Player.Items.ToString(true)}!");
            }

            if (this.ExplodeOnCollision)
            {
                ev.Projectile.GameObject.AttachActionOnCollision(
                    () =>
                    {
                        if (ev.Projectile is TimeGrenadeProjectile grenadeProjectile)
                        {
                            grenadeProjectile.FuseTime = 0.1f;
                        }
                    },
                    ev.Projectile.PreviousOwner ?? Server.Host,
                    this.ExplodeOnCollisionFuseTime);
            }
        }

        private void OnInternalExplodingGrenade(ExplodingGrenadeEventArgs ev)
        {
            if (this.Check(ev.Projectile))
            {
                this.OnExploding(ev);
                Log.Debug($"A {this.Name} is exploding! IsAllowed: {ev.IsAllowed}");
            }
        }

        private void OnInternalChangedIntoGrenade(ChangedIntoGrenadeEventArgs ev)
        {
            if (!this.Check(ev.Pickup))
                return;

            this.OnChangedIntoGrenade(ev);
            Log.Debug($"A {this.Name} ChangedIntoGrenade");

            if (this.ExplodeOnCollision)
            {
                ev.Projectile.GameObject.AttachActionOnCollision(
                    () =>
                    {
                        if (ev.Projectile is TimeGrenadeProjectile grenadeProjectile)
                        {
                            grenadeProjectile.FuseTime = 0.1f;
                        }
                    },
                    ev.Projectile.PreviousOwner ?? Server.Host,
                    this.ExplodeOnCollisionFuseTime);
            }
        }
    }
}