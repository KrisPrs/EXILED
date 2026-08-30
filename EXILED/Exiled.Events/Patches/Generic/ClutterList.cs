// -----------------------------------------------------------------------
// <copyright file="ClutterList.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Generic
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    using API.Features;
    using API.Features.Pools;

    using HarmonyLib;

    using UnityEngine;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="Clutter.SpawnClutter"/> to keep track of the clutter spawned in each <see cref="Room"/>.
    /// </summary>
    /// <remarks>The spawned prop is only known as the return value of the Object.Instantiate call, since the clutter component holding it is destroyed right after.</remarks>
    [HarmonyPatch(typeof(global::Clutter), nameof(global::Clutter.SpawnClutter))]
    internal class ClutterList
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            int index = newInstructions.FindIndex(instruction =>
                instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo { Name: nameof(Object.Instantiate) });

            if (index == -1)
            {
                Log.Error($"{nameof(ClutterList)}: the clutter instantiation could not be found, {nameof(Room)}.{nameof(Room.Clutter)} will stay empty.");
            }
            else
            {
                const int offset = 1;

                // Room.RegisterClutter(instance);
                newInstructions.InsertRange(
                    index + offset,
                    new CodeInstruction[]
                    {
                        new(OpCodes.Dup),
                        new(OpCodes.Call, Method(typeof(Room), nameof(Room.RegisterClutter))),
                    });
            }

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}
