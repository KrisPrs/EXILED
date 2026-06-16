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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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

        Text textToy = Text.Create(Vector3.zero, string.Empty);
        textToy.DisplaySize = new(10f, 5f);
        textToy.Spawn();

        textToy.Transform.SetParent(pickup.Transform, true);

        Vector3 pickupScale = pickup.Scale;
        textToy.Transform.localScale = new(1f / pickupScale.x, 1f / pickupScale.y, 1f / pickupScale.z);

        textToy.Transform.localPosition = config.DisplayTextOffset;
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

                Vector3 textWorldPos = data.TextToy.Transform.position;
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
                        Ray ray = new(player.CameraTransform.position, player.CameraTransform.forward);
                        Vector3 toText = textWorldPos - ray.origin;

                        if (Vector3.Dot(ray.direction, toText) <= 0f)
                            continue;

                        Vector3 closestPointOnRay = ray.origin + (ray.direction * Vector3.Dot(ray.direction, toText));
                        if (Vector3.Distance(closestPointOnRay, textWorldPos) > 0.2f)
                            continue;

                        HitscanHitregModuleBase.ToggleColliders(player.ReferenceHub, false);
                        if (Physics.Linecast(player.CameraTransform.position, textWorldPos, out RaycastHit hit))
                        {
                            if (hit.distance < Vector3.Distance(player.CameraTransform.position, textWorldPos))
                            {
                                HitscanHitregModuleBase.ToggleColliders(player.ReferenceHub, true);
                                continue;
                            }
                        }

                        HitscanHitregModuleBase.ToggleColliders(player.ReferenceHub, true);
                    }

                    currentObservers.Add(player);
                    Vector3 dir = player.CameraTransform.position - textWorldPos;
                    if (dir.sqrMagnitude < 0.01f)
                        continue;

                    Quaternion desiredWorldRot = Quaternion.LookRotation(dir, Vector3.up);
                    Quaternion parentRot = data.TextToy.Transform.parent ? data.TextToy.Transform.parent.rotation : Quaternion.identity;
                    Quaternion localRot = Quaternion.Inverse(parentRot) * desiredWorldRot;

                    bool sendRotation = true;
                    if (data.LastRotations.TryGetValue(player, out Quaternion lastRot))
                    {
                        if (Quaternion.Angle(lastRot, localRot) < 5f)
                            sendRotation = false;
                    }

                    if (sendRotation)
                    {
                        data.LastRotations[player] = localRot;
                        player.SendFakeSyncVar(data.TextToy.Base.netIdentity, typeof(TextToy), "NetworkRotation", localRot);
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