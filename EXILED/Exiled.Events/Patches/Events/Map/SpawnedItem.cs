// -----------------------------------------------------------------------
// <copyright file="SpawnedItem.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Map
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features.Pickups;
    using API.Features.Pools;
    using Attributes;
    using Exiled.Events.EventArgs.Map;
    using Exiled.Events.EventArgs.Player;
    using Handlers;
    using HarmonyLib;
    using InventorySystem.Items.Pickups;
    using MapGeneration.Distributors;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="ItemDistributor.SpawnPickup" />.
    /// Adds the <see cref="Map.SpawnedItem" /> event.
    /// </summary>
    [EventPatch(typeof(Map), nameof(Map.SpawnedItem))]
    [HarmonyPatch(typeof(ItemDistributor), nameof(ItemDistributor.SpawnPickup))]
    internal static class SpawnedItem
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            newInstructions.InsertRange(newInstructions.Count - 1, new CodeInstruction[]
            {
                // ItemPickupBase
                new(OpCodes.Ldarg_0),

                // SpawnedItemEventArgs ev = new(Pickup)
                new(OpCodes.Newobj, GetDeclaredConstructors(typeof(SpawnedItemEventArgs))[0]),
                new(OpCodes.Call, Method(typeof(Map), nameof(Map.OnSpawnedItem))),
            });

            foreach (CodeInstruction t in newInstructions)
                yield return t;

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}