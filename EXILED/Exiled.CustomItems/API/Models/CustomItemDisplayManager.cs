// -----------------------------------------------------------------------
// <copyright file="CustomItemDisplayManager.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.API.Models;

using System.Collections.Generic;
using System.Linq;
using Configs;
using InventorySystem.Items.Firearms.Modules;
using Exiled.API.Extensions;
using AdminToys;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Toys;
using Features;
using MEC;
using UnityEngine;

#pragma warning disable CS1591
#pragma warning disable SA1600
public static class CustomItemDisplayManager
{
    private static readonly Dictionary<uint, DisplayData> TrackedDisplays = new();
    private static CoroutineHandle updateHandle;

    public static void Init()
    {
        Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        Exiled.Events.Handlers.Map.PickupDestroyed += OnPickupDestroyed;
        updateHandle = Timing.RunCoroutine(UpdateLoop());
    }

    public static void Destroy()
    {
        Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        Exiled.Events.Handlers.Map.PickupDestroyed -= OnPickupDestroyed;
        Timing.KillCoroutines(updateHandle);
        ClearAll();
    }

    public static void Register(Pickup pickup, CustomItem customItem)
    {
        if (TrackedDisplays.ContainsKey(pickup.Base.netId))
            return;

        DisplayConfig config = customItem.DisplayConfig;

        string? displayText = config.DisplayName;
        if (displayText == null)
            return;

        if (string.IsNullOrEmpty(displayText))
            displayText = customItem.Name;

        Text textToy = Text.Create(pickup.Position + config.DisplayTextOffset, string.Empty);
        textToy.DisplaySize = new(8f, 4f);
        textToy.Spawn();

        string formattedText = $"<size=1>{displayText}</size>";
        TrackedDisplays[pickup.Base.netId] = new(pickup, textToy, formattedText, config);
    }

    private static void Unregister(uint netId)
    {
        if (!TrackedDisplays.TryGetValue(netId, out DisplayData data))
            return;

        foreach (Player player in data.ActiveObservers.Where(player => player.IsConnected))
            player.SendFakeSyncVar(data.TextToy.Base.netIdentity, typeof(TextToy), "Network_textFormat", string.Empty);

        data.TextToy.Destroy();
        TrackedDisplays.Remove(netId);
    }

    private static void OnWaitingForPlayers()
        => ClearAll();

    private static void OnPickupDestroyed(Exiled.Events.EventArgs.Map.PickupDestroyedEventArgs ev)
        => Unregister(ev.Pickup.Base.netId);

    private static void ClearAll()
    {
        foreach (DisplayData data in TrackedDisplays.Values)
            data.TextToy.Destroy();

        TrackedDisplays.Clear();
    }

    private static IEnumerator<float> UpdateLoop()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(0.12f);

            if (TrackedDisplays.Count == 0)
                continue;

            List<uint> keys = new(TrackedDisplays.Keys);
            foreach (uint netId in keys)
            {
                if (!TrackedDisplays.TryGetValue(netId, out DisplayData data))
                    continue;

                if (!data.Pickup.Base)
                {
                    Unregister(netId);
                    continue;
                }

                data.TextToy.Position = data.Pickup.Position + data.Config.DisplayTextOffset;

                Vector3 textWorldPos = data.TextToy.Position;
                float maxDist = data.Config.DistanceToAppear;
                float maxDistSqr = maxDist * maxDist;

                HashSet<Player> currentObservers = new();
                foreach (Player player in Player.List)
                {
                    if (!player.IsAlive)
                        continue;

                    Vector3 playerPos = player.Position;
                    if ((textWorldPos - playerPos).sqrMagnitude > maxDistSqr)
                        continue;

                    if (data.Config.IsStraightLookNeeded)
                    {
                        Vector3 camPos = player.CameraTransform.position;

                        Vector3 dirToPickup = data.Pickup.Position - camPos;

                        if (Vector3.Dot(player.CameraTransform.forward, dirToPickup) <= 0f)
                            continue;

                        Vector3 closestPointOnRay = camPos + Vector3.Project(dirToPickup, player.CameraTransform.forward);
                        if (Vector3.Distance(closestPointOnRay, data.Pickup.Position) > 0.5f)
                            continue;

                        HitscanHitregModuleBase.ToggleColliders(player.ReferenceHub, false);

                        RaycastHit[] hits = Physics.RaycastAll(camPos, (textWorldPos - camPos).normalized, Vector3.Distance(camPos, textWorldPos));
                        bool blocked = false;

                        foreach (RaycastHit hit in hits)
                        {
                            if (hit.collider.transform == data.Pickup.Transform || hit.collider.transform.IsChildOf(data.Pickup.Transform))
                                continue;

                            blocked = true;
                            break;
                        }

                        HitscanHitregModuleBase.ToggleColliders(player.ReferenceHub, true);

                        if (blocked)
                            continue;
                    }

                    currentObservers.Add(player);

                    Vector3 dir = player.CameraTransform.position - textWorldPos;
                    if (dir.sqrMagnitude < 0.01f)
                        continue;

                    Quaternion desiredWorldRot = Quaternion.LookRotation(-dir, Vector3.up);

                    bool sendRotation = true;
                    if (data.LastRotations.TryGetValue(player, out Quaternion lastRot))
                    {
                        if (Quaternion.Angle(lastRot, desiredWorldRot) < 1f)
                            sendRotation = false;
                    }

                    if (sendRotation)
                    {
                        data.LastRotations[player] = desiredWorldRot;
                        player.SendFakeSyncVar(data.TextToy.Base.netIdentity, typeof(TextToy), "NetworkRotation", desiredWorldRot);
                    }

                    if (!data.ActiveObservers.Contains(player))
                        player.SendFakeSyncVar(data.TextToy.Base.netIdentity, typeof(TextToy), "Network_textFormat", data.FormattedText);
                }

                foreach (Player prevObserver in data.ActiveObservers.Where(prevObserver => !currentObservers.Contains(prevObserver)))
                {
                    if (prevObserver.IsConnected)
                        prevObserver.SendFakeSyncVar(data.TextToy.Base.netIdentity, typeof(TextToy), "Network_textFormat", string.Empty);

                    data.LastRotations.Remove(prevObserver);
                }

                data.ActiveObservers = currentObservers;
            }
        }
    }

    private class DisplayData(Pickup pickup, Text textToy, string formattedText, DisplayConfig config)
    {
        public Pickup Pickup { get; set; } = pickup;

        public Text TextToy { get; set; } = textToy;

        public string FormattedText { get; set; } = formattedText;

        public DisplayConfig Config { get; set; } = config;

        public HashSet<Player> ActiveObservers { get; set; } = new();

        public Dictionary<Player, Quaternion> LastRotations { get; set; } = new();
    }
}