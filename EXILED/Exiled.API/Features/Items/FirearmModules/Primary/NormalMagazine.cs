// -----------------------------------------------------------------------
// <copyright file="NormalMagazine.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Items.FirearmModules.Primary
{
    using System;

    using Exiled.API.Enums;
    using Exiled.API.Extensions;

    using InventorySystem.Items.Firearms.Attachments;
    using InventorySystem.Items.Firearms.Modules;

    /// <summary>
    /// Basic realization of <see cref="InventorySystem.Items.Firearms.Modules.MagazineModule"/>.
    /// </summary>
    public class NormalMagazine : PrimaryMagazine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NormalMagazine"/> class.
        /// </summary>
        /// <param name="magazine">target <see cref="IPrimaryAmmoContainerModule"/>.</param>
        public NormalMagazine(MagazineModule magazine)
            : base(magazine) =>
            this.MagazineModule = magazine;

        /// <summary>
        /// Gets an original <see cref="MagazineModule"/>.
        /// </summary>
        public MagazineModule MagazineModule { get; }

        /// <inheritdoc/>
        public override Firearm Firearm => Item.Get<Firearm>(this.MagazineModule.Firearm);

        /// <inheritdoc/>
        public override int MaxAmmo
        {
            set => this.MagazineModule._defaultCapacity = value - (int)this.MagazineModule.Firearm.AttachmentsValue(AttachmentParam.MagazineCapacityModifier);
        }

        /// <inheritdoc/>
        public override int Ammo
        {
            set
            {
                MagazineModule.SyncData[this.MagazineModule.ItemSerial] = Math.Max(value, 0) + 1;
                this.Resync();
            }
        }

        /// <inheritdoc/>
        public override int ConstantMaxAmmo
        {
            get => this.MagazineModule._defaultCapacity;
            set => this.MagazineModule._defaultCapacity = value;
        }

        /// <inheritdoc/>
        public override AmmoType AmmoType
        {
            get => this.Magazine.AmmoType.GetAmmoType();

            set => this.MagazineModule._ammoType = value.GetItemType();
        }

        /// <summary>
        /// Gets or sets a value indicating whether magazine is inserted.
        /// </summary>
        public bool MagazineInserted
        {
            get => this.MagazineModule.MagazineInserted;

            set
            {
                this.MagazineModule.MagazineInserted = value;
                this.Resync();
            }
        }

        /// <summary>
        /// Removes magazine from current <see cref="Exiled.API.Features.Items.Firearm"/>.
        /// </summary>
        /// <remarks>
        /// Affects on actual ammo count.
        /// Removes all ammo from magazine.
        /// </remarks>
        public void RemoveMagazine() => this.MagazineModule.ServerRemoveMagazine();

        /// <summary>
        /// Inserts current magazine from current <see cref="Exiled.API.Features.Items.Firearm"/>.
        /// </summary>
        public void InsertMagazine() => this.MagazineModule.ServerInsertEmptyMagazine();

        /// <inheritdoc/>
        public override void Resync() => this.MagazineModule.ServerResyncData();
    }
}
