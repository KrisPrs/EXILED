// -----------------------------------------------------------------------
// <copyright file="Room.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features.Doors;
    using Exiled.API.Features.Pickups;
    using Exiled.API.Interfaces;
    using MapGeneration;
    using MapGeneration.Holidays;
    using MapGeneration.Rooms;
    using MEC;
    using Mirror;
    using PlayerRoles.PlayableScps.Scp079;
    using RelativePositioning;
    using UnityEngine;
    using Utils.NonAllocLINQ;

    /// <summary>
    /// The in-game room.
    /// </summary>
    public class Room : MonoBehaviour, IWorldSpace
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> containing all known <see cref="RoomIdentifier"/>s and their corresponding <see cref="Room"/>.
        /// </summary>
        internal static readonly Dictionary<RoomIdentifier, Room> RoomIdentifierToRoom = new(250, new ComponentsEqualityComparer());

        /// <summary>
        /// Список всех имён у префабов в комнатах.
        /// </summary>
        private static HashSet<string> roomNames =
        [
            "Tank-Supported Shelf Open Connector",
            "Simple Boxes Open Connector",
            "Pipes Long Open Connector",
            "Huge Orange Pipes Open Connector",
            "Pipes Short Open Connector",
            "Atlas_WoodCardboard_CrateB",
            "Atlas_WoodCardboard_CrateA",
            "Box",
            "Trim_Catwalk_Shelf",
            "Trim_Vents_GridFence_1m",
            "Modular_Large_Pipe_Angle55degre",
            "Atlas_Wood_Cardboard_Cratelarge",
            "Atlas_Wood_Cardboard_Cratedoublelarge",
            "Atlas_Wood_Cardboard_Palet",
            "nitrogenTank_small",
            "Palet",
            "Atlas_WoodCardboard_BoxA",
            "Atlas_WoodCardboard_CardboardBox5",
            "Atlas_WoodCardboard_CardboardBox3",
            "Atlas_WoodCardboard_CardboardBox4",
            "Atlas_WoodCardboard_CardboardBox2",
            "Atlas_WoodCardboard_CardboardBox1",
            "Atlas_WoodCardboard_BoxB",
            "Boxes Ladder Open Connector"
        ];

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="Room"/> which contains all the <see cref="Room"/> instances.
        /// </summary>
        public static IReadOnlyCollection<Room> List => RoomIdentifierToRoom.Values;

        /// <summary>
        /// Gets the <see cref="Room"/> name.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Gets the <see cref="Room"/> <see cref="UnityEngine.GameObject"/>.
        /// </summary>
        public GameObject GameObject => gameObject;

        /// <summary>
        /// Gets the <see cref="Room"/> <see cref="UnityEngine.Transform"/>.
        /// </summary>
        public Transform Transform => transform;

        /// <summary>
        /// Gets all Hubert's prefabs in room.
        /// </summary>
        public Dictionary<string, List<GameObject>> RoomPrefabs { get; private set; } = new();

        /// <summary>
        /// Gets the <see cref="Room"/> position.
        /// </summary>
        public Vector3 Position => transform.position;

        /// <summary>
        /// Gets the <see cref="Room"/> rotation.
        /// </summary>
        public Quaternion Rotation => transform.rotation;

        /// <summary>
        /// Gets the <see cref="ZoneType"/> in which the room is located.
        /// </summary>
        public ZoneType Zone { get; private set; } = ZoneType.Unspecified;

        /// <summary>
        /// Gets the <see cref="MapGeneration.RoomName"/> enum representing this room.
        /// </summary>
        /// <remarks>This property is the internal <see cref="MapGeneration.RoomName"/> of the room. For the actual string of the Room's name, see <see cref="Name"/>.</remarks>
        /// <seealso cref="Name"/>
        public RoomName RoomName => Identifier.Name;

        /// <summary>
        /// Gets the room's <see cref="MapGeneration.RoomShape"/>.
        /// </summary>
        /// <remarks>Will return null if the Room is not a <see cref="MultiLevelRoomIdentifier"/>.</remarks>
        public RoomLevelName? LevelName => Identifier is MultiLevelRoomIdentifier multiLevelRoomIdentifier ? (RoomLevelName?)multiLevelRoomIdentifier.Name : null;

        /// <summary>
        /// Gets the room's <see cref="MapGeneration.RoomShape"/>.
        /// </summary>
        public RoomShape RoomShape => Identifier.Shape;

        /// <summary>
        /// Gets the <see cref="RoomType"/>.
        /// </summary>
        public RoomType Type { get; private set; } = RoomType.Unknown;

        /// <summary>
        /// Gets a reference to the room's <see cref="RoomIdentifier"/>.
        /// </summary>
        public RoomIdentifier Identifier { get; private set; }

        /// <summary>
        /// Gets a reference to the <see cref="global::TeslaGate"/> in the room, or <see langword="null"/> if this room does not contain one.
        /// </summary>
        public TeslaGate TeslaGate { get; internal set; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="Player"/> in the <see cref="Room"/>.
        /// </summary>
        public IEnumerable<Player> Players => Player.List.Where(player => player.IsAlive && player.CurrentRoom is not null && (player.CurrentRoom.Transform == Transform));

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}"/> of <see cref="Window"/> in the <see cref="Room"/>.
        /// </summary>
        public IReadOnlyCollection<Window> Windows { get; private set; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}"/> of <see cref="Door"/> in the <see cref="Room"/>.
        /// </summary>
        public IReadOnlyCollection<Door> Doors { get; private set; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}"/> of <see cref="Scp079Speaker"/> in the <see cref="Room"/>.
        /// </summary>
        public IReadOnlyCollection<Scp079Speaker> Speakers { get; private set; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}"/> of <see cref="Camera"/> in the <see cref="Room"/>.
        /// </summary>
        public IReadOnlyCollection<Camera> Cameras { get; private set; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}"/> of <see cref="RoomLightController"/> in the <see cref="Room"/>.
        /// </summary>
        /// <remarks>
        /// Using that will make sense only for rooms with more than one light controller, in other cases better to use <see cref="RoomLightController"/>.
        /// </remarks>
        public IReadOnlyCollection<RoomLightController> RoomLightControllers { get; private set; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyList{T}"/> of <see cref="Room"/> around the <see cref="Room"/>.
        /// </summary>
        public IReadOnlyList<Room> NearestRooms
        {
            get
            {
                if (NearestRoomsValue.Count == 0 && Identifier.ConnectedRooms.Count > 0)
                    NearestRoomsValue.AddRange(Identifier.ConnectedRooms.Select(Get));

                return NearestRoomsValue;
            }
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="Pickup"/> in the <see cref="Room"/>.
        /// </summary>
        public IEnumerable<Pickup> Pickups => Pickup.List.Where(pickup => FindParentRoom(pickup.GameObject) == this);

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of the clutter spawned in the <see cref="Room"/> by the map generator.
        /// </summary>
        /// <remarks>
        /// Clutter is the junk piles, crates, racks and other decorative props randomly placed by <see cref="ClutterSpawner"/> during the map generation.
        /// Those objects are not networked, so any change made to them server-side will not be replicated to the clients.
        /// Only the clutter instantiated by the game is listed here, the props already baked into the room prefab are not, use <see cref="GetChildren"/> or <see cref="FindChildren"/> to find those.
        /// </remarks>
        public IEnumerable<GameObject> Clutter => ClutterValue.Where(clutter => clutter != null);

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="ClutterSpawner"/> in the <see cref="Room"/>.
        /// </summary>
        /// <remarks>The spawners stay in the room after the generation, but the clutter components they hold are destroyed as soon as they have spawned their props.</remarks>
        public IEnumerable<ClutterSpawner> ClutterSpawners => GetComponentsInChildren<ClutterSpawner>(true);

        /// <summary>
        /// Gets or sets the color of the room's lights by changing the warhead color.
        /// </summary>
        /// <remarks>Will return <see cref="Color.clear"/> when <see cref="RoomLightController"/> is <see langword="null"/>.</remarks>
        public Color Color
        {
            get => RoomLightController == null ? Color.clear : RoomLightController.NetworkOverrideColor;
            set
            {
                foreach (RoomLightController light in RoomLightControllers)
                {
                    light.NetworkOverrideColor = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the lights in this room are currently off.
        /// </summary>
        public bool AreLightsOff
        {
            get => RoomLightController != null && !RoomLightController.NetworkLightsEnabled;
            set
            {
                foreach (RoomLightController light in RoomLightControllers)
                {
                    light.NetworkLightsEnabled = !value;
                }
            }
        }

        /// <summary>
        /// Gets the FlickerableLightController's NetworkIdentity.
        /// </summary>
        public NetworkIdentity RoomLightControllerNetIdentity => RoomLightController?.netIdentity;

        /// <summary>
        /// Gets the room's FlickerableLightController.
        /// </summary>
        public RoomLightController RoomLightController => RoomLightControllers.FirstOrDefault();

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing all known <see cref="Window"/>s in that <see cref="Room"/>.
        /// </summary>
        internal List<Window> WindowsValue { get; } = new();

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing all known <see cref="Door"/>s in that <see cref="Room"/>.
        /// </summary>
        internal List<Door> DoorsValue { get; } = new();

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing all known <see cref="Scp079Speaker"/>s in that <see cref="Room"/>.
        /// </summary>
        internal List<Scp079Speaker> SpeakersValue { get; } = new();

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing all known <see cref="Camera"/>s in that <see cref="Room"/>.
        /// </summary>
        internal List<Camera> CamerasValue { get; } = new();

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing all the clutter spawned in that <see cref="Room"/>.
        /// </summary>
        internal List<GameObject> ClutterValue { get; } = new();

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing all known <see cref="RoomLightController"/>s in that <see cref="Room"/>.
        /// </summary>
        internal List<RoomLightController> RoomLightControllersValue { get; } = new();

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing all known <see cref="Room"/>s around that <see cref="Room"/>.
        /// </summary>
        internal List<Room> NearestRoomsValue { get; } = new();

        /// <summary>
        /// Gets a <see cref="Room"/> given the specified <see cref="RoomType"/>.
        /// </summary>
        /// <param name="roomType">The <see cref="RoomType"/> to search for.</param>
        /// <returns>The <see cref="Room"/> with the given <see cref="RoomType"/> or <see langword="null"/> if not found.</returns>
        public static Room Get(RoomType roomType) => Get(room => room.Type == roomType).FirstOrDefault();

        /// <summary>
        /// Gets a <see cref="Room"/> from a given <see cref="Identifier"/>.
        /// </summary>
        /// <param name="roomIdentifier">The <see cref="Identifier"/> to search with.</param>
        /// <returns>The <see cref="Room"/> of the given identified, if any. Can be <see langword="null"/>.</returns>
        public static Room Get(RoomIdentifier roomIdentifier) => roomIdentifier == null ? null :
            RoomIdentifierToRoom.TryGetValue(roomIdentifier, out Room room) ? room : null;

        /// <summary>
        /// Gets a <see cref="Room"/> from a given <see cref="RoomIdentifier"/>.
        /// </summary>
        /// <param name="flickerableLightController">The <see cref="RoomLightController"/> to search with.</param>
        /// <returns>The <see cref="Room"/> of the given identified, if any. Can be <see langword="null"/>.</returns>
        public static Room Get(RoomLightController flickerableLightController) => flickerableLightController.GetComponentInParent<Room>();

        /// <summary>
        /// Gets a <see cref="Room"/> given the specified <see cref="Vector3"/>.
        /// </summary>
        /// <param name="position">The <see cref="Vector3"/> to search for.</param>
        /// <returns>The <see cref="Room"/> with the given <see cref="Vector3"/> or <see langword="null"/> if not found.</returns>
        public static Room Get(Vector3 position) => position.TryGetRoom(out RoomIdentifier room) ? Get(room) : null;

        /// <summary>
        /// Gets a <see cref="Room"/> given the specified <see cref="RelativePosition"/>.
        /// </summary>
        /// <param name="position">The <see cref="RelativePosition"/> to search for.</param>
        /// <returns>The <see cref="Room"/> with the given <see cref="RelativePosition"/> or <see langword="null"/> if not found.</returns>
        public static Room Get(RelativePosition position) => Get(position.Position);

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="Room"/> given the specified <see cref="ZoneType"/>.
        /// </summary>
        /// <param name="zoneType">The <see cref="ZoneType"/> to search for.</param>
        /// <returns>The <see cref="Room"/> with the given <see cref="ZoneType"/> or <see langword="null"/> if not found.</returns>
        public static IEnumerable<Room> Get(ZoneType zoneType) => Get(room => room.Zone.HasFlag(zoneType));

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="Room"/> filtered based on a predicate.
        /// </summary>
        /// <param name="predicate">The condition to satify.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="Room"/> which contains elements that satify the condition.</returns>
        public static IEnumerable<Room> Get(Func<Room, bool> predicate) => List.Where(predicate);

        /// <summary>
        /// Tries to find the room that a <see cref="GameObject"/> is inside, first using the <see cref="Transform"/>'s parents, then using a Raycast if no room was found.
        /// </summary>
        /// <param name="objectInRoom">The <see cref="GameObject"/> inside the room.</param>
        /// <returns>The <see cref="Room"/> that the <see cref="GameObject"/> is located inside. Can be <see langword="null"/>.</returns>
        /// <seealso cref="Get(Vector3)"/>
        public static Room FindParentRoom(GameObject objectInRoom)
        {
            if (objectInRoom == null)
                return default;

            Room room = null;

            const string playerTag = "Player";

            // First try to find the room owner quickly.
            if (!objectInRoom.CompareTag(playerTag))
            {
                room = objectInRoom.GetComponentInParent<Room>();
            }
            else
            {
                // Check for SCP-079 if it's a player
                Player ply = Player.Get(objectInRoom);

                // Raycasting doesn't make sense,
                // SCP-079 position is constant,
                // let it be 'Outside' instead
                if (ply.Role.Is(out Roles.Scp079Role role))
                    room = FindParentRoom(role.Camera.GameObject);
            }

            // Finally, try for objects that aren't children, like players and pickups.
            return room ?? Get(objectInRoom.transform.position) ?? default;
        }

        /// <summary>
        /// Gets a random <see cref="Room"/>.
        /// </summary>
        /// <param name="zoneType">Filters by <see cref="ZoneType"/>.</param>
        /// <returns><see cref="Room"/> object.</returns>
        public static Room Random(ZoneType zoneType = ZoneType.Unspecified) => (zoneType is not ZoneType.Unspecified ? Get(r => r.Zone.HasFlag(zoneType)) : List).GetRandomValue();

        /// <summary>
        /// Returns the local space position, based on a world space position.
        /// </summary>
        /// <param name="position">World position.</param>
        /// <returns>Local position, based on the room.</returns>
        public Vector3 LocalPosition(Vector3 position) => Transform.InverseTransformPoint(position);

        /// <summary>
        /// Returns the World position, based on a local space position.
        /// </summary>
        /// <param name="offset">Local position.</param>
        /// <returns>World position, based on the room.</returns>
        public Vector3 WorldPosition(Vector3 offset) => Transform.TransformPoint(offset);

        /// <summary>
        /// Gets every <see cref="Transform"/> of the <see cref="Room"/> hierarchy, the room itself excluded.
        /// </summary>
        /// <param name="includeInactive">Whether inactive children should be included.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of the children of the <see cref="Room"/>.</returns>
        /// <remarks>Most of the decorative props of a room can only be found this way, as they hold no dedicated component.</remarks>
        public IEnumerable<Transform> GetChildren(bool includeInactive = true) => GetComponentsInChildren<Transform>(includeInactive).Where(child => child != Transform);

        /// <summary>
        /// Gets every child of the <see cref="Room"/> whose name matches the given one.
        /// </summary>
        /// <param name="name">The name to look for.</param>
        /// <param name="exactMatch">Whether the name has to match exactly, instead of only being contained in it.</param>
        /// <param name="includeInactive">Whether inactive children should be included.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of the matching children.</returns>
        /// <remarks>Clutter props are instantiated from prefabs, so their name ends with the usual clone suffix.</remarks>
        public IEnumerable<Transform> FindChildren(string name, bool exactMatch = false, bool includeInactive = true) => GetChildren(includeInactive).Where(child => exactMatch
            ? child.name.Equals(name, StringComparison.OrdinalIgnoreCase)
            : child.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) != -1);

        /// <summary>
        /// Gets the first child of the <see cref="Room"/> whose name matches the given one.
        /// </summary>
        /// <param name="name">The name to look for.</param>
        /// <param name="exactMatch">Whether the name has to match exactly, instead of only being contained in it.</param>
        /// <param name="includeInactive">Whether inactive children should be included.</param>
        /// <returns>The matching child, or <see langword="null"/> if there is none.</returns>
        public Transform FindChild(string name, bool exactMatch = false, bool includeInactive = true) => FindChildren(name, exactMatch, includeInactive).FirstOrDefault();

        /// <summary>
        /// Tries to get the first child of the <see cref="Room"/> whose name matches the given one.
        /// </summary>
        /// <param name="name">The name to look for.</param>
        /// <param name="child">The matching child, or <see langword="null"/> if there is none.</param>
        /// <param name="exactMatch">Whether the name has to match exactly, instead of only being contained in it.</param>
        /// <param name="includeInactive">Whether inactive children should be included.</param>
        /// <returns><see langword="true"/> if a child has been found; otherwise, <see langword="false"/>.</returns>
        public bool TryFindChild(string name, out Transform child, bool exactMatch = false, bool includeInactive = true)
        {
            child = FindChild(name, exactMatch, includeInactive);
            return child != null;
        }

        /// <summary>
        /// Gets every component of the given type in the <see cref="Room"/> hierarchy.
        /// </summary>
        /// <typeparam name="T">The type of the components to look for.</typeparam>
        /// <param name="includeInactive">Whether components on inactive objects should be included.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of the found components.</returns>
        public IEnumerable<T> GetComponentsInRoom<T>(bool includeInactive = true) => GetComponentsInChildren<T>(includeInactive);

        /// <summary>
        /// Gets the first component of the given type in the <see cref="Room"/> hierarchy.
        /// </summary>
        /// <typeparam name="T">The type of the component to look for.</typeparam>
        /// <param name="includeInactive">Whether components on inactive objects should be included.</param>
        /// <returns>The found component, or <see langword="null"/> if there is none.</returns>
        public T GetComponentInRoom<T>(bool includeInactive = true) => GetComponentInChildren<T>(includeInactive);

        /// <summary>
        /// Flickers the room's lights off for a duration.
        /// </summary>
        /// <param name="duration">Duration in seconds, or -1 for an indefinite duration.</param>
        public void TurnOffLights(float duration = -1)
        {
            if (duration == -1)
            {
                foreach (RoomLightController light in RoomLightControllers)
                {
                    light.SetLights(false);
                }

                return;
            }

            foreach (RoomLightController light in RoomLightControllers)
            {
                light.ServerFlickerLights(duration);
            }
        }

        /// <summary>
        /// Locks all the doors in the room.
        /// </summary>
        /// <param name="duration">Duration in seconds, or <c>-1</c> for permanent lockdown.</param>
        /// <param name="lockType">DoorLockType of the lockdown.</param>
        /// <seealso cref="Door.LockAll(float, ZoneType, DoorLockType)"/>
        /// <seealso cref="Door.LockAll(float, IEnumerable{ZoneType}, DoorLockType)"/>
        public void LockDown(float duration, DoorLockType lockType = DoorLockType.Regular079)
        {
            foreach (Door door in Doors)
            {
                door.ChangeLock(lockType);
                door.IsOpen = false;
            }

            if (duration < 0)
                return;

            Timing.CallDelayed(duration, UnlockAll);
        }

        /// <summary>
        /// Locks all the doors and turns off all lights in the room.
        /// </summary>
        /// <param name="duration">Duration in seconds, or <c>-1</c> for permanent blackout.</param>
        /// <param name="lockType">DoorLockType of the blackout.</param>
        /// <seealso cref="Map.TurnOffAllLights(float, ZoneType)"/>
        /// <seealso cref="Map.TurnOffAllLights(float, IEnumerable{ZoneType})"/>
        public void Blackout(float duration, DoorLockType lockType = DoorLockType.Regular079)
        {
            LockDown(duration, lockType);
            TurnOffLights(duration);
        }

        /// <summary>
        /// Unlocks all the doors in the room.
        /// </summary>
        /// <seealso cref="Door.UnlockAll()"/>
        /// <seealso cref="Door.UnlockAll(ZoneType)"/>
        /// <seealso cref="Door.UnlockAll(IEnumerable{ZoneType})"/>
        /// <seealso cref="Door.UnlockAll(Func{Door, bool})"/>
        public void UnlockAll()
        {
            foreach (Door door in Doors)
                door.Unlock();
        }

        /// <summary>
        /// Resets the room color to default.
        /// </summary>
        public void ResetColor() => Color = Color.clear;

        /// <summary>
        /// Returns the Room in a human-readable format.
        /// </summary>
        /// <returns>A string containing Room-related data.</returns>
        public override string ToString() => $"{Type} ({Zone}) [{Doors?.Count}] *{Cameras?.Count}* |{TeslaGate != null}|";

        /// <summary>
        /// Registers a clutter object spawned by the map generator in the <see cref="Room"/> it belongs to.
        /// </summary>
        /// <param name="clutter">The spawned clutter <see cref="GameObject"/>.</param>
        internal static void RegisterClutter(GameObject clutter)
        {
            if (clutter == null)
                return;

            Room room = clutter.GetComponentInParent<Room>(true);

            if (room == null)
                room = FindParentRoom(clutter);

            // Clutter spawned outside of any room, e.g. by a room connector of an open hallway.
            if (room == null)
                return;

            room.ClutterValue.Add(clutter);
        }

        /// <summary>
        /// Factory method to create and add a <see cref="Room"/> component to a Transform.
        /// We can add parameters to be set privately here.
        /// </summary>
        /// <param name="baseRoom">The Game Object to attach the Room component to.</param>
        internal static void CreateComponent(GameObject baseRoom) => baseRoom.AddComponent<Room>().InternalCreate();

        /// <summary>
        /// Factory method to complete all element inside a Room.
        /// </summary>
        internal void InternalCreate()
        {
            Identifier = gameObject.GetComponent<RoomIdentifier>();
            RoomIdentifierToRoom.Add(Identifier, this);

            Type = FindType(gameObject);

            if (Type is RoomType.Unknown)
                Log.Warn($"[ROOMTYPE UNKNOWN] {Identifier} Name : {gameObject?.name.RemoveBracketsOnEndOfName()} Shape : {Identifier?.Shape}");

            Zone = Type is RoomType.Pocket ? ZoneType.Pocket : Identifier.Zone.GetZone();

            if (Zone is ZoneType.Unspecified)
                Log.Warn($"[ZONETYPE UNKNOWN] {Identifier} Zone : {Identifier?.Zone}");

            RoomLightControllers = RoomLightControllersValue.AsReadOnly();

            GetComponentsInChildren<BreakableWindow>().ForEach(component =>
            {
                Window window = new(component, this);
                window.Room.WindowsValue.Add(window);
            });

            if (GetComponentInChildren<global::TeslaGate>() is global::TeslaGate tesla)
            {
                TeslaGate = new TeslaGate(tesla, this);
            }

            Windows = WindowsValue.AsReadOnly();
            Doors = DoorsValue.AsReadOnly();
            Speakers = SpeakersValue.AsReadOnly();
            Cameras = CamerasValue.AsReadOnly();

            foreach (string s in roomNames)
            {
                List<GameObject> objects = new List<GameObject>();
                gameObject.ForEachComponentInChildren(
                    (GameObject obj) =>
                {
                    if (obj.name.ToLower().Contains(s.ToLower()))
                    {
                        objects.Add(obj);
                    }
                }, false);

                RoomPrefabs[s] = objects;
            }
        }

        private static RoomType FindType(GameObject gameObject)
        {
            // Try to remove brackets if they exist.
            return TryRemovePostfixes(gameObject.name.RemoveBracketsOnEndOfName()) switch
            {
                "PocketWorld" => RoomType.Pocket,
                "Outside" => RoomType.Surface,
                "LCZ_Cafe" => RoomType.LczCafe,
                "LCZ_Toilets" => RoomType.LczToilets,
                "LCZ_TCross" => RoomType.LczTCross,
                "LCZ_Airlock" => RoomType.LczAirlock,
                "LCZ_ChkpA" => RoomType.LczCheckpointA,
                "LCZ_ChkpB" => RoomType.LczCheckpointB,
                "LCZ_Plants" => RoomType.LczPlants,
                "LCZ_Straight" => RoomType.LczStraight,
                "LCZ_Armory" => RoomType.LczArmory,
                "LCZ_Crossing" => RoomType.LczCrossing,
                "LCZ_Curve" => RoomType.LczCurve,
                "LCZ_173" => RoomType.Lcz173,
                "LCZ_330" => RoomType.Lcz330,
                "LCZ_372" => RoomType.LczGlassBox,
                "LCZ_914" => RoomType.Lcz914,
                "LCZ_ClassDSpawn" => RoomType.LczClassDSpawn,
                "HCZ_Nuke" => RoomType.HczNuke,
                "HCZ_TArmory" => RoomType.HczArmory,
                "HCZ_MicroHID_New" => RoomType.HczHid,
                "HCZ_Crossroom_Water" => RoomType.HczCrossRoomWater,
                "HCZ_IncineratorWayside" => RoomType.HczIncineratorWayside,
                "HCZ_Testroom" => RoomType.HczTestRoom,
                "HCZ_049" => RoomType.Hcz049,
                "HCZ_079" => RoomType.Hcz079,
                "HCZ_096" => RoomType.Hcz096,
                "HCZ_106_Rework" => RoomType.Hcz106,
                "HCZ_939" => RoomType.Hcz939,
                "HCZ_Tesla_Rework" => RoomType.HczTesla,
                "HCZ_Curve" => RoomType.HczCurve,
                "HCZ_Crossing" => RoomType.HczCrossing,
                "HCZ_Intersection" => RoomType.HczIntersection,
                "HCZ_Intersection_Junk" => RoomType.HczIntersectionJunk,
                "HCZ_Corner_Deep" => RoomType.HczCornerDeep,
                "HCZ_Straight" => RoomType.HczStraight,
                "HCZ_Straight_C" => RoomType.HczStraightC,
                "HCZ_Straight_PipeRoom"=> RoomType.HczStraightPipeRoom,
                "HCZ_Straight Variant" => RoomType.HczStraightVariant,
                "HCZ_ChkpA" => RoomType.HczElevatorA,
                "HCZ_ChkpB" => RoomType.HczElevatorB,
                "HCZ_127" => RoomType.Hcz127,
                "HCZ_ServerRoom" => RoomType.HczServerRoom,
                "HCZ_Intersection_Ramp" => RoomType.HczLoadingBay,
                "EZ_GateA" => RoomType.EzGateA,
                "EZ_GateB" => RoomType.EzGateB,
                "EZ_ThreeWay" => RoomType.EzTCross,
                "EZ_Crossing" => RoomType.EzCrossing,
                "EZ_Curve" => RoomType.EzCurve,
                "EZ_PCs" => RoomType.EzPcs,
                "EZ_upstairs" => RoomType.EzUpstairsPcs,
                "EZ_Intercom" => RoomType.EzIntercom,
                "EZ_Smallrooms2" => RoomType.EzSmallrooms,
                "EZ_PCs_small" => RoomType.EzDownstairsPcs,
                "EZ_Chef" => RoomType.EzChef,
                "EZ_Endoof" => RoomType.EzVent,
                "EZ_CollapsedTunnel" => RoomType.EzCollapsedTunnel,
                "EZ_Smallrooms1" => RoomType.EzConference,
                "EZ_Straight" => RoomType.EzStraight,
                "EZ_StraightColumn" => RoomType.EzStraightColumn,
                "EZ_Cafeteria" => RoomType.EzCafeteria,
                "EZ_Shelter" => RoomType.EzShelter,
                "EZ_HCZ_Checkpoint Part" => gameObject.transform.position.z switch
                {
                    > 95 => RoomType.EzCheckpointHallwayA,
                    _ => RoomType.EzCheckpointHallwayB,
                },
                "HCZ_EZ_Checkpoint Part" => gameObject.transform.position.z switch
                {
                    > 95 => RoomType.HczEzCheckpointA,
                    _ => RoomType.HczEzCheckpointB
                },
                _ => RoomType.Unknown,
            };
        }

        private static string TryRemovePostfixes(string str)
        {
            if (HolidayUtils.IsAnyHolidayActive())
                return str.Replace(HolidayUtils.GetActiveHoliday().ToString(), string.Empty).TrimEnd();
            return str;
        }
    }
}
