// -----------------------------------------------------------------------
// <copyright file="Scp3114Role.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Roles
{
    using System.Collections.Generic;

    using Exiled.API.Enums;
    using PlayerRoles;
    using PlayerRoles.PlayableScps;
    using PlayerRoles.PlayableScps.HumeShield;
    using PlayerRoles.PlayableScps.Scp3114;
    using PlayerRoles.Subroutines;

    using static PlayerRoles.PlayableScps.Scp3114.Scp3114Identity;

    using Scp3114GameRole = PlayerRoles.PlayableScps.Scp3114.Scp3114Role;

    /// <summary>
    /// Defines a role that represents SCP-3114.
    /// </summary>
    public class Scp3114Role : FpcRole, ISubroutinedScpRole, IHumeShieldRole, ISpawnableScp
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Scp3114Role"/> class.
        /// </summary>
        /// <param name="baseRole">the base <see cref="Scp3114GameRole"/>.</param>
        internal Scp3114Role(Scp3114GameRole baseRole)
            : base(baseRole)
        {
            this.Base = baseRole;
            this.SubroutineModule = baseRole.SubroutineModule;
            this.HumeShieldModule = baseRole.HumeShieldModule;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp3114Slap scp3114Slap))
                Log.Error("Scp3114Slap not found in Scp3114Role::ctor");

            this.Slap = scp3114Slap;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp3114Dance scp3114Dance))
                Log.Error("Scp3114Dance not found in Scp3114Role::ctor");

            this.Dance = scp3114Dance;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp3114Reveal scp3114Reveal))
                Log.Error("Scp3114Reveal not found in Scp3114Role::ctor");

            this.Reveal = scp3114Reveal;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp3114Identity scp3114Identity))
                Log.Error("Scp3114Identity not found in Scp3114Role::ctor");

            this.Identity = scp3114Identity;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp3114History scp3114History))
                Log.Error("Scp3114History not found in Scp3114Role::ctor");

            this.History = scp3114History;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp3114FakeModelManager scp3114FakeModelManager))
                Log.Error("Scp3114FakeModelManager not found in Scp3114Role::ctor");

            this.FakeModelManager = scp3114FakeModelManager;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp3114Disguise scp3114Disguise))
                Log.Error("Scp3114Disguise not found in Scp3114Role::ctor");

            this.Disguise = scp3114Disguise;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp3114VoiceLines scp3114VoiceLines))
                Log.Error("Scp3114VoiceLines not found in Scp3114Role::ctor");

            this.VoiceLines = scp3114VoiceLines;
        }

        /// <inheritdoc/>
        public override RoleTypeId Type { get; } = RoleTypeId.Scp3114;

        /// <inheritdoc/>
        public SubroutineManagerModule SubroutineModule { get; }

        /// <inheritdoc/>
        public HumeShieldModuleBase HumeShieldModule { get; }

        /// <summary>
        /// Gets Scp3114's <see cref="Scp3114Slap"/>.
        /// </summary>
        public Scp3114Slap Slap { get; }

        /// <summary>
        /// Gets Scp3114's <see cref="Scp3114Dance"/>.
        /// </summary>
        public Scp3114Dance Dance { get; }

        /// <summary>
        /// Gets Scp3114's <see cref="Scp3114Reveal"/>.
        /// </summary>
        public Scp3114Reveal Reveal { get; }

        /// <summary>
        /// Gets Scp3114's <see cref="Scp3114Identity"/>.
        /// </summary>
        public Scp3114Identity Identity { get; }

        /// <summary>
        /// Gets Scp3114's <see cref="Scp3114History"/>.
        /// </summary>
        public Scp3114History History { get; }

        /// <summary>
        /// Gets Scp3114's <see cref="Scp3114FakeModelManager"/>.
        /// </summary>
        public Scp3114FakeModelManager FakeModelManager { get; }

        /// <summary>
        /// Gets Scp3114's <see cref="Scp3114Disguise"/>.
        /// </summary>
        public Scp3114Disguise Disguise { get; }

        /// <summary>
        /// Gets Scp3114's <see cref="Scp3114VoiceLines"/>.
        /// </summary>
        public Scp3114VoiceLines VoiceLines { get; }

        /// <summary>
        /// Gets the <see cref="Scp3114GameRole"/> instance.
        /// </summary>
        public new Scp3114GameRole Base { get; }

        /// <summary>
        /// Gets a value indicating whether .
        /// </summary>
        public bool RespectPreferences => true;

        /// <summary>
        /// Gets a value indicating whether .
        /// </summary>
        public bool AllowFallback => true;

        /// <summary>
        /// Gets the damage amount of SCP-3114's slap ability.
        /// </summary>
        public float SlapDamage => this.Slap.DamageAmount;

        /// <summary>
        /// Gets the current target of SCP-3114's strangle ability. Can be <see langword="null"/>.
        /// </summary>
        public Player StrangleTarget => Player.Get(this.Slap._strangle.SyncTarget?.Target);

        /// <summary>
        /// Gets or sets the SCP-3114's Stolen Role.
        /// </summary>
        public RoleTypeId StolenRole
        {
            get => this.Identity.CurIdentity.StolenRole;
            set
            {
                if (this.IdentityRagdoll is null)
                    return;

                this.IdentityRagdoll.Role = value;
                this.UpdateIdentity();
            }
        }

        /// <summary>
        /// Gets or sets the SCP-3114's Ragdoll used for it's FakeIdentity.
        /// </summary>
        public Ragdoll IdentityRagdoll
        {
            get => Features.Ragdoll.Get(this.Identity.CurIdentity.Ragdoll);
            set
            {
                this.Identity.CurIdentity.Ragdoll = value?.Base;
                this.UpdateIdentity();
            }
        }

        /// <summary>
        /// Gets or sets the SCP-3114's UnitId used for it's FakeIdentity.
        /// </summary>
        public byte UnitId
        {
            get => this.Identity.CurIdentity.UnitNameId;
            set
            {
                this.Identity.CurIdentity.UnitNameId = value;
                this.UpdateIdentity();
            }
        }

        /// <summary>
        /// Gets or sets current state of SCP-3114's disguise ability.
        /// </summary>
        public DisguiseStatus DisguiseStatus
        {
            get => this.Identity.CurIdentity.Status;
            set
            {
                this.Identity.CurIdentity.Status = value;
                this.UpdateIdentity();
            }
        }

        /// <summary>
        /// Gets or sets the SCP-3114's Disguise duration.
        /// </summary>
        public float DisguiseDuration
        {
            get => this.Identity._disguiseDurationSeconds;
            set
            {
                this.Identity._disguiseDurationSeconds = value;
                this.UpdateIdentity();
            }
        }

        /// <summary>
        /// Gets or sets the warning time seconds.
        /// </summary>
        public float WarningTime
        {
            get => this.Identity._warningTimeSeconds;
            set => this.Identity._warningTimeSeconds = value;
        }

        /// <summary>
        /// Gets or sets the next bound dance.
        /// </summary>
        public DanceType? NextDanceType { get; set; }

        /// <summary>
        /// Gets or sets the bound dance.
        /// </summary>
        internal DanceType DanceType { get; set; } = DanceType.None;

        /// <summary>
        /// Updates the identity of SCP-3114.
        /// </summary>
        public void UpdateIdentity() => this.Identity.ServerResendIdentity();

        /// <summary>
        /// Reset Scp3114 FakeIdentity.
        /// </summary>
        public void ResetIdentity()
        {
            this.Identity.CurIdentity.Reset();
            this.UpdateIdentity();
        }

        /// <summary>
        /// Plays a random Scp3114 voice line.
        /// </summary>
        /// <param name="voiceLine">The type of voice line to play.</param>
        public void PlaySound(Scp3114VoiceLines.VoiceLinesName voiceLine = Scp3114VoiceLines.VoiceLinesName.RandomIdle)
            => this.VoiceLines.ServerPlayConditionally(voiceLine);

        /// <summary>
        /// Gets the Spawn Chance of SCP-3114.
        /// </summary>
        /// <param name="alreadySpawned">The List of Roles already spawned.</param>
        /// <returns>The Spawn Chance.</returns>
        public float GetSpawnChance(List<RoleTypeId> alreadySpawned) => this.Base is ISpawnableScp spawnableScp ? spawnableScp.GetSpawnChance(alreadySpawned) : 0;

        /// <summary>
        /// SCP-3114 starts dancing.
        /// </summary>
        /// <param name="danceType">The dance you want to do.</param>
        public void StartDancing(DanceType danceType)
        {
            this.Dance.IsDancing = true;
            this.DanceType = danceType;
            this.Dance._serverStartPos = new RelativePositioning.RelativePosition(this.Dance.CastRole.FpcModule.Position);
            this.Dance.ServerSendRpc(true);
        }

        /// <summary>
        /// Stops the SCP-3114 from Dancing.
        /// </summary>
        public void StopDancing()
        {
            this.Dance.IsDancing = false;
            this.Dance.ServerSendRpc(true);
        }
    }
}