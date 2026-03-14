// -----------------------------------------------------------------------
// <copyright file="Scp106Role.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Roles
{
    using System.Collections.Generic;

    using Exiled.API.Enums;

    using MEC;

    using PlayerRoles;
    using PlayerRoles.PlayableScps;
    using PlayerRoles.PlayableScps.HumeShield;
    using PlayerRoles.PlayableScps.Scp106;
    using PlayerRoles.Subroutines;
    using PlayerStatsSystem;

    using UnityEngine;

    using Scp106GameRole = PlayerRoles.PlayableScps.Scp106.Scp106Role;

    /// <summary>
    /// Defines a role that represents SCP-106.
    /// </summary>
    public class Scp106Role : FpcRole, ISubroutinedScpRole, IHumeShieldRole, ISpawnableScp
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Scp106Role"/> class.
        /// </summary>
        /// <param name="baseRole">the base <see cref="Scp106GameRole"/>.</param>
        internal Scp106Role(Scp106GameRole baseRole)
            : base(baseRole)
        {
            this.SubroutineModule = baseRole.SubroutineModule;
            this.HumeShieldModule = baseRole.HumeShieldModule;
            this.Base = baseRole;
            this.MovementModule = this.FirstPersonController.FpcModule as Scp106MovementModule;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp106VigorAbilityBase scp106VigorAbilityBase))
                Log.Error("Scp106VigorAbilityBase subroutine not found in Scp106Role::ctor");

            this.VigorAbility = scp106VigorAbilityBase;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp106Attack scp106Attack))
                Log.Error("Scp106Attack subroutine not found in Scp106Role::ctor");

            this.Attack = scp106Attack;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp106StalkAbility scp106StalkAbility))
                Log.Error("Scp106StalkAbility not found in Scp106Role::ctor");

            this.StalkAbility = scp106StalkAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp106HuntersAtlasAbility scp106HuntersAtlasAbility))
                Log.Error("Scp106HuntersAtlasAbility not found in Scp106Role::ctor");

            this.HuntersAtlasAbility = scp106HuntersAtlasAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp106SinkholeController scp106SinkholeController))
                Log.Error("Scp106SinkholeController not found in Scp106Role::ctor");

            this.SinkholeController = scp106SinkholeController;
        }

        /// <inheritdoc/>
        public override RoleTypeId Type { get; } = RoleTypeId.Scp106;

        /// <inheritdoc/>
        public SubroutineManagerModule SubroutineModule { get; }

        /// <summary>
        /// Gets the <see cref="HumeShieldModuleBase"/>.
        /// </summary>
        public HumeShieldModuleBase HumeShieldModule { get; }

        /// <summary>
        /// Gets the <see cref="Scp106VigorAbilityBase"/>.
        /// </summary>
        public Scp106VigorAbilityBase VigorAbility { get; }

        /// <summary>
        /// Gets the <see cref="VigorStat"/>.
        /// </summary>
        public VigorStat VigorComponent => this.VigorAbility.Vigor;

        /// <summary>
        /// Gets the <see cref="Scp106Attack"/>.
        /// </summary>
        public Scp106Attack Attack { get; }

        /// <summary>
        /// Gets the <see cref="Scp106Attack"/>.
        /// </summary>
        public Scp106StalkAbility StalkAbility { get; }

        /// <summary>
        /// Gets the <see cref="Scp106HuntersAtlasAbility"/>.
        /// </summary>
        public Scp106HuntersAtlasAbility HuntersAtlasAbility { get; }

        /// <summary>
        /// Gets the <see cref="Scp106SinkholeController"/>.
        /// </summary>
        public Scp106SinkholeController SinkholeController { get; }

        /// <summary>
        /// Gets the <see cref="Scp106MovementModule"/>.
        /// </summary>
        public Scp106MovementModule MovementModule { get; }

        /// <summary>
        /// Gets a value indicating whether .
        /// </summary>
        public bool RespectPreferences => true;

        /// <summary>
        /// Gets a value indicating whether .
        /// </summary>
        public bool AllowFallback => true;

        /// <summary>
        /// Gets or sets SCP-106's Vigor Level.
        /// </summary>
        public float Vigor
        {
            get => this.VigorAbility.VigorAmount;
            set => this.VigorAbility.VigorAmount = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether SCP-106 is currently submerged.
        /// </summary>
        public bool IsSubmerged
        {
            get => this.HuntersAtlasAbility._syncSubmerged;
            set
            {
                this.HuntersAtlasAbility._syncSubmerged = value;
                this.HuntersAtlasAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets a value indicating whether SCP-106 can activate teslas.
        /// </summary>
        public bool CanActivateTesla => this.Base.CanActivateShock;

        /// <summary>
        /// Gets a value indicating whether if SCP-106 <see cref="Scp106StalkAbility"/> can be cleared.
        /// </summary>
        public bool CanStopStalk => this.StalkAbility.CanBeCleared;

        /// <summary>
        /// Gets a value indicating whether SCP-106 is currently slow down by a door.
        /// </summary>
        public bool IsSlowdown => this.MovementModule._slowndownTarget is < 1;

        /// <summary>
        /// Gets a value indicating the current time of the sinkhole.
        /// </summary>
        public float SinkholeCurrentTime => this.SinkholeController.ElapsedToggle;

        /// <summary>
        /// Gets a value indicating whether SCP-106 is currently in the middle of an animation.
        /// </summary>
        public bool IsDuringAnimation => this.SinkholeController.IsDuringAnimation;

        /// <summary>
        /// Gets a value indicating whether SCP-106 sinkhole is hidden.
        /// </summary>
        public bool IsSinkholeHidden => this.SinkholeController.IsHidden;

        /// <summary>
        /// Gets or sets a value indicating whether the current sinkhole state.
        /// </summary>
        public bool SinkholeState
        {
            get => this.StalkAbility.StalkActive;
            set => this.StalkAbility.ServerSetStalk(value);
        }

        /// <summary>
        /// Gets the sinkhole target duration.
        /// </summary>
        public float SinkholeTargetDuration => this.SinkholeController.TargetTransitionDuration;

        /// <summary>
        /// Gets or sets how mush damage Scp106 will dealt when attacking a player.
        /// </summary>
        public int AttackDamage
        {
            get => this.Attack._damage;
            set => this.Attack._damage = value;
        }

        /// <summary>
        /// Gets or sets the amount of time in between player captures.
        /// </summary>
        public float CaptureCooldown
        {
            get => this.Attack._hitCooldown;
            set
            {
                this.Attack._hitCooldown = value;
                this.Attack.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the Sinkhole cooldown.
        /// </summary>
        public float RemainingSinkholeCooldown
        {
            get => this.SinkholeController._submergeCooldown.Remaining;
            set
            {
                this.SinkholeController._submergeCooldown.Remaining = value;
                this.SinkholeController.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether SCP-106 will enter his stalking mode.
        /// </summary>
        public bool IsStalking
        {
            get => this.StalkAbility.StalkActive;
            set => this.StalkAbility.ServerSetStalk(value);
        }

        /// <summary>
        /// Gets the <see cref="Scp106GameRole"/>.
        /// </summary>
        public new Scp106GameRole Base { get; }

        /// <summary>
        /// Forces SCP-106 to use its portal, and Teleport to position.
        /// </summary>
        /// <param name="position">Where the player will be teleported.</param>
        /// <param name="cost">The amount of vigor that is required and will be consumed.</param>
        /// <returns>If the player will be teleport.</returns>
        public bool UsePortal(Vector3 position, float cost = 0f)
        {
            if (Room.Get(position) is not Room room)
                return false;

            this.HuntersAtlasAbility._syncRoom = room.Identifier;
            this.HuntersAtlasAbility._syncPos = position;

            if (this.Vigor < cost)
                return false;

            this.HuntersAtlasAbility._estimatedCost = cost;
            this.HuntersAtlasAbility._syncSubmerged = true;

            Timing.CallDelayed(2f, () =>
            {
                if (this.IsValid)
                    this.Owner.Position = position;
            });

            return true;
        }

        /// <summary>
        /// Send a player to the pocket dimension.
        /// </summary>
        /// <param name="player">The <see cref="Player"/>to send.</param>
        /// <returns>If the player will be capture.</returns>
        public bool CapturePlayer(Player player)
        {
            if (player is null)
                return false;
            this.Attack._targetHub = player.ReferenceHub;
            DamageHandlerBase handler = new ScpDamageHandler(this.Attack.Owner, this.AttackDamage, DeathTranslations.PocketDecay);

            if (!this.Attack._targetHub.playerStats.DealDamage(handler))
                return false;

            this.Attack.SendCooldown(this.Attack._hitCooldown);
            this.Vigor += Scp106Attack.VigorCaptureReward;
            this.Attack.ReduceSinkholeCooldown();
            Hitmarker.SendHitmarkerDirectly(this.Attack.Owner, 1f);

            player.EnableEffect(EffectType.PocketCorroding);
            return true;
        }

        /// <summary>
        /// Gets the Spawn Chance of SCP-106.
        /// </summary>
        /// <param name="alreadySpawned">The List of Roles already spawned.</param>
        /// <returns>The Spawn Chance.</returns>
        public float GetSpawnChance(List<RoleTypeId> alreadySpawned) => this.Base.GetSpawnChance(alreadySpawned);
    }
}
