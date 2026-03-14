// -----------------------------------------------------------------------
// <copyright file="SliderSetting.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Core.UserSettings
{
    using System;

    using Exiled.API.Interfaces;
    using global::UserSettings.ServerSpecific;

    /// <summary>
    /// Represents a slider setting.
    /// </summary>
    public class SliderSetting : SettingBase, IWrapper<SSSliderSetting>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SliderSetting"/> class.
        /// </summary>
        /// <param name="id"><inheritdoc cref="SettingBase.Id"/></param>
        /// <param name="label"><inheritdoc cref="SettingBase.Label"/></param>
        /// <param name="minValue"><inheritdoc cref="MinimumValue"/></param>
        /// <param name="maxValue"><inheritdoc cref="MaximumValue"/></param>
        /// <param name="defaultValue"><inheritdoc cref="DefaultValue"/></param>
        /// <param name="isInteger"><inheritdoc cref="IsInteger"/></param>
        /// <param name="stringFormat"><inheritdoc cref="StringFormat"/></param>
        /// <param name="displayFormat"><inheritdoc cref="DisplayFormat"/></param>
        /// <param name="hintDescription"><inheritdoc cref="SettingBase.HintDescription"/></param>
        /// <param name="collectionId"><inheritdoc cref="SettingBase.CollectionId"/></param>
        /// <param name="isServerOnly"><inheritdoc cref="SettingBase.IsServerOnly"/></param>
        /// <param name="header"><inheritdoc cref="SettingBase.Header"/></param>
        /// <param name="onChanged"><inheritdoc cref="SettingBase.OnChanged"/></param>
        public SliderSetting(int id, string label, float minValue, float maxValue, float defaultValue, bool isInteger = false, string stringFormat = "0.##", string displayFormat = "{0}", string hintDescription = null, byte collectionId = byte.MaxValue, bool isServerOnly = false, HeaderSetting header = null, Action<Player, SettingBase> onChanged = null)
            : base(new SSSliderSetting(id, label, minValue, maxValue, defaultValue, isInteger, stringFormat, displayFormat, hintDescription, collectionId, isServerOnly), header, onChanged) =>
            this.Base = (SSSliderSetting)base.Base;

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderSetting"/> class.
        /// </summary>
        /// <param name="settingBase">A <see cref="SSSliderSetting"/> instance.</param>
        internal SliderSetting(SSSliderSetting settingBase)
            : base(settingBase) =>
            this.Base = settingBase;

        /// <summary>
        /// Gets or sets the minimum value of the slider.
        /// </summary>
        public float MinimumValue
        {
            get => this.Base.MinValue;
            set => this.Base.MinValue = value;
        }

        /// <summary>
        /// Gets or sets the maximum value of the slider.
        /// </summary>
        public float MaximumValue
        {
            get => this.Base.MaxValue;
            set => this.Base.MaxValue = value;
        }

        /// <summary>
        /// Gets or sets the default value of the slider.
        /// </summary>
        public float DefaultValue
        {
            get => this.Base.DefaultValue;
            set => this.Base.DefaultValue = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the slider displays integers.
        /// </summary>
        public bool IsInteger
        {
            get => this.Base.Integer;
            set => this.Base.Integer = value;
        }

        /// <summary>
        /// Gets a value indicating whether the slider is currently being dragged.
        /// </summary>
        public bool IsBeingDragged => this.Base.SyncDragging;

        /// <summary>
        /// Gets a float that represents the current value of the slider.
        /// </summary>
        public float SliderValue => this.Base.Integer ? this.Base.SyncIntValue : this.Base.SyncFloatValue;

        /// <summary>
        /// Gets or sets the formatting used for the number in the slider.
        /// </summary>
        public string StringFormat
        {
            get => this.Base.ValueToStringFormat;
            set => this.Base.ValueToStringFormat = value;
        }

        /// <summary>
        /// Gets or sets the formatting used for the final display of the value of the slider.
        /// </summary>
        public string DisplayFormat
        {
            get => this.Base.FinalDisplayFormat;
            set => this.Base.FinalDisplayFormat = value;
        }

        /// <inheritdoc/>
        public new SSSliderSetting Base { get; }

        /// <summary>
        /// Sends updated values to clients.
        /// </summary>
        /// <param name="min"><inheritdoc cref="MinimumValue"/></param>
        /// <param name="max"><inheritdoc cref="MaximumValue"/></param>
        /// <param name="isInteger"><inheritdoc cref="IsInteger"/></param>
        /// <param name="stringFormat"><inheritdoc cref="StringFormat"/></param>
        /// <param name="displayFormat"><inheritdoc cref="DisplayFormat"/></param>
        /// <param name="overrideValue">If false, sends fake values.</param>
        /// <param name="filter">Who to send the update to.</param>
        public void UpdateSetting(float min, float max, bool isInteger, string stringFormat, string displayFormat, bool overrideValue = true, Predicate<Player> filter = null)
        {
            filter ??= _ => true;
            this.Base.SendSliderUpdate(min, max, isInteger, stringFormat, displayFormat, overrideValue, hub => filter(Player.Get(hub)));
        }

        /// <summary>
        /// If setting is server only, sends updated values to clients.
        /// </summary>
        /// <param name="value"><inheritdoc cref="SliderValue"/></param>
        /// <param name="overrideValue">If false, sends fake values.</param>
        /// <param name="filter">Who to send the update to.</param>
        public void UpdateValue(float value, bool overrideValue = true, Predicate<Player> filter = null)
        {
            filter ??= _ => true;
            this.Base.SendValueUpdate(value, overrideValue, hub => filter(Player.Get(hub)));
        }

        /// <summary>
        /// Returns a representation of this <see cref="SliderSetting"/>.
        /// </summary>
        /// <returns>A string in human-readable format.</returns>
        public override string ToString() => base.ToString() + $" /{this.MinimumValue}/ *{this.MaximumValue}* +{this.DefaultValue}+ '{this.SliderValue}'";

        /// <summary>
        /// Represents a config for SliderSetting.
        /// </summary>
        public class SliderConfig : SettingConfig<SliderSetting>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SliderConfig"/> class.
            /// </summary>
            /// <param name="label"/><inheritdoc cref="Label"/>
            /// <param name="minValue"><inheritdoc cref="MinimumValue"/></param>
            /// <param name="maxValue"><inheritdoc cref="MaximumValue"/></param>
            /// <param name="defaultValue"><inheritdoc cref="DefaultValue"/></param>
            /// <param name="isInteger"><inheritdoc cref="IsInteger"/></param>
            /// <param name="stringFormat"><inheritdoc cref="StringFormat"/></param>
            /// <param name="displayFormat"><inheritdoc cref="DisplayFormat"/></param>
            /// <param name="isServerOnly"><inheritdoc cref="IsServerOnly"/></param>
            /// <param name="hintDescription"><inheritdoc cref="HintDescription"/></param>
            /// <param name="headerDescription"><inheritdoc cref="HeaderDescription"/></param>
            /// <param name="headerPaddling"><inheritdoc cref="HeaderPaddling"/></param>
            public SliderConfig(string label, float minValue, float maxValue, float defaultValue, bool isInteger = false, string stringFormat = "0.##", string displayFormat = "{0}",  bool isServerOnly = false, string hintDescription = null, string headerDescription = null, bool headerPaddling = false)
            {
                this.Label = label;
                this.MinimumValue = minValue;
                this.MaximumValue = maxValue;
                this.DefaultValue = defaultValue;
                this.IsInteger = isInteger;
                this.DisplayFormat = displayFormat;
                this.StringFormat = stringFormat;
                this.IsServerOnly = isServerOnly;
                this.HintDescription = hintDescription;
                this.HeaderDescription = headerDescription;
                this.HeaderPaddling = headerPaddling;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SliderConfig"/> class.
            /// </summary>
            public SliderConfig()
            {
            }

            /// <summary>
            /// Gets or sets label of a ButtonConfig.
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets MinimumValue of a ButtonConfig.
            /// </summary>
            public float MinimumValue { get; set; }

            /// <summary>
            /// Gets or sets MaximumValue of a ButtonConfig.
            /// </summary>
            public float MaximumValue { get; set; }

            /// <summary>
            /// Gets or sets DefaultValue of a ButtonConfig.
            /// </summary>
            public float DefaultValue { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether value is integer.
            /// </summary>
            public bool IsInteger { get; set; }

            /// <summary>
            /// Gets or sets string format of Slider Setting.
            /// </summary>
            public string StringFormat { get; set; }

            /// <summary>
            /// Gets or sets display format of Slider Setting.
            /// </summary>
            public string DisplayFormat { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether updates come from client.
            /// </summary>
            public bool IsServerOnly { get; set; }

            /// <summary>
            /// Gets or sets HeaderName of a ButtonConfig.
            /// </summary>
            public string HeaderName { get; set; }

            /// <summary>
            /// Gets or sets HeaderDescription of a ButtonConfig.
            /// </summary>
            public string HintDescription { get; set; }

            /// <summary>
            /// Gets or sets HeaderDescription of a ButtonConfig.
            /// </summary>
            public string HeaderDescription { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether HeaderPaddling is needed.
            /// </summary>
            public bool HeaderPaddling { get; set; }

            /// <summary>
            /// Creates a ButtonSetting instanse.
            /// </summary>
            /// <returns>ButtonSetting.</returns>
            public override SliderSetting Create() => new(++IdIncrementor, this.Label, this.MinimumValue, this.MaximumValue, this.DefaultValue, this.IsInteger, this.StringFormat, this.DisplayFormat,
                this.HintDescription, 255, this.IsServerOnly, this.HeaderName == null ? null : new HeaderSetting(this.HeaderName, this.HeaderDescription, this.HeaderPaddling));
        }
    }
}