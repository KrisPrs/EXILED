﻿// -----------------------------------------------------------------------
// <copyright file="ActivatingSense.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Scp049
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using Exiled.API.Features;
    using Exiled.API.Features.Pools;

    using Exiled.Events.EventArgs.Scp049;

    using HarmonyLib;

    using Mirror;

    using PlayerRoles;
    using PlayerRoles.PlayableScps;
    using PlayerRoles.PlayableScps.Scp049;

    using Utils.Networking;

    using static HarmonyLib.AccessTools;

    /// <summary>
    ///     Patches <see cref="Scp049SenseAbility.ServerProcessCmd" />.
    ///     Adds the <see cref="Handlers.Scp049.ActivatingSense" /> event.
    /// </summary>
    // TODO: REWORK TRANSPILER
    [HarmonyPatch]
    public class ActivatingSense
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Scp049SenseAbility), nameof(Scp049SenseAbility.ServerProcessCmd))]
        private static IEnumerable<CodeInstruction> OnSendingSense(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);
            Label returnLabel = generator.DefineLabel();

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Call, Method(typeof(ActivatingSense), nameof(ActivatingSense.ProcessSense))),
                new(OpCodes.Br, returnLabel),
            });

            newInstructions[newInstructions.Count - 1].WithLabels(returnLabel);

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }

        /// <summary>
        /// Process the Sense Ability for the Event.
        /// </summary>
        /// <param name="senseAbility"> 049's <see cref="Scp049SenseAbility"/> ability. </param>
        /// <param name="reader"> <see cref="NetworkReader"/> to get <see cref="ReferenceHub"/> from network data. </param>
        private static void ProcessSense(Scp049SenseAbility senseAbility, NetworkReader reader)
        {
            if (!senseAbility.Cooldown.IsReady || !senseAbility.Duration.IsReady)
                return;

            Player scp049 = Player.Get(senseAbility.Owner);
            var target = reader.ReadReferenceHub();

            var ev = new ActivatingSenseEventArgs(scp049, Player.Get(target));
            if (ev.Target is not null && ev.Target.IsTutorial && !Exiled.Events.Events.Instance.Config.CanScp049SenseTutorial)
                ev.IsAllowed = false;
            Handlers.Scp049.OnActivatingSense(ev);

            if (!ev.IsAllowed)
                return;

            senseAbility._distanceThreshold = 100f;
            senseAbility.HasTarget = false;
            senseAbility.Target = ev.Target?.ReferenceHub;

            if (senseAbility.Target == null)
            {
                senseAbility.Cooldown.Trigger(ev.Cooldown);
                senseAbility.ServerSendRpc(true);
                return;
            }

            HumanRole humanRole;
            if ((humanRole = target.roleManager.CurrentRole as HumanRole) == null)
                return;

            senseAbility._distanceThreshold = 100f;
            if (!VisionInformation.GetVisionInformation(senseAbility.Owner, senseAbility.Owner.PlayerCameraReference, humanRole.CameraPosition, humanRole.FpcModule.CharController.radius, senseAbility._distanceThreshold, true, true, 0).IsLooking)
                return;

            senseAbility.Duration.Trigger(ev.Duration);
            senseAbility.HasTarget = true;
            senseAbility.ServerSendRpc(true);
        }
    }
}