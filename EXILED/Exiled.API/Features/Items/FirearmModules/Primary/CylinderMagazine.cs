// -----------------------------------------------------------------------
// <copyright file="CylinderMagazine.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Items.FirearmModules.Primary
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Exiled.API.Enums;
    using Exiled.API.Extensions;

    using InventorySystem.Items.Firearms.Attachments;
    using InventorySystem.Items.Firearms.Modules;

    /// <summary>
    /// Basic realization of <see cref="CylinderAmmoModule"/>.
    /// </summary>
    public class CylinderMagazine : PrimaryMagazine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CylinderMagazine"/> class.
        /// </summary>
        /// <param name="magazine">target <see cref="CylinderAmmoModule"/>.</param>
        public CylinderMagazine(CylinderAmmoModule magazine)
            : base(magazine) =>
            this.CylinderModule = magazine;

        /// <summary>
        /// Gets an original <see cref="IPrimaryAmmoContainerModule"/>.
        /// </summary>
        public CylinderAmmoModule CylinderModule { get; }

        /// <inheritdoc/>
        public override Firearm Firearm => Item.Get<Firearm>(this.CylinderModule.Firearm);

        /// <inheritdoc/>
        public override int MaxAmmo
        {
            set
            {
                this.CylinderModule._defaultCapacity = value - (int)this.CylinderModule.Firearm.AttachmentsValue(AttachmentParam.MagazineCapacityModifier);
                this.Resync();
            }
        }

        /// <inheritdoc/>
        public override int ConstantMaxAmmo
        {
            get => this.CylinderModule._defaultCapacity;
            set => this.CylinderModule._defaultCapacity = value;
        }

        /// <summary>
        /// Gets or sets an used <see cref="Exiled.API.Enums.AmmoType"/> for this magazine.
        /// </summary>
        public override AmmoType AmmoType
        {
            get => this.Magazine.AmmoType.GetAmmoType();
            set => this.CylinderModule.AmmoType = value.GetItemType();
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of chambers in cylindric magazine.
        /// </summary>
        public IEnumerable<Chamber> Chambers => CylinderAmmoModule.GetChambersArrayForSerial(this.CylinderModule.ItemSerial, this.MaxAmmo).Select(baseChamber => new Chamber(baseChamber));

        /// <inheritdoc/>
        public override void Resync() => this.CylinderModule._needsResyncing = true;

        /// <summary>
        /// Rotates cylindric magazine by fixed rotatins.
        /// </summary>
        /// <param name="rotations">Rotations count.</param>
        public void Rotate(int rotations) => this.CylinderModule.RotateCylinder(rotations);

        /// <summary>
        /// A basic wrapper for chamber in cylinder magazine.
        /// </summary>
        public class Chamber
        {
            private CylinderAmmoModule.Chamber baseChamber;

            /// <summary>
            /// Initializes a new instance of the <see cref="Chamber"/> class.
            /// </summary>
            /// <param name="baseChamber">Basic <see cref="CylinderAmmoModule.Chamber"/> class.</param>
            internal Chamber(CylinderAmmoModule.Chamber baseChamber) => this.baseChamber = baseChamber;

            /// <summary>
            /// Gets or sets an state for current chamber.
            /// </summary>
            public RevolverChamberState State
            {
                get => (RevolverChamberState)this.baseChamber.ContextState;
                set => this.baseChamber.ContextState = (CylinderAmmoModule.ChamberState)value;
            }
        }
    }
}
