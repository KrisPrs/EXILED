// -----------------------------------------------------------------------
// <copyright file="Firearm.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Items
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CameraShaking;
    using Enums;

    using Exiled.API.Features.Items.FirearmModules;
    using Exiled.API.Features.Items.FirearmModules.Barrel;
    using Exiled.API.Features.Items.FirearmModules.Primary;
    using Exiled.API.Features.Pickups;
    using Exiled.API.Interfaces;
    using Exiled.API.Structs;
    using Extensions;
    using InventorySystem.Items.Autosync;
    using InventorySystem.Items.Firearms.Attachments;
    using InventorySystem.Items.Firearms.Attachments.Components;
    using InventorySystem.Items.Firearms.Modules;

    using static InventorySystem.Items.Firearms.Modules.AnimatorReloaderModuleBase;

    using BaseFirearm = InventorySystem.Items.Firearms.Firearm;
    using FirearmPickup = Pickups.FirearmPickup;

    /// <summary>
    /// A wrapper class for <see cref="InventorySystem.Items.Firearms.Firearm"/>.
    /// </summary>
    public class Firearm : Item, IWrapper<BaseFirearm>
    {
        /// <summary>
        /// A <see cref="List{T}"/> of <see cref="Firearm"/> which contains all the existing firearms based on all the <see cref="FirearmType"/>s.
        /// </summary>
        internal static readonly Dictionary<FirearmType, Firearm> ItemTypeToFirearmInstance = new();

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey, TValue}"/> which contains all the base codes expressed in <see cref="FirearmType"/> and <see cref="uint"/>.
        /// </summary>
        internal static readonly Dictionary<FirearmType, uint> BaseCodesValue = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Firearm"/> class.
        /// </summary>
        /// <param name="itemBase">The base <see cref="InventorySystem.Items.Firearms.Firearm"/> class.</param>
        public Firearm(BaseFirearm itemBase)
            : base(itemBase)
        {
            this.Base = itemBase;

            foreach (ModuleBase module in this.Base.Modules)
            {
                switch (module)
                {
                    case IPrimaryAmmoContainerModule primaryAmmoModule:
                        this.PrimaryMagazine ??= (PrimaryMagazine)Magazine.Get(primaryAmmoModule);
                        break;

                    case IAmmoContainerModule ammoModule:
                        this.BarrelMagazine ??= (BarrelMagazine)Magazine.Get(ammoModule);
                        break;

                    case HitscanHitregModuleBase hitregModule:
                        this.HitscanHitregModule = hitregModule;
                        break;

                    case AnimatorReloaderModuleBase animatorReloaderModule:
                        this.AnimatorReloaderModule = animatorReloaderModule;
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Firearm"/> class.
        /// </summary>
        /// <param name="type">The <see cref="ItemType"/> of the firearm.</param>
        internal Firearm(ItemType type)
            : this((BaseFirearm)Server.Host.Inventory.CreateItemInstance(new(type, 0), false))
        {
            FlashlightAttachment flashlight = this.Attachments.OfType<FlashlightAttachment>().FirstOrDefault();

            if (flashlight != null && flashlight.IsEnabled)
                flashlight.ServerSendStatus(true);
        }

        /// <inheritdoc cref="BaseCodesValue"/>.
        public static IReadOnlyDictionary<FirearmType, uint> BaseCodes => BaseCodesValue;

        /// <inheritdoc cref="AvailableAttachmentsValue"/>.
        public static IReadOnlyDictionary<FirearmType, AttachmentIdentifier[]> AvailableAttachments => AvailableAttachmentsValue;

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey, TValue}"/> which represents all the preferences for each <see cref="Player"/>.
        /// </summary>
        public static IReadOnlyDictionary<Player, Dictionary<FirearmType, AttachmentIdentifier[]>> PlayerPreferences
        {
            get
            {
                IEnumerable<KeyValuePair<Player, Dictionary<FirearmType, AttachmentIdentifier[]>>> playerPreferences =
                    AttachmentsServerHandler.PlayerPreferences.Where(
                        kvp => kvp.Key is not null).Select(
                        (KeyValuePair<ReferenceHub, Dictionary<ItemType, uint>> keyValuePair) =>
                        {
                            return new KeyValuePair<Player, Dictionary<FirearmType, AttachmentIdentifier[]>>(
                                Player.Get(keyValuePair.Key),
                                keyValuePair.Value.ToDictionary(
                                    kvp => kvp.Key.GetFirearmType(),
                                    kvp => kvp.Key.GetFirearmType().GetAttachmentIdentifiers(kvp.Value).ToArray()));
                        });

                return playerPreferences.Where(kvp => kvp.Key is not null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        /// <summary>
        /// Gets the <see cref="InventorySystem.Items.Firearms.Firearm"/> that this class is encapsulating.
        /// </summary>
        public new BaseFirearm Base { get; }

        /// <summary>
        /// Gets a primaty magazine for current firearm.
        /// </summary>
        public PrimaryMagazine PrimaryMagazine { get; }

        /// <summary>
        /// Gets a barrel magazine for current firearm.
        /// </summary>
        /// <remarks>
        /// <see langword="null"/> for Revolver and ParticleDisruptor.
        /// </remarks>
        public BarrelMagazine BarrelMagazine { get; }

        /// <summary>
        /// Gets a primaty magazine for current firearm.
        /// </summary>
        public HitscanHitregModuleBase HitscanHitregModule { get; }

        /// <summary>
        /// Gets an animator reloader module for the current firearm.
        /// </summary>
        public AnimatorReloaderModuleBase AnimatorReloaderModule { get; }

        /// <summary>
        /// Gets or sets the amount of ammo in the firearm magazine.
        /// </summary>
        public int MagazineAmmo
        {
            get => this.PrimaryMagazine.Ammo;
            set => this.PrimaryMagazine.Ammo = value;
        }

        /// <summary>
        /// Gets or sets the amount of ammo in the firearm barrel.
        /// </summary>
        /// <remarks>
        /// not working for Revolver and ParticleDisruptor.
        /// </remarks>
        public int BarrelAmmo
        {
            get => this.BarrelMagazine?.Ammo ?? 0;

            set
            {
                if (this.BarrelMagazine != null)
                    this.BarrelMagazine.Ammo = value;
            }
        }

        /// <summary>
        /// Gets the total amount of ammo in the firearm.
        /// </summary>
        public int TotalAmmo => this.Base.GetTotalStoredAmmo();

        /// <summary>
        /// Gets or sets the max ammo for this firearm.
        /// </summary>
        public int MaxMagazineAmmo
        {
            get => this.PrimaryMagazine.MaxAmmo;
            set => this.PrimaryMagazine.MaxAmmo = value;
        }

        /// <summary>
        /// Gets or sets the damage for this firearm.
        /// </summary>
        public float Damage
        {
            get => this.HitscanHitregModule.BaseDamage;
            set => this.HitscanHitregModule.BaseDamage = value;
        }

        /// <summary>
        /// Gets or sets the inaccuracy for this firearm.
        /// </summary>
        public float Inaccuracy
        {
            get => this.HitscanHitregModule.BaseBulletInaccuracy;
            set => this.HitscanHitregModule.BaseBulletInaccuracy = value;
        }

        /// <summary>
        /// Gets or sets the penetration for this firearm.
        /// </summary>
        public float Penetration
        {
            get => this.HitscanHitregModule.BasePenetration;
            set => this.HitscanHitregModule.BasePenetration = value;
        }

        /// <summary>
        /// Gets or sets how much fast the value drop over the distance.
        /// </summary>
        public float DamageFalloffDistance
        {
            get => this.HitscanHitregModule.DamageFalloffDistance;
            set => this.HitscanHitregModule.DamageFalloffDistance = value;
        }

        /// <summary>
        /// Gets the damage for this firearm with attachement modifier.
        /// </summary>
        public float EffectiveDamage => this.HitscanHitregModule.EffectiveDamage;

        /// <summary>
        /// Gets the inaccuracy for this firearm with attachement modifier.
        /// </summary>
        public float EffectiveInaccuracy => this.HitscanHitregModule.CurrentInaccuracy;

        /// <summary>
        /// Gets the penetration for this firearm with attachement modifier.
        /// </summary>
        public float EffectivePenetration => this.HitscanHitregModule.DisplayPenetration;

        /// <summary>
        /// Gets or sets the amount of max ammo in the firearm barrel.
        /// </summary>
        /// <remarks>
        /// not working for Revolver and ParticleDisruptor.
        /// </remarks>
        public int MaxBarrelAmmo
        {
            get => this.BarrelMagazine?.MaxAmmo ?? 0;

            set
            {
                if (this.BarrelMagazine != null)
                    this.BarrelMagazine.MaxAmmo = value;
            }
        }

        /// <summary>
        /// Gets the total amount of ammo in the firearm.
        /// </summary>
        public int TotalMaxAmmo => this.Base.GetTotalMaxAmmo();

        /// <summary>
        /// Gets or sets a ammo drain per shoot.
        /// </summary>
        /// <remarks>
        /// Always <see langword="1"/> by default.
        /// Applied on a high layer nether basegame ammo controllers.
        /// </remarks>
        public int AmmoDrain { get; set; } = 1;

        /// <summary>
        /// Gets a value indicating whether the weapon is reloading.
        /// </summary>
        public bool IsReloading => this.Base.TryGetModule(out IReloaderModule module) && module.IsReloading;

        /// <summary>
        /// Gets the <see cref="Enums.FirearmType"/> of the firearm.
        /// </summary>
        public FirearmType FirearmType => this.Type.GetFirearmType();

        /// <summary>
        /// Gets the <see cref="Enums.AmmoType"/> of the firearm.
        /// </summary>
        public AmmoType AmmoType => this.PrimaryMagazine.AmmoType;

        /// <summary>
        /// Gets a value indicating whether the firearm is being aimed.
        /// </summary>
        public bool Aiming => this.Base.TryGetModule(out IAdsModule module) && module.AdsTarget;

        /// <summary>
        /// Gets a value indicating whether the firearm's flashlight module is enabled.
        /// </summary>
        public bool FlashlightEnabled => this.Base.IsEmittingLight;

        /// <summary>
        /// Gets a value indicating whether the firearm's NightVision is being used.
        /// </summary>
        public bool NightVisionEnabled => this.Aiming && this.Base.HasAdvantageFlag(AttachmentDescriptiveAdvantages.NightVision);

        /// <summary>
        /// Gets a value indicating whether the firearm's flashlight module is enabled or NightVision is being used.
        /// </summary>
        public bool CanSeeThroughDark => this.FlashlightEnabled || this.NightVisionEnabled;

        /// <summary>
        /// Gets a value indicating whether the firearm is automatic.
        /// </summary>
        public bool IsAutomatic => this.BarrelMagazine is AutomaticBarrelMagazine;

        /// <summary>
        /// Gets the <see cref="Attachment"/>s of the firearm.
        /// </summary>
        public Attachment[] Attachments => this.Base.Attachments;

        /// <summary>
        /// Gets the <see cref="AttachmentIdentifier"/>s of the firearm.
        /// </summary>
        public IEnumerable<AttachmentIdentifier> AttachmentIdentifiers
        {
            get
            {
                foreach (Attachment attachment in this.Attachments.Where(att => att.IsEnabled))
                    yield return AvailableAttachments[this.FirearmType].FirstOrDefault(att => att == attachment);
            }
        }

        /// <summary>
        /// Gets the base code of the firearm.
        /// </summary>
        public uint BaseCode => BaseCodesValue[this.FirearmType];

        /// <summary>
        /// Gets or sets the recoil settings of the firearm, if it's an automatic weapon.
        /// </summary>
        /// <remarks>This property will not do anything if the firearm is not an automatic weapon.</remarks>
        /// <seealso cref="IsAutomatic"/>
        public RecoilSettings Recoil
        {
            get => this.Base.TryGetModule(out RecoilPatternModule module) ? module.BaseRecoil : default;
            set
            {
                if (this.Base.TryGetModule(out RecoilPatternModule module))
                    module.BaseRecoil = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey, TValue}"/> of <see cref="ItemType"/> and <see cref="AttachmentIdentifier"/>[] which contains all available attachments for all firearms.
        /// </summary>
        internal static Dictionary<FirearmType, AttachmentIdentifier[]> AvailableAttachmentsValue { get; } = new();

        /// <summary>
        /// Creates and returns a <see cref="Firearm"/> representing the provided <see cref="Enums.FirearmType"/>.
        /// </summary>
        /// <param name="type">The type of firearm to create.</param>
        /// <returns>The newly created firearm.</returns>
        public static Firearm Create(FirearmType type)
            => type is not FirearmType.None ? Create<Firearm>(type.GetItemType()) : null;

        /// <summary>
        /// Adds a <see cref="AttachmentIdentifier"/> to the firearm.
        /// </summary>
        /// <param name="identifier">The <see cref="AttachmentIdentifier"/> to add.</param>
        public void AddAttachment(AttachmentIdentifier identifier)
        {
            // Fallback addedCode onto AvailableAttachments' code in case it's 0
            uint addedCode = identifier.Code == 0
                ? AvailableAttachments[this.FirearmType].FirstOrDefault(attId => attId.Name == identifier.Name).Code
                : identifier.Code;

            // Look for conflicting attachment (attachment that occupies the same slot)
            uint conflicting = 0;
            uint current = 1;

            foreach (Attachment attachment in this.Base.Attachments)
            {
                if (attachment.Slot == identifier.Slot && attachment.IsEnabled)
                {
                    conflicting = current;
                    break;
                }

                current *= 2;
            }

            uint code = this.Base.ValidateAttachmentsCode((this.Base.GetCurrentAttachmentsCode() & ~conflicting) | addedCode);
            this.Base.ApplyAttachmentsCode(code, false);
            AttachmentCodeSync.ServerSetCode(this.Serial, code);
        }

        /// <summary>
        /// Adds a <see cref="Attachment"/> of the specified <see cref="AttachmentName"/> to the firearm.
        /// </summary>
        /// <param name="attachmentName">The <see cref="AttachmentName"/> to add.</param>
        public void AddAttachment(AttachmentName attachmentName) => this.AddAttachment(AttachmentIdentifier.Get(this.FirearmType, attachmentName));

        /// <summary>
        /// Adds a <see cref="IEnumerable{T}"/> of <see cref="AttachmentIdentifier"/> to the firearm.
        /// </summary>
        /// <param name="identifiers">The <see cref="IEnumerable{T}"/> of <see cref="AttachmentIdentifier"/> to add.</param>
        public void AddAttachment(IEnumerable<AttachmentIdentifier> identifiers)
        {
            foreach (AttachmentIdentifier identifier in identifiers)
                this.AddAttachment(identifier);
        }

        /// <summary>
        /// Adds a <see cref="IEnumerable{T}"/> of <see cref="AttachmentName"/> to the firearm.
        /// </summary>
        /// <param name="attachmentNames">The <see cref="IEnumerable{T}"/> of <see cref="AttachmentName"/> to add.</param>
        public void AddAttachment(IEnumerable<AttachmentName> attachmentNames)
        {
            foreach (AttachmentName attachmentName in attachmentNames)
                this.AddAttachment(attachmentName);
        }

        /// <summary>
        /// Removes a <see cref="AttachmentIdentifier"/> from the firearm.
        /// </summary>
        /// <param name="identifier">The <see cref="AttachmentIdentifier"/> to remove.</param>
        public void RemoveAttachment(AttachmentIdentifier identifier)
        {
            if (!this.Attachments.Any(attachment => (attachment.Name == identifier.Name) && attachment.IsEnabled))
                return;

            uint code = identifier.Code;

            this.Base.ApplyAttachmentsCode(this.Base.GetCurrentAttachmentsCode() & ~code, true);

            // TODO: Not finish
            /*
            if (identifier.Name == AttachmentName.Flashlight)
                Base.Status = new FirearmStatus(Math.Min(Ammo, MaxAmmo), Base.Status.Flags & ~FirearmStatusFlags.FlashlightEnabled, Base.GetCurrentAttachmentsCode());
            else
                Base.Status = new FirearmStatus(Math.Min(Ammo, MaxAmmo), Base.Status.Flags, Base.GetCurrentAttachmentsCode());*/
        }

        /// <summary>
        /// Removes a <see cref="Attachment"/> of the specified <see cref="AttachmentName"/> from the firearm.
        /// </summary>
        /// <param name="attachmentName">The <see cref="AttachmentName"/> to remove.</param>
        public void RemoveAttachment(AttachmentName attachmentName)
        {
            uint code = AttachmentIdentifier.Get(this.FirearmType, attachmentName).Code;

            this.Base.ApplyAttachmentsCode(this.Base.GetCurrentAttachmentsCode() & ~code, true);

            // TODO Not finish
            /*
            if (attachmentName == AttachmentName.Flashlight)
                Base.Status = new FirearmStatus(Math.Min(Ammo, MaxAmmo), Base.Status.Flags & ~FirearmStatusFlags.FlashlightEnabled, Base.GetCurrentAttachmentsCode());
            else
                Base.Status = new FirearmStatus(Math.Min(Ammo, MaxAmmo), Base.Status.Flags, Base.GetCurrentAttachmentsCode());*/
        }

        /// <summary>
        /// Removes a <see cref="Attachment"/> of the specified <see cref="AttachmentSlot"/> from the firearm.
        /// </summary>
        /// <param name="attachmentSlot">The <see cref="AttachmentSlot"/> to remove.</param>
        public void RemoveAttachment(AttachmentSlot attachmentSlot)
        {
            Attachment firearmAttachment = this.Attachments.FirstOrDefault(att => (att.Slot == attachmentSlot) && att.IsEnabled);

            if (firearmAttachment is null)
                return;

            uint code = AvailableAttachments[this.FirearmType].FirstOrDefault(attId => attId == firearmAttachment).Code;

            this.Base.ApplyAttachmentsCode(this.Base.GetCurrentAttachmentsCode() & ~code, true);

            // TODO Not finish
            /*
            if (firearmAttachment.Name == AttachmentName.Flashlight)
                Base.Status = new FirearmStatus(Math.Min(Ammo, MaxAmmo), Base.Status.Flags & ~FirearmStatusFlags.FlashlightEnabled, Base.GetCurrentAttachmentsCode());
            else
                Base.Status = new FirearmStatus(Math.Min(Ammo, MaxAmmo), Base.Status.Flags, Base.GetCurrentAttachmentsCode());*/
        }

        /// <summary>
        /// Removes a <see cref="IEnumerable{T}"/> of <see cref="AttachmentIdentifier"/> from the firearm.
        /// </summary>
        /// <param name="identifiers">The <see cref="IEnumerable{T}"/> of <see cref="AttachmentIdentifier"/> to remove.</param>
        public void RemoveAttachment(IEnumerable<AttachmentIdentifier> identifiers)
        {
            foreach (AttachmentIdentifier identifier in identifiers)
                this.RemoveAttachment(identifier);
        }

        /// <summary>
        /// Removes a list of <see cref="Attachment"/> of the specified <see cref="IEnumerable{T}"/> of <see cref="AttachmentName"/> from the firearm.
        /// </summary>
        /// <param name="attachmentNames">The <see cref="IEnumerable{T}"/> of <see cref="AttachmentName"/> to remove.</param>
        public void RemoveAttachment(IEnumerable<AttachmentName> attachmentNames)
        {
            foreach (AttachmentName attachmentName in attachmentNames)
                this.RemoveAttachment(attachmentName);
        }

        /// <summary>
        /// Removes a list of <see cref="Attachment"/> of the specified <see cref="IEnumerable{T}"/> of <see cref="AttachmentSlot"/> from the firearm.
        /// </summary>
        /// <param name="attachmentSlots">The <see cref="IEnumerable{T}"/> of <see cref="AttachmentSlot"/> to remove.</param>
        public void RemoveAttachment(IEnumerable<AttachmentSlot> attachmentSlots)
        {
            foreach (AttachmentSlot attachmentSlot in attachmentSlots)
                this.RemoveAttachment(attachmentSlot);
        }

        /// <summary>
        /// Removes all attachments from the firearm.
        /// </summary>
        public void ClearAttachments() => this.Base.ApplyAttachmentsCode(this.BaseCode, true);

        /// <summary>
        /// Gets a <see cref="Attachment"/> of the specified <see cref="AttachmentIdentifier"/>.
        /// </summary>
        /// <param name="identifier">The <see cref="AttachmentIdentifier"/> to check.</param>
        /// <returns>The corresponding <see cref="Attachment"/>.</returns>
        public Attachment GetAttachment(AttachmentIdentifier identifier) => this.Attachments.FirstOrDefault(attachment => attachment == identifier);

        /// <summary>
        /// Tries to get a <see cref="Attachment"/> of the specified <see cref="AttachmentIdentifier"/>.
        /// </summary>
        /// <param name="identifier">The <see cref="AttachmentIdentifier"/> to check.</param>
        /// <param name="firearmAttachment">The corresponding <see cref="Attachment"/>.</param>
        /// <returns>A value indicating whether the firearm has the specified <see cref="Attachment"/>.</returns>
        public bool TryGetAttachment(AttachmentIdentifier identifier, out Attachment firearmAttachment)
        {
            firearmAttachment = default;

            if (!this.Attachments.Any(attachment => attachment.Name == identifier.Name))
                return false;

            firearmAttachment = this.GetAttachment(identifier);

            return true;
        }

        /// <summary>
        /// Tries to get a <see cref="Attachment"/> of the specified <see cref="AttachmentName"/>.
        /// </summary>
        /// <param name="attachmentName">The <see cref="AttachmentName"/> to check.</param>
        /// <param name="firearmAttachment">The corresponding <see cref="Attachment"/>.</param>
        /// <returns>A value indicating whether the firearm has the specified <see cref="Attachment"/>.</returns>
        public bool TryGetAttachment(AttachmentName attachmentName, out Attachment firearmAttachment)
        {
            firearmAttachment = default;

            if (this.Attachments.All(attachment => attachment.Name != attachmentName))
                return false;

            firearmAttachment = this.GetAttachment(AttachmentIdentifier.Get(this.FirearmType, attachmentName));

            return true;
        }

        /// <summary>
        /// Adds or replaces an existing preference to the <see cref="PlayerPreferences"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> of which must be added.</param>
        /// <param name="itemType">The <see cref="Enums.FirearmType"/> to add.</param>
        /// <param name="attachments">The <see cref="AttachmentIdentifier"/>[] to add.</param>
        public void AddPreference(Player player, FirearmType itemType, AttachmentIdentifier[] attachments)
        {
            foreach (KeyValuePair<Player, Dictionary<FirearmType, AttachmentIdentifier[]>> kvp in PlayerPreferences)
            {
                if (kvp.Key != player)
                    continue;

                if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(player.ReferenceHub, out Dictionary<ItemType, uint> dictionary))
                    dictionary[itemType.GetItemType()] = attachments.GetAttachmentsCode();
            }
        }

        /// <summary>
        /// Adds or replaces an existing preference to the <see cref="PlayerPreferences"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> of which must be added.</param>
        /// <param name="preference">The <see cref="KeyValuePair{TKey, TValue}"/> of <see cref="Enums.FirearmType"/> and <see cref="AttachmentIdentifier"/>[] to add.</param>
        public void AddPreference(Player player, KeyValuePair<FirearmType, AttachmentIdentifier[]> preference) => this.AddPreference(player, preference.Key, preference.Value);

        /// <summary>
        /// Adds or replaces an existing preference to the <see cref="PlayerPreferences"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> of which must be added.</param>
        /// <param name="preference">The <see cref="Dictionary{TKey, TValue}"/> of <see cref="Enums.FirearmType"/> and <see cref="AttachmentIdentifier"/>[] to add.</param>
        public void AddPreference(Player player, Dictionary<FirearmType, AttachmentIdentifier[]> preference)
        {
            foreach (KeyValuePair<FirearmType, AttachmentIdentifier[]> kvp in preference)
                this.AddPreference(player, kvp);
        }

        /// <summary>
        /// Adds or replaces an existing preference to the <see cref="PlayerPreferences"/>.
        /// </summary>
        /// <param name="players">The <see cref="IEnumerable{T}"/> of <see cref="Player"/> of which must be added.</param>
        /// <param name="type">The <see cref="Enums.FirearmType"/> to add.</param>
        /// <param name="attachments">The <see cref="AttachmentIdentifier"/>[] to add.</param>
        public void AddPreference(IEnumerable<Player> players, FirearmType type, AttachmentIdentifier[] attachments)
        {
            foreach (Player player in players)
                this.AddPreference(player, type, attachments);
        }

        /// <summary>
        /// Adds or replaces an existing preference to the <see cref="PlayerPreferences"/>.
        /// </summary>
        /// <param name="players">The <see cref="IEnumerable{T}"/> of <see cref="Player"/> of which must be added.</param>
        /// <param name="preference">The <see cref="KeyValuePair{TKey, TValue}"/> of <see cref="Enums.FirearmType"/> and <see cref="AttachmentIdentifier"/>[] to add.</param>
        public void AddPreference(IEnumerable<Player> players, KeyValuePair<FirearmType, AttachmentIdentifier[]> preference)
        {
            foreach (Player player in players)
                this.AddPreference(player, preference.Key, preference.Value);
        }

        /// <summary>
        /// Adds or replaces an existing preference to the <see cref="PlayerPreferences"/>.
        /// </summary>
        /// <param name="players">The <see cref="IEnumerable{T}"/> of <see cref="Player"/> of which must be added.</param>
        /// <param name="preference">The <see cref="Dictionary{TKey, TValue}"/> of <see cref="Enums.FirearmType"/> and <see cref="AttachmentIdentifier"/>[] to add.</param>
        public void AddPreference(IEnumerable<Player> players, Dictionary<FirearmType, AttachmentIdentifier[]> preference)
        {
            foreach ((Player player, KeyValuePair<FirearmType, AttachmentIdentifier[]> kvp) in players.SelectMany(player => preference.Select(kvp => (player, kvp))))
                this.AddPreference(player, kvp);
        }

        /// <summary>
        /// Removes a preference from the <see cref="PlayerPreferences"/> if it already exists.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> of which must be removed.</param>
        /// <param name="type">The <see cref="Enums.FirearmType"/> to remove.</param>
        public void RemovePreference(Player player, FirearmType type)
        {
            foreach (KeyValuePair<Player, Dictionary<FirearmType, AttachmentIdentifier[]>> kvp in PlayerPreferences)
            {
                if (kvp.Key != player)
                    continue;

                if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(player.ReferenceHub, out Dictionary<ItemType, uint> dictionary))
                    dictionary[type.GetItemType()] = type.GetBaseCode();
            }
        }

        /// <summary>
        /// Removes a preference from the <see cref="PlayerPreferences"/> if it already exists.
        /// </summary>
        /// <param name="players">The <see cref="IEnumerable{T}"/> of <see cref="Player"/> of which must be removed.</param>
        /// <param name="type">The <see cref="Enums.FirearmType"/> to remove.</param>
        public void RemovePreference(IEnumerable<Player> players, FirearmType type)
        {
            foreach (Player player in players)
                this.RemovePreference(player, type);
        }

        /// <summary>
        /// Removes a preference from the <see cref="PlayerPreferences"/> if it already exists.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> of which must be removed.</param>
        /// <param name="types">The <see cref="IEnumerable{T}"/> of <see cref="Enums.FirearmType"/> to remove.</param>
        public void RemovePreference(Player player, IEnumerable<FirearmType> types)
        {
            foreach (FirearmType itemType in types)
                this.RemovePreference(player, itemType);
        }

        /// <summary>
        /// Removes a preference from the <see cref="PlayerPreferences"/> if it already exists.
        /// </summary>
        /// <param name="players">The <see cref="IEnumerable{T}"/> of <see cref="Player"/> of which must be removed.</param>
        /// <param name="types">The <see cref="IEnumerable{T}"/> of <see cref="Enums.FirearmType"/> to remove.</param>
        public void RemovePreference(IEnumerable<Player> players, IEnumerable<FirearmType> types)
        {
            foreach ((Player player, FirearmType firearmType) in players.SelectMany(player => types.Select(itemType => (player, itemType))))
                this.RemovePreference(player, firearmType);
        }

        /// <summary>
        /// Clears all the existing preferences from <see cref="PlayerPreferences"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> of which must be cleared.</param>
        public void ClearPreferences(Player player)
        {
            if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(player.ReferenceHub, out Dictionary<ItemType, uint> dictionary))
            {
                foreach (KeyValuePair<ItemType, uint> kvp in dictionary)
                    dictionary[kvp.Key] = kvp.Key.GetFirearmType().GetBaseCode();
            }
        }

        /// <summary>
        /// Clears all the existing preferences from <see cref="PlayerPreferences"/>.
        /// </summary>
        /// <param name="players">The <see cref="IEnumerable{T}"/> of <see cref="Player"/> of which must be cleared.</param>
        public void ClearPreferences(IEnumerable<Player> players)
        {
            foreach (Player player in players)
                this.ClearPreferences(player);
        }

        /// <summary>
        /// Clears all the existing preferences from <see cref="PlayerPreferences"/>.
        /// </summary>
        public void ClearPreferences()
        {
            foreach (Player player in Player.List)
                this.ClearPreferences(player);
        }

        /// <summary>
        /// Reloads current <see cref="Firearm"/>.
        /// </summary>
        /// <remarks>
        /// For specific reloading logic you also can use <see cref="NormalMagazine"/> for avaible weapons.
        /// </remarks>
        public void Reload()
        {
            if (this.AnimatorReloaderModule == null)
                return;

            this.AnimatorReloaderModule.IsReloading = true;
            this.AnimatorReloaderModule.SendRpcHeaderWithRandomByte(ReloaderMessageHeader.Reload);
        }

        /// <summary>
        /// Attempts to reload the firearm with server-side validation.
        /// </summary>
        /// <returns><see langword="true"/> if the firearm was successfully reloaded. Otherwise, <see langword="false"/>.</returns>
        public bool TryReload()
        {
            if (this.AnimatorReloaderModule == null)
                return false;

            return this.AnimatorReloaderModule.ServerTryReload();
        }

        /// <summary>
        /// Attempts to unload the firearm with server-side validation.
        /// </summary>
        /// <returns><see langword="true"/> if the firearm was successfully unload. Otherwise, <see langword="false"/>.</returns>
        public bool TryUnload()
        {
            if (this.AnimatorReloaderModule == null)
                return false;

            return this.AnimatorReloaderModule.ServerTryUnload();
        }

        /// <summary>
        /// Forces the firearm's client-side unload animation, bypassing server-side checks.
        /// </summary>
        /// <remarks>
        /// This only plays the animation and is not guaranteed to result in a successful unload. For server-validated unloading, use <see cref="Unload"/>.
        /// </remarks>
        public void Unload()
        {
            if (this.AnimatorReloaderModule == null)
                return;

            this.AnimatorReloaderModule.IsUnloading = true;
            this.AnimatorReloaderModule.SendRpcHeaderWithRandomByte(ReloaderMessageHeader.Unload);
        }

        /// <summary>
        /// Clones current <see cref="Firearm"/> object.
        /// </summary>
        /// <returns> New <see cref="Firearm"/> object. </returns>
        public override Item Clone()
        {
            Firearm cloneableItem = new(this.Type)
            {
            };

            // TODO Not finish
            /*
            if (cloneableItem.Base is AutomaticFirearm)
            {
                cloneableItem.FireRate = FireRate;
                cloneableItem.Recoil = Recoil;
            }*/

            cloneableItem.AddAttachment(this.AttachmentIdentifiers);

            return cloneableItem;
        }

        /// <summary>
        /// Change the owner of the <see cref="Firearm"/>.
        /// </summary>
        /// <param name="oldOwner">old <see cref="Firearm"/> owner.</param>
        /// <param name="newOwner">new <see cref="Firearm"/> owner.</param>
        internal override void ChangeOwner(Player oldOwner, Player newOwner)
        {
            this.Base.InstantiationStatus = newOwner == Server.Host ? AutosyncInstantiationStatus.SimulatedInstance : AutosyncInstantiationStatus.InventoryInstance;
            this.Base.Owner = newOwner.ReferenceHub;
            this.Base._footprintCacheSet = false;
            foreach (ModuleBase module in this.Base.Modules)
            {
                module.OnAdded();
            }
        }

        /// <inheritdoc/>
        internal override void ReadPickupInfoBefore(Pickup pickup)
        {
            base.ReadPickupInfoBefore(pickup);

            if (pickup is FirearmPickup firearmPickup)
            {
                PrimaryMagazine.MaxAmmo = firearmPickup.MaxAmmo;
                AmmoDrain = firearmPickup.AmmoDrain;
                Damage = firearmPickup.Damage;
                Inaccuracy = firearmPickup.Inaccuracy;
                Penetration = firearmPickup.Penetration;
                DamageFalloffDistance = firearmPickup.DamageFalloffDistance;
            }
        }
    }
}
