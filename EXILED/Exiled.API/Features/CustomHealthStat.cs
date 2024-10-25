// -----------------------------------------------------------------------
// <copyright file="CustomHealthStat.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features
{
    using System;

    using PlayerStatsSystem;

    /// <summary>
    /// A custom version of <see cref="HealthStat"/> which allows the player's max amount of health to be changed.
    /// </summary>
    public class CustomHealthStat : HealthStat
    {
        private float customMaxValue;

        /// <inheritdoc/>
        public override float MaxValue => CustomMaxValue == default ? base.MaxValue : CustomMaxValue;

        /// <summary>
        /// Gets or sets the maximum amount of health the player will have.
        /// </summary>
        public float CustomMaxValue
        {
            get
            {
                float result = customMaxValue;

                if (Hub.playerStats.TryGetModule(out MaxHealthStat maxHealthStat))
                {
                    result += maxHealthStat.CurValue;
                }

                return result;
            }

            set
            {
                if (Hub.playerStats.TryGetModule(out MaxHealthStat maxHealthStat))
                {
                    if (value > 100)
                    {
                        customMaxValue = 100;
                        maxHealthStat.CurValue = value - 100;
                    }
                    else
                    {
                        customMaxValue = value;
                        maxHealthStat.CurValue = 0;
                    }
                }
                else
                {
                    customMaxValue = value;
                }
            }
        }
    }
}
