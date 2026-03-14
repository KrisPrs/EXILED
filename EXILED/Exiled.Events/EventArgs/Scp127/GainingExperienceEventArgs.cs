// -----------------------------------------------------------------------
// <copyright file="GainingExperienceEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Scp127
{
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.Events.EventArgs.Interfaces;
    using InventorySystem.Items.Firearms.Modules.Scp127;

    /// <summary>
    /// Contains all information before SCP-127 gains experience.
    /// </summary>
    public class GainingExperienceEventArgs : IScp127Event, IDeniableEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GainingExperienceEventArgs"/> class.
        /// </summary>
        /// <param name="scp127"><inheritdoc cref="Scp127"/></param>
        /// <param name="experience"><inheritdoc cref="Experience"/></param>
        /// <param name="isAllowed"><inheritdoc cref="IsAllowed"/></param>
        public GainingExperienceEventArgs(Scp127 scp127, float experience, bool isAllowed = true)
        {
            this.Scp127 = scp127;
            this.Experience = experience;
            this.IsAllowed = isAllowed;
        }

        /// <inheritdoc />
        public Player Player => this.Scp127.Owner;

        /// <inheritdoc />
        public Item Item => this.Scp127;

        /// <inheritdoc />
        public Scp127 Scp127 { get; }

        /// <inheritdoc />
        public bool IsAllowed { get; set; }

        /// <summary>
        /// Gets or sets the gaining experience.
        /// </summary>
        public float Experience { get; set; }

        /// <summary>
        /// Gets or sets the new tier.
        /// </summary>
        public Scp127Tier Tier
        {
            get => this.Scp127.TierManagerModule.GetTierForExp(this.Experience + this.Scp127.Experience);
            set => this.Experience = this.Scp127.TierManagerModule.GetExpForTier(value) - this.Scp127.Experience;
        }
    }
}