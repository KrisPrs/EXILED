// -----------------------------------------------------------------------
// <copyright file="CustomHumeShieldStat.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.CustomStats
{
    using Mirror;
    using PlayerRoles.PlayableScps.HumeShield;
    using PlayerStatsSystem;
    using UnityEngine;
    using Utils.Networking;

    /// <summary>
    /// A custom version of <see cref="HumeShieldStat"/> which allows the player's max amount of HumeShield to be changed.
    /// </summary>
    public class CustomHumeShieldStat : HumeShieldStat
    {
        /// <summary>
        /// Gets or sets the multiplier for gaining HumeShield.
        /// </summary>
        public float ShieldRegenerationMultiplier { get; set; } = 1;

        private float ShieldRegeneration
        {
            get
            {
                IHumeShieldProvider.GetForHub(this.Hub, out _, out _, out float hsRegen, out _);
                return hsRegen * this.ShieldRegenerationMultiplier;
            }
        }

        /// <inheritdoc/>
        public override void Update()
        {
            if (this.ShieldRegenerationMultiplier is 1)
            {
                base.Update();
                return;
            }

            if (!NetworkServer.active)
                return;

            if (this.ValueDirty)
            {
                new SyncedStatMessages.StatMessage()
                {
                    Stat = this,
                    SyncedValue = this.CurValue,
                }.SendToHubsConditionally(this.CanReceive);
                this._lastSent = this.CurValue;
                this.ValueDirty = false;
            }

            if (this.ShieldRegeneration == 0)
                return;

            float delta = this.ShieldRegeneration * Time.deltaTime;

            if (delta > 0)
            {
                if (this.CurValue >= this.MaxValue)
                    return;

                this.CurValue = Mathf.MoveTowards(this.CurValue, this.MaxValue, delta);
                return;
            }

            if (this.CurValue <= 0)
                return;

            this.CurValue += delta;
        }
    }
}