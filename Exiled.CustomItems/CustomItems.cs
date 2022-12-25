// -----------------------------------------------------------------------
// <copyright file="CustomItems.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems
{
    using System;

    using Exiled.API.Features;
    using Exiled.CustomItems.Events;

    using HarmonyLib;

    /// <summary>
    /// Handles all CustomItem API.
    /// </summary>
    public class CustomItems : Plugin<Config>
    {
        private RoundHandler roundHandler;
        private Harmony harmony;

        /// <summary>
        /// Gets the static reference to this <see cref="CustomItems"/> class.
        /// </summary>
        public static CustomItems Instance { get; private set; }

        /// <inheritdoc />
        public override void OnEnabled()
        {
            Instance = this;
            roundHandler = new RoundHandler();

            Exiled.Events.Handlers.Server.RoundStarted += roundHandler.OnRoundStarted;

            harmony = new Harmony($"com.{nameof(CustomItems)}.flx-{DateTime.Now.Ticks}");
            harmony.PatchAll();

            base.OnEnabled();
        }

        /// <inheritdoc />
        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= roundHandler.OnRoundStarted;

            harmony.UnpatchAll();

            harmony = null;
            roundHandler = null;

            base.OnDisabled();
        }
    }
}
