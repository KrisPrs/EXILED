// -----------------------------------------------------------------------
// <copyright file="AutomaticBarrelMagazine.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Items.FirearmModules.Barrel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using InventorySystem.Items.Firearms.Modules;

    using UnityEngine;

    /// <summary>
    /// Basic realization of <see cref="AutomaticActionModule"/> barrel.
    /// </summary>
    public class AutomaticBarrelMagazine : BarrelMagazine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticBarrelMagazine"/> class.
        /// </summary>
        /// <param name="automaticModule">Target <see cref="AutomaticActionModule"/>.</param>
        public AutomaticBarrelMagazine(AutomaticActionModule automaticModule)
            : base(automaticModule) =>
            this.AutomaticBarrel = automaticModule;

        /// <summary>
        /// Gets an original <see cref="IAmmoContainerModule"/>.
        /// </summary>
        public AutomaticActionModule AutomaticBarrel { get; }

        /// <inheritdoc/>
        public override Firearm Firearm => Item.Get<Firearm>(this.AutomaticBarrel.Firearm);

        /// <inheritdoc/>
        public override int Ammo
        {
            get => this.AutomaticBarrel.AmmoStored;

            set
            {
                this.AutomaticBarrel.AmmoStored = Mathf.Max(value, 0);
                this.Resync();
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Will be ranged between <see langword="0"/> and <see langword="16"/> due basegame logic.
        /// </remarks>
        public override int MaxAmmo
        {
            get => this.AutomaticBarrel.ChamberSize;

            set => this.AutomaticBarrel.ChamberSize = Mathf.Clamp(value, 0, 16);
        }

        /// <inheritdoc/>
        public override bool IsCocked
        {
            get => this.AutomaticBarrel.Cocked;

            set
            {
                this.AutomaticBarrel.Cocked = value;
                this.Resync();
            }
        }

        /// <summary>
        /// Gets a value indicating whether barrel magazine has open bolt or not.
        /// </summary>
        public bool IsOpenBolted => this.AutomaticBarrel.OpenBolt;

        /// <summary>
        /// Gets or sets a value indicating whether barrel bolt is currently locked.
        /// </summary>
        public bool BoltLocked
        {
            get => this.AutomaticBarrel.BoltLocked;

            set
            {
                this.AutomaticBarrel.BoltLocked = value;
                this.Resync();
            }
        }

        /// <summary>
        /// Gets the fire rate of the firearm.
        /// </summary>
        public float FireRate => this.AutomaticBarrel.BaseFireRate;

        /// <inheritdoc/>
        public override void Resync() => this.AutomaticBarrel.ServerResync();
    }
}
