// -----------------------------------------------------------------------
// <copyright file="TemporaryHazard.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Hazards
{
    using BaseHazard = global::Hazards.TemporaryHazard;

    /// <summary>
    /// Represents temporary hazard.
    /// </summary>
    public class TemporaryHazard : Hazard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryHazard"/> class.
        /// </summary>
        /// <param name="hazard">The <see cref="BaseHazard"/> instance.</param>
        public TemporaryHazard(BaseHazard hazard)
            : base(hazard) =>
            this.Base = hazard;

        /// <summary>
        /// Gets the <see cref="BaseHazard"/>.
        /// </summary>
        public new BaseHazard Base { get; }

        /// <summary>
        /// Gets or sets a value indicating whether hazard is destroyed.
        /// </summary>
        public bool IsDestroyed
        {
            get => this.Base._destroyed;
            set
            {
                if (!value)
                {
                    this.Duration = 0;
                }

                this.Base._destroyed = value;
            }
        }

        /// <summary>
        /// Gets the total duration before hazard gets destroyed.
        /// </summary>
        public float TotalDuration => this.Base.HazardDuration;

        /// <summary>
        /// Gets or sets elapsed time which has spend after creating.
        /// </summary>
        public float Duration
        {
            get => this.Base.Elapsed;
            set => this.Base.Elapsed = value;
        }

        /// <summary>
        /// Destroys this hazard.
        /// </summary>
        public void Destroy() => this.Base.ServerDestroy();
    }
}