// -----------------------------------------------------------------------
// <copyright file="KeybindSetting.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Core.UserSettings
{
    using System;

    using Exiled.API.Interfaces;
    using global::UserSettings.ServerSpecific;
    using UnityEngine;

    /// <summary>
    /// Represents a keybind setting.
    /// </summary>
    public class KeybindSetting : SettingBase, IWrapper<SSKeybindSetting>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeybindSetting"/> class.
        /// </summary>
        /// <param name="id"><inheritdoc cref="SettingBase.Id"/></param>
        /// <param name="label"><inheritdoc cref="SettingBase.Label"/></param>
        /// <param name="suggested"><inheritdoc cref="KeyCode"/></param>
        /// <param name="preventInteractionOnGUI"><inheritdoc cref="PreventInteractionOnGUI"/></param>
        /// <param name="allowSpectatorTrigger"><inheritdoc cref="AllowSpectatorTrigger"/></param>
        /// <param name="hintDescription"><inheritdoc cref="SettingBase.HintDescription"/></param>
        /// <param name="collectionId"><inheritdoc cref="SettingBase.CollectionId"/></param>
        /// <param name="header"><inheritdoc cref="SettingBase.Header"/></param>
        /// <param name="onChanged"><inheritdoc cref="SettingBase.OnChanged"/></param>
        public KeybindSetting(int id, string label, KeyCode suggested, bool preventInteractionOnGUI = false, bool allowSpectatorTrigger = false, string hintDescription = "", byte collectionId = byte.MaxValue, HeaderSetting header = null, Action<Player, SettingBase> onChanged = null)
            : base(new SSKeybindSetting(id, label, suggested, preventInteractionOnGUI, allowSpectatorTrigger, hintDescription, collectionId), header, onChanged) =>
            this.Base = (SSKeybindSetting)base.Base;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeybindSetting"/> class.
        /// </summary>
        /// <param name="settingBase">A <see cref="SSKeybindSetting"/> instance.</param>
        internal KeybindSetting(SSKeybindSetting settingBase)
            : base(settingBase) =>
            this.Base = settingBase;

        /// <inheritdoc/>
        public new SSKeybindSetting Base { get; }

        /// <summary>
        /// Gets a value indicating whether the key is pressed.
        /// </summary>
        public bool IsPressed => this.Base.SyncIsPressed;

        /// <summary>
        /// Gets or sets a value indicating whether the interaction is prevented while player is in RA, Settings etc.
        /// </summary>
        public bool PreventInteractionOnGUI
        {
            get => this.Base.PreventInteractionOnGUI;
            set => this.Base.PreventInteractionOnGUI = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the interaction is prevented in spectator roles.
        /// </summary>
        public bool AllowSpectatorTrigger
        {
            get => this.Base.AllowSpectatorTrigger;
            set => this.Base.AllowSpectatorTrigger = value;
        }

        /// <summary>
        /// Gets or sets the assigned key.
        /// </summary>
        public KeyCode KeyCode
        {
            get => this.Base.SuggestedKey;
            set => this.Base.SuggestedKey = value;
        }

        /// <summary>
        /// Returns a representation of this <see cref="KeybindSetting"/>.
        /// </summary>
        /// <returns>A string in human-readable format.</returns>
        public override string ToString() => base.ToString() + $" /{this.IsPressed}/ *{this.KeyCode}* +{this.PreventInteractionOnGUI}+";

        /// <summary>
        /// Represents a config for KeybindSetting.
        /// </summary>
        public class KeybindConfig : SettingConfig<KeybindSetting>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="KeybindConfig"/> class.
            /// </summary>
            /// <param name="label"/><inheritdoc cref="Label"/>
            /// <param name="keyCode"><inheritdoc cref="KeyCode"/></param>
            /// <param name="headerName"><inheritdoc cref="HeaderName"/></param>
            /// <param name="preventInteractionOnGui"><inheritdoc cref="PreventInteractionOnGUI"/></param>
            /// <param name="allowSpectatorTrigger"><inheritdoc cref="AllowSpectatorTrigger"/></param>
            /// <param name="hintDescription"><inheritdoc cref="HintDescription"/></param>
            /// <param name="headerDescription"><inheritdoc cref="HeaderDescription"/></param>
            /// <param name="headerPaddling"><inheritdoc cref="HeaderPaddling"/></param>
            public KeybindConfig(string label, KeyCode keyCode, string hintDescription = null, bool preventInteractionOnGui = false, bool allowSpectatorTrigger = true, string headerName = null, string headerDescription = null, bool headerPaddling = false)
            {
                this.Label = label;
                this.KeyCode = keyCode;
                this.HintDescription = hintDescription;
                this.PreventInteractionOnGUI = preventInteractionOnGui;
                this.AllowSpectatorTrigger = allowSpectatorTrigger;
                this.HeaderName = headerName;
                this.HeaderDescription = headerDescription;
                this.HeaderPaddling = headerPaddling;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="KeybindConfig"/> class.
            /// </summary>
            public KeybindConfig()
            {
            }

            /// <summary>
            /// Gets or sets label of a KeybindConfig.
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets ButtonText of a KeybindConfig.
            /// </summary>
            public KeyCode KeyCode { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether interaction on GUI would be prevented.
            /// </summary>
            public bool PreventInteractionOnGUI { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether interaction on GUI would be prevented.
            /// </summary>
            public bool AllowSpectatorTrigger { get; set; }

            /// <summary>
            /// Gets or sets HintDescription of a KeybindConfig.
            /// </summary>
            public string HintDescription { get; set; }

            /// <summary>
            /// Gets or sets HeaderName of a KeybindConfig.
            /// </summary>
            public string HeaderName { get; set; }

            /// <summary>
            /// Gets or sets HeaderDescription of a KeybindConfig.
            /// </summary>
            public string HeaderDescription { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether HeaderPaddling is needed.
            /// </summary>
            public bool HeaderPaddling { get; set; }

            /// <summary>
            /// Creates a KeybindSetting instanse.
            /// </summary>
            /// <returns>KeybindSetting.</returns>
            public override KeybindSetting Create() => new(++IdIncrementor, this.Label, this.KeyCode, this.PreventInteractionOnGUI, this.AllowSpectatorTrigger, this.HintDescription, 255, this.HeaderName == null ? null : new HeaderSetting(this.HeaderName, this.HeaderDescription, this.HeaderPaddling));
        }
    }
}