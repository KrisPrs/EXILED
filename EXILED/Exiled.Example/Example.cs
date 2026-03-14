// -----------------------------------------------------------------------
// <copyright file="Example.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Example
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.Example.Events;

    /// <summary>
    /// The example plugin.
    /// </summary>
    public class Example : Plugin<Config>
    {
        private static readonly Example Singleton = new();

        private ServerHandler serverHandler;
        private PlayerHandler playerHandler;
        private WarheadHandler warheadHandler;
        private MapHandler mapHandler;
        private ItemHandler itemHandler;
        private Scp914Handler scp914Handler;
        private Scp096Handler scp096Handler;

        private Example()
        {
        }

        /// <summary>
        /// Gets the only existing instance of this plugin.
        /// </summary>
        public static Example Instance => Singleton;

        /// <inheritdoc/>
        public override PluginPriority Priority { get; } = PluginPriority.Last;

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            this.RegisterEvents();

            Log.Warn($"I correctly read the string config, its value is: {this.Config.String}");
            Log.Warn($"I correctly read the int config, its value is: {this.Config.Int}");
            Log.Warn($"I correctly read the float config, its value is: {this.Config.Float}");

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            this.UnregisterEvents();
            base.OnDisabled();
        }

        /// <summary>
        /// Registers the plugin events.
        /// </summary>
        private void RegisterEvents()
        {
            this.serverHandler = new ServerHandler();
            this.playerHandler = new PlayerHandler();
            this.warheadHandler = new WarheadHandler();
            this.mapHandler = new MapHandler();
            this.itemHandler = new ItemHandler();
            this.scp914Handler = new Scp914Handler();
            this.scp096Handler = new Scp096Handler();

            Exiled.Events.Handlers.Server.WaitingForPlayers += this.serverHandler.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += this.serverHandler.OnRoundStarted;

            Exiled.Events.Handlers.Player.Destroying += this.playerHandler.OnDestroying;
            Exiled.Events.Handlers.Player.Spawned += this.playerHandler.OnSpawned;
            Exiled.Events.Handlers.Player.Escaping += this.playerHandler.OnEscaping;
            Exiled.Events.Handlers.Player.Hurting += this.playerHandler.OnHurting;
            Exiled.Events.Handlers.Player.Dying += this.playerHandler.OnDying;
            Exiled.Events.Handlers.Player.Died += this.playerHandler.OnDied;
            Exiled.Events.Handlers.Player.ChangingRole += this.playerHandler.OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingItem += this.playerHandler.OnChangingItem;
            Exiled.Events.Handlers.Player.UsingItem += this.playerHandler.OnUsingItem;
            Exiled.Events.Handlers.Player.PickingUpItem += this.playerHandler.OnPickingUpItem;
            Exiled.Events.Handlers.Player.DroppingItem += this.playerHandler.OnDroppingItem;
            Exiled.Events.Handlers.Player.Verified += this.playerHandler.OnVerified;
            Exiled.Events.Handlers.Player.FailingEscapePocketDimension += this.playerHandler.OnFailingEscapePocketDimension;
            Exiled.Events.Handlers.Player.EscapingPocketDimension += this.playerHandler.OnEscapingPocketDimension;
            Exiled.Events.Handlers.Player.UnlockingGenerator += this.playerHandler.OnUnlockingGenerator;
            Exiled.Events.Handlers.Player.PreAuthenticating += this.playerHandler.OnPreAuthenticating;
            Exiled.Events.Handlers.Player.Shooting += this.playerHandler.OnShooting;
            Exiled.Events.Handlers.Player.ReloadingWeapon += this.playerHandler.OnReloading;
            Exiled.Events.Handlers.Player.ReceivingEffect += this.playerHandler.OnReceivingEffect;

            Exiled.Events.Handlers.Warhead.Stopping += this.warheadHandler.OnStopping;
            Exiled.Events.Handlers.Warhead.Starting += this.warheadHandler.OnStarting;

            Exiled.Events.Handlers.Scp106.Teleporting += this.playerHandler.OnTeleporting;

            Exiled.Events.Handlers.Scp914.Activating += this.playerHandler.OnActivating;
            Exiled.Events.Handlers.Scp914.ChangingKnobSetting += this.playerHandler.OnChangingKnobSetting;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer += this.playerHandler.OnUpgradingPlayer;

            Exiled.Events.Handlers.Map.ExplodingGrenade += this.mapHandler.OnExplodingGrenade;
            Exiled.Events.Handlers.Map.GeneratorActivating += this.mapHandler.OnGeneratorActivated;

            Exiled.Events.Handlers.Item.ChangingAmmo += this.itemHandler.OnChangingAmmo;
            Exiled.Events.Handlers.Item.ChangingAttachments += this.itemHandler.OnChangingAttachments;
            Exiled.Events.Handlers.Item.ReceivingPreference += this.itemHandler.OnReceivingPreference;

            Exiled.Events.Handlers.Scp914.UpgradingPickup += this.scp914Handler.OnUpgradingItem;

            Exiled.Events.Handlers.Scp096.AddingTarget += this.scp096Handler.OnAddingTarget;
        }

        /// <summary>
        /// Unregisters the plugin events.
        /// </summary>
        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.serverHandler.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= this.serverHandler.OnRoundStarted;

            Exiled.Events.Handlers.Player.Destroying -= this.playerHandler.OnDestroying;
            Exiled.Events.Handlers.Player.Dying -= this.playerHandler.OnDying;
            Exiled.Events.Handlers.Player.Died -= this.playerHandler.OnDied;
            Exiled.Events.Handlers.Player.ChangingRole -= this.playerHandler.OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingItem -= this.playerHandler.OnChangingItem;
            Exiled.Events.Handlers.Player.PickingUpItem += this.playerHandler.OnPickingUpItem;
            Exiled.Events.Handlers.Player.Verified -= this.playerHandler.OnVerified;
            Exiled.Events.Handlers.Player.FailingEscapePocketDimension -= this.playerHandler.OnFailingEscapePocketDimension;
            Exiled.Events.Handlers.Player.EscapingPocketDimension -= this.playerHandler.OnEscapingPocketDimension;
            Exiled.Events.Handlers.Player.UnlockingGenerator -= this.playerHandler.OnUnlockingGenerator;
            Exiled.Events.Handlers.Player.PreAuthenticating -= this.playerHandler.OnPreAuthenticating;

            Exiled.Events.Handlers.Warhead.Stopping -= this.warheadHandler.OnStopping;
            Exiled.Events.Handlers.Warhead.Starting -= this.warheadHandler.OnStarting;

            Exiled.Events.Handlers.Scp106.Teleporting -= this.playerHandler.OnTeleporting;

            Exiled.Events.Handlers.Scp914.Activating -= this.playerHandler.OnActivating;
            Exiled.Events.Handlers.Scp914.ChangingKnobSetting -= this.playerHandler.OnChangingKnobSetting;

            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.mapHandler.OnExplodingGrenade;
            Exiled.Events.Handlers.Map.GeneratorActivating -= this.mapHandler.OnGeneratorActivated;

            Exiled.Events.Handlers.Item.ChangingAmmo -= this.itemHandler.OnChangingAmmo;
            Exiled.Events.Handlers.Item.ChangingAttachments -= this.itemHandler.OnChangingAttachments;
            Exiled.Events.Handlers.Item.ReceivingPreference -= this.itemHandler.OnReceivingPreference;

            Exiled.Events.Handlers.Scp914.UpgradingPickup -= this.scp914Handler.OnUpgradingItem;

            Exiled.Events.Handlers.Scp096.AddingTarget -= this.scp096Handler.OnAddingTarget;

            this.serverHandler = null;
            this.playerHandler = null;
            this.warheadHandler = null;
            this.mapHandler = null;
            this.itemHandler = null;
            this.scp914Handler = null;
            this.scp096Handler = null;
        }
    }
}