// -----------------------------------------------------------------------
// <copyright file="CustomWeapon.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.API.Features
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.DamageHandlers;
    using Exiled.API.Features.Items;
    using Exiled.API.Structs;
    using Exiled.CustomItems.API.Models;
    using Exiled.Events.EventArgs.Item;
    using Exiled.Events.EventArgs.Player;
    using InventorySystem.Items.Firearms.Attachments;
    using InventorySystem.Items.Firearms.Attachments.Components;
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Features.Wrappers;
    using MEC;
    using PlayerRoles;

    /// <summary>
    ///     The Custom Weapon base class.
    /// </summary>
    public abstract class CustomWeapon : CustomItem
    {
        /// <inheritdoc />
        public override ItemType Type
        {
            get => base.Type;
            set
            {
                if (!value.IsWeapon(false) && value != ItemType.None)
                    throw new ArgumentOutOfRangeException($"{nameof(this.Type)}", value, "Invalid weapon type.");

                base.Type = value;
            }
        }

        /// <summary>
        /// Gets or sets value indicating what <see cref="Attachment" />s the weapon will have.
        /// </summary>
        public virtual AttachmentName[] Attachments { get; set; } = { };

        /// <summary>
        /// Gets or sets value indicating what <see cref="AttachmentName" />s the weapon wont have.
        /// </summary>
        public virtual AttachmentName[] BannedAttachments { get; set; } = { };

        /// <summary>
        ///     Gets or sets the weapon damage.
        /// </summary>
        public abstract float Damage { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating how big of a clip the weapon will have.
        /// </summary>
        public virtual byte ClipSize { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating how many ammo will be spent per shot.
        /// </summary>
        public virtual byte AmmoUsage { get; set; } = 1;

        /// <summary>
        ///     Gets or sets a value indicating whether firearm can be unloaded.
        /// </summary>
        public virtual bool CanUnload { get; set; } = true;

        /// <summary>
        ///     Gets or sets a value indicating whether firearm's attachments can be modified.
        /// </summary>
        public bool AllowAttachmentsChange { get; set; } = true;

        /// <summary>
        ///     Gets or sets a value indicating shot cooldown.
        /// </summary>
        [Description("Кулдаун на выстрелы. Работает только при ClipSize > 1 и FireCooldown > 0. -1 для отключения.")]
        public float FireCooldown { get; set; } = -1;

        /// <summary>
        ///     Gets or sets a value indicating message, displayed to players, trying to reload cooldowned weapon.
        /// </summary>
        [Description("Сообщение при попытке перезарядить оружие под кулдауном. {0} - кулдаун из конфига")]
        public string WeaponNotReady { get; set; } = "Оружие ещё не готово к выстрелу! Оно может стрелять только раз в {0} секунд.";

        /// <summary>
        ///     Gets or sets a value indicating damage multipliers by ArmorType and HitboxType.
        /// </summary>
        [Description("Множители урона в зависимости от брони и точки попадания. Словарь ТипБрони: (ЗонаПопадания: МножительУрона)")]
        public Dictionary<ItemType, Dictionary<HitboxType, float>> ArmorAndZoneDamageMultipliers { get; set; } = new()
        {
            [ItemType.None] = new Dictionary<HitboxType, float>
            {
                [HitboxType.Headshot] = 1,
                [HitboxType.Limb] = 1,
                [HitboxType.Body] = 1,
            },
            [ItemType.ArmorLight] = new Dictionary<HitboxType, float>
            {
                [HitboxType.Headshot] = 1,
            },
            [ItemType.ArmorCombat] = new Dictionary<HitboxType, float>
            {
                [HitboxType.Headshot] = 1,
            },
            [ItemType.ArmorHeavy] = new Dictionary<HitboxType, float>
            {
                [HitboxType.Headshot] = 1,
            },
        };

        /// <summary>
        ///     Gets or sets a value indicating  damage multipliers by target RoleTypeId.
        /// </summary>
        [Description("Множители урона для ролей. Словарь RoleType: МножительУрона")]
        public Dictionary<RoleTypeId, float> RoleDamageMultipliers { get; set; } = new()
        {
            { RoleTypeId.Scp096, 1 },
            { RoleTypeId.Scp173, 1 },
        };

        /// <summary>
        /// Gets or sets a value indicating whether firearm's will be reset after shot.
        /// </summary>
        [Description("Будет ли оружие убрано-возвращено в руки после выстрела")]
        public bool ForceResetWeaponOnShot { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether firearm's will be double shot.
        /// </summary>
        [Description("При использовании КД выстрелов, разрешать ли двойной")]
        public bool AllowDoubleShot { get; set; } = false;

        /// <inheritdoc />
        public override Exiled.API.Features.Items.Item CreateItem()
        {
            Exiled.API.Features.Items.Item item = base.CreateItem();

            if (item is Firearm firearm)
            {
                if (!this.Attachments.IsEmpty())
                    firearm.AddAttachment(this.Attachments);

                firearm.MagazineAmmo = firearm.MaxMagazineAmmo = this.ClipSize;
                firearm.AmmoDrain = this.AmmoUsage;
            }

            return item;
        }

        /// <inheritdoc />
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ReloadingWeapon += this.OnInternalReloading;
            LabApi.Events.Handlers.PlayerEvents.ShotWeapon += this.OnInternalShot;
            Exiled.Events.Handlers.Player.Hurting += this.OnInternalHurting;
            Exiled.Events.Handlers.Player.UnloadingWeapon += this.OnInternalUnloading;
            Exiled.Events.Handlers.Item.ChangingAttachments += this.OnInternalChangingAttachments;

            base.SubscribeEvents();
        }

        /// <inheritdoc />
        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ReloadingWeapon -= this.OnInternalReloading;
            LabApi.Events.Handlers.PlayerEvents.ShotWeapon -= this.OnInternalShot;
            Exiled.Events.Handlers.Player.Hurting -= this.OnInternalHurting;
            Exiled.Events.Handlers.Player.UnloadingWeapon -= this.OnInternalUnloading;
            Exiled.Events.Handlers.Item.ChangingAttachments -= this.OnInternalChangingAttachments;

            base.UnsubscribeEvents();
        }

        /// <summary>
        ///     Handles reloading for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="ReloadingWeaponEventArgs" />.</param>
        protected virtual void OnReloading(ReloadingWeaponEventArgs ev)
        {
        }

        /// <summary>
        /// Handles reloaded for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="ReloadedWeaponEventArgs"/>.</param>
        protected virtual void OnReloaded(ReloadedWeaponEventArgs ev)
        {
        }

        /// <summary>
        /// Handles shooting for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="ShootingEventArgs" />.</param>
        protected virtual void OnShooting(PlayerShootingWeaponEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles shot for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="ShotEventArgs" />.</param>
        protected virtual void OnShot(PlayerShotWeaponEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles hurting for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="HurtingEventArgs" />.</param>
        protected virtual void OnHurting(HurtingEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles unloading for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="HurtingEventArgs" />.</param>
        protected virtual void OnUnloading(UnloadingWeaponEventArgs ev)
        {
        }

        private void OnInternalChangingAttachments(ChangingAttachmentsEventArgs ev)
        {
            if (!this.Check(ev.Player.CurrentItem))
                return;

            IEnumerable<AttachmentIdentifier> newAttachments = ev.NewAttachmentIdentifiers.Except(ev.CurrentAttachmentIdentifiers);
            if (!this.AllowAttachmentsChange || newAttachments.Any(x => this.BannedAttachments.Contains(x.Name)))
                ev.IsAllowed = false;
        }

        private void OnInternalReloading(ReloadingWeaponEventArgs ev)
        {
            if (!this.Check(ev.Player.CurrentItem))
                return;

            if (!ev.Firearm.Base.gameObject.TryGetComponent(out CockedController controller))
            {
                controller = ev.Firearm.Base.gameObject.AddComponent<CockedController>();
                controller.Init(FirearmItem.Get(ev.Firearm.Base), this);
            }

            if (controller.IsOnCooldown)
            {
                ev.IsAllowed = false;
                ev.Player.ShowHint(string.Format(this.WeaponNotReady, this.FireCooldown));
                return;
            }

            Log.Debug($"{nameof(this.Name)}.{nameof(this.OnInternalReloading)}: Reloading weapon. Calling external reload event..");
            this.OnReloading(ev);

            Log.Debug($"{nameof(this.Name)}.{nameof(this.OnInternalReloading)}: External event ended. {ev.IsAllowed}");
        }

        private void OnInternalShot(PlayerShotWeaponEventArgs ev)
        {
            Exiled.API.Features.Items.Item curItem = Exiled.API.Features.Items.Item.Get(ev.Player.CurrentItem?.Base);
            if (!this.Check(curItem))
                return;

            if (!ev.FirearmItem.Base.gameObject.TryGetComponent(out CockedController controller))
            {
                controller = ev.FirearmItem.Base.gameObject.AddComponent<CockedController>();
                controller.Init(ev.FirearmItem, this);
                Log.Debug("Инициализация после выстрела");
            }

            controller.ProcessShot();
            this.OnShot(ev);
            if (this.ForceResetWeaponOnShot)
                Timing.RunCoroutine(this.ResetWeapon(ev.Player));
        }

        private IEnumerator<float> ResetWeapon(Exiled.API.Features.Player player)
        {
            Exiled.API.Features.Items.Item curItem = player.CurrentItem;
            yield return Timing.WaitForSeconds(0.01f);
            player.CurrentItem = null;
            yield return Timing.WaitForSeconds(0.08f);
            player.CurrentItem = curItem;
        }

        private void OnInternalHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker is null || ev.Player is null || ev.Attacker == ev.Player || !this.Check(ev.Attacker.CurrentItem) || ev.DamageHandler == null)
                return;

            if (!ev.DamageHandler.CustomBase.BaseIs(out FirearmDamageHandler firearmDamageHandler))
            {
                Log.Debug($"{this.Name}: {nameof(this.OnInternalHurting)}: Handler not firearm");
                return;
            }

            if (!this.Check(firearmDamageHandler.Item))
            {
                Log.Debug($"{this.Name}: {nameof(this.OnInternalHurting)}: type != type");
                return;
            }

            ev.Amount = this.Damage;
            if (ev.Player.IsHuman && this.ArmorAndZoneDamageMultipliers.TryGetValue(ev.Player.CurrentArmor?.Type ?? ItemType.None, out Dictionary<HitboxType, float> dic) &&
                dic.TryGetValue(firearmDamageHandler.Hitbox, out float multiplier))
            {
                Log.Debug($"{this.Name}: {nameof(this.OnInternalHurting)}: Found damage muptiplier for armor/hitbox {multiplier}");
                ev.Amount *= multiplier;
            }

            if (this.RoleDamageMultipliers.TryGetValue(ev.Player.Role.Type, out multiplier))
            {
                Log.Debug($"{this.Name}: {nameof(this.OnInternalHurting)}: Found damage muptiplier for target role: {multiplier}");
                ev.Amount *= multiplier;
            }

            this.OnHurting(ev);
        }

        private void OnInternalUnloading(UnloadingWeaponEventArgs ev)
        {
            if (!this.Check(ev.Firearm))
                return;

            ev.IsAllowed = this.CanUnload;

            this.OnUnloading(ev);
        }
    }
}
