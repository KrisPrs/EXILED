// -----------------------------------------------------------------------
// <copyright file="BroadcastSingletonFix.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
    using HarmonyLib;

    /// <summary>
    /// Патчит <see cref="global::Broadcast.Singleton"/>: игра ставит флаг кеша раньше поиска хоста, и вызов во время рестарта раунда,
    /// когда хоста уже нет, навсегда оставляет в кеше уничтоженный компонент. Все бродкасты процесса после этого молча не доходят до клиентов.
    /// </summary>
    [HarmonyPatch(typeof(global::Broadcast), nameof(global::Broadcast.Singleton), MethodType.Getter)]
    internal static class BroadcastSingletonFix
    {
        /// <summary>
        /// Возвращает компонент текущего хоста и кеширует его только при успешном поиске.
        /// </summary>
        /// <param name="__result">Компонент хоста или <see langword="null"/>, если хоста сейчас нет.</param>
        /// <returns>Всегда <see langword="false"/>: исходный геттер не выполняется.</returns>
#pragma warning disable SA1313
        private static bool Prefix(ref global::Broadcast __result)
#pragma warning restore SA1313
        {
            if (!global::Broadcast._broadcastSet || global::Broadcast._broadcast == null)
            {
                global::Broadcast._broadcast = ReferenceHub.TryGetLocalHub(out ReferenceHub hub) && hub != null ? hub.GetComponent<global::Broadcast>() : null;
                global::Broadcast._broadcastSet = global::Broadcast._broadcast != null;
            }

            __result = global::Broadcast._broadcast;
            return false;
        }
    }
}
