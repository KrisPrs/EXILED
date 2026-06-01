// -----------------------------------------------------------------------
// <copyright file="FixClearEffectCommand.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using CommandSystem.Commands.RemoteAdmin;
    using CustomPlayerEffects;
    using Exiled.API.Features.Pools;
    using HarmonyLib;
    using InventorySystem;
    using InventorySystem.Items;
    using InventorySystem.Items.Usables.Scp1344;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches the <see cref="Scp1344Item.OnPlayerInventoryDropped"/> method.
    /// Fixes the dupe where 2 copies of SCP-1344 can be created when a player dies.
    /// Bug not reported to NW yet (rare in vanilla servers).
    /// </summary>
    [HarmonyPatch(typeof(ClearEffectsCommand), nameof(ClearEffectsCommand.Execute))]
    public class FixClearEffectCommand
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);
            const int offset = 0;
            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Callvirt && x.operand.ToString().Contains("set_Intensity")) + offset;

            newInstructions.Insert(index, new CodeInstruction(OpCodes.Ldc_R4, 0f));
            newInstructions.Insert(index + 1, new CodeInstruction(OpCodes.Ldc_I4_0));
            newInstructions[index + 2] = new CodeInstruction(OpCodes.Call, Method(typeof(StatusEffectBase), nameof(StatusEffectBase.ServerSetState)));

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}