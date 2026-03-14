// -----------------------------------------------------------------------
// <copyright file="FpcRole.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Roles
{
    using System;
    using System.Collections.Generic;

    using Exiled.API.Extensions;
    using Exiled.API.Features.Pools;
    using HarmonyLib;

    using Mirror;

    using PlayerRoles;
    using PlayerRoles.FirstPersonControl;
    using PlayerRoles.FirstPersonControl.Thirdperson;
    using PlayerRoles.Ragdolls;
    using PlayerRoles.Spectating;
    using PlayerRoles.Visibility;
    using PlayerRoles.Voice;
    using PlayerStatsSystem;
    using RelativePositioning;
    using UnityEngine;

    /// <summary>
    /// Defines a role that represents an fpc class.
    /// </summary>
    public abstract class FpcRole : Role, IVoiceRole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FpcRole"/> class.
        /// </summary>
        /// <param name="baseRole">the base <see cref="PlayerRoleBase"/>.</param>
        protected FpcRole(FpcStandardRoleBase baseRole)
            : base(baseRole)
        {
            this.FirstPersonController = baseRole;
            this.IsUsingStamina = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FpcRole"/> class.
        /// </summary>
        ~FpcRole() => HashSetPool<Player>.Pool.Return(this.IsInvisibleFor);

        /// <summary>
        /// Gets the <see cref="FirstPersonController"/>.
        /// </summary>
        public FpcStandardRoleBase FirstPersonController { get; }

        /// <summary>
        /// Gets or sets the player's relative position as perceived by the server.
        /// </summary>
        public RelativePosition RelativePosition
        {
            get => new(this.Owner.Position);
            set => this.Owner.Position = value.Position;
        }

        /// <summary>
        /// Gets or sets the player's relative position as perceived by the client.
        /// </summary>
        public RelativePosition ClientRelativePosition
        {
            get => this.FirstPersonController.FpcModule.Motor.ReceivedPosition;
            set => this.FirstPersonController.FpcModule.Motor.ReceivedPosition = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="CharacterModel"/> associated with the player.
        /// </summary>
        public CharacterModel Model
        {
            get => this.FirstPersonController.FpcModule.CharacterModelInstance;
            set => this.FirstPersonController.FpcModule.CharacterModelInstance = value;
        }

        /// <summary>
        /// Gets or sets the player's gravity.
        /// </summary>
        public Vector3 Gravity
        {
            get => this.FirstPersonController.FpcModule.Motor.GravityController.Gravity;
            set => this.FirstPersonController.FpcModule.Motor.GravityController.Gravity = value;
        }

        /// <summary>
        /// Gets or sets the player's scale.
        /// </summary>
        public Vector3 Scale
        {
            get => this.FirstPersonController.FpcModule.Motor.ScaleController.Scale;
            set => this.FirstPersonController.FpcModule.Motor.ScaleController.Scale = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if the player should get <see cref="Enums.DamageType.Falldown"/> damage.
        /// </summary>
        public bool IsFallDamageEnable
        {
            get => this.FirstPersonController.FpcModule.Motor._fallDamageSettings.Enabled;
            set => this.FirstPersonController.FpcModule.Motor._fallDamageSettings.Enabled = value;
        }

        /// <summary>
        /// Gets or sets the multiplier of <see cref="Enums.DamageType.Falldown"/> damage.
        /// </summary>
        public float FallDamageMultiplier
        {
            get => this.FirstPersonController.FpcModule.Motor._fallDamageSettings.Multiplier;
            set => this.FirstPersonController.FpcModule.Motor._fallDamageSettings.Multiplier = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if a rotation is detected on the player.
        /// </summary>
        public bool RotationDetected
        {
            get => this.FirstPersonController.FpcModule.Motor.RotationDetected;
            set => this.FirstPersonController.FpcModule.Motor.RotationDetected = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Role"/> walking speed.
        /// </summary>
        public float WalkingSpeed
        {
            get => this.FirstPersonController.FpcModule.WalkSpeed;
            set => this.FirstPersonController.FpcModule.WalkSpeed = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Role"/> sprinting speed.
        /// </summary>
        public float SprintingSpeed
        {
            get => this.FirstPersonController.FpcModule.SprintSpeed;
            set => this.FirstPersonController.FpcModule.SprintSpeed = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Role"/> jumping speed.
        /// </summary>
        public float JumpingSpeed
        {
            get => this.FirstPersonController.FpcModule.JumpSpeed;
            set => this.FirstPersonController.FpcModule.JumpSpeed = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Role"/> crouching speed.
        /// </summary>
        public float CrouchingSpeed
        {
            get => this.FirstPersonController.FpcModule.CrouchSpeed;
            set => this.FirstPersonController.FpcModule.CrouchSpeed = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Player"/> velocity.
        /// </summary>
        public Vector3 Velocity
        {
            get => this.FirstPersonController.FpcModule.Motor.Velocity;
            set => this.FirstPersonController.FpcModule.Motor.Velocity = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if a movement is detected on a <see cref="Player"/>.
        /// </summary>
        public bool MovementDetected
        {
            get => this.FirstPersonController.FpcModule.Motor.MovementDetected;
            set => this.FirstPersonController.FpcModule.Motor.MovementDetected = value;
        }

        /// <summary>
        /// Gets a value indicating whether the player can send inputs.
        /// </summary>
        public bool CanSendInputs => this.FirstPersonController.FpcModule.LockMovement;

        /// <summary>
        /// Gets or sets a value indicating whether the player is invisible.
        /// </summary>
        public bool IsInvisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player should use stamina system. Resets on death.
        /// </summary>
        public bool IsUsingStamina
        {
            get;
            set
            {
                if (!value)
                    this.Owner.ResetStamina();
                field = value;
            }
        }

        /// <summary>
        /// Gets or sets the stamina usage multiplier. Resets on death.
        /// </summary>
        public float StaminaUsageMultiplier { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the stamina regen multiplier. Resets on death.
        /// </summary>
        public float StaminaRegenMultiplier { get; set; } = 1f;

        /// <summary>
        /// Gets a list of players who can't see the player.
        /// </summary>
        public HashSet<Player> IsInvisibleFor { get; } = HashSetPool<Player>.Pool.Get();

        /// <summary>
        /// Gets or sets the player's current <see cref="PlayerMovementState"/>.
        /// </summary>
        public PlayerMovementState MoveState
        {
            get => this.FirstPersonController.FpcModule.CurrentMovementState;
            set => this.FirstPersonController.FpcModule.CurrentMovementState = value;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Player"/> is crouching.
        /// </summary>
        public bool IsCrouching => this.FirstPersonController.FpcModule.StateProcessor.CrouchPercent > 0;

        /// <summary>
        /// Gets a value indicating whether the player is on the ground.
        /// </summary>
        public bool IsGrounded => this.FirstPersonController.FpcModule.IsGrounded;

        /// <summary>
        /// Gets the <see cref="Player"/>'s current movement speed.
        /// </summary>
        public virtual float MovementSpeed => this.FirstPersonController.FpcModule.VelocityForState(this.MoveState, this.IsCrouching);

        /// <summary>
        /// Gets a value indicating whether the <see cref="Player"/> is in darkness.
        /// </summary>
        public bool IsInDarkness => this.FirstPersonController.InDarkness;

        /// <summary>
        /// Gets the <see cref="Player"/>'s vertical rotation.
        /// </summary>
        public float VerticalRotation => this.FirstPersonController.VerticalRotation;

        /// <summary>
        /// Gets the <see cref="Player"/>'s horizontal rotation.
        /// </summary>
        public float HorizontalRotation => this.FirstPersonController.HorizontalRotation;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Player"/> is AFK.
        /// </summary>
        public bool IsAfk => this.FirstPersonController.IsAFK;

        /// <summary>
        /// Gets a value indicating whether this role is protected by a hume shield.
        /// </summary>
        public bool IsHumeShieldedRole => this is IHumeShieldRole;

        /// <summary>
        /// Gets or sets a value indicating whether the player has noclip enabled.
        /// </summary>
        /// <returns><see cref="bool"/> indicating status.</returns>
        /// <remarks>For permitting a player to enter and exit noclip freely, see <see cref="Player.IsNoclipPermitted"/>.</remarks>
        /// <seealso cref="Player.IsNoclipPermitted"/>
        [Obsolete("Use Player::IsNoclipEnabled instead")]
        public bool IsNoclipEnabled
        {
            get => this.Owner.ReferenceHub.playerStats.GetModule<AdminFlagsStat>().HasFlag(AdminFlags.Noclip);
            set => this.Owner.ReferenceHub.playerStats.GetModule<AdminFlagsStat>().SetFlag(AdminFlags.Noclip, value);
        }

        /// <summary>
        /// Gets or sets a prefab ragdoll for this role.
        /// </summary>
        public BasicRagdoll Ragdoll
        {
            get => this.FirstPersonController.Ragdoll;
            set => this.FirstPersonController.Ragdoll = value;
        }

        /// <summary>
        /// Gets a voice module for this role.
        /// </summary>
        public VoiceModuleBase VoiceModule => this.FirstPersonController.VoiceModule;

        /// <summary>
        /// Gets a <see cref="VisibilityController"/> for this role.
        /// </summary>
        public VisibilityController VisibilityController => this.FirstPersonController.VisibilityController;

        /// <summary>
        /// Gets a <see cref="SpectatableModuleBase"/> for this role.
        /// </summary>
        public SpectatableModuleBase SpectatableModuleBase => this.FirstPersonController.SpectatorModule;

        /// <summary>
        /// Tries to get the <see cref="Transform"/> of a specified <see cref="HumanBodyBones"/> bone.
        /// </summary>
        /// <param name="bone">The bone to get the <see cref="Transform"/> of.</param>
        /// <param name="boneTransform">
        /// When this method returns, contains the <see cref="Transform"/> of the specified bone, if found;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if the bone transform was found; otherwise, <c>false</c>.</returns>
        public bool TryGetBoneTransform(HumanBodyBones bone, out Transform boneTransform)
        {
            boneTransform = null;

            if (this.Model is not AnimatedCharacterModel animatedModel)
                return false;

            Animator animator = animatedModel.Animator;
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                return false;

            boneTransform = animator.GetBoneTransform(bone);
            return boneTransform != null;
        }

        /// <summary>
        /// Resets the <see cref="Player"/>'s stamina.
        /// </summary>
        /// <param name="multipliers">Resets <see cref="StaminaUsageMultiplier"/> and <see cref="StaminaRegenMultiplier"/>.</param>
        public void ResetStamina(bool multipliers = false)
        {
            this.Owner.Stamina = this.Owner.StaminaStat.MaxValue;

            if (!multipliers)
                return;

            this.StaminaUsageMultiplier = 1f;
            this.StaminaRegenMultiplier = 1f;
        }

        /// <summary>
        /// Makes the player jump using the default or a specified strength.
        /// </summary>
        /// <param name="jumpStrength">Optional. The strength of the jump. If not provided, the default jump speed for Role is used.</param>
        public void Jump(float? jumpStrength = null)
        {
            float strength = jumpStrength ?? this.FirstPersonController.FpcModule.JumpSpeed;
            this.FirstPersonController.FpcModule.Motor.JumpController.ForceJump(strength);
        }

        /// <inheritdoc/>
        internal override bool CheckAppearanceCompatibility(RoleTypeId newAppearance)
        {
            if (!RoleExtensions.TryGetRoleBase(newAppearance, out PlayerRoleBase roleBase))
                return false;

            return roleBase is FpcStandardRoleBase;
        }

        /// <inheritdoc/>
        internal override void SendAppearanceSpawnMessage(NetworkWriter writer, PlayerRoleBase basicRole)
        {
            FpcStandardRoleBase fpcRole = (FpcStandardRoleBase)basicRole;
            fpcRole.FpcModule.MouseLook.GetSyncValues(0, out ushort syncH, out ushort _);
            writer.WriteRelativePosition(new RelativePosition(fpcRole._hubTransform.position));
            writer.WriteUShort(syncH);
        }
    }
}
