// -----------------------------------------------------------------------
// <copyright file="SettingBase.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Core.UserSettings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Exiled.API.Interfaces;
    using global::UserSettings.ServerSpecific;
    using Interfaces;

    /// <summary>
    /// A base class for all Server Specific Settings.
    /// </summary>
    public abstract class SettingBase : TypeCastObject<SettingBase>, IWrapper<ServerSpecificSettingBase>
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> that contains <see cref="SettingBase"/> that are currently with players.
        /// </summary>
        public static readonly Dictionary<Player, HashSet<SettingBase>> PlayerSettings = new();

        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> that contains <see cref="int"/> Id and <see cref="SettingBase"/> with this Id.
        /// </summary>
        public static readonly Dictionary<int, SettingBase> Settings = new();

        /// <summary>
        /// A <see cref="HashSet{TValue}"/> that contains <see cref="SettingBase"/> that was ever Defined.
        /// </summary>
        public static readonly HashSet<SettingBase> SyncedSettings = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingBase"/> class.
        /// </summary>
        /// <param name="settingBase">A <see cref="ServerSpecificSettingBase"/> instance.</param>
        /// <param name="header"><inheritdoc cref="Header"/></param>
        internal SettingBase(ServerSpecificSettingBase settingBase, HeaderSetting header)
        {
            Base = settingBase;
            Header = header;
            Settings.Add(settingBase.SettingId, this);
            UpdateSynced();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingBase"/> class.
        /// </summary>
        /// <param name="settingBase"><inheritdoc cref="Base"/></param>
        internal SettingBase(ServerSpecificSettingBase settingBase)
        {
            Base = settingBase;
        }

        /// <summary>
        /// Gets or sets next Id to give.
        /// </summary>
        public static int NextId { get; set; }

        /// <inheritdoc/>
        public ServerSpecificSettingBase Base { get; }

        /// <summary>
        /// Gets or sets the id of this setting.
        /// </summary>
        public int Id
        {
            get => Base.SettingId;
            set => Base.SetId(value, string.Empty);
        }

        /// <summary>
        /// Gets or sets the label of this setting.
        /// </summary>
        public string Label
        {
            get => Base.Label;
            set => Base.Label = value;
        }

        /// <summary>
        /// Gets or sets the description of this setting.
        /// </summary>
        public string HintDescription
        {
            get => Base.HintDescription;
            set => Base.HintDescription = value;
        }

        /// <summary>
        /// Gets the response mode of this setting.
        /// </summary>
        public ServerSpecificSettingBase.UserResponseMode ResponseMode => Base.ResponseMode;

        /// <summary>
        /// Gets or sets the header of this setting.
        /// </summary>
        /// <remarks>Can be <c>null</c>.</remarks>
        public HeaderSetting Header { get; set; }

        /// <summary>
        /// Tries to get the setting with the specified id.
        /// </summary>
        /// <param name="player">Player who has received the setting.</param>
        /// <param name="id">Id of the setting.</param>
        /// <param name="setting">A <see cref="SettingBase"/> instance if found. Otherwise, <c>null</c>.</param>
        /// <typeparam name="T">Type of the setting.</typeparam>
        /// <returns><c>true</c> if the setting was found, <c>false</c> otherwise.</returns>
        public static bool TryGetSetting<T>(Player player, int id, out T setting)
            where T : SettingBase
        {
            setting = null;

            if (!PlayerSettings.TryGetValue(player, out HashSet<SettingBase> list))
                return false;

            setting = (T)list.FirstOrDefault(x => x.Id == id);
            return setting != null;
        }

        /// <summary>
        /// Send DefinedSettings to specified Player.
        /// </summary>
        /// <param name="player">A player which get settings.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void SendToPlayer(Player player, Func<Player, bool> predicate = null)
        {
            if (predicate == null || predicate(player))
                return;

            ServerSpecificSettingsSync.SendToPlayer(player.ReferenceHub);
        }

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="collection">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void AddToAll(List<SettingBase> collection, Func<Player, bool> predicate = null)
        {
            foreach (Player player in Player.List)
                AddToPlayer(player, collection, predicate);
        }

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="setting">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void AddToAll(SettingBase setting, Func<Player, bool> predicate = null)
        {
            foreach (Player player in Player.List)
                AddToPlayer(player, setting, predicate);
        }

        /// <summary>
        /// Syncs setting with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        /// <param name="setting">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void AddToPlayer(Player player, SettingBase setting, Func<Player, bool> predicate = null)
        {
            if (predicate != null && !predicate(player))
                return;

            if (PlayerSettings.TryGetValue(player, out HashSet<SettingBase> list))
            {
                Log.Info(setting.ToString());
                list.Add(setting);
            }
            else
            {
                list = new HashSet<SettingBase> { setting };
                PlayerSettings.Add(player, list);
            }
        }

        /// <summary>
        /// Syncs setting with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        /// <param name="collection">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void AddToPlayer(Player player, List<SettingBase> collection, Func<Player, bool> predicate = null)
        {
            if (predicate != null && !predicate(player))
                return;

            if (PlayerSettings.TryGetValue(player, out HashSet<SettingBase> list))
                PlayerSettings[player] = list.Concat(collection).ToHashSet();
            else
                PlayerSettings[player] = collection.ToHashSet();
        }

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="collection">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveFromAll(List<SettingBase> collection, Func<Player, bool> predicate = null)
        {
            foreach (Player player in Player.List)
                RemoveFromPlayer(player, collection, predicate);
        }

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="setting">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveFromAll(SettingBase setting, Func<Player, bool> predicate = null)
        {
            foreach (Player player in Player.List)
                RemoveFromPlayer(player, setting, predicate);
        }

        /// <summary>
        /// Syncs setting with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        /// <param name="collection">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveFromPlayer(Player player, List<SettingBase> collection, Func<Player, bool> predicate = null)
        {
            if ((predicate != null && !predicate(player)) || !PlayerSettings.TryGetValue(player, out HashSet<SettingBase> list))
                return;

            list.RemoveWhere(collection.Contains);
        }

        /// <summary>
        /// Syncs setting with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        /// <param name="setting">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveFromPlayer(Player player, SettingBase setting, Func<Player, bool> predicate = null)
        {
            if ((predicate != null && !predicate(player)) || !PlayerSettings.TryGetValue(player, out HashSet<SettingBase> list))
                return;

            list.Remove(setting);
        }

        /// <summary>
        /// Registers all settings from the specified collection.
        /// </summary>
        /// <param name="settings">A collection of settings to register.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="SettingBase"/> instances that were successfully registered.</returns>
        /// <remarks>This method is used to sync new settings with players.</remarks>
        public static IEnumerable<SettingBase> GroupByHeaders(IEnumerable<SettingBase> settings)
        {
            IEnumerable<IGrouping<HeaderSetting, SettingBase>> grouped = settings.Where(s => s != null).GroupBy(s => s.Header);

            List<SettingBase> result = new();
            foreach (IGrouping<HeaderSetting, SettingBase> grouping in grouped)
            {
                if (grouping.Key != null)
                    result.Add(grouping.Key);

                result.AddRange(grouping);
            }

            return result;
        }

        /// <summary>
        /// Penis Penis Penis.
        /// </summary>
        /// <param name="referenceHub"> f.</param>
        /// <param name="settingBase"> fuck you man.</param>
        public static void OnRandomSettingTriggered(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (!Player.TryGet(referenceHub, out Player player) ||
                !Settings.TryGetValue(settingBase.SettingId, out SettingBase setting) ||
                !PlayerSettings.TryGetValue(player, out HashSet<SettingBase> settingBases) ||
                !settingBases.Contains(setting) || setting is not ISettingHandler handler)
                return;

            handler.Handle(player, setting);
        }

        /// <summary>
        /// Trying to add new setting to a SyncedSettings, if does so, creates new array and put it as DefinedSettings.
        /// </summary>
        public void UpdateSynced()
        {
            if (!SyncedSettings.Add(this))
                return;

            ServerSpecificSettingsSync.DefinedSettings = GroupByHeaders(SyncedSettings).Select(x => x.Base).ToArray();
        }

        /// <summary>
        /// Returns a string representation of this <see cref="SettingBase"/>.
        /// </summary>
        /// <returns>A string in human-readable format.</returns>
        public override string ToString() => $"{Id} ({Label}) [{HintDescription}] {{{ResponseMode}}} ^{Header}^";

        /// <summary>
        /// Represents a config for TextInputSetting.
        /// </summary>
        /// <typeparam name="TSetting">Penis.</typeparam>
        public abstract class SettingConfig<TSetting>
                    where TSetting : SettingBase
        {
            /// <summary>
            /// Creates a TextInputSetting instanse.
            /// </summary>
            /// <returns>TextInputSetting.</returns>
            public abstract TSetting Create();
        }
    }
}