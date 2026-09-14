// -----------------------------------------------------------------------
// <copyright file="UserSettingsArchive.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Core.UserSettings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Keeps a persistent id for every Server Specific Setting, keyed by its label, so that ids stay the same across restarts.
    /// </summary>
    internal static class UserSettingsArchive
    {
        private static readonly string ArchivesFolder = Path.Combine(Paths.Exiled, "UserSettingsArchives");

        private static readonly Dictionary<string, int> Entries = new();

        private static string filePath;

        private static bool loaded;

        /// <summary>
        /// Gets a persistent id for a label, allocating and saving a new one when the label is not archived yet.
        /// </summary>
        /// <param name="label">A label of the setting.</param>
        /// <returns>An id that stays the same for this label across restarts.</returns>
        internal static int ProvideId(string label)
        {
            if (!loaded)
                Load();

            if (Entries.TryGetValue(label, out int id))
                return id;

            id = Entries.Count == 0 ? 1 : Entries.Values.Max() + 1;
            Entries[label] = id;
            Save();

            return id;
        }

        private static void Load()
        {
            loaded = true;
            filePath = Path.Combine(ArchivesFolder, $"{Server.Port}.yml");
            Entries.Clear();

            if (!File.Exists(filePath))
                return;

            string raw;

            try
            {
                raw = File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch (Exception exception)
            {
                Log.Error($"Unable to read the settings archive '{filePath}': {exception.Message}. All ids will be reassigned.");
                return;
            }

            try
            {
                Dictionary<string, int> parsed = Loader.Loader.Deserializer.Deserialize<Dictionary<string, int>>(raw);

                if (parsed != null)
                {
                    foreach (KeyValuePair<string, int> pair in parsed)
                        Entries[pair.Key] = pair.Value;

                    return;
                }
            }
            catch (Exception exception)
            {
                Log.Error($"The settings archive '{filePath}' is damaged: {exception.Message}");
            }

            Log.Warn($"Recovered {Salvage(raw)} entries from the damaged settings archive. Rewriting '{filePath}'.");
            Save();
        }

        private static int Salvage(string raw)
        {
            int recovered = 0;

            foreach (string line in raw.Split('\n'))
            {
                string trimmed = line.Trim();

                if (trimmed.Length == 0 || trimmed[0] == '#')
                    continue;

                int separator = trimmed.LastIndexOf(": ", StringComparison.Ordinal);

                if (separator <= 0)
                    continue;

                if (!int.TryParse(trimmed.Substring(separator + 2).Trim(), out int id) || id < 1)
                    continue;

                string label = Unquote(trimmed.Substring(0, separator).Trim());

                if (label.Length == 0 || Entries.ContainsKey(label) || Entries.ContainsValue(id))
                    continue;

                Entries[label] = id;
                recovered++;
            }

            return recovered;
        }

        private static string Unquote(string value)
        {
            if (value.Length < 2)
                return value;

            char quote = value[0];

            if ((quote == '\'' || quote == '"') && value[value.Length - 1] == quote)
                return value.Substring(1, value.Length - 2);

            return value;
        }

        private static void Save()
        {
            try
            {
                if (!Directory.Exists(ArchivesFolder))
                    Directory.CreateDirectory(ArchivesFolder);

                File.WriteAllText(filePath, Loader.Loader.Serializer.Serialize(Entries), new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                Log.Error($"Unable to save the settings archive '{filePath}': {exception.Message}");
            }
        }
    }
}
