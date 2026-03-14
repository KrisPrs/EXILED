// -----------------------------------------------------------------------
// <copyright file="GenericDamageHandler.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.DamageHandlers
{
    using System;

    using Enums;
    using Exiled.API.Features.Pickups.Projectiles;

    using Footprinting;
    using InventorySystem.Items.Scp1509;
    using Items;
    using PlayerRoles;
    using PlayerRoles.PlayableScps.Scp096;
    using PlayerRoles.PlayableScps.Scp1507;
    using PlayerRoles.PlayableScps.Scp3114;
    using PlayerRoles.PlayableScps.Scp939;
    using PlayerStatsSystem;
    using UnityEngine;

    /// <summary>
    /// Allows generic damage to a player.
    /// </summary>
    public class GenericDamageHandler : CustomReasonDamageHandler
    {
        private const string DamageTextDefault = "You were damaged by Unknown Cause";
        private string genericDamageText;
        private string genericEnvironmentDamageText;
        private Player player;
        private DamageType damageType;
        private DamageHandlerBase.CassieAnnouncement customCassieAnnouncement;
        private bool overrideCassieForAllRole;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDamageHandler"/> class.
        /// Transform input data to custom generic handler.
        /// </summary>
        /// <param name="player"> Current player (Target). </param>
        /// <param name="attacker"> Attacker. </param>
        /// <param name="damage"> Damage quantity. </param>
        /// <param name="damageType"> Damage type. </param>
        /// <param name="cassieAnnouncement"> Custom cassie announcment. </param>
        /// <param name="damageText"> Text to provide to player death screen. </param>
        /// <param name="overrideCassieForAllRole">Whether to play Cassie for non-SCPs as well.</param>
        public GenericDamageHandler(Player player, Player attacker, float damage, DamageType damageType, DamageHandlerBase.CassieAnnouncement cassieAnnouncement, string damageText = null, bool overrideCassieForAllRole = false)
            : base(DamageTextDefault)
        {
            this.player = player;
            this.damageType = damageType;
            this.overrideCassieForAllRole = overrideCassieForAllRole;
            cassieAnnouncement ??= DamageHandlerBase.CassieAnnouncement.Default;
            this.customCassieAnnouncement = cassieAnnouncement;

            if (this.customCassieAnnouncement is not null)
                this.customCassieAnnouncement.Announcement ??= $"{player.Nickname} killed by {attacker.Nickname} utilizing {damageType}";

            this.Attacker = attacker.Footprint;
            this.AllowSelfDamage = true;
            this.Damage = damage;
            this.ServerLogsText = $"GenericDamageHandler damage processing";
            this.genericDamageText = $"You were damaged by {damageType}";
            this.genericEnvironmentDamageText = $"Environemntal damage of type {damageType}";

            switch (damageType)
            {
                case DamageType.Falldown:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.Falldown, cassieAnnouncement);
                    break;
                case DamageType.Hypothermia:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.Hypothermia, cassieAnnouncement);
                    break;
                case DamageType.Asphyxiation:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.Asphyxiated, cassieAnnouncement);
                    break;
                case DamageType.Poison:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.Poisoned, cassieAnnouncement);
                    break;
                case DamageType.Bleeding:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.Bleeding, cassieAnnouncement);
                    break;
                case DamageType.Crushed:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.Crushed, cassieAnnouncement);
                    break;
                case DamageType.FemurBreaker:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.UsedAs106Bait, cassieAnnouncement);
                    break;
                case DamageType.PocketDimension:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.PocketDecay, cassieAnnouncement);
                    break;
                case DamageType.FriendlyFireDetector:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.FriendlyFireDetector, cassieAnnouncement);
                    break;
                case DamageType.SeveredHands:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.SeveredHands, cassieAnnouncement);
                    break;
                case DamageType.SeveredEyes:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.Scp1344, cassieAnnouncement);
                    break;
                case DamageType.Warhead:
                    this.Base = new WarheadDamageHandler();
                    break;
                case DamageType.Decontamination:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.Decontamination, cassieAnnouncement);
                    break;
                case DamageType.Tesla:
                    this.Base = new UniversalDamageHandler(damage, DeathTranslations.Tesla, cassieAnnouncement);
                    break;
                case DamageType.Recontainment:
                    this.Base = new RecontainmentDamageHandler(this.Attacker);
                    break;
                case DamageType.Jailbird:
                    this.Base = new JailbirdDamageHandler(this.Attacker.Hub, damage, Vector3.zero);
                    break;
                case DamageType.Scp1509:
                    this.Base = new Scp1509DamageHandler(this.Attacker.Hub, damage, Vector3.zero);
                    break;
                case DamageType.GrayCandy:
                    this.Base = new GrayCandyDamageHandler(this.Attacker.Hub, damage);
                    break;
                case DamageType.MicroHid:
                    InventorySystem.Items.MicroHID.MicroHIDItem microHidOwner = new();
                    microHidOwner.Owner = attacker.ReferenceHub;
                    this.Base = new MicroHidDamageHandler(damage, microHidOwner);
                    break;
                case DamageType.Explosion:
                    this.Base = new ExplosionDamageHandler(attacker.Footprint, UnityEngine.Vector3.zero, damage, 0, ExplosionType.Grenade);
                    break;
                case DamageType.Firearm:
                case DamageType.AK:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunAK);
                    break;
                case DamageType.Crossvec:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunCrossvec);
                    break;
                case DamageType.Logicer:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunLogicer);
                    break;
                case DamageType.Revolver:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunRevolver);
                    break;
                case DamageType.Shotgun:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunShotgun);
                    break;
                case DamageType.Com15:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunCOM15);
                    break;
                case DamageType.Com18:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunCOM18);
                    break;
                case DamageType.Fsp9:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunFSP9);
                    break;
                case DamageType.E11Sr:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunE11SR);
                    break;
                case DamageType.Com45:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunCom45);
                    break;
                case DamageType.Frmg0:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunFRMG0);
                    break;
                case DamageType.A7:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunA7);
                    break;
                case DamageType.Scp127:
                    this.GenericFirearm(player, attacker, damage, damageType, ItemType.GunSCP127);
                    break;
                case DamageType.ParticleDisruptor:
                    this.Base = new DisruptorDamageHandler(new (Item.Create(ItemType.ParticleDisruptor, attacker).Base as InventorySystem.Items.Firearms.Firearm, InventorySystem.Items.Firearms.Modules.DisruptorActionModule.FiringState.FiringSingle), Vector3.up, damage);
                    break;
                case DamageType.Scp096:
                    Scp096Role curr096 = attacker.ReferenceHub.roleManager.CurrentRole as Scp096Role ?? new Scp096Role();

                    if (curr096 != null)
                        curr096._lastOwner = attacker.ReferenceHub;

                    this.Base = new Scp096DamageHandler(curr096, damage, Scp096DamageHandler.AttackType.SlapRight);
                    break;
                case DamageType.Scp939:
                    Scp939Role curr939 = attacker.ReferenceHub.roleManager.CurrentRole as Scp939Role ?? new Scp939Role();

                    if (curr939 != null)
                        curr939._lastOwner = attacker.ReferenceHub;

                    this.Base = new Scp939DamageHandler(curr939, damage, Scp939DamageType.LungeTarget);
                    break;
                case DamageType.Scp: // TODO replace ScpDamageHandler with specific SCP-Role damage handler
                    this.Base = new PlayerStatsSystem.ScpDamageHandler(attacker.ReferenceHub, damage, DeathTranslations.Unknown);
                    break;
                case DamageType.Scp018:
                    Scp018Projectile scp018Projectile = Projectile.Create<Scp018Projectile>(ProjectileType.Scp018);
                    scp018Projectile.PreviousOwner = attacker;
                    this.Base = new Scp018DamageHandler(scp018Projectile.Base, damage, true);
                    break;
                case DamageType.Scp207:
                    this.Base = new PlayerStatsSystem.ScpDamageHandler(attacker.ReferenceHub, damage, DeathTranslations.Scp207);
                    break;
                case DamageType.Scp049:
                    this.Base = new PlayerStatsSystem.ScpDamageHandler(attacker.ReferenceHub, damage, DeathTranslations.Scp049);
                    break;
                case DamageType.Scp173:
                    this.Base = new PlayerStatsSystem.ScpDamageHandler(attacker.ReferenceHub, damage, DeathTranslations.Scp173);
                    break;
                case DamageType.Scp0492:
                    this.Base = new PlayerStatsSystem.ScpDamageHandler(attacker.ReferenceHub, damage, DeathTranslations.Zombie);
                    break;
                case DamageType.Scp106:
                    this.Base = new PlayerStatsSystem.ScpDamageHandler(attacker.ReferenceHub, damage, DeathTranslations.PocketDecay);
                    break;
                case DamageType.CardiacArrest:
                    this.Base = new Scp049DamageHandler(attacker.ReferenceHub, damage, Scp049DamageHandler.AttackType.CardiacArrest);
                    break;
                case DamageType.Scp3114:
                    this.Base = new Scp3114DamageHandler(attacker.ReferenceHub, damage, Scp3114DamageHandler.HandlerType.Slap);
                    break;
                case DamageType.Strangled:
                    this.Base = new Scp3114DamageHandler(attacker.ReferenceHub, damage, Scp3114DamageHandler.HandlerType.Strangulation);
                    break;
                case DamageType.Scp1507:
                    this.Base = new Scp1507DamageHandler(attacker.Footprint, damage);
                    break;
                case DamageType.Scp956:
                    this.Base = new Scp956DamageHandler(Vector3.forward);
                    break;
                case DamageType.SnowBall:
                    this.Base = new SnowballDamageHandler(attacker.Footprint, damage, Vector3.forward);
                    break;
                case DamageType.Custom:
                case DamageType.Unknown:
                case DamageType.Marshmallow:
                default:
                    this.Base = new CustomReasonDamageHandler(damageText ?? this.genericDamageText, damage, cassieAnnouncement.Announcement);
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDamageHandler"/> class.
        /// Transform input data to custom generic handler.
        /// </summary>
        /// <param name="player"> Current player (Target). </param>
        /// <param name="attacker"> Attacker. </param>
        /// <param name="damage"> Damage quantity. </param>
        /// <param name="damageType"> Damage type. </param>
        /// <param name="cassieAnnouncement"> Custom cassie announcment. </param>
        /// <param name="damageText"> Text to provide to player death screen. </param>
        [Obsolete("This constructor will be deleted in Exiled 10")]
        public GenericDamageHandler(Player player, Player attacker, float damage, DamageType damageType, DamageHandlerBase.CassieAnnouncement cassieAnnouncement, string damageText)
            : this(player, attacker, damage, damageType, cassieAnnouncement, damageText, false)
        {
        }

        /// <summary>
        /// Gets or sets a custom base.
        /// </summary>
        public PlayerStatsSystem.DamageHandlerBase Base { get; set; }

        /// <summary>
        /// Gets the <see cref="PlayerStatsSystem.DamageHandlerBase.CassieAnnouncement"/> the base game uses when a player dies.
        /// </summary>
        public override CassieAnnouncement CassieDeathAnnouncement => this.customCassieAnnouncement;

        /// <summary>
        /// Gets or sets the current attacker.
        /// </summary>
        public Footprint Attacker { get; set; }

        /// <summary>
        /// Gets a value indicating whether allow self damage.
        /// </summary>
        public bool AllowSelfDamage { get; }

        /// <inheritdoc />
        public override float Damage { get; set; }

        /// <inheritdoc />
        public override string ServerLogsText { get; }

        /// <summary>
        /// Custom Exiled process damage.
        /// </summary>
        /// <param name="ply"> Current player hub. </param>
        /// <returns> Handles processing damage outcome. </returns>
        public override HandlerOutput ApplyDamage(ReferenceHub ply)
        {
            HandlerOutput output = base.ApplyDamage(ply);
            if (output is HandlerOutput.Death)
            {
                if (this.customCassieAnnouncement?.Announcement != null && (this.overrideCassieForAllRole || ply.IsSCP()))
                {
                    Cassie.Message(this.customCassieAnnouncement.Announcement);
                }
            }

            return output;
        }

        /// <summary>
        /// Generic firearm path for handle type.
        /// </summary>
        /// <param name="player"> Current player. </param>
        /// <param name="attacker"> Current attacker. </param>
        /// <param name="amount"> Damage amount. </param>
        /// <param name="damageType"> Damage type. </param>
        /// <param name="itemType"> ItemType. </param>
        private void GenericFirearm(Player player, Player attacker, float amount, DamageType damageType, ItemType itemType)
        {
            Firearm firearm = new(itemType)
            {
                Base =
                {
                    Owner = attacker.ReferenceHub,
                },
            };
            this.Base = new PlayerStatsSystem.FirearmDamageHandler() { Firearm = firearm.Base, Damage = amount };
        }
    }
}
