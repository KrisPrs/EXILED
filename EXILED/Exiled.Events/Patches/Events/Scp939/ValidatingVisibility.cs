// -----------------------------------------------------------------------
// <copyright file="ValidatingVisibility.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Scp939;

using System.Collections.Generic;
using System.Reflection.Emit;

using API.Enums;
using Attributes;
using Exiled.API.Features.Pools;
using Exiled.Events.EventArgs.Scp939;
using HarmonyLib;
using Mirror;
using PlayerRoles.PlayableScps.Scp939;

using static HarmonyLib.AccessTools;

/// <summary>
/// Patches <see cref="Scp939VisibilityController.ValidateVisibility(ReferenceHub)"/>
/// to add the <see cref="Handlers.Scp939.ValidatingVisibility"/> event.
/// </summary>
[EventPatch(typeof(Handlers.Scp939), nameof(Handlers.Scp939.ValidatingVisibility))]
[HarmonyPatch(typeof(Scp939VisibilityController), nameof(Scp939VisibilityController.ValidateVisibility))]
internal class ValidatingVisibility
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

        LocalBuilder ev = generator.DeclareLocal(typeof(ValidatingVisibilityEventArgs));

        Label returnFalse = generator.DefineLabel();
        int offset = 0;

        // ldc.i4.0 - return false after !base.ValidateVisibility
        int index = 4;
        List<CodeInstruction> block1 = CreateEventBlock(generator, ev, returnFalse, Scp939VisibilityState.None);
        MoveLabels(newInstructions[index + offset], block1[0]);
        newInstructions.InsertRange(index + offset, block1);
        offset += block1.Count;

        // ldc.i4.1 - return true after !IsEnemy (target is SCP)
        index = 11;
        List<CodeInstruction> block2 = CreateEventBlock(generator, ev, returnFalse, Scp939VisibilityState.SeenAsScp);
        MoveLabels(newInstructions[index + offset], block2[0]);
        newInstructions.InsertRange(index + offset, block2);
        offset += block2.Count;

        // ldc.i4.1 - return true after !(role is FpcStandardRoleBase)
        index = 20;
        List<CodeInstruction> block3 = CreateEventBlock(generator, ev, returnFalse, Scp939VisibilityState.None);
        MoveLabels(newInstructions[index + offset], block3[0]);
        newInstructions.InsertRange(index + offset, block3);
        offset += block3.Count;

        // ldc.i4.1 - return true after Detonated || IsOneTargetLeft
        index = 26;
        List<CodeInstruction> block4 = CreateEventBlockWithStateCheck(generator, ev, returnFalse);
        MoveLabels(newInstructions[index + offset], block4[0]);
        newInstructions.InsertRange(index + offset, block4);
        offset += block4.Count;

        // ldloc.3 - before return ldloc.3 (in range or lastSeen check)
        index = 89;
        List<CodeInstruction> block5 = CreateEventBlockForRangeCheck(generator, ev, returnFalse);
        MoveLabels(newInstructions[index + offset], block5[0]);
        newInstructions.InsertRange(index + offset, block5);
        offset += block5.Count;

        // ldsfld LastSeen - writing to LastSeen dictionary (SeenByRange)
        index = 91;
        List<CodeInstruction> block6 = CreateEventBlock(generator, ev, returnFalse, Scp939VisibilityState.SeenByRange);
        MoveLabels(newInstructions[index + offset], block6[0]);
        newInstructions.InsertRange(index + offset, block6);

        // return false
        newInstructions.Add(new CodeInstruction(OpCodes.Ldc_I4_0).WithLabels(returnFalse));
        newInstructions.Add(new CodeInstruction(OpCodes.Ret));

        foreach (CodeInstruction instr in newInstructions)
            yield return instr;

        ListPool<CodeInstruction>.Pool.Return(newInstructions);
    }

    private static void MoveLabels(CodeInstruction from, CodeInstruction to)
    {
        to.labels.AddRange(from.labels);
        from.labels.Clear();
    }

    /// <summary>
    /// Creates event block with fixed visibility state.
    /// </summary>
    private static List<CodeInstruction> CreateEventBlock(ILGenerator generator, LocalBuilder ev, Label returnFalse, Scp939VisibilityState state)
    {
        List<CodeInstruction> result = new()
        {
            // state
            new(OpCodes.Ldc_I4, (int)state),
        };

        result.AddRange(CreateEventHandlerBlock(generator, ev, returnFalse));
        return result;
    }

    /// <summary>
    /// Creates event block that checks Detonated to determine state.
    /// </summary>
    private static List<CodeInstruction> CreateEventBlockWithStateCheck(ILGenerator generator, LocalBuilder ev, Label returnFalse)
    {
        Label isDetonated = generator.DefineLabel();
        Label afterStateLoad = generator.DefineLabel();

        List<CodeInstruction> result = new()
        {
            // if (AlphaWarheadController.Detonated) goto isDetonated
            new(OpCodes.Call, PropertyGetter(typeof(AlphaWarheadController), nameof(AlphaWarheadController.Detonated))),
            new(OpCodes.Brtrue, isDetonated),

            // state = SeenByLastTracker
            new(OpCodes.Ldc_I4, (int)Scp939VisibilityState.SeenByLastTracker),
            new(OpCodes.Br, afterStateLoad),

            // isDetonated: state = SeenByDetonation
            new CodeInstruction(OpCodes.Ldc_I4, (int)Scp939VisibilityState.SeenByDetonation).WithLabels(isDetonated),
        };

        // afterStateLoad: state is on stack
        List<CodeInstruction> handlerBlock = CreateEventHandlerBlock(generator, ev, returnFalse);
        handlerBlock[0].labels.Add(afterStateLoad);
        result.AddRange(handlerBlock);

        return result;
    }

    /// <summary>
    /// Creates event block for range/lastSeen check at [089].
    /// </summary>
    private static List<CodeInstruction> CreateEventBlockForRangeCheck(ILGenerator generator, LocalBuilder ev, Label returnFalse)
    {
        Label skipEventLabel = generator.DefineLabel();

        List<CodeInstruction> result = new()
        {
            // if (!loc.3) goto skipEvent
            new(OpCodes.Ldloc_3),
            new(OpCodes.Brfalse, skipEventLabel),

            // state = SeenByLastTime
            new(OpCodes.Ldc_I4, (int)Scp939VisibilityState.SeenByLastTime),
        };

        result.AddRange(CreateEventHandlerBlock(generator, ev, returnFalse));

        // skipEvent:
        result.Add(new CodeInstruction(OpCodes.Nop).WithLabels(skipEventLabel));

        return result;
    }

    /// <summary>
    /// Creates common event handler block.
    /// Expects state already on stack.
    /// </summary>
    private static List<CodeInstruction> CreateEventHandlerBlock(ILGenerator generator, LocalBuilder ev, Label returnFalse)
    {
        Label continueLabel = generator.DefineLabel();

        return new List<CodeInstruction>
        {
            // new ValidatingVisibilityEventArgs(state, Owner, hub)
            new(OpCodes.Ldarg_0),
            new(OpCodes.Call, PropertyGetter(typeof(Scp939VisibilityController), nameof(Scp939VisibilityController.Owner))),
            new(OpCodes.Ldarg_1),
            new(OpCodes.Newobj, GetDeclaredConstructors(typeof(ValidatingVisibilityEventArgs))[0]),
            new(OpCodes.Dup),
            new(OpCodes.Stloc, ev.LocalIndex),

            // Handlers.Scp939.OnValidatingVisibility(ev)
            new(OpCodes.Call, Method(typeof(Handlers.Scp939), nameof(Handlers.Scp939.OnValidatingVisibility))),

            // if (!ev.IsAllowed) return false
            new(OpCodes.Ldloc, ev.LocalIndex),
            new(OpCodes.Callvirt, PropertyGetter(typeof(ValidatingVisibilityEventArgs), nameof(ValidatingVisibilityEventArgs.IsAllowed))),
            new(OpCodes.Brfalse, returnFalse),

            // if (!ev.IsLateSeen) goto continue
            new(OpCodes.Ldloc, ev.LocalIndex),
            new(OpCodes.Callvirt, PropertyGetter(typeof(ValidatingVisibilityEventArgs), nameof(ValidatingVisibilityEventArgs.IsLateSeen))),
            new(OpCodes.Brfalse, continueLabel),

            // SetToLastSeen(hub); return true
            new(OpCodes.Ldarg_1),
            new(OpCodes.Call, Method(typeof(ValidatingVisibility), nameof(SetToLastSeen))),
            new(OpCodes.Ldc_I4_1),
            new(OpCodes.Ret),

            // continue:
            new CodeInstruction(OpCodes.Nop).WithLabels(continueLabel),
        };
    }

    private static void SetToLastSeen(ReferenceHub target) =>
        Scp939VisibilityController.LastSeen[target.netId] = new()
        {
            Time = NetworkTime.time,
        };
}