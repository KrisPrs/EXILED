// -----------------------------------------------------------------------
// <copyright file="Kicking.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features;
    using CommandSystem;
    using Exiled.API.Features.Pools;
    using Exiled.Events.EventArgs.Player;
    using HarmonyLib;

    using static HarmonyLib.AccessTools;

    /// <summary>
    ///     Patches <see cref="BanPlayer.KickUser(ReferenceHub, ICommandSender , string)" />.
    ///     Adds the <see cref="Handlers.Player.Kicking" /> event.
    /// </summary>
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.KickUser), typeof(ReferenceHub), typeof(ICommandSender), typeof(string))]
    internal static class Kicking
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label continueLabel = generator.DefineLabel();

            LocalBuilder ev = generator.DeclareLocal(typeof(KickingEventArgs));

            newInstructions.InsertRange(
                0,
                new[]
                {
                    // Player target = Player.Get(target);
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),

                    // Player player = Player.Get(issuer);
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ICommandSender) })),

                    // reason
                    new(OpCodes.Ldarg_2),

                    // true
                    new(OpCodes.Ldc_I4_1),

                    // KickingEventArgs ev = new(player, target, reason, true);
                    new(OpCodes.Newobj, GetDeclaredConstructors(typeof(KickingEventArgs))[0]),
                    /*new(OpCodes.Dup),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, ev.LocalIndex),*/

                    // Handlers.Player.OnKicking(ev);
                    new(OpCodes.Call, Method(typeof(Handlers.Player), nameof(Handlers.Player.OnKicking))),

                    // if (!ev.IsAllowed)
                    //      return;
                    /*new(OpCodes.Callvirt, PropertyGetter(typeof(KickedEventArgs), nameof(KickingEventArgs.IsAllowed))),
                    new(OpCodes.Brtrue_S, continueLabel),

                    new(OpCodes.Ret),*/

                    // loading ev 3 times
                    new CodeInstruction(OpCodes.Ldloc_S, ev.LocalIndex).WithLabels(continueLabel),
                    new(OpCodes.Dup),
                    new(OpCodes.Dup),

                    // reason = ev.Reason;
                    new(OpCodes.Callvirt, PropertyGetter(typeof(KickingEventArgs), nameof(KickingEventArgs.Reason))),
                    new(OpCodes.Starg_S, 2),

                    // target = ev.Target.ReferenceHub;
                    new(OpCodes.Callvirt, PropertyGetter(typeof(KickingEventArgs), nameof(KickingEventArgs.Target))),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Player), nameof(Player.ReferenceHub))),
                    new(OpCodes.Starg_S, 0),

                    // issuer = ev.Player.Sender;
                    new(OpCodes.Callvirt, PropertyGetter(typeof(KickingEventArgs), nameof(KickingEventArgs.Player))),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Player), nameof(Player.Sender))),
                    new(OpCodes.Starg_S, 1),
                });

            int index = newInstructions.FindLastIndex(x => x.opcode == OpCodes.Ldstr);

            newInstructions.RemoveRange(index, 3);

            newInstructions.InsertRange(
                index,
                new CodeInstruction[]
                {
                    // ev.FullMessage
                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(KickingEventArgs), nameof(KickingEventArgs.FullMessage))),
                });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}