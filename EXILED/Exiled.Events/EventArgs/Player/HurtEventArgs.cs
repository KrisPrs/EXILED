// -----------------------------------------------------------------------
// <copyright file="HurtEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using API.Features;
    using API.Features.DamageHandlers;

    using Interfaces;

    using CustomAttackerHandler = API.Features.DamageHandlers.AttackerDamageHandler;
    using DamageHandlerBase = PlayerStatsSystem.DamageHandlerBase;

    /// <summary>
    /// Contains all information before a player gets damaged.
    /// </summary>
    public class HurtEventArgs : IAttackerEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HurtEventArgs" /> class.
        /// </summary>
        /// <param name="referenceHub">
        /// <inheritdoc cref="Player" />
        /// </param>
        /// <param name="damageHandler">
        /// <inheritdoc cref="DamageHandler" />
        /// </param>
        /// <param name="handlerOutput">
        /// <inheritdoc cref="HandlerOutput" />
        /// </param>
        public HurtEventArgs(ReferenceHub referenceHub, DamageHandlerBase damageHandler, DamageHandlerBase.HandlerOutput handlerOutput)
        {
            this.Player = Player.Get(referenceHub);
            this.DamageHandler = new CustomDamageHandler(this.Player, damageHandler);
            this.HandlerOutput = handlerOutput;

            if (this.DamageHandler.BaseIs(out CustomAttackerHandler attackerDamageHandler))
                this.Attacker = attackerDamageHandler.Attacker;
            else if (damageHandler is GenericDamageHandler genericDamageHandler)
                this.Attacker = Player.Get(genericDamageHandler.Attacker);
            else
                this.Attacker = null;
        }

        /// <inheritdoc/>
        public Player Player { get; }

        /// <inheritdoc/>
        public Player Attacker { get; }

        /// <summary>
        /// Gets the amount of inflicted damage.
        /// </summary>
        public float Amount => this.DamageHandler.Damage;

        /// <summary>
        /// Gets or sets the action than will be made on the player.
        /// </summary>
        public DamageHandlerBase.HandlerOutput HandlerOutput { get; set; }

        /// <inheritdoc/>
        public CustomDamageHandler DamageHandler { get; set; }
    }
}