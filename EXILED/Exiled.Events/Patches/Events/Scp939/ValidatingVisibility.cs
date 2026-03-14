// -----------------------------------------------------------------------
// <copyright file="ValidatingVisibility.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Scp939;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

using API.Enums;
using Attributes;
using Exiled.API.Features.Pools;
using Exiled.Events.EventArgs.Scp939;
using HarmonyLib;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.HumanTracker;
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

        // Block 1: after failed base.ValidateVisibility → return false (None)
        // ldc.i4.0
        int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Ldc_I4_0);
        newInstructions.InsertRange(index, StaticCallEvent(generator, ev, returnFalse, newInstructions[index], Scp939VisibilityState.None));

        // Block 2: after !IsEnemy (target is SCP) → return true (SeenAsScp)
        // first ldc.i4.1
        index = newInstructions.FindIndex(i => i.opcode == OpCodes.Ldc_I4_1);
        newInstructions.InsertRange(index, StaticCallEvent(generator, ev, returnFalse, newInstructions[index], Scp939VisibilityState.SeenAsScp));

        // Block 3: after !(role is FpcStandardRoleBase) → return true (None)
        // ldc.i4.1 following isinst FpcStandardRoleBase
        int isinstIndex = newInstructions.FindIndex(i => i.opcode == OpCodes.Isinst && i.operand is Type t && t == typeof(FpcStandardRoleBase));
        index = newInstructions.FindIndex(isinstIndex, i => i.opcode == OpCodes.Ldc_I4_1);
        newInstructions.InsertRange(index, StaticCallEvent(generator, ev, returnFalse, newInstructions[index], Scp939VisibilityState.None));

        // Block 4: after Detonated || IsOneTargetLeft → return true (SeenByDetonation or SeenByLastTracker)
        // ldc.i4.1 following IsOneTargetLeft call
        int lastTrackerIndex = newInstructions.FindIndex(i => i.Calls(PropertyGetter(typeof(LastHumanTracker), nameof(LastHumanTracker.IsOneTargetLeft))));
        index = newInstructions.FindIndex(lastTrackerIndex, i => i.opcode == OpCodes.Ldc_I4_1);
        newInstructions.InsertRange(index, DetonationCallEvent(generator, ev, returnFalse, newInstructions[index]));

        // Block 5: before final ldloc.3 return — pre-check for SeenByLastTime
        // last ldloc.3 (flag before ret)
        index = newInstructions.FindLastIndex(i => i.opcode == OpCodes.Ldloc_3);

        Label skipEvent = generator.DefineLabel();

        newInstructions.InsertRange(index, new CodeInstruction[]
            {
                // if (!flag) skip event
                new CodeInstruction(OpCodes.Ldloc_3).MoveLabelsFrom(newInstructions[index]),
                new (OpCodes.Brfalse_S, skipEvent),

                // state = SeenByLastTime
                new (OpCodes.Ldc_I4, (int)Scp939VisibilityState.SeenByLastTime),
            }
            .Concat(CallEvent(generator, ev, returnFalse))
            .Append(new CodeInstruction(OpCodes.Nop).WithLabels(skipEvent)));

        // Block 6: before writing to LastSeen dictionary → SeenByRange
        // last ldsfld LastSeen
        index = newInstructions.FindLastIndex(i => i.LoadsField(Field(typeof(Scp939VisibilityController), nameof(Scp939VisibilityController.LastSeen))));
        newInstructions.InsertRange(index, StaticCallEvent(generator, ev, returnFalse, newInstructions[index], Scp939VisibilityState.SeenByRange));

        // return false
        newInstructions.Add(new CodeInstruction(OpCodes.Ldc_I4_0).WithLabels(returnFalse));
        newInstructions.Add(new CodeInstruction(OpCodes.Ret));

        for (int z = 0; z < newInstructions.Count; z++)
            yield return newInstructions[z];

        ListPool<CodeInstruction>.Pool.Return(newInstructions);
    }

    /// <summary>
    /// Creates an event block with a fixed <see cref="Scp939VisibilityState"/>, moving labels from the target instruction.
    /// </summary>
    /// <param name="generator">The <see cref="ILGenerator"/>.</param>
    /// <param name="ev">The <see cref="LocalBuilder"/> storing the event args.</param>
    /// <param name="returnFalse">The <see cref="Label"/> to jump to when the event is not allowed.</param>
    /// <param name="target">The <see cref="CodeInstruction"/> whose labels will be moved to the first emitted instruction.</param>
    /// <param name="state">The <see cref="Scp939VisibilityState"/> to pass into the event.</param>
    /// <returns>The emitted <see cref="CodeInstruction"/>s.</returns>
    private static IEnumerable<CodeInstruction> StaticCallEvent(ILGenerator generator, LocalBuilder ev, Label returnFalse, CodeInstruction target, Scp939VisibilityState state)
    {
        yield return new CodeInstruction(OpCodes.Ldc_I4, (int)state).MoveLabelsFrom(target);

        foreach (CodeInstruction instruction in CallEvent(generator, ev, returnFalse))
            yield return instruction;
    }

    /// <summary>
    /// Creates an event block that distinguishes <see cref="Scp939VisibilityState.SeenByDetonation"/>
    /// from <see cref="Scp939VisibilityState.SeenByLastTracker"/> by checking <see cref="AlphaWarheadController.Detonated"/>.
    /// </summary>
    /// <param name="generator">The <see cref="ILGenerator"/>.</param>
    /// <param name="ev">The <see cref="LocalBuilder"/> storing the event args.</param>
    /// <param name="returnFalse">The <see cref="Label"/> to jump to when the event is not allowed.</param>
    /// <param name="target">The <see cref="CodeInstruction"/> whose labels will be moved to the first emitted instruction.</param>
    /// <returns>The emitted <see cref="CodeInstruction"/>s.</returns>
    private static IEnumerable<CodeInstruction> DetonationCallEvent(ILGenerator generator, LocalBuilder ev, Label returnFalse, CodeInstruction target)
    {
        Label isDetonated = generator.DefineLabel();
        Label afterStateLoad = generator.DefineLabel();

        // if (AlphaWarheadController.Detonated) goto isDetonated
        yield return new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(AlphaWarheadController), nameof(AlphaWarheadController.Detonated))).MoveLabelsFrom(target);
        yield return new (OpCodes.Brtrue_S, isDetonated);

        // state = SeenByLastTracker
        yield return new (OpCodes.Ldc_I4, (int)Scp939VisibilityState.SeenByLastTracker);
        yield return new (OpCodes.Br_S, afterStateLoad);

        // isDetonated: state = SeenByDetonation
        yield return new CodeInstruction(OpCodes.Ldc_I4, (int)Scp939VisibilityState.SeenByDetonation).WithLabels(isDetonated);

        // afterStateLoad: state is on stack, proceed to event
        bool first = true;
        foreach (CodeInstruction instruction in CallEvent(generator, ev, returnFalse))
        {
            if (first)
            {
                instruction.labels.Add(afterStateLoad);
                first = false;
            }

            yield return instruction;
        }
    }

    /// <summary>
    /// Main IL logic for invoking the event. Expects the <see cref="Scp939VisibilityState"/> already on the evaluation stack.
    /// </summary>
    /// <param name="generator">The <see cref="ILGenerator"/>.</param>
    /// <param name="ev">The <see cref="LocalBuilder"/> storing the event args.</param>
    /// <param name="returnFalse">The <see cref="Label"/> to jump to when the event is not allowed.</param>
    /// <returns>The emitted <see cref="CodeInstruction"/>s.</returns>
    private static IEnumerable<CodeInstruction> CallEvent(ILGenerator generator, LocalBuilder ev, Label returnFalse)
    {
        Label continueLabel = generator.DefineLabel();

        // ...VisibilityState loaded in stack
        // ValidatingVisibilityEventArgs ev = new(state, scp939, target)
        yield return new (OpCodes.Ldarg_0);
        yield return new (OpCodes.Call, PropertyGetter(typeof(Scp939VisibilityController), nameof(Scp939VisibilityController.Owner)));
        yield return new (OpCodes.Ldarg_1);
        yield return new (OpCodes.Newobj, GetDeclaredConstructors(typeof(ValidatingVisibilityEventArgs))[0]);
        yield return new (OpCodes.Dup);
        yield return new (OpCodes.Stloc_S, ev.LocalIndex);

        // Scp939.OnValidatingVisibility(ev)
        yield return new (OpCodes.Call, Method(typeof(Handlers.Scp939), nameof(Handlers.Scp939.OnValidatingVisibility)));

        // if (!ev.IsAllowed)
        //     return false;
        yield return new (OpCodes.Ldloc_S, ev.LocalIndex);
        yield return new (OpCodes.Callvirt, PropertyGetter(typeof(ValidatingVisibilityEventArgs), nameof(ValidatingVisibilityEventArgs.IsAllowed)));
        yield return new (OpCodes.Brfalse_S, returnFalse);

        // if (ev.IsLateSeen)
        //     ValidatingVisibility.SetToLastSeen(target);
        //     return true;
        yield return new (OpCodes.Ldloc_S, ev.LocalIndex);
        yield return new (OpCodes.Callvirt, PropertyGetter(typeof(ValidatingVisibilityEventArgs), nameof(ValidatingVisibilityEventArgs.IsLateSeen)));
        yield return new (OpCodes.Brfalse_S, continueLabel);

        yield return new (OpCodes.Ldarg_1);
        yield return new (OpCodes.Call, Method(typeof(ValidatingVisibility), nameof(SetToLastSeen)));
        yield return new (OpCodes.Ldc_I4_1);
        yield return new (OpCodes.Ret);

        // continue:
        yield return new CodeInstruction(OpCodes.Nop).WithLabels(continueLabel);
    }

    private static void SetToLastSeen(ReferenceHub target) =>
        Scp939VisibilityController.LastSeen[target.netId] = new ()
        {
            Time = NetworkTime.time,
        };
}