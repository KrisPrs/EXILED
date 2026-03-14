// -----------------------------------------------------------------------
// <copyright file="AmnesticCloudHazard.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// ------------------------------------------------------------------------

namespace Exiled.API.Features.Hazards
{
    using Exiled.API.Enums;
    using PlayerRoles.PlayableScps.Scp939;

    /// <summary>
    /// A wrapper for SCP-939's amnestic cloud.
    /// </summary>
    public class AmnesticCloudHazard : TemporaryHazard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AmnesticCloudHazard"/> class.
        /// </summary>
        /// <param name="hazard">The <see cref="Scp939AmnesticCloudInstance"/> instance.</param>
        public AmnesticCloudHazard(Scp939AmnesticCloudInstance hazard)
            : base(hazard)
        {
            this.Base = hazard;
            this.Ability = this.Base._cloud;
            this.Owner = Player.Get(this.Ability.Owner);
        }

        /// <summary>
        /// Gets the amnestic cloud prefab.
        /// </summary>
        public static Scp939AmnesticCloudInstance AmnesticCloudPrefab
        {
            get
            {
                if (field == null)
                    field = PrefabHelper.GetPrefab<Scp939AmnesticCloudInstance>(PrefabType.AmnesticCloudHazard);

                return field;
            }
        }

        /// <inheritdoc cref="Hazard.Base"/>
        public new Scp939AmnesticCloudInstance Base { get; }

        /// <inheritdoc />
        public override HazardType Type => HazardType.AmnesticCloud;

        /// <summary>
        /// Gets the <see cref="Scp939AmnesticCloudAbility"/> for this instance.
        /// </summary>
        public Scp939AmnesticCloudAbility Ability { get; }

        /// <summary>
        /// Gets the player who controls SCP-939.
        /// </summary>
        public Player Owner { get; }

        /// <summary>
        /// Gets or sets current state of cloud.
        /// </summary>
        public Scp939AmnesticCloudInstance.CloudState State
        {
            get => this.Base.State;
            set => this.Base.State = value;
        }

        /// <summary>
        /// Gets or sets duration for effects.
        /// </summary>
        public float EffectDuration
        {
            get => this.Base._amnesiaDuration;
            set => this.Base._amnesiaDuration = value;
        }

        /// <summary>
        /// Gets or sets minimum time to press key to spawn cloud.
        /// </summary>
        public float MinHoldTime
        {
            get => this.Base._minHoldTime;
            set => this.Base._minHoldTime = value;
        }

        /// <summary>
        /// Gets or sets maximum time to press key to spawn cloud.
        /// </summary>
        public float MaxHoldTime
        {
            get => this.Base._maxHoldTime;
            set => this.Base._maxHoldTime = value;
        }

        /// <summary>
        /// Gets or sets total duration before hazard will get destroyed.
        /// </summary>
        public new float TotalDuration
        {
            get => this.Base._targetDuration;
            set => this.Base._targetDuration = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether hazard is active.
        /// </summary>
        public bool TargetState
        {
            get => this.Ability.TargetState;
            set => this.Ability.TargetState = value;
        }
    }
}