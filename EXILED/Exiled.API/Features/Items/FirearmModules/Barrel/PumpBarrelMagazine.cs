// -----------------------------------------------------------------------
// <copyright file="PumpBarrelMagazine.cs" company="ExMod Team">
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
    /// Basic realization of <see cref="PumpActionModule"/> barrel.
    /// </summary>
    public class PumpBarrelMagazine : BarrelMagazine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PumpBarrelMagazine"/> class.
        /// </summary>
        /// <param name="pumpModule">Target <see cref="PumpActionModule"/>.</param>
        public PumpBarrelMagazine(PumpActionModule pumpModule)
            : base(pumpModule) =>
            this.PumpBarrel = pumpModule;

        /// <summary>
        /// Gets an original <see cref="IAmmoContainerModule"/>.
        /// </summary>
        public PumpActionModule PumpBarrel { get; }

        /// <inheritdoc/>
        public override Firearm Firearm => Item.Get<Firearm>(this.PumpBarrel.Firearm);

        /// <inheritdoc/>
        public override int Ammo
        {
            get => this.PumpBarrel.SyncChambered;

            set
            {
                this.PumpBarrel.SyncChambered = Mathf.Max(value, 0);
                this.Resync();
            }
        }

        /// <summary>
        /// Gets or sets an amount of bullets, that pump module will try to shot.
        /// </summary>
        public int CockedAmmo
        {
            get => this.PumpBarrel.SyncCocked;

            set
            {
                this.PumpBarrel.SyncCocked = Mathf.Max(value, 0);
                this.Resync();
            }
        }

        /// <inheritdoc/>
        public override int MaxAmmo
        {
            get => this.PumpBarrel._numberOfBarrels;
            set => this.PumpBarrel._numberOfBarrels = Mathf.Max(value, 0);
        }

        /// <inheritdoc/>
        public override bool IsCocked
        {
            get => this.PumpBarrel.SyncCocked > 0;

            set
            {
                this.PumpBarrel.SyncCocked = value ? this.MaxAmmo : 0;
                this.Resync();
            }
        }

        /// <inheritdoc/>
        public override void Resync() => this.PumpBarrel.ServerResync();
    }
}
