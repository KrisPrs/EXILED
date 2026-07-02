// -----------------------------------------------------------------------
// <copyright file="CockedController.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.API.Models;

using Exiled.API.Features;
using Features;
using LabApi.Features.Wrappers;
using UnityEngine;

#pragma warning disable SA1600
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class CockedController : MonoBehaviour
{
    private float сooldownTime;

    private float lastShotTime;

    private FirearmItem firearmItem = null!;

    public bool IsOnCooldown => Time.time - lastShotTime < сooldownTime;

    public bool IsCocked
    {
        get => firearmItem.Cocked;
        set => firearmItem.Cocked = value;
    }

    public bool IsBoltLocked
    {
        get => firearmItem.BoltLocked;
        set => firearmItem.BoltLocked = value;
    }

    public void Init(FirearmItem firearm, CustomWeapon weapon)
    {
        firearmItem = firearm;
        сooldownTime = weapon.FireCooldown;
    }

    public void ProcessShot()
    {
        Log.Debug("Лочим пушку");
        lastShotTime = Time.time;
        IsBoltLocked = true;
    }

    public void Update()
    {
        if (firearmItem.StoredAmmo + firearmItem.ChamberedAmmo == 0 || IsOnCooldown)
            return;

        Log.Debug("Разлочим пушку");
        IsBoltLocked = false;
    }
}
#pragma warning restore SA1600
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member