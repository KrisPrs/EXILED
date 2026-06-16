// -----------------------------------------------------------------------
// <copyright file="DisplayConfig.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.API.Models.Configs;

using System.ComponentModel;
using UnityEngine;

/// <summary>
/// DisplayConfig class.
/// </summary>
public class DisplayConfig
{
    /// <summary>
    /// Gets or sets what display name would be shown.
    /// </summary>
    [Description("Отображаемый в виде текст-тоя текст над пикапом. Пустая строка - возьмёт Name предмета. null - отключить")]
    public string? DisplayName { get; set; } = null;

    /// <summary>
    /// Gets or sets on what distance display name would be shown.
    /// </summary>
    [Description("Расстояние от игрока на котором текст-тои будут появляться над пикапами")]
    public float DistanceToAppear { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether display name will be shown only when player looking at pickup.
    /// </summary>
    [Description("Нужно ли смотреть на пикап чтобы текст над ним появился?")]
    public bool IsStraightLookNeeded { get; set; } = true;

    /// <summary>
    /// Gets or sets an offset for a display text.
    /// </summary>
    [Description("Офссет с которым текст будет помещён относительно предмета")]
    public Vector3 DisplayTextOffset { get; set; } = Vector3.zero;
}