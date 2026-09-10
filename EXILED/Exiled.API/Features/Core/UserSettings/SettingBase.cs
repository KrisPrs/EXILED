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
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Exiled.API.Features.Pools;
    using Exiled.API.Interfaces;
    using global::UserSettings.ServerSpecific;

    /// <summary>
    /// A base class for all Server Specific Settings.
    /// </summary>
    public class SettingBase : TypeCastObject<SettingBase>, IWrapper<ServerSpecificSettingBase>
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> that contains <see cref="SettingBase"/> that were received by a players.
        /// </summary>
        internal static readonly Dictionary<Player, List<SettingBase>> ReceivedSettings = new();

        /// <summary>
        /// A collection that contains all settings that were sent to clients.
        /// </summary>
        internal static readonly List<SettingBase> Settings = new();

        /// <summary>
        /// A collection that contains all ids taken by buttons.
        /// </summary>
        internal static readonly HashSet<int> TakenIds = new();

        private static readonly Dictionary<Player, bool> WasPressed = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingBase"/> class.
        /// </summary>
        /// <param name="settingBase">A <see cref="ServerSpecificSettingBase"/> instance.</param>
        /// <param name="header"><inheritdoc cref="Header"/></param>
        /// <param name="onChanged"><inheritdoc cref="OnChanged"/></param>
        internal SettingBase(ServerSpecificSettingBase settingBase, HeaderSetting header, Action<Player, SettingBase> onChanged)
        {
            this.Base = settingBase;

            this.Header = header;
            this.OnChanged = onChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingBase"/> class.
        /// </summary>
        /// <param name="settingBase"><inheritdoc cref="Base"/></param>
        internal SettingBase(ServerSpecificSettingBase settingBase)
        {
            this.Base = settingBase;

            if (this.OriginalDefinition != null)
            {
                this.Header = this.OriginalDefinition.Header;
                this.OnChanged = this.OriginalDefinition.OnChanged;
                this.Label = this.OriginalDefinition.Label;
                this.HintDescription = this.OriginalDefinition.HintDescription;
            }
        }

        /// <summary>
        /// Gets the list of all synced settings.
        /// </summary>
        public static IReadOnlyDictionary<Player, ReadOnlyCollection<SettingBase>> SyncedList
            => new ReadOnlyDictionary<Player, ReadOnlyCollection<SettingBase>>(ReceivedSettings.ToDictionary(x => x.Key, x => x.Value.AsReadOnly()));

        /// <summary>
        /// Gets the list of settings that were used as a prefabs.
        /// </summary>
        public static IReadOnlyCollection<SettingBase> List => Settings;

        /// <inheritdoc/>
        public ServerSpecificSettingBase Base { get; }

        /// <summary>
        /// Gets or sets the id of this setting.
        /// </summary>
        public int Id
        {
            get => this.Base.SettingId;
            set => this.Base.SetId(value, string.Empty);
        }

        /// <summary>
        /// Gets or sets the label of this setting.
        /// </summary>
        public string Label
        {
            get => this.Base.Label;
            set => this.Base.Label = value;
        }

        /// <summary>
        /// Gets or sets the description of this setting.
        /// </summary>
        public string HintDescription
        {
            get => this.Base.HintDescription;
            set => this.Base.HintDescription = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the setting receives updates from server (and client stops sending updates).
        /// </summary>
        /// <remarks>
        /// Useful for displaying information, you cannot receive updates from a setting with this enabled.
        /// </remarks>
        public bool IsServerOnly
        {
            get => this.Base.IsServerOnly;
            set => this.Base.IsServerOnly = value;
        }

        /// <summary>
        /// Gets or sets a value controlling if this setting is shared across servers with <b>same Ip address!</b>. Default value is 255.
        /// </summary>
        /// <remarks>
        /// Settings with this value between 0 and 20 will store the servers Ip (or other unique identifier) instead of port, meaning the client treats a setting between 1.1.1.1:7777 and 1.1.1.1:7778 the same.
        /// <br/>
        /// <br/>
        /// If this value is above 20, the aforementioned behavior will not occur and the setting will behave as normal.
        /// </remarks>
        public byte CollectionId
        {
            get => this.Base.CollectionId;
            set => this.Base.CollectionId = value;
        }

        /// <summary>
        /// Gets the response mode of this setting.
        /// </summary>
        public ServerSpecificSettingBase.UserResponseMode ResponseMode => this.Base.ResponseMode;

        /// <summary>
        /// Gets the setting that was sent to players.
        /// </summary>
        /// <remarks>Can be <c>null</c> if this <see cref="SettingBase"/> is a prefab.</remarks>
        public SettingBase OriginalDefinition => Settings.Find(x => x.Id == this.Id);

        /// <summary>
        /// Gets or sets the header of this setting.
        /// </summary>
        /// <remarks>Can be <c>null</c>.</remarks>
        public HeaderSetting Header { get; set; }

        /// <summary>
        /// Gets or sets the action to be executed when this setting is changed.
        /// </summary>
        public Action<Player, SettingBase> OnChanged { get; set; }

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

            if (!ReceivedSettings.TryGetValue(player, out List<SettingBase> list))
                return false;

            setting = (T)list.FirstOrDefault(x => x.Id == id);
            return setting != null;
        }

        /// <summary>
        /// Tries to get the setting with the specified id.
        /// </summary>
        /// <param name="player">Player who has received the setting.</param>
        /// <param name="id">Id of the setting.</param>
        /// <param name="setting">A <see cref="SettingBase"/> instance if found. Otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the setting was found, <c>false</c> otherwise.</returns>
        public static bool TryGetSetting(Player player, int id, out SettingBase setting) => TryGetSetting<SettingBase>(player, id, out setting);

        /// <summary>
        /// Creates a new instance of this setting.
        /// </summary>
        /// <param name="settingBase">A <see cref="ServerSpecificSettingBase"/> instance.</param>
        /// <returns>A new instance of this setting.</returns>
        /// <remarks>
        /// This method is used only to create a new instance of <see cref="SettingBase"/> from an existing <see cref="ServerSpecificSettingBase"/> instance.
        /// New setting won't be synced with players.
        /// </remarks>
        public static SettingBase Create(ServerSpecificSettingBase settingBase) => settingBase switch
        {
            SSButton button => new ButtonSetting(button),
            SSDropdownSetting dropdownSetting => new DropdownSetting(dropdownSetting),
            SSTextArea textArea => new TextInputSetting(textArea),
            SSGroupHeader header => new HeaderSetting(header),
            SSKeybindSetting keybindSetting => new KeybindSetting(keybindSetting),
            SSTwoButtonsSetting twoButtonsSetting => new TwoButtonsSetting(twoButtonsSetting),
            SSPlaintextSetting plainTextSetting => new UserTextInputSetting(plainTextSetting),
            SSSliderSetting sliderSetting => new SliderSetting(sliderSetting),
            _ => new SettingBase(settingBase)
        };

        /// <summary>
        /// Creates a new instance of this setting.
        /// </summary>
        /// <param name="settingBase">A<see cref="ServerSpecificSettingBase"/> instance.</param>
        /// <typeparam name="T">Type of the setting.</typeparam>
        /// <returns>A new instance of this setting.</returns>
        /// <remarks>
        /// This method is used only to create a new instance of <see cref="SettingBase"/> from an existing <see cref="ServerSpecificSettingBase"/> instance.
        /// New setting won't be synced with players.
        /// </remarks>
        public static T Create<T>(ServerSpecificSettingBase settingBase)
            where T : SettingBase => Create(settingBase) as T;

        /// <summary>
        /// Syncs setting with all players.
        /// </summary>
        public static void SendToAll() => ServerSpecificSettingsSync.SendToAll();

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="predicate">A requirement to meet.</param>
        public static void SendToAll(Func<Player, bool> predicate)
        {
            foreach (Player player in Player.List)
            {
                if (predicate(player))
                    SendToPlayer(player);
            }
        }

        /// <summary>
        /// Syncs setting with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        public static void SendToPlayer(Player player) => ServerSpecificSettingsSync.SendToPlayer(player.ReferenceHub);

        /// <summary>
        /// Syncs specific settings with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        /// <param name="settings">Settings to send to the player.</param>
        public static void SendToPlayer(Player player, IEnumerable<SettingBase> settings) =>
            ServerSpecificSettingsSync.SendToPlayer(player.ReferenceHub, settings.Select(setting => setting.Base).ToArray());

        /// <summary>
        /// Registers all settings from the specified collection.
        /// </summary>
        /// <param name="settings">A collection of settings to register.</param>
        /// <param name="predicate">A requirement to meet when sending settings to players.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="SettingBase"/> instances that were successfully registered.</returns>
        /// <remarks>This method is used to sync new settings with players.</remarks>
        public static IEnumerable<SettingBase> Register(IEnumerable<SettingBase> settings, Func<Player, bool> predicate = null)
        {
            SettingBase[] settingBases = settings as SettingBase[] ?? settings.ToArray();

            List<SettingBase> fullList = (ServerSpecificSettingsSync.DefinedSettings ?? Array.Empty<ServerSpecificSettingBase>())
                .Select(Create)
                .Concat(settingBases)
                .ToList();

            Dictionary<string, HeaderSetting> headersDict = new();
            Dictionary<string, List<SettingBase>> settingsByLabel = new();
            List<SettingBase> settingsWithoutHeaders = new();

            foreach (SettingBase setting in fullList)
            {
                if (setting is HeaderSetting)
                    continue;

                if (setting.Header == null)
                {
                    settingsWithoutHeaders.Add(setting);
                    continue;
                }

                if (!headersDict.ContainsKey(setting.Header.Label))
                    headersDict[setting.Header.Label] = setting.Header;

                if (!settingsByLabel.ContainsKey(setting.Header.Label))
                    settingsByLabel[setting.Header.Label] = new List<SettingBase>();

                settingsByLabel[setting.Header.Label].Add(setting);
            }

            List<SettingBase> result = new();
            foreach (HeaderSetting header in headersDict.Values.OrderBy(h => h.Label))
            {
                result.Add(header);
                result.AddRange(settingsByLabel[header.Label]);
            }

            result.AddRange(settingsWithoutHeaders);

            ServerSpecificSettingsSync.DefinedSettings = result.Select(x => x.Base).ToArray();
            Settings.AddRange(settingBases);

            if (predicate == null)
                SendToAll();
            else
                SendToAll(predicate);

            return result;
        }

        /// <summary>
        /// Registers all settings from the specified collection to player.
        /// </summary>
        /// <param name="player">A player that will receive settings.</param>
        /// <param name="settings">A collection of settings to register.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="SettingBase"/> instances that were successfully registered.</returns>
        /// <remarks>This method is used to sync new settings with players.</remarks>
        public static IEnumerable<SettingBase> Register(Player player, IEnumerable<SettingBase> settings)
        {
            SettingBase[] settingBases = settings as SettingBase[] ?? settings.ToArray();

            List<SettingBase> fullList = (ServerSpecificSettingsSync.DefinedSettings ?? Array.Empty<ServerSpecificSettingBase>())
                .Select(Create)
                .Concat(settingBases)
                .ToList();

            Dictionary<string, HeaderSetting> headersDict = new();
            Dictionary<string, List<SettingBase>> settingsByLabel = new();
            List<SettingBase> settingsWithoutHeaders = new();

            foreach (SettingBase setting in fullList)
            {
                if (setting is HeaderSetting)
                    continue;

                if (setting.Header == null)
                {
                    settingsWithoutHeaders.Add(setting);
                    continue;
                }

                if (!headersDict.ContainsKey(setting.Header.Label))
                    headersDict[setting.Header.Label] = setting.Header;

                if (!settingsByLabel.ContainsKey(setting.Header.Label))
                    settingsByLabel[setting.Header.Label] = new List<SettingBase>();

                settingsByLabel[setting.Header.Label].Add(setting);
            }

            List<SettingBase> result = new();
            foreach (HeaderSetting header in headersDict.Values.OrderBy(h => h.Label))
            {
                result.Add(header);
                result.AddRange(settingsByLabel[header.Label]);
            }

            result.AddRange(settingsWithoutHeaders);

            ServerSpecificSettingsSync.DefinedSettings = result.Select(x => x.Base).ToArray();
            Settings.AddRange(settingBases);
            SendToPlayer(player);

            return result;
        }

        /// <summary>
        /// Removes settings from players.
        /// </summary>
        /// <param name="predicate">Determines which players will receive this update.</param>
        /// <param name="settings">Settings to remove. If <c>null</c>, all settings will be removed.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="SettingBase"/> instances that were successfully removed.</returns>
        /// <remarks>This method is used to unsync settings from players. Using it with <see cref="Register(IEnumerable{SettingBase},Func{Player,bool})"/> provides an opportunity to update synced settings.</remarks>
        public static IEnumerable<SettingBase> Unregister(Func<Player, bool> predicate = null, IEnumerable<SettingBase> settings = null)
        {
            List<ServerSpecificSettingBase> list = ListPool<ServerSpecificSettingBase>.Pool.Get(ServerSpecificSettingsSync.DefinedSettings);
            List<SettingBase> list2 = new((settings ?? Settings).Where(setting => list.Remove(setting.Base)));

            ServerSpecificSettingsSync.DefinedSettings = list.ToArray();

            if (predicate == null)
                SendToAll();
            else
                SendToAll(predicate);

            ListPool<ServerSpecificSettingBase>.Pool.Return(list);

            return list2;
        }

        /// <summary>
        /// Removes settings from players.
        /// </summary>
        /// <param name="player">Determines which player will receive this update.</param>
        /// <param name="settings">Settings to remove. If <c>null</c>, all settings will be removed.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="SettingBase"/> instances that were successfully removed.</returns>
        /// <remarks>This method is used to unsync settings from players. Using it with <see cref="Register(Player,IEnumerable{SettingBase})"/> provides an opportunity to update synced settings.</remarks>
        public static IEnumerable<SettingBase> Unregister(Player player, IEnumerable<SettingBase> settings = null)
        {
            List<ServerSpecificSettingBase> list = ListPool<ServerSpecificSettingBase>.Pool.Get(ServerSpecificSettingsSync.DefinedSettings);
            List<SettingBase> list2 = new((settings ?? Settings).Where(setting => list.Remove(setting.Base)));

            ServerSpecificSettingsSync.DefinedSettings = list.ToArray();

            SendToPlayer(player);

            ListPool<ServerSpecificSettingBase>.Pool.Return(list);

            return list2;
        }

        /// <summary>
        /// Sends an updated label and hint to clients.
        /// </summary>
        /// <param name="label"><inheritdoc cref="Label"/></param>
        /// <param name="hint"><inheritdoc cref="Hint"/></param>
        /// <param name="overrideValue">If false, sends fake values.</param>
        /// <param name="filter">Who to send the update to.</param>
        public void UpdateLabelAndHint(string label, string hint, bool overrideValue = true, Predicate<Player> filter = null)
        {
            filter ??= _ => true;
            this.Base.SendUpdate(label, hint, overrideValue, hub => filter(Player.Get(hub)));
        }

        /// <summary>
        /// Returns a string representation of this <see cref="SettingBase"/>.
        /// </summary>
        /// <returns>A string in human-readable format.</returns>
        public override string ToString() => $"{this.Id} ({this.Label}) [{this.HintDescription}] {{{this.ResponseMode}}} ^{this.Header}^";

        /// <summary>
        /// Internal method that fires when a setting is updated.
        /// </summary>
        /// <param name="hub"><see cref="ReferenceHub"/> that has updates the setting.</param>
        /// <param name="settingBase">A new updated setting.</param>
        internal static void OnSettingUpdated(ReferenceHub hub, ServerSpecificSettingBase settingBase)
        {
            if (!Player.TryGet(hub, out Player player) || hub.IsHost)
                return;

            SettingBase setting;

            if (!ReceivedSettings.TryGetValue(player, out List<SettingBase> list))
            {
                setting = Create(settingBase);
                ReceivedSettings.Add(player, new() { setting });

                if (setting.Is(out ButtonSetting _))
                    goto invoke;

                return;
            }

            if (!list.Exists(x => x.Id == settingBase.SettingId))
            {
                setting = Create(settingBase);
                list.Add(setting);

                if (setting.Is(out ButtonSetting _))
                    goto invoke;

                return;
            }

            setting = list.Find(x => x.Id == settingBase.SettingId);

            invoke:

            if (setting.OriginalDefinition == null)
            {
                Settings.Add(Create(settingBase.OriginalDefinition));
            }

            if (setting is KeybindSetting keybindSetting)
            {
                if (!WasPressed.TryGetValue(player, out bool wasPressedPreviously))
                    wasPressedPreviously = false;

                if (wasPressedPreviously == keybindSetting.IsPressed)
                    return;

                WasPressed[player] = keybindSetting.IsPressed;
            }

            setting.OriginalDefinition?.OnChanged?.Invoke(player, setting);
        }

        /// <summary>
        /// A base class for all Server Specific Settings configs.
        /// </summary>
        /// <typeparam name="TSetting">Type of Server Specific Setting.</typeparam>
        public abstract class SettingConfig<TSetting>
            where TSetting : SettingBase
        {
            private static readonly string ArchivesFolder = Path.Combine(Paths.Exiled, "UserSettingsArchives");

            private static readonly string FilePath = Path.Combine(ArchivesFolder, $"{Server.Port}.yml");

            private static Dictionary<string, int> loadedArchives = new Dictionary<string, int>();

            private static bool archivesLoaded = false;

            /// <summary>
            /// Creates a SettingBase instanse.
            /// </summary>
            /// <returns>TextInputSetting.</returns>
            public abstract TSetting Create();

            /// <summary>
            /// Provides and id from archives for a settings.
            /// </summary>
            /// <param name="label">A label for the setting. Must be unique.</param>
            /// <returns>An id for the setting.</returns>
            protected int ProvideIdFromArchives(string label)
            {
                Log.Debug($"Started providing id for button with label - {label}");

                if (!Directory.Exists(ArchivesFolder))
                {
                    Log.Debug("Archives folder isn't created. Creating...");
                    Directory.CreateDirectory(ArchivesFolder);
                }

                using FileStream fs = new(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

                if (!archivesLoaded)
                {
                    Log.Debug("Archives aren't loaded. Loading the archives!");

                    using StreamReader reader = new(fs, Encoding.UTF8, true, 1024, leaveOpen: true);

                    loadedArchives = Loader.Loader.Deserializer.
                        Deserialize<Dictionary<string, int>>(reader.ReadToEnd()) ?? new Dictionary<string, int>();

                    archivesLoaded = true;
                }

                Log.Debug($"Checking the archives for label: {label}");

                if (!loadedArchives.TryGetValue(label, out int archivedId))
                {
                    Log.Debug($"Failed to find an archived Id for a setting with label: {label}. Providing a new one...");
                    archivedId = loadedArchives.IsEmpty() ? 1 : FindMinFreeNumber(loadedArchives.Values.ToList());

                    Log.Debug($"Archiving setting with label {label} with new id: {archivedId}");

                    loadedArchives.Add(label, archivedId);

                    using StreamWriter writer = new StreamWriter(fs);
                    writer.Write(Loader.Loader.Serializer.Serialize(loadedArchives));
                }

                Log.Debug($"Returning ID {archivedId} for a setting with label {label}");
                return archivedId;
            }

            private static int FindMinFreeNumber(List<int> numbers)
            {
                int min = numbers.Min();
                int max = numbers.Max();

                IEnumerable<int> free = Enumerable.Range(min, max - min + 1)
                                     .Except(numbers);

                return free.Any() ? free.Min() : max + 1;
            }
        }
    }
}
