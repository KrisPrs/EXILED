// -----------------------------------------------------------------------
// <copyright file="EActor.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Exiled.API.Features.Core.Interfaces;
    using Exiled.API.Features.DynamicEvents;
    using Exiled.API.Features.Pools;
    using Exiled.API.Interfaces;
    using MEC;

    using UnityEngine;

    /// <summary>
    /// Actor is the base class for a <see cref="EObject"/> that can be placed or spawned in-game.
    /// </summary>
    public abstract class EActor : EObject, IEntity, IWorldSpace
    {
        /// <summary>
        /// The default fixed tick rate.
        /// </summary>
        public const float DefaultFixedTickRate = TickComponent.DefaultFixedTickRate;

        private readonly HashSet<EActor> componentsInChildren = HashSetPool<EActor>.Pool.Get();
        private CoroutineHandle serverTick;

        /// <summary>
        /// Initializes a new instance of the <see cref="EActor"/> class.
        /// </summary>
        protected EActor()
            : base()
        {
            this.IsEditable = true;
            this.CanEverTick = true;
            this.FixedTickRate = DefaultFixedTickRate;
            this.PostInitialize();
            Timing.CallDelayed(this.FixedTickRate, this.OnBeginPlay);
            Timing.CallDelayed(this.FixedTickRate * 2, () => this.serverTick = Timing.RunCoroutine(this.ServerTick()));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EActor"/> class.
        /// </summary>
        /// <param name="gameObject">The base <see cref="GameObject"/>.</param>
        protected EActor(GameObject gameObject = null)
            : this()
        {
            if (gameObject)
                this.Base = gameObject;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<EActor> ComponentsInChildren => this.componentsInChildren;

        /// <summary>
        /// Gets the <see cref="UnityEngine.Transform"/>.
        /// </summary>
        public Transform Transform => this.Base.transform;

        /// <summary>
        /// Gets or sets the <see cref="Vector3">position</see>.
        /// </summary>
        public virtual Vector3 Position
        {
            get => this.Transform.position;
            set => this.Transform.position = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Quaternion">rotation</see>.
        /// </summary>
        public virtual Quaternion Rotation
        {
            get => this.Transform.rotation;
            set => this.Transform.rotation = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Vector3">scale</see>.
        /// </summary>
        public virtual Vector3 Scale
        {
            get => this.Transform.localScale;
            set => this.Transform.localScale = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="EActor"/> can tick.
        /// </summary>
        public virtual bool CanEverTick
        {
            get;
            set
            {
                if (!this.IsEditable)
                    return;

                field = value;

                if (field)
                {
                    Timing.ResumeCoroutines(this.serverTick);
                    return;
                }

                Timing.PauseCoroutines(this.serverTick);
            }
        }

        /// <summary>
        /// Gets or sets the value which determines the size of every tick.
        /// </summary>
        public virtual float FixedTickRate
        {
            get;
            set
            {
                if (!this.IsEditable)
                    return;

                field = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="EActor"/>[] containing all the components in parent.
        /// </summary>
        protected IEnumerable<EActor> ComponentsInParent => FindActiveObjectsOfType<EActor>().Where(actor => actor.ComponentsInChildren.Any(comp => comp == this));

        /// <summary>
        /// Attaches a <see cref="EActor"/> to the specified <see cref="GameObject"/>.
        /// </summary>
        /// <param name="comp"><see cref="EActor"/>.</param>
        /// <param name="gameObject"><see cref="GameObject"/>.</param>
        public static void AttachTo(EActor comp, GameObject gameObject) => comp.Base = gameObject;

        /// <summary>
        /// Attaches a <see cref="EActor"/> to the specified <see cref="EActor"/>.
        /// </summary>
        /// <param name="to">The actor to be modified.</param>
        /// <param name="from">The source actor.</param>
        public static void AttachTo(EActor to, EActor from) => to.Base = from.Base;

        /// <inheritdoc/>
        public T AddComponent<T>(string name = "")
            where T : EActor
        {
            T component = CreateDefaultSubobject<T>(this.Base, string.IsNullOrEmpty(name) ? $"{this.GetType().Name}-Component#{this.ComponentsInChildren.Count}" : name).Cast<T>();
            if (component is null)
                return null;

            this.componentsInChildren.Add(component);
            return component.Cast<T>();
        }

        /// <inheritdoc/>
        public EActor AddComponent(Type type, string name = "")
        {
            EActor component = CreateDefaultSubobject(type, this.Base, string.IsNullOrEmpty(name) ? $"{this.GetType().Name}-Component#{this.ComponentsInChildren.Count}" : name).Cast<EActor>();
            if (component is null)
                return null;

            this.componentsInChildren.Add(component);
            return component;
        }

        /// <inheritdoc/>
        public T AddComponent<T>(Type type, string name = "")
            where T : EActor => this.ComponentsInChildren.FirstOrDefault(comp => type == comp.GetType()).Cast<T>();

        /// <inheritdoc/>
        public EActor GetComponent(Type type) => this.ComponentsInChildren.FirstOrDefault(comp => type == comp.GetType());

        /// <inheritdoc/>
        public T GetComponent<T>()
            where T : EActor => this.ComponentsInChildren.FirstOrDefault(comp => typeof(T) == comp.GetType()).Cast<T>();

        /// <inheritdoc/>
        public T GetComponent<T>(Type type)
            where T : EActor => this.ComponentsInChildren.FirstOrDefault(comp => type == comp.GetType()).Cast<T>();

        /// <inheritdoc/>
        public bool TryGetComponent<T>(Type type, out T component)
            where T : EActor
        {
            EActor actor = this.GetComponent(type);

            if (actor.Cast(out component))
                component = actor.Cast<T>();

            return component;
        }

        /// <inheritdoc/>
        public bool TryGetComponent<T>(out T component)
            where T : EActor
        {
            component = null;

            if (this.HasComponent<T>())
                component = this.GetComponent<T>().Cast<T>();

            return component is not null;
        }

        /// <inheritdoc/>
        public bool TryGetComponent(Type type, out EActor component)
        {
            component = null;

            if (this.HasComponent(type))
                component = this.GetComponent(type);

            return component is not null;
        }

        /// <inheritdoc/>
        public bool HasComponent<T>(bool depthInheritance = false) => depthInheritance
            ? this.ComponentsInChildren.Any(comp => typeof(T).IsSubclassOf(comp.GetType()))
            : this.ComponentsInChildren.Any(comp => typeof(T) == comp.GetType());

        /// <inheritdoc/>
        public bool HasComponent(Type type, bool depthInheritance = false) => depthInheritance
            ? this.ComponentsInChildren.Any(comp => type.IsSubclassOf(comp.GetType()))
            : this.ComponentsInChildren.Any(comp => type == comp.GetType());

        /// <summary>
        /// Fired after the <see cref="EActor"/> instance is created.
        /// </summary>
        protected virtual void PostInitialize()
        {
        }

        /// <summary>
        /// Fired after the first fixed tick.
        /// </summary>
        protected virtual void OnBeginPlay() => this.SubscribeEvents();

        /// <summary>
        /// Fired every tick.
        /// </summary>
        protected virtual void Tick()
        {
        }

        /// <summary>
        /// Fired before the current <see cref="EActor"/> instance is destroyed.
        /// </summary>
        protected virtual void OnEndPlay() => this.UnsubscribeEvents();

        /// <summary>
        /// Subscribes all the events.
        /// </summary>
        protected virtual void SubscribeEvents() => StaticActor.Get<DynamicEventManager>().BindAllFromTypeInstance(this);

        /// <summary>
        /// Unsubscribes all the events.
        /// </summary>
        protected virtual void UnsubscribeEvents() => StaticActor.Get<DynamicEventManager>().UnbindAllFromTypeInstance(this);

        /// <inheritdoc/>
        protected override void OnBeginDestroy()
        {
            base.OnBeginDestroy();

            HashSetPool<EActor>.Pool.Return(this.componentsInChildren);
            Timing.KillCoroutines(this.serverTick);

            this.OnEndPlay();
        }

        private IEnumerator<float> ServerTick()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(this.FixedTickRate);

                this.Tick();
            }
        }
    }
}