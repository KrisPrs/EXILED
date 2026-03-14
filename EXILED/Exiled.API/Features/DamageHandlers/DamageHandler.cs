// -----------------------------------------------------------------------
// <copyright file="DamageHandler.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.DamageHandlers
{
    using CustomPlayerEffects;

    using Footprinting;

    using PlayerStatsSystem;
    using UnityEngine;

    using BaseHandler = PlayerStatsSystem.DamageHandlerBase;

    /// <summary>
    /// A wrapper to easily manipulate the behavior of the <see cref="BaseHandler"/>.
    /// </summary>
    public class DamageHandler : DamageHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DamageHandler"/> class.
        /// </summary>
        public DamageHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DamageHandler"/> class.
        /// </summary>
        /// <param name="target">The target to be set.</param>
        /// <param name="attacker">The attacker to be set.</param>
        public DamageHandler(Player target, Player attacker)
        {
            this.Target = target;
            this.Attacker = attacker;
            this.TargetFootprint = target?.Footprint ?? default;
            this.AttackerFootprint = this.Attacker?.Footprint ?? default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DamageHandler"/> class.
        /// </summary>
        /// <param name="target">The target to be set.</param>
        /// <param name="baseHandler"><inheritdoc cref="DamageHandlerBase.Base"/></param>
        public DamageHandler(Player target, BaseHandler baseHandler)
            : base(baseHandler)
        {
            this.Target = target;
            this.TargetFootprint = target?.Footprint ?? default;

            if (baseHandler is PlayerStatsSystem.AttackerDamageHandler attackerDamageHandler)
            {
                this.Attacker = Player.Get(attackerDamageHandler.Attacker.Hub);
                this.AttackerFootprint = attackerDamageHandler.Attacker;
            }
            else if (baseHandler is GenericDamageHandler genericDamageHandler)
            {
                this.Attacker = Player.Get(genericDamageHandler.Attacker.Hub);
                this.AttackerFootprint = genericDamageHandler.Attacker;
            }
            else
            {
                this.Attacker = null;
                this.AttackerFootprint = default;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Player"/> target.
        /// </summary>
        public Player Target { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="Player"/> attacker. CAN BE NULL!.
        /// </summary>
        public Player Attacker { get; set; }

        /// <summary>
        /// Gets or sets the target's <see cref="Footprint"/>.
        /// </summary>
        public Footprint TargetFootprint { get; protected set; }

        /// <summary>
        /// Gets or sets the attacker's <see cref="Footprint"/>.
        /// </summary>
        public Footprint AttackerFootprint { get; protected set; }

        /// <summary>
        /// Gets or sets the amount of damage to be dealt.
        /// </summary>
        public virtual float Damage
        {
            get => this.Is(out StandardDamageHandler handler) ? handler.Damage : 0f;
            set
            {
                if (this.Is(out StandardDamageHandler handler))
                    handler.Damage = value;
            }
        }

        /// <summary>
        /// Gets or sets the start velocity.
        /// </summary>
        public Vector3 StartVelocity
        {
            get => this.Is(out StandardDamageHandler handler) ? handler.StartVelocity : Vector3.zero;
            set
            {
                if (this.Is(out StandardDamageHandler handler))
                    handler.StartVelocity = value;
            }
        }

        /// <summary>
        /// Gets or sets the direct damage.
        /// </summary>
        public float DealtHealthDamage
        {
            get => this.Is(out StandardDamageHandler handler) ? handler.DealtHealthDamage : 0f;
            set
            {
                if (this.Is(out StandardDamageHandler handler))
                    handler.DealtHealthDamage = value;
            }
        }

        /// <summary>
        /// Gets or sets the damage absorbed by AHP processes.
        /// </summary>
        public float AbsorbedAhpDamage
        {
            get => this.Is(out StandardDamageHandler handler) ? handler.AbsorbedAhpDamage : 0f;
            set
            {
                if (this.Is(out StandardDamageHandler handler))
                    handler.AbsorbedAhpDamage = value;
            }
        }

        /// <inheritdoc/>
        public override Action ApplyDamage(Player player)
        {
            if (!this.Is(out StandardDamageHandler damageHandler))
                return player.GetModule<HealthStat>().CurValue > 0f ? Action.Damage : Action.Death;

            if (this.Damage <= 0f)
                return Action.None;

            damageHandler.ApplyDamage(player.ReferenceHub);

            this.StartVelocity = player.Velocity;
            this.As<StandardDamageHandler>().StartVelocity.y = Mathf.Max(damageHandler.StartVelocity.y, 0f);
            AhpStat ahpModule = player.GetModule<AhpStat>();
            HealthStat healthModule = player.GetModule<HealthStat>();

            if (this.Damage <= StandardDamageHandler.KillValue)
            {
                ahpModule.CurValue = 0f;
                healthModule.CurValue = 0f;
                return Action.Death;
            }

            this.ProcessDamage(player);

            foreach (StatusEffectBase effect in player.ActiveEffects)
            {
                if (effect is IDamageModifierEffect damageModifierEffect)
                    this.Damage *= damageModifierEffect.GetDamageModifier(this.Damage, damageHandler, damageHandler.Hitbox);
            }

            // DealtHealthDamage = ahpModule.ServerProcessDamage(Damage);
            this.AbsorbedAhpDamage = this.Damage - this.DealtHealthDamage;

            // healthModule.CurValue -= DealtHealthDamage;
            return player.GetModule<HealthStat>().CurValue > 0f ? Action.Damage : Action.Death;
        }

        /// <inheritdoc/>
        public override string ToString() => $"{this.Target} {this.Damage} ({this.Type}) {(this.Attacker is not null ? this.Attacker.Nickname : "No one")}";
    }
}