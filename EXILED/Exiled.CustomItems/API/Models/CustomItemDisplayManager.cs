// -----------------------------------------------------------------------
// <copyright file="CustomItemDisplayManager.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.API.Models;

using System.Collections.Generic;
using AdminToys;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.API.Extensions;
using Exiled.API.Features.Toys;
using Features;
using MEC;
using UnityEngine;

#pragma warning disable CS1591
#pragma warning disable SA1600
public static class CustomItemDisplayManager
{
    private static readonly Dictionary<uint, DisplayData> TrackedDisplays = new();
    private static readonly RaycastHit[] RaycastBuffer = new RaycastHit[10];
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

        string? displayText = customItem.DisplayConfig.DisplayName;
        if (displayText == null)
            return;

        if (string.IsNullOrEmpty(displayText))
            displayText = customItem.Name;

        Vector3 offset = customItem.DisplayConfig.DisplayTextOffset;
        Text textToy = Text.Create(pickup.Position + offset, string.Empty);
        textToy.DisplaySize = new(8f, 4f);
        textToy.Spawn();

        string formattedText = $"<size=1>{displayText}</size>";
        TrackedDisplays[pickup.Base.netId] = new(pickup, textToy, formattedText, offset);
    }

    private static void Unregister(uint netId)
    {
        if (!TrackedDisplays.TryGetValue(netId, out DisplayData data))
            return;

        foreach (Player player in data.ActiveObservers)
        {
            if (player.IsConnected)
                player.SendFakeSyncVar(data.TextToy.Base.netIdentity, typeof(TextToy), "Network_textFormat", string.Empty);
        }

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
            yield return Timing.WaitForSeconds(0.15f);

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

                Vector3 pickupPos = data.Pickup.Position;
                data.TextToy.Position = pickupPos + data.Offset;
                Vector3 textWorldPos = data.TextToy.Position;

                foreach (Player player in Player.List)
                {
                    if (!player.IsAlive)
                        continue;

                    Vector3 camPos = player.CameraTransform.position;
                    Vector3 playerPos = player.Position;

                    bool shouldSee = false;

                    if ((pickupPos - playerPos).sqrMagnitude <= 100f)
                    {
                        Vector3 dirToPickup = pickupPos - camPos;
                        if (Vector3.Angle(player.CameraTransform.forward, dirToPickup) <= 25f)
                        {
                            float distToPickup = dirToPickup.magnitude;
                            if (distToPickup > 0.1f)
                            {
                                int hitCount = Physics.RaycastNonAlloc(camPos, dirToPickup / distToPickup, RaycastBuffer, distToPickup);

                                bool blocked = false;
                                for (int i = 0; i < hitCount; i++)
                                {
                                    Transform hitTransform = RaycastBuffer[i].collider.transform;
                                    if (hitTransform == data.Pickup.Transform || hitTransform.IsChildOf(data.Pickup.Transform))
                                        continue;

                                    if (hitTransform == player.Transform || hitTransform.IsChildOf(player.Transform))
                                        continue;

                                    blocked = true;
                                    break;
                                }

                                if (!blocked)
                                    shouldSee = true;
                            }
                        }
                    }

                    if (shouldSee)
                    {
                        Vector3 dir = camPos - textWorldPos;
                        if (dir.sqrMagnitude >= 0.01f)
                        {
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
                            {
                                player.SendFakeSyncVar(data.TextToy.Base.netIdentity, typeof(TextToy), "Network_textFormat", data.FormattedText);
                                data.ActiveObservers.Add(player);
                            }
                        }
                    }
                    else
                    {
                        if (data.ActiveObservers.Contains(player))
                        {
                            if (player.IsConnected)
                                player.SendFakeSyncVar(data.TextToy.Base.netIdentity, typeof(TextToy), "Network_textFormat", string.Empty);

                            data.ActiveObservers.Remove(player);
                            data.LastRotations.Remove(player);
                        }
                    }
                }
            }
        }
    }

    private class DisplayData(Pickup pickup, Text textToy, string formattedText, Vector3 offset)
    {
        public Pickup Pickup { get; set; } = pickup;

        public Text TextToy { get; set; } = textToy;

        public string FormattedText { get; set; } = formattedText;

        public Vector3 Offset { get; set; } = offset;

        public HashSet<Player> ActiveObservers { get; set; } = new();

        public Dictionary<Player, Quaternion> LastRotations { get; set; } = new();
    }
}