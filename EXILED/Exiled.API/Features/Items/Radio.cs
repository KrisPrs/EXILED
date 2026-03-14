// -----------------------------------------------------------------------
// <copyright file="Radio.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Items
{
    using Enums;
    using Exiled.API.Interfaces;

    using InventorySystem.Items.Radio;

    using Structs;

    /// <summary>
    /// A wrapper class for <see cref="RadioItem"/>.
    /// </summary>
    public class Radio : Item, IWrapper<RadioItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Radio"/> class.
        /// </summary>
        /// <param name="itemBase">The base <see cref="RadioItem"/> class.</param>
        public Radio(RadioItem itemBase)
            : base(itemBase) =>
            this.Base = itemBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="Radio"/> class, as well as a new Radio item.
        /// </summary>
        internal Radio()
            : this((RadioItem)Server.Host.Inventory.CreateItemInstance(new(ItemType.Radio, 0), false))
        {
        }

        /// <summary>
        /// Gets the <see cref="RadioItem"/> that this class is encapsulating.
        /// </summary>
        public new RadioItem Base { get; }

        /// <summary>
        /// Gets or sets the percentage of the radio's battery, between <c>0-100</c>.
        /// </summary>
        public byte BatteryLevel
        {
            get => this.Base.BatteryPercent;
            set => this.Base.BatteryPercent = value;
        }

        /// <summary>
        /// Gets or sets the current <see cref="RadioRange"/>.
        /// </summary>
        public RadioRange Range
        {
            get => (RadioRange)this.Base._rangeId;
            set => this.Base._rangeId = (byte)value;
        }

        /// <summary>
        /// Gets or sets the <see cref="RadioRangeSettings"/> for the current <see cref="Range"/>.
        /// </summary>
        public RadioRangeSettings RangeSettings
        {
            get =>
                new()
                {
                    IdleUsage = this.Base.Ranges[(int)this.Range].MinuteCostWhenIdle,
                    TalkingUsage = this.Base.Ranges[(int)this.Range].MinuteCostWhenTalking,
                    MaxRange = this.Base.Ranges[(int)this.Range].MaximumRange,
                };
            set =>
                this.Base.Ranges[(int)this.Range] = new RadioRangeMode
                {
                    MaximumRange = value.MaxRange,
                    MinuteCostWhenIdle = value.IdleUsage,
                    MinuteCostWhenTalking = value.TalkingUsage,
                };
        }

        /// <summary>
        /// Gets or sets a value indicating whether the radio is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => this.Base._enabled;
            set => this.Base._enabled = value;
        }

        /// <summary>
        /// Sets the <see cref="RadioRangeSettings"/> of the given <paramref name="range"/>.
        /// </summary>
        /// <param name="range">The <see cref="RadioRange"/> to modify.</param>
        /// <param name="settings">The new settings of the specified range.</param>
        public void SetRangeSettings(RadioRange range, RadioRangeSettings settings) =>
            this.Base.Ranges[(int)range] = new RadioRangeMode
            {
                MaximumRange = settings.MaxRange,
                MinuteCostWhenIdle = settings.IdleUsage,
                MinuteCostWhenTalking = settings.TalkingUsage,
            };

        /// <summary>
        /// Clones current <see cref="Radio"/> object.
        /// </summary>
        /// <returns> New <see cref="Radio"/> object. </returns>
        public override Item Clone() => new Radio()
        {
            BatteryLevel = this.BatteryLevel,
            Range = this.Range,
            RangeSettings = this.RangeSettings,
        };

        /// <summary>
        /// Returns the Radio in a human readable format.
        /// </summary>
        /// <returns>A string containing Radio-related data.</returns>
        public override string ToString() => $"{this.Type} ({this.Serial}) [{this.Weight}] *{this.Scale}* |{this.Range}| -{this.BatteryLevel}-";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="oldOwner">old <see cref="Item"/> owner.</param>
        /// <param name="newOwner">new <see cref="Item"/> owner.</param>
        internal override void ChangeOwner(Player oldOwner, Player newOwner) => this.Base.Owner = newOwner.ReferenceHub;
    }
}
