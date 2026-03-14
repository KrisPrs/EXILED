// -----------------------------------------------------------------------
// <copyright file="CustomDamageHandler.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.DamageHandlers
{
    using CustomPlayerEffects;

    using Enums;

    using Exiled.API.Extensions;

    using Items;

    using PlayerStatsSystem;

    using UnityEngine;

    using BaseFirearmHandler = PlayerStatsSystem.FirearmDamageHandler;
    using BaseHandler = PlayerStatsSystem.DamageHandlerBase;
    using BaseScpDamageHandler = PlayerStatsSystem.ScpDamageHandler;

    /// <summary>
    /// A wrapper to easily manipulate the behavior of <see cref="BaseHandler"/>.
    /// </summary>
    public sealed class CustomDamageHandler : AttackerDamageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDamageHandler"/> class.
        /// </summary>
        /// <param name="target">The target to be set.</param>
        /// <param name="baseHandler">The base <see cref="BaseHandler"/>.</param>
        public CustomDamageHandler(Player target, BaseHandler baseHandler)
            : base(target, baseHandler)
        {
            if (this.Attacker is not null)
            {
                if (baseHandler is BaseScpDamageHandler)
                {
                    this.CustomBase = new ScpDamageHandler(target, baseHandler);
                }
                else
                {
                    Item item = this.Attacker.CurrentItem;
                    if (item is not null && item.Type.IsWeapon() && baseHandler is BaseFirearmHandler)
                        this.CustomBase = new FirearmDamageHandler(item, target, baseHandler);
                    else
                        this.CustomBase = new DamageHandler(target, this.Attacker);
                }
            }
            else
            {
                this.CustomBase = new DamageHandler(target, baseHandler);
            }

            this.Type = this.CustomBase.Type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDamageHandler"/> class.
        /// </summary>
        /// <param name="target">The target to be set.</param>
        /// <param name="attacker">The attacker to be set.</param>
        /// <param name="damage">The amount of damage to be set.</param>
        /// <param name="damageType">The <see cref="DamageType"/> to be set.</param>
        public CustomDamageHandler(Player target, Player attacker, float damage, DamageType damageType = DamageType.Unknown)
            : base(target, attacker)
        {
            this.Damage = damage;
            this.Type = damageType;

            Firearm firearm = new(ItemType.GunAK)
            {
                Base = { Owner = attacker.ReferenceHub },
            };

            this.CustomBase = new FirearmDamageHandler(firearm, target, new PlayerStatsSystem.FirearmDamageHandler() { Firearm = firearm.Base, Damage = damage });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDamageHandler"/> class.
        /// </summary>
        /// <param name="target">The target to be set.</param>
        /// <param name="attacker">The attacker to be set.</param>
        /// <param name="damage">The amount of damage to be set.</param>
        /// <param name="damageType">The <see cref="DamageType"/> to be set.</param>
        /// <param name="cassieAnnouncement">The <see cref="DamageHandlerBase.CassieAnnouncement"/> to be set.</param>
        public CustomDamageHandler(Player target, Player attacker, float damage, DamageType damageType, CassieAnnouncement cassieAnnouncement)
            : this(target, attacker, damage, damageType) =>
            this.CassieDeathAnnouncement = cassieAnnouncement;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDamageHandler"/> class.
        /// </summary>
        /// <param name="target">The target to be set.</param>
        /// <param name="attacker">The attacker to be set.</param>
        /// <param name="damage">The amount of damage to be set.</param>
        /// <param name="damageType">The <see cref="DamageType"/> to be set.</param>
        /// <param name="cassieAnnouncement">The <see cref="DamageHandlerBase.CassieAnnouncement"/> to be set.</param>
        public CustomDamageHandler(Player target, Player attacker, float damage, DamageType damageType, string cassieAnnouncement)
            : this(target, attacker, damage, damageType) =>
            this.CassieDeathAnnouncement = new CassieAnnouncement(cassieAnnouncement);

        /// <summary>
        /// Gets the base <see cref="DamageHandlerBase"/>.
        /// </summary>
        public DamageHandlerBase CustomBase { get; }

        /// <inheritdoc/>
        public override Action ApplyDamage(Player player)
        {
            if (this.Damage <= 0f)
                return Action.None;

            this.StartVelocity = player.Velocity;

            this.As<BaseFirearmHandler>().StartVelocity.y = Mathf.Max(this.As<BaseFirearmHandler>().StartVelocity.y, 0f);
            AhpStat ahpModule = player.GetModule<AhpStat>();
            HealthStat healthModule = player.GetModule<HealthStat>();

            if (this.Damage <= StandardDamageHandler.KillValue)
                return KillPlayer(player, this.CustomBase);

            this.ProcessDamage(player);

            foreach (StatusEffectBase statusEffect in player.ActiveEffects)
            {
                if (statusEffect is IDamageModifierEffect damageModifierEffect)
                    this.Damage *= damageModifierEffect.GetDamageModifier(this.Damage, this.CustomBase, this.As<BaseFirearmHandler>().Hitbox);
            }

            this.DealtHealthDamage = ahpModule.ServerProcessDamage(this.Damage);
            this.AbsorbedAhpDamage = this.Damage - this.DealtHealthDamage;

            return healthModule.CurValue - this.DealtHealthDamage > 0f ? Action.Damage : KillPlayer(player, this.CustomBase);
        }

        private static Action KillPlayer(Player player, DamageHandlerBase damageHandlerBase)
        {
            player.ReferenceHub.playerStats.KillPlayer(damageHandlerBase);

            return Action.Death;
        }
    }
}