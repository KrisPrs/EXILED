// -----------------------------------------------------------------------
// <copyright file="BodyArmorPickup.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Pickups
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Exiled.API.Features.Items;
    using Exiled.API.Interfaces;
    using Exiled.API.Structs;

    using InventorySystem.Items;
    using InventorySystem.Items.Armor;

    using UnityEngine;

    using BaseBodyArmor = InventorySystem.Items.Armor.BodyArmorPickup;

    /// <summary>
    /// A wrapper class for a Body Armor pickup.
    /// </summary>
    public class BodyArmorPickup : Pickup, IWrapper<BaseBodyArmor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BodyArmorPickup"/> class.
        /// </summary>
        /// <param name="pickupBase">The base <see cref="BaseBodyArmor"/> class.</param>
        internal BodyArmorPickup(BaseBodyArmor pickupBase)
            : base(pickupBase)
        {
            Base = pickupBase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyArmorPickup"/> class.
        /// </summary>
        /// <param name="type">The <see cref="ItemType"/> of the pickup.</param>
        internal BodyArmorPickup(ItemType type)
            : base(type)
        {
            Base = (BaseBodyArmor)((Pickup)this).Base;
        }

        /// <summary>
        /// Gets the <see cref="BaseBodyArmor"/> that this class is encapsulating.
        /// </summary>
        public new BaseBodyArmor Base { get; }

        /// <summary>
        /// Gets a value indicating whether this item is equippable.
        /// </summary>
        public bool Equippable { get; } = false;

        /// <summary>
        /// Gets a value indicating whether this item is holsterable.
        /// </summary>
        public bool Holsterable { get; } = false;

        /// <summary>
        /// Gets a value indicating whether or not this is a worn item.
        /// </summary>
        public bool IsWorn { get; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not excess ammo should be removed when the armor is dropped.
        /// </summary>
        public bool RemoveExcessOnDrop { get; set; }

        /// <summary>
        /// Gets or sets how strong the helmet on the armor is.
        /// </summary>
        public int HelmetEfficacy
        {
            get => helmetEfficacy;
            set
            {
                if (value is > 100 or < 0)
                {
                    Log.Warn($"{nameof(HelmetEfficacy)} Value of armor efficacy must be between 0 and 100.");
                    value = Mathf.Clamp(value, 0, 100);
                }

                helmetEfficacy = value;
            }
        }

        /// <summary>
        /// Gets or sets how strong the vest on the armor is.
        /// </summary>
        public int VestEfficacy
        {
            get => vestEfficacy;
            set
            {
                if (value is > 101 or < 0)
                {
                    Log.Warn($"{nameof(VestEfficacy)} Value of armor efficacy must be between 0 and 100.");
                    value = Mathf.Clamp(value, 0, 100);
                }

                vestEfficacy = value;
            }
        }

        /// <summary>
        /// Gets or sets how much faster stamina will drain when wearing this armor.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When attempting to set the value below 1 or above 2.</exception>
        public float StaminaUseMultiplier { get; set; }

        /// <summary>
        /// Gets how much the users movement speed should be affected when wearing this armor. (higher values = slower movement).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When attempting to set the value below 0 or above 1.</exception>
        public float MovementSpeedMultiplier { get; }

        /// <summary>
        /// Gets or sets the ammo limit of the wearer when using this armor.
        /// </summary>
        public IEnumerable<ArmorAmmoLimit> AmmoLimits { get; set; }

        /// <summary>
        /// Gets or sets the item caterory limit of the wearer when using this armor.
        /// </summary>
        public IEnumerable<BodyArmor.ArmorCategoryLimitModifier> CategoryLimits { get; set; }

        /// <summary>
        /// Returns the BodyArmorPickup in a human readable format.
        /// </summary>
        /// <returns>A string containing BodyArmorPickup related data.</returns>
        public override string ToString() => $"{Type} ({Serial}) [{Weight}] *{Scale}*";

        /// <inheritdoc/>
        internal override void ReadItemInfo(Item item)
        {
            base.ReadItemInfo(item);
            if (item is Armor armoritem)
            {
                helmetEfficacy = armoritem.HelmetEfficacy;
                vestEfficacy = armoritem.VestEfficacy;
                RemoveExcessOnDrop = armoritem.RemoveExcessOnDrop;
                StaminaUseMultiplier = armoritem.StaminaUseMultiplier;
                AmmoLimits = armoritem.AmmoLimits;
                CategoryLimits = armoritem.CategoryLimits;
            }
        }

        /// <inheritdoc/>
        protected override void InitializeProperties(ItemBase itemBase)
        {
            base.InitializeProperties(itemBase);
            if (itemBase is BodyArmor armoritem)
            {
                helmetEfficacy = armoritem.HelmetEfficacy;
                vestEfficacy = armoritem.VestEfficacy;
                RemoveExcessOnDrop = !armoritem.DontRemoveExcessOnDrop;
                AmmoLimits = armoritem.AmmoLimits.Select(limit => (ArmorAmmoLimit)limit);
                CategoryLimits = armoritem.CategoryLimits;
            }
        }
    }
}
