// -----------------------------------------------------------------------
// <copyright file="QueryProcessorNullUserIdFix.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
    using HarmonyLib;
    using RemoteAdmin;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="QueryProcessor.OnDestroy"/> to prevent
    /// <see cref="System.ArgumentNullException"/> when a player or NPC
    /// without a UserId (e.g. NPCs, unverified connections) is destroyed.
    /// </summary>
    [HarmonyPatch(typeof(QueryProcessor), "OnDestroy")]
    internal static class QueryProcessorNullUserIdFix
    {
#pragma warning disable SA1313
        private static bool Prefix(QueryProcessor __instance)
#pragma warning restore SA1313
        {
            // QueryProcessor. OnDestroy вызывает TryRemove(UserId) внутри.
            // Если UserId == null, ConcurrentDictionary бросает ArgumentNullException.
            // Это происходит у NPC и игроков, отключившихся до полной аутентификации.
            // Используем GetComponent чтобы не зависеть от имён приватных полей.
            ReferenceHub hub = __instance.GetComponent<ReferenceHub>();
            return hub != null && !string.IsNullOrEmpty(hub.authManager?.UserId);
        }
    }
}