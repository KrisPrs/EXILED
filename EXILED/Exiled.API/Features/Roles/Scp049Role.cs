// -----------------------------------------------------------------------
// <copyright file="Scp049Role.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Roles
{
    using System.Collections.Generic;
    using System.Linq;

    using CustomPlayerEffects;
    using PlayerRoles;
    using PlayerRoles.PlayableScps;
    using PlayerRoles.PlayableScps.HumeShield;
    using PlayerRoles.PlayableScps.Scp049;
    using PlayerRoles.Ragdolls;
    using PlayerRoles.Subroutines;
    using PlayerStatsSystem;
    using UnityEngine;

    using Scp049GameRole = PlayerRoles.PlayableScps.Scp049.Scp049Role;

    /// <summary>
    /// Defines a role that represents SCP-049.
    /// </summary>
    public class Scp049Role : FpcRole, ISubroutinedScpRole, IHumeShieldRole, ISpawnableScp
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Scp049Role"/> class.
        /// </summary>
        /// <param name="baseRole">the base <see cref="Scp049GameRole"/>.</param>
        internal Scp049Role(Scp049GameRole baseRole)
            : base(baseRole)
        {
            this.Base = baseRole;
            this.SubroutineModule = baseRole.SubroutineModule;
            this.HumeShieldModule = baseRole.HumeShieldModule;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp049ResurrectAbility scp049ResurrectAbility))
                Log.Error("Scp049ResurrectAbility subroutine not found in Scp049Role::ctor");

            this.ResurrectAbility = scp049ResurrectAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp049CallAbility scp049CallAbility))
                Log.Error("Scp049CallAbility subroutine not found in Scp049Role::ctor");

            this.CallAbility = scp049CallAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp049SenseAbility scp049SenseAbility))
                Log.Error("Scp049SenseAbility subroutine not found in Scp049Role::ctor");

            this.SenseAbility = scp049SenseAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp049AttackAbility scp049AttackAbility))
                Log.Error("Scp049AttackAbility subroutine not found in Scp049Role::ctor");

            this.AttackAbility = scp049AttackAbility;
        }

        /// <summary>
        /// Gets a list of players who are turned away from SCP-049 Sense Ability.
        /// </summary>
        public static HashSet<Player> TurnedPlayers { get; } = new(20);

        /// <inheritdoc/>
        public override RoleTypeId Type { get; } = RoleTypeId.Scp049;

        /// <inheritdoc/>
        public SubroutineManagerModule SubroutineModule { get; }

        /// <inheritdoc/>
        public HumeShieldModuleBase HumeShieldModule { get; }

        /// <summary>
        /// Gets SCP-049's <see cref="Scp049ResurrectAbility"/>.
        /// </summary>
        public Scp049ResurrectAbility ResurrectAbility { get; }

        /// <summary>
        /// Gets SCP-049's <see cref="Scp049AttackAbility"/>.
        /// </summary>
        public Scp049AttackAbility AttackAbility { get; }

        /// <summary>
        /// Gets SCP-049's <see cref="Scp049CallAbility"/>.
        /// </summary>
        public Scp049CallAbility CallAbility { get; }

        /// <summary>
        /// Gets SCP-049's <see cref="Scp049SenseAbility"/>.
        /// </summary>
        public Scp049SenseAbility SenseAbility { get; }

        /// <summary>
        /// Gets a value indicating whether SCP-049 is currently reviving a player.
        /// </summary>
        public bool IsRecalling => this.ResurrectAbility.IsInProgress;

        /// <summary>
        /// Gets a value indicating whether SCP-049's "Doctor's Call" ability is currently active.
        /// </summary>
        public bool IsCallActive => this.CallAbility.IsMarkerShown;

        /// <summary>
        /// Gets the player that is currently being revived by SCP-049. Will be <see langword="null"/> if <see cref="IsRecalling"/> is <see langword="false"/>.
        /// </summary>
        public Player RecallingPlayer => this.ResurrectAbility.CurRagdoll == null ? null : Player.Get(this.ResurrectAbility.CurRagdoll.Info.OwnerHub);

        /// <summary>
        /// Gets the ragdoll that is currently being revived by SCP-049. Will be <see langword="null"/> if <see cref="IsRecalling"/> is <see langword="false"/>.
        /// </summary>
        public Ragdoll RecallingRagdoll => Features.Ragdoll.Get(this.ResurrectAbility.CurRagdoll);

        /// <summary>
        /// Gets all the dead zombies.
        /// </summary>
        public IEnumerable<Player> DeadZombies => Scp049ResurrectAbility.DeadZombies.Select(x => Player.Get(x));

        /// <summary>
        /// Gets all the resurrected players.
        /// </summary>
        public Dictionary<Player, int> ResurrectedPlayers => Scp049ResurrectAbility.ResurrectedPlayers.ToDictionary(x => Player.Get(x.Key), x => x.Value);

        /// <summary>
        /// Gets a value indicating whether .
        /// </summary>
        public bool RespectPreferences => true;

        /// <summary>
        /// Gets a value indicating whether .
        /// </summary>
        public bool AllowFallback => true;

        /// <summary>
        /// Gets or sets the amount of time before SCP-049 can use its Doctor's Call ability again.
        /// </summary>
        public float CallCooldown
        {
            get => this.CallAbility.Cooldown.Remaining;
            set
            {
                this.CallAbility.Cooldown.Remaining = value;
                this.CallAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the amount of time before SCP-049 can use its Good Sense of the Doctor ability again.
        /// </summary>
        public float GoodSenseCooldown
        {
            get => this.SenseAbility.Cooldown.Remaining;
            set
            {
                this.SenseAbility.Cooldown.Remaining = value;
                this.SenseAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the amount of time before SCP-049 can attack again.
        /// </summary>
        public float RemainingAttackCooldown
        {
            get => this.AttackAbility.Cooldown.Remaining;
            set
            {
                this.AttackAbility.Cooldown.Remaining = value;
                this.AttackAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the duration of the <see cref="Scp049CallAbility"/>.
        /// </summary>
        public float RemainingCallDuration
        {
            get => this.CallAbility.Duration.Remaining;
            set
            {
                this.CallAbility.Duration.Remaining = value;
                this.CallAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the duration of the <see cref="Scp049SenseAbility"/>.
        /// </summary>
        public float RemainingGoodSenseDuration
        {
            get => this.SenseAbility.Duration.Remaining;
            set
            {
                this.SenseAbility.Duration.Remaining = value;
                this.SenseAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the distance of the Sense Ability.
        /// </summary>
        public float SenseDistance
        {
            get => this.SenseAbility._distanceThreshold;
            set => this.SenseAbility._distanceThreshold = value;
        }

        /// <summary>
        /// Gets the <see cref="Scp049GameRole"/> instance.
        /// </summary>
        public new Scp049GameRole Base { get; }

        /// <summary>
        /// Lose the current target of the Good Sense ability.
        /// </summary>
        public void LoseSenseTarget() => this.SenseAbility.ServerLoseTarget();

        /// <summary>
        /// Resurrects a <see cref="Player"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/>to resurrect.</param>
        /// <returns>The Resurrected player.</returns>
        public bool Resurrect(Player player)
        {
            if (player is null)
                return false;
            player.ReferenceHub.transform.position = this.ResurrectAbility.CastRole.FpcModule.Position;

            HumeShieldModuleBase humeShield = this.ResurrectAbility.CastRole.HumeShieldModule;
            humeShield.HsCurrent = Mathf.Min(humeShield.HsCurrent + 100f, humeShield.HsMax);

            return this.Resurrect(Features.Ragdoll.GetLast(player));
        }

        /// <summary>
        /// Resurrects a <see cref="Ragdoll"/> owner.
        /// </summary>
        /// <param name="ragdoll">The Ragdoll to resurrect.</param>
        /// <returns>The Resurrected Ragdoll.</returns>
        public bool Resurrect(Ragdoll ragdoll)
        {
            if (ragdoll is null)
                return false;

            this.ResurrectAbility.CurRagdoll = ragdoll.Base;
            this.ResurrectAbility.ServerComplete();

            return true;
        }

        /// <summary>
        /// Attacks a Player.
        /// </summary>
        /// <param name="player">The <see cref="Player"/>to attack.</param>
        public void Attack(Player player)
        {
            this.AttackAbility._target = player?.ReferenceHub;

            if (this.AttackAbility._target is null || !this.AttackAbility.IsTargetValid(this.AttackAbility._target))
                return;

            this.AttackAbility.Cooldown.Trigger(Scp049AttackAbility.CooldownTime);
            CardiacArrest cardiacArrest = this.AttackAbility._target.playerEffectsController.GetEffect<CardiacArrest>();

            if (cardiacArrest.IsEnabled)
            {
                this.AttackAbility._target.playerStats.DealDamage(new Scp049DamageHandler(this.AttackAbility.Owner, StandardDamageHandler.KillValue, Scp049DamageHandler.AttackType.Instakill));
            }
            else
            {
                cardiacArrest.SetAttacker(this.AttackAbility.Owner);
                cardiacArrest.ServerSetState(1, this.AttackAbility._statusEffectDuration, false);
            }

            this.SenseAbility.OnServerHit(this.AttackAbility._target);

            this.AttackAbility.ServerSendRpc(true);
            Hitmarker.SendHitmarkerDirectly(this.AttackAbility.Owner, 1f);
        }

        /// <summary>
        /// Trigger the Sense Ability on the specified <see cref="Player"/>.
        /// </summary>
        /// <param name="player">The Player to sense.</param>
        public void Sense(Player player)
        {
            if (!this.SenseAbility.Cooldown.IsReady || !this.SenseAbility.Duration.IsReady)
                return;

            this.SenseAbility.HasTarget = false;
            this.SenseAbility.Target = player?.ReferenceHub;

            if (this.SenseAbility.Target is null)
            {
                this.SenseAbility.Cooldown.Trigger(Scp049SenseAbility.AttemptFailCooldown);
                this.SenseAbility.ServerSendRpc(true);
                return;
            }
            else
            {
                if (this.SenseAbility.Target.roleManager.CurrentRole is not PlayerRoles.HumanRole humanRole)
                    return;

                float radius = humanRole.FpcModule.CharController.radius;
                if (!VisionInformation.GetVisionInformation(this.SenseAbility.Owner, this.SenseAbility.Owner.PlayerCameraReference, humanRole.CameraPosition, radius, this.SenseAbility._distanceThreshold).IsLooking)
                    return;

                this.SenseAbility.Duration.Trigger(Scp049SenseAbility.EffectDuration);
                this.SenseAbility.HasTarget = true;
                this.SenseAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Refresh the <see cref="Scp049CallAbility"/> duration.
        /// </summary>
        public void RefreshCallDuration() => this.CallAbility.ServerRefreshDuration();

        /// <summary>
        /// Gets the amount of resurrections of a <see cref="Player"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/>to check.</param>
        /// <returns>The amount of resurrections of the checked player.</returns>
        public int GetResurrectionCount(Player player) => player is not null ? Scp049ResurrectAbility.GetResurrectionsNumber(player.ReferenceHub) : 0;

        /// <summary>
        /// Returns a <see langword="bool"/> indicating whether the ragdoll can be resurrected by SCP-049.
        /// </summary>
        /// <param name="ragdoll">The ragdoll to check.</param>
        /// <returns><see langword="true"/> if the body can be revived; otherwise, <see langword="false"/>.</returns>
        public bool CanResurrect(BasicRagdoll ragdoll) => ragdoll != null && this.ResurrectAbility.CheckRagdoll(ragdoll);

        /// <summary>
        /// Returns a <see langword="bool"/> indicating whether the ragdoll can be resurrected by SCP-049.
        /// </summary>
        /// <param name="ragdoll">The ragdoll to check.</param>
        /// <returns><see langword="true"/> if the body can be revived; otherwise, <see langword="false"/>.</returns>
        public bool CanResurrect(Ragdoll ragdoll) => ragdoll is not null && this.ResurrectAbility.CheckRagdoll(ragdoll.Base);

        /// <summary>
        /// Returns a <see langword="bool"/> indicating whether SCP-049 is close enough to a ragdoll to revive it.
        /// </summary>
        /// <remarks>This method only returns whether SCP-049 is close enough to the body to revive it; the body may have expired. Make sure to check <see cref="CanResurrect(BasicRagdoll)"/> to ensure the body can be revived.</remarks>
        /// <param name="ragdoll">The ragdoll to check.</param>
        /// <returns><see langword="true"/> if close enough to revive the body; otherwise, <see langword="false"/>.</returns>
        public bool IsInRecallRange(BasicRagdoll ragdoll) => ragdoll != null && this.ResurrectAbility.IsCloseEnough(this.Owner.Position, ragdoll.transform.position);

        /// <summary>
        /// Returns a <see langword="bool"/> indicating whether SCP-049 is close enough to a ragdoll to revive it.
        /// </summary>
        /// <remarks>This method only returns whether SCP-049 is close enough to the body to revive it; the body may have expired. Make sure to check <see cref="CanResurrect(Ragdoll)"/> to ensure the body can be revived.</remarks>
        /// <param name="ragdoll">The ragdoll to check.</param>
        /// <returns><see langword="true"/> if close enough to revive the body; otherwise, <see langword="false"/>.</returns>
        public bool IsInRecallRange(Ragdoll ragdoll) => ragdoll is not null && this.IsInRecallRange(ragdoll.Base);

        /// <summary>
        /// Gets the Spawn Chance of SCP-049.
        /// </summary>
        /// <param name="alreadySpawned">The List of Roles already spawned.</param>
        /// <returns>The Spawn Chance.</returns>
        public float GetSpawnChance(List<RoleTypeId> alreadySpawned) => this.Base.GetSpawnChance(alreadySpawned);
    }
}
