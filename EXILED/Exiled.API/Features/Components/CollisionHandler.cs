// -----------------------------------------------------------------------
// <copyright file="CollisionHandler.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Components
{
    using System;

    using Features;
    using InventorySystem.Items.ThrowableProjectiles;
    using Mirror;
    using UnityEngine;

    /// <summary>
    /// Collision Handler for grenades.
    /// </summary>
    public class CollisionHandler : MonoBehaviour
    {
        private bool initialized;
        private float activableTime;
        private Action onCollisionAction;

        /// <summary>
        /// Gets the thrower of the grenade.
        /// </summary>
        public GameObject Owner { get; private set; }

        /// <summary>
        /// Inits the <see cref="CollisionHandler"/> object.
        /// </summary>
        /// <param name="owner">The grenade owner.</param>
        /// <param name="onCollisionAction">Action on collision.</param>
        /// <param name="fuseDelay">Delay before onCollisionAction may be executed by collision.</param>
        public void Init(GameObject owner, Action onCollisionAction, float fuseDelay = 0.15f)
        {
            this.Owner = owner;
            this.initialized = true;
            this.onCollisionAction = onCollisionAction;
            this.activableTime = (float)NetworkTime.time + fuseDelay;
        }

        private void OnCollisionEnter(Collision collision)
        {
            try
            {
                if (!this.initialized)
                    return;
                if (this.activableTime > NetworkTime.time)
                    return;

                if (this.Owner == null)
                    Log.Error($"Owner is null!");
                if (collision.gameObject == null)
                    Log.Error("pepehm");
                if (collision.collider.gameObject == this.Owner || collision.collider.gameObject.TryGetComponent<EffectGrenade>(out _))
                    return;

                this.onCollisionAction.Invoke();
            }
            catch (Exception exception)
            {
                Log.Error($"{nameof(this.OnCollisionEnter)} error:\n{exception}");
                Destroy(this);
            }
        }
    }
}
