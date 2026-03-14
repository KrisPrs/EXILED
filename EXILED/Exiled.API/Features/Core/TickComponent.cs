// -----------------------------------------------------------------------
// <copyright file="TickComponent.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Exiled.API.Features.Pools;

    using MEC;

    /// <summary>
    /// The component which handles tick related features.
    /// </summary>
    public sealed class TickComponent : EObject
    {
        /// <summary>
        /// The default fixed tick rate.
        /// </summary>
        public const float DefaultFixedTickRate = 0.016f;

        private readonly HashSet<CoroutineHandle> boundHandles;
        private readonly CoroutineHandle executeAllHandle;
        private bool canEverTick;

        /// <summary>
        /// Initializes a new instance of the <see cref="TickComponent"/> class.
        /// </summary>
        internal TickComponent()
            : base()
        {
            this.executeAllHandle = Timing.RunCoroutine(this.ExecuteAll());
            this.boundHandles = new HashSet<CoroutineHandle>();
            this.CanEverTick = true;
        }

        /// <summary>
        /// Gets or sets the current tick rate.
        /// </summary>
        public float TickRate { get; set; } = DefaultFixedTickRate;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="EActor"/> can tick.
        /// </summary>
        public bool CanEverTick
        {
            get => this.canEverTick;
            set
            {
                if (!this.IsEditable || this.canEverTick == value)
                    return;

                this.canEverTick = value;

                if (this.canEverTick)
                {
                    Timing.ResumeCoroutines(this.executeAllHandle);
                    Timing.ResumeCoroutines(this.boundHandles.ToArray());
                    return;
                }

                Timing.PauseCoroutines(this.executeAllHandle);
                Timing.PauseCoroutines(this.boundHandles.ToArray());
            }
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="Action"/> containing all the delegates to be invoked.
        /// </summary>
        public List<Action> Instructions { get; } = ListPool<Action>.Pool.Get();

        /// <summary>
        /// Gets all the currently bound handles.
        /// </summary>
        public IReadOnlyCollection<CoroutineHandle> BoundHandles => this.boundHandles;

        /// <summary>
        /// Binds a <see cref="CoroutineHandle"/>.
        /// </summary>
        /// <param name="handle">The <see cref="CoroutineHandle"/> to bind.</param>
        public void BindHandle(CoroutineHandle handle) => this.boundHandles.Add(handle);

        /// <summary>
        /// Binds a <see cref="CoroutineHandle"/>.
        /// </summary>
        /// <param name="handle">The <see cref="CoroutineHandle"/> to bind.</param>
        /// <param name="coroutine">The coroutine to handle.</param>
        public void BindHandle(ref CoroutineHandle handle, IEnumerator<float> coroutine) => this.BindHandle(handle = Timing.RunCoroutine(coroutine));

        /// <summary>
        /// Unbinds a <see cref="CoroutineHandle"/>.
        /// </summary>
        /// <param name="handle">The <see cref="CoroutineHandle"/> to unbind.</param>
        public void UnbindHandle(CoroutineHandle handle)
        {
            Timing.KillCoroutines(handle);
            this.boundHandles.RemoveWhere(ax => ax == handle);
        }

        /// <summary>
        /// Unbinds all the currently bound handles.
        /// </summary>
        public void UnbindAllHandles()
        {
            Timing.KillCoroutines(this.boundHandles.ToArray());
            this.boundHandles.Clear();
        }

        /// <inheritdoc/>
        protected override void OnBeginDestroy()
        {
            base.OnBeginDestroy();

            ListPool<Action>.Pool.Return(this.Instructions);
            this.UnbindAllHandles();
            Timing.KillCoroutines(this.executeAllHandle);
        }

        private IEnumerator<float> ExecuteAll()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(this.TickRate);

                foreach (Action action in this.Instructions)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }
            }
        }
    }
}