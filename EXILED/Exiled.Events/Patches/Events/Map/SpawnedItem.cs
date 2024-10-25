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

            const int offset = 1;
            int index = newInstructions.FindLastIndex(instruction => instruction.opcode == OpCodes.Call) + offset;

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                // Pickup::Get(ItemPickupBase)
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, FirstMethod(typeof(Pickup), x => x.Name == nameof(Pickup.Get) && !x.IsGenericMethod && x.GetParameters()[0].ParameterType == typeof(ItemPickupBase))),

                // Scp244SpawnedEventArgs ev = new(Pickup)
                new(OpCodes.Newobj, GetDeclaredConstructors(typeof(SpawnedEventArgs))[0]),
                new(OpCodes.Call, Method(typeof(Map), nameof(Map.OnSpawningItem))),
            });

            foreach (CodeInstruction t in newInstructions)
                yield return t;

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}