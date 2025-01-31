// -----------------------------------------------------------------------
// <copyright file="TextInputSetting.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Core.UserSettings
{
    using System;

    using Exiled.API.Features.Core.Interfaces;
    using Exiled.API.Interfaces;
    using global::UserSettings.ServerSpecific;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Represents a text input setting.
    /// </summary>
    public class TextInputSetting : SettingBase, IWrapper<SSTextArea>, ISettingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextInputSetting"/> class.
        /// </summary>
        /// <param name="label"><inheritdoc cref="SettingBase.Label"/></param>
        /// <param name="foldoutMode"><inheritdoc cref="FoldoutMode"/></param>
        /// <param name="alignment"><inheritdoc cref="Alignment"/></param>
        /// <param name="hintDescription"><inheritdoc cref="SettingBase.HintDescription"/></param>
        /// <param name="header"><inheritdoc cref="SettingBase.Header"/></param>
        public TextInputSetting(
            string label,
            SSTextArea.FoldoutMode foldoutMode = SSTextArea.FoldoutMode.NotCollapsable,
            TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft,
            string hintDescription = null,
            HeaderSetting header = null)
            : base(new SSTextArea(NextId++, label, foldoutMode, hintDescription, alignment), header)
        {
            Base = (SSTextArea)base.Base;
        }

        /// <summary>
        /// Gets or sets the action to be executed when this setting is triggered.
        /// </summary>
        public event Action<Player, TextInputSetting> OnTriggered;

        /// <inheritdoc/>
        public new SSTextArea Base { get; }

        /// <summary>
        /// Gets or sets the text for the setting.
        /// </summary>
        public new string Label
        {
            get => Base.Label;
            set => Base.SendTextUpdate(value);
        }

        /// <summary>
        /// Gets or sets the foldout mode.
        /// </summary>
        public SSTextArea.FoldoutMode FoldoutMode
        {
            get => Base.Foldout;
            set => Base.Foldout = value;
        }

        /// <summary>
        /// Gets or sets the text alignment options.
        /// </summary>
        public TextAlignmentOptions Alignment
        {
            get => Base.AlignmentOptions;
            set => Base.AlignmentOptions = value;
        }

        /// <summary>
        /// Returns a representation of this <see cref="TextInputSetting"/>.
        /// </summary>
        /// <returns>A string in human-readable format.</returns>
        public override string ToString() => base.ToString() + $" /{FoldoutMode}/ *{Alignment}*";

        /// <inheritdoc cref="ISettingHandler"/>>
        public void Handle(Player player, SettingBase setting)
        {
            if (setting != this)
                return;

            OnTriggered?.Invoke(player, this);
        }

        /// <summary>
        /// Represents a config for TextInputSetting.
        /// </summary>
        public class TextInputConfig : SettingConfig<TextInputSetting>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TextInputConfig"/> class.
            /// </summary>
            /// <param name="label"/><inheritdoc cref="Label"/>
            /// <param name="headerName"><inheritdoc cref="HeaderName"/></param>
            /// <param name="textAlignmentOptions"><inheritdoc cref="TextAlignmentOptions"/></param>
            /// <param name="hintDescription"><inheritdoc cref="HintDescription"/></param>
            /// <param name="headerDescription"><inheritdoc cref="HeaderDescription"/></param>
            /// <param name="headerPaddling"><inheritdoc cref="HeaderPaddling"/></param>
            /// <param name="foldoutMode"></param><inheritdoc cref="FoldoutMode"/>
            public TextInputConfig(string label, SSTextArea.FoldoutMode foldoutMode, TextAlignmentOptions textAlignmentOptions, string hintDescription = null, string headerName = null, string headerDescription = null, bool headerPaddling = false)
            {
                Label = label;
                HintDescription = hintDescription;
                FoldoutMode = foldoutMode;
                TextAlignmentOptions = textAlignmentOptions;
                HeaderName = headerName;
                HeaderDescription = headerDescription;
                HeaderPaddling = headerPaddling;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TextInputConfig"/> class.
            /// </summary>
            public TextInputConfig()
            {
            }

            /// <summary>
            /// Gets or sets label of a TextInputConfig.
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets FoldoutMode of a TextInputConfig.
            /// </summary>
            public SSTextArea.FoldoutMode FoldoutMode { get; set; }

            /// <summary>
            /// Gets or sets TextAlignmentOptions for TextInputConfig.
            /// </summary>
            public TextAlignmentOptions TextAlignmentOptions { get; set; }

            /// <summary>
            /// Gets or sets HintDescription of a TextInputConfig.
            /// </summary>
            public string HintDescription { get; set; }

            /// <summary>
            /// Gets or sets HeaderName of a TextInputConfig.
            /// </summary>
            public string HeaderName { get; set; }

            /// <summary>
            /// Gets or sets HeaderDescription of a TextInputConfig.
            /// </summary>
            public string HeaderDescription { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether HeaderPaddling is needed.
            /// </summary>
            public bool HeaderPaddling { get; set; }

            /// <summary>
            /// Creates a TextInputSetting instanse.
            /// </summary>
            /// <returns>TextInputSetting.</returns>
            public override TextInputSetting Create() => new(Label, FoldoutMode, TextAlignmentOptions, HintDescription, HeaderName == null ? null : new HeaderSetting(HeaderName, HeaderDescription, HeaderPaddling));
        }
    }
}