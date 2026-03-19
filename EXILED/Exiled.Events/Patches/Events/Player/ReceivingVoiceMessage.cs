// -----------------------------------------------------------------------
// <copyright file="ReceivingVoiceMessage.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using API.Features.Pools;
    using Exiled.Events.Attributes;
    using Exiled.Events.EventArgs.Player;
    using HarmonyLib;
    using Mirror;
    using PlayerRoles.Voice;
    using VoiceChat;
    using VoiceChat.Networking;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="VoiceTransceiver.ServerReceiveMessage(NetworkConnection, VoiceMessage)"/>.
    /// Adds the <see cref="Handlers.Player.ReceivingVoiceMessage"/> event.
    /// </summary>
    [EventPatch(typeof(Handlers.Player), nameof(Handlers.Player.ReceivingVoiceMessage))]
    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    internal static class ReceivingVoiceMessage
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label skipValidateLabel = generator.DefineLabel();
            Label skipIterationLabel = (Label)newInstructions.FindLast(i => i.opcode == OpCodes.Brfalse_S).operand;

            LocalBuilder ev = generator.DeclareLocal(typeof(ReceivingVoiceMessageEventArgs));

            int offset = -5;
            int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Callvirt &&
                                                       i.operand is MethodInfo mi &&
                                                       mi.Name == nameof(VoiceModuleBase.ValidateReceive)) + offset;

            // IL_00ec adding label
            newInstructions[index + 10].labels.Add(skipValidateLabel);

            newInstructions.InsertRange(index, new[]
            {
                // Player.Get(allHub)
                new CodeInstruction(OpCodes.Ldloc_S, 4),
                new CodeInstruction(OpCodes.Call, Method(typeof(API.Features.Player), nameof(API.Features.Player.Get), new[] { typeof(ReferenceHub) })),

                // Player.Get(msg.Speaker)
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(VoiceMessage), nameof(VoiceMessage.Speaker))),
                new CodeInstruction(OpCodes.Call, Method(typeof(API.Features.Player), nameof(API.Features.Player.Get), new[] { typeof(ReferenceHub) })),

                // currentRole2.VoiceModule
                new CodeInstruction(OpCodes.Ldloc_S, 5),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(IVoiceRole), nameof(IVoiceRole.VoiceModule))),

                // msg
                new CodeInstruction(OpCodes.Ldarg_1),

                // channel
                new CodeInstruction(OpCodes.Ldloc_1),

                // new ReceivingVoiceMessageEventArgs(...)
                new CodeInstruction(OpCodes.Newobj, GetDeclaredConstructors(typeof(ReceivingVoiceMessageEventArgs))[0]),
                new CodeInstruction(OpCodes.Stloc_S, ev.LocalIndex),

                // Handlers.Player.OnReceivingVoiceMessage(ev)
                new CodeInstruction(OpCodes.Ldloc_S, ev.LocalIndex),
                new CodeInstruction(OpCodes.Call, Method(typeof(Handlers.Player), nameof(Handlers.Player.OnReceivingVoiceMessage))),

                // if (!ev.IsAllowed) goto skipIterationLabel
                new CodeInstruction(OpCodes.Ldloc_S, ev.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(ReceivingVoiceMessageEventArgs), nameof(ReceivingVoiceMessageEventArgs.IsAllowed))),
                new CodeInstruction(OpCodes.Brfalse_S, skipIterationLabel),

                // msg = ev.VoiceMessage
                new CodeInstruction(OpCodes.Ldloc_S, ev.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(ReceivingVoiceMessageEventArgs), nameof(ReceivingVoiceMessageEventArgs.VoiceMessage))),
                new CodeInstruction(OpCodes.Starg_S, 1),

                // if (ev.SkipValidate) goto skipValidateLabel
                new CodeInstruction(OpCodes.Ldloc_S, ev.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(ReceivingVoiceMessageEventArgs), nameof(ReceivingVoiceMessageEventArgs.SkipValidate))),
                new CodeInstruction(OpCodes.Brtrue_S, skipValidateLabel),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}