// -----------------------------------------------------------------------
// <copyright file="Scp127.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Items
{
    using System.Collections.Generic;
    using System.Linq;

    using InventorySystem.Items.Firearms.Modules;
    using InventorySystem.Items.Firearms.Modules.Scp127;
    using UnityEngine;

    /// <summary>
    /// Represents SCP-127.
    /// </summary>
    public class Scp127 : Firearm
    {
        #pragma warning disable SA1401
        /// <summary>
        /// Custom amount of max HS.
        /// </summary>
        internal float? CustomHsMax;
        #pragma warning restore SA1401

        /// <summary>
        /// Initializes a new instance of the <see cref="Scp127"/> class.
        /// </summary>
        /// <param name="itemBase"><inheritdoc cref="Firearm.Base"/></param>
        public Scp127(InventorySystem.Items.Firearms.Firearm itemBase)
            : base(itemBase)
        {
            foreach (ModuleBase module in this.Base.Modules)
            {
                switch (module)
                {
                    case Scp127HumeModule humeModule:
                        this.HumeModule = humeModule;
                        break;
                    case Scp127TierManagerModule tierManagerModule:
                        this.TierManagerModule = tierManagerModule;
                        break;
                    case Scp127VoiceLineManagerModule voiceLineManagerModule:
                        this.VoiceLineManagerModule = voiceLineManagerModule;
                        break;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scp127"/> class.
        /// </summary>
        internal Scp127()
            : this((InventorySystem.Items.Firearms.Firearm)Server.Host.Inventory.CreateItemInstance(new(ItemType.GunSCP127, 0), false))
        {
        }

        /// <summary>
        /// Gets a collection of all active HS sessions.
        /// </summary>
        public static List<Scp127HumeModule.HumeShieldSession> ActiveHumeShieldSessions => Scp127HumeModule.ServerActiveSessions;

        /// <summary>
        /// Gets the <see cref="Scp127HumeModule"/> instance.
        /// </summary>
        public Scp127HumeModule HumeModule { get; }

        /// <summary>
        /// Gets the <see cref="Scp127TierManagerModule"/> instance.
        /// </summary>
        public Scp127TierManagerModule TierManagerModule { get; }

        /// <summary>
        /// Gets the <see cref="Scp127VoiceLineManagerModule"/> instance.
        /// </summary>
        public Scp127VoiceLineManagerModule VoiceLineManagerModule { get; }

        /// <summary>
        /// Gets or sets the maximum amount of HS.
        /// </summary>
        /// <remarks>If setter is used, after tier chance this value won't be edited automatically.</remarks>
        public float HsMax
        {
            get => this.CustomHsMax ?? this.HumeModule.HsMax;
            set => this.CustomHsMax = value;
        }

        /// <summary>
        /// Gets or sets a shield regeneration rate.
        /// </summary>
        public float ShieldRegenRate
        {
            get => this.HumeModule.ShieldRegenRate;
            set => this.HumeModule.ShieldRegenRate = value;
        }

        /// <summary>
        /// Gets or sets a shield decay time.
        /// </summary>
        public float ShieldDecayRate
        {
            get => this.HumeModule.ShieldDecayRate;
            set => this.HumeModule.ShieldDecayRate = value;
        }

        /// <summary>
        /// Gets or sets a pause before HS starts regeneration after damage being taken.
        /// </summary>
        public float ShieldOnDamagePause
        {
            get => this.HumeModule.ShieldOnDamagePause;
            set => this.HumeModule.ShieldOnDamagePause = value;
        }

        /// <summary>
        /// Gets or sets a delay before HS starts dropping after unequipment.
        /// </summary>
        public float UnequipDecayDelay
        {
            get => this.HumeModule.UnequipDecayDelay;
            set => this.HumeModule.UnequipDecayDelay = value;
        }

        /// <summary>
        /// Gets or sets a HumeShield regeneration.
        /// </summary>
        public float HsRegeneration
        {
            get => this.HumeModule.HsRegeneration;
            set => this.HumeModule.HsRegeneration = value;
        }

        /// <summary>
        /// Gets or sets an experience bonus for kill.
        /// </summary>
        public float KillBonus
        {
            get => this.TierManagerModule.KillBonus;
            set => this.TierManagerModule.KillBonus = value;
        }

        /// <summary>
        /// Gets or sets an amount of passive experience increase.
        /// </summary>
        public float PassiveExpAmount
        {
            get => this.TierManagerModule.PassiveExpAmount;
            set => this.TierManagerModule.PassiveExpAmount = value;
        }

        /// <summary>
        /// Gets or sets an interval of passive experience increase.
        /// </summary>
        public float PassiveExpInterval
        {
            get => this.TierManagerModule.PassiveExpInterval;
            set => this.TierManagerModule.PassiveExpInterval = value;
        }

        /// <summary>
        /// Gets or sets a collection of all tier thresholds.
        /// </summary>
        public Scp127TierManagerModule.TierThreshold[] TierThresholds
        {
            get => this.TierManagerModule.Thresholds;
            set => this.TierManagerModule.Thresholds = value;
        }

        /// <summary>
        /// Gets or sets current tier.
        /// </summary>
        public Scp127Tier CurrentTier
        {
            get => this.TierManagerModule.CurTier;
            set => this.TierManagerModule.CurTier = value;
        }

        /// <summary>
        /// Gets or sets the current experience amount.
        /// </summary>
        public float Experience
        {
            get => this.TierManagerModule.ServerExp;
            set => this.TierManagerModule.ServerExp = value;
        }

        /// <summary>
        /// Gets the instance record.
        /// </summary>
        public Scp127TierManagerModule.InstanceRecord InstanceRecord => Scp127TierManagerModule.GetRecord(this.Serial);

        /// <summary>
        /// Gets the Owner stats.
        /// </summary>
        public Scp127TierManagerModule.OwnerStats OwnerStats => Scp127TierManagerModule.GetStats(this.Base);

        /// <summary>
        /// Gets or sets all Voice Triggers.
        /// </summary>
        public Scp127VoiceTriggerBase[] VoiceTriggers
        {
            get => this.VoiceLineManagerModule._foundTriggers;
            set => this.VoiceLineManagerModule._foundTriggers = value;
        }

        /// <summary>
        /// Gets a collection of players that are friends with this SCP-127.
        /// </summary>
        public IEnumerable<Player> Friends
        {
            get
            {
                if (!Scp127VoiceLineManagerModule.FriendshipMemory.TryGetValue(this.Serial, out HashSet<uint> uintSet))
                    return null;

                return uintSet.Select(Player.Get);
            }
        }

        /// <summary>
        /// Increases experience.
        /// </summary>
        /// <param name="amount">Amount to add.</param>
        public void IncreaseExperience(float amount) => this.TierManagerModule.ServerIncreaseExp(this.Base, amount);

        /// <summary>
        /// Sets owner stats.
        /// </summary>
        /// <param name="exp">New experience amount.</param>
        public void SetOwnerStats(float exp) => this.TierManagerModule.ServerSetStats(exp);

        /// <summary>
        /// Sends tier stats.
        /// </summary>
        /// <param name="tier">New tier.</param>
        /// <param name="progress">New progress.</param>
        public void SendTierStats(Scp127Tier tier, byte progress) => this.TierManagerModule.ServerSendStats(this.Serial, this.Owner.ReferenceHub, tier, progress);

        /// <summary>
        /// Tries to play voice line.
        /// </summary>
        /// <param name="voiceLine">Voice line to play.</param>
        /// <param name="priority">Priority of the play.</param>
        /// <returns><c>true</c> if voice line has been played successfully. Otherwise, <c>false</c>.</returns>
        public bool TryPlayVoiceLine(Scp127VoiceLinesTranslation voiceLine, Scp127VoiceTriggerBase.VoiceLinePriority priority = Scp127VoiceTriggerBase.VoiceLinePriority.Normal)
        {
            if (!this.VoiceLineManagerModule.TryFindVoiceLine(voiceLine, out Scp127VoiceTriggerBase triggerBase, out AudioClip audioClip))
                return false;

            this.VoiceLineManagerModule.ServerSendVoiceLine(triggerBase, null, audioClip, (byte)priority);
            return true;
        }

        /// <summary>
        /// Checks if this instance of SCP-127 and <paramref name="player"/> have friendship.
        /// </summary>
        /// <param name="player">Target to check.</param>
        /// <returns><c>true</c> if this instance of SCP-127 and <paramref name="player"/> have friendship. Otherwise, <c>false</c>.</returns>
        public bool HasFriendship(Player player) => Scp127VoiceLineManagerModule.HasFriendship(this.Serial, player.ReferenceHub);

        /// <summary>
        /// Adds player as a friend.
        /// </summary>
        /// <param name="player">Target to be added.</param>
        public void AddFriend(Player player)
        {
            HashSet<uint> uints = Scp127VoiceLineManagerModule.FriendshipMemory.GetOrAddNew(this.Serial);
            uints.Add(player.NetId);
        }
    }
}