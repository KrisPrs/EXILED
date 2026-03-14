// -----------------------------------------------------------------------
// <copyright file="Scp079Role.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Roles
{
    using System.Collections.Generic;
    using System.Linq;

    using Exiled.API.Enums;
    using Exiled.API.Features.Doors;
    using Interactables.Interobjects.DoorUtils;
    using MapGeneration;
    using Mirror;
    using PlayerRoles;
    using PlayerRoles.PlayableScps;
    using PlayerRoles.PlayableScps.Scp079;
    using PlayerRoles.PlayableScps.Scp079.Cameras;
    using PlayerRoles.PlayableScps.Scp079.Pinging;
    using PlayerRoles.PlayableScps.Scp079.Rewards;
    using PlayerRoles.Subroutines;
    using PlayerRoles.Voice;
    using RelativePositioning;
    using Utils.NonAllocLINQ;

    using Mathf = UnityEngine.Mathf;
    using Scp079GameRole = PlayerRoles.PlayableScps.Scp079.Scp079Role;
    using Vector3 = UnityEngine.Vector3;

    /// <summary>
    /// Defines a role that represents SCP-079.
    /// </summary>
    public class Scp079Role : Role, ISubroutinedScpRole, ISpawnableScp, IVoiceRole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Scp079Role"/> class.
        /// </summary>
        /// <param name="baseRole">the base <see cref="Scp079GameRole"/>.</param>
        internal Scp079Role(Scp079GameRole baseRole)
            : base(baseRole)
        {
            this.SubroutineModule = baseRole.SubroutineModule;
            this.Base = baseRole;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079SpeakerAbility scp079SpeakerAbility))
                Log.Error("Scp079SpeakerAbility subroutine not found in Scp079Role::ctor");

            this.SpeakerAbility = scp079SpeakerAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079DoorStateChanger scp079DoorAbility))
                Log.Error("Scp079DoorStateChanger subroutine not found in Scp079Role::ctor");

            this.DoorStateChanger = scp079DoorAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079DoorLockChanger scp079DoorLockChanger))
                Log.Error("Scp079DoorLockChanger subroutine not found in Scp079Role::ctor");
            this.DoorLockChanger = scp079DoorLockChanger;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079AuxManager scp079AuxManager))
                Log.Error("Scp079AuxManager not found in Scp079Role::ctor");

            this.AuxManager = scp079AuxManager;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079TierManager scp079TierManager))
                Log.Error("Scp079TierManager subroutine not found in Scp079Role::ctor");

            this.TierManager = scp079TierManager;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079RewardManager scp079RewardManager))
                Log.Error("Scp079RewardManager subroutine not found in Scp079Role::ctor");

            this.RewardManager = scp079RewardManager;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079LockdownRoomAbility scp079LockdownRoomAbility))
                Log.Error("Scp079LockdownRoomAbility subroutine not found in Scp079Role::ctor");

            this.LockdownRoomAbility = scp079LockdownRoomAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079BlackoutRoomAbility scp079BlackoutRoomAbility))
                Log.Error("Scp079BlackoutRoomAbility subroutine not found in Scp079Role::ctor");

            this.BlackoutRoomAbility = scp079BlackoutRoomAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079BlackoutZoneAbility scp079BlackoutZoneAbility))
                Log.Error("Scp079BlackoutZoneAbility subroutine not found in Scp079Role::ctor");

            this.BlackoutZoneAbility = scp079BlackoutZoneAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079LostSignalHandler scp079LostSignalHandler))
                Log.Error("Scp079LostSignalHandler subroutine not found in Scp079Role::ctor");

            this.LostSignalHandler = scp079LostSignalHandler;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079CurrentCameraSync scp079CameraSync))
                Log.Error("Scp079CurrentCameraSync subroutine not found in Scp079Role::ctor");

            this.CurrentCameraSync = scp079CameraSync;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079PingAbility scp079PingAbility))
                Log.Error("Scp079PingAbility subroutine not found in Scp079Role::ctor");

            this.PingAbility = scp079PingAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079TeslaAbility scp079TeslaAbility))
                Log.Error("Scp079TeslaAbility subroutine not found in Scp079Role::ctor");

            this.TeslaAbility = scp079TeslaAbility;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079ScannerTracker scp079ScannerTracker))
                Log.Error("Scp079ScannerTracker subroutine not found in Scp079Role::ctor");

            this.ScannerTracker = scp079ScannerTracker;

            if (!this.SubroutineModule.TryGetSubroutine(out Scp079ScannerZoneSelector scp079ScannerZoneSelector))
                Log.Error("Scp079ScannerZoneSelector subroutine not found in Scp079Role::ctor");

            this.ScannerZoneSelector = scp079ScannerZoneSelector;
        }

        /// <summary>
        /// Gets a list of players who will be turned away from SCP-079's scan.
        /// </summary>
        public static HashSet<Player> TurnedPlayers { get; } = new(20);

        /// <inheritdoc/>
        public override RoleTypeId Type { get; } = RoleTypeId.Scp079;

        /// <inheritdoc/>
        public SubroutineManagerModule SubroutineModule { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079SpeakerAbility"/>.
        /// </summary>
        public Scp079SpeakerAbility SpeakerAbility { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079DoorAbility"/>.
        /// </summary>
        public Scp079DoorStateChanger DoorStateChanger { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079DoorLockChanger"/>.
        /// </summary>
        public Scp079DoorLockChanger DoorLockChanger { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079AuxManager"/>.
        /// </summary>
        public Scp079AuxManager AuxManager { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079TierManager"/>.
        /// </summary>
        public Scp079TierManager TierManager { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079RewardManager"/>.
        /// </summary>
        public Scp079RewardManager RewardManager { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079PingAbility"/>.
        /// </summary>
        public Scp079PingAbility PingAbility { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079TeslaAbility"/>.
        /// </summary>
        public Scp079TeslaAbility TeslaAbility { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079LockdownRoomAbility"/>.
        /// </summary>
        public Scp079LockdownRoomAbility LockdownRoomAbility { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079BlackoutRoomAbility"/>.
        /// </summary>
        public Scp079BlackoutRoomAbility BlackoutRoomAbility { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079BlackoutZoneAbility"/>.
        /// </summary>
        public Scp079BlackoutZoneAbility BlackoutZoneAbility { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079LostSignalHandler"/>.
        /// </summary>
        public Scp079LostSignalHandler LostSignalHandler { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079CurrentCameraSync"/>.
        /// </summary>
        public Scp079CurrentCameraSync CurrentCameraSync { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079ScannerTracker"/>.
        /// </summary>
        public Scp079ScannerTracker ScannerTracker { get; }

        /// <summary>
        /// Gets SCP-079's <see cref="Scp079ScannerZoneSelector"/>.
        /// </summary>
        public Scp079ScannerZoneSelector ScannerZoneSelector { get; }

        /// <summary>
        /// Gets or sets the camera SCP-079 is currently controlling.
        /// <remarks>This value will return the <c>Hcz079ContChamber</c> Camera if SCP-079's current camera cannot be detected.</remarks>
        /// </summary>
        public Camera Camera
        {
            get => Camera.Get(this.Base.CurrentCamera) ?? Camera.Get(CameraType.Hcz079ContChamber);
            set => this.Base._curCamSync.CurrentCamera = value.Base;
        }

        /// <summary>
        /// Gets a value indicating whether SCP-079 can transmit its voice to a speaker.
        /// </summary>
        public bool CanTransmit => this.SpeakerAbility.CanTransmit;

        /// <summary>
        /// Gets a list of rooms that have been marked by SCP-079. Marked rooms grant SCP-079 experience if a kill occurs in them.
        /// </summary>
        public IEnumerable<Room> MarkedRooms => this.RewardManager._markedRooms.Select(kvp => Room.Get(kvp.Key));

        /// <summary>
        /// Gets the speaker SCP-079 is currently using. Can be <see langword="null"/>.
        /// </summary>
        public Scp079Speaker Speaker => Scp079Speaker.TryGetSpeaker(this.Base.CurrentCamera, out Scp079Speaker speaker) ? speaker : null;

        /// <summary>
        /// Gets the doors SCP-079 has locked. Can be <see langword="null"/>.
        /// </summary>
        public Door LockedDoor => Door.Get(this.DoorLockChanger.LockedDoor);

        /// <summary>
        /// Gets a value indicating whether .
        /// </summary>
        public bool RespectPreferences => true;

        /// <summary>
        /// Gets a value indicating whether .
        /// </summary>
        public bool AllowFallback => true;

        /// <summary>
        /// Gets or sets SCP-079's abilities. Can be <see langword="null"/>.
        /// </summary>
        public IScp079AuxRegenModifier[] Abilities
        {
            get => this.AuxManager._abilities;
            set => this.AuxManager._abilities = value;
        }

        /// <summary>
        /// Gets or sets the amount of experience SCP-079 has.
        /// </summary>
        public int Experience
        {
            get => this.TierManager.TotalExp;
            set => this.TierManager.TotalExp = value;
        }

        /// <summary>
        /// Gets the Current Camera Position.
        /// </summary>
        public Vector3 CameraPosition => this.Base.CameraPosition;

        /// <summary>
        /// Gets the relative experience.
        /// </summary>
        public float RelativeExperience => this.TierManager.RelativeExp;

        /// <summary>
        /// Gets or sets SCP-079's level.
        /// </summary>
        public int Level
        {
            get => this.TierManager.AccessTierLevel;
            set => this.Experience = value <= 1 ? 0 : this.TierManager.AbsoluteThresholds[Mathf.Clamp(value - 1, 0, this.TierManager.AbsoluteThresholds.Length - 1)];
        }

        /// <summary>
        /// Gets or sets SCP-079's level index.
        /// </summary>
        public int LevelIndex
        {
            get => this.TierManager.AccessTierIndex;
            set => this.Level = value + 1;
        }

        /// <summary>
        /// Gets SCP-079's next level threshold.
        /// </summary>
        public int NextLevelThreshold => this.TierManager.NextLevelThreshold;

        /// <summary>
        /// Gets or sets SCP-079's energy.
        /// </summary>
        public float Energy
        {
            get => this.AuxManager.CurrentAux;
            set => this.AuxManager.CurrentAux = value;
        }

        /// <summary>
        /// Gets or sets SCP-079's max energy.
        /// </summary>
        public float MaxEnergy
        {
            get => this.AuxManager.MaxAux;
            set => this.AuxManager._maxPerTier[this.LevelIndex] = value;
        }

        /// <summary>
        /// Gets or sets SCP-079's room lockdown cooldown.
        /// </summary>
        public float RoomLockdownCooldown
        {
            get => this.LockdownRoomAbility.RemainingCooldown;
            set
            {
                this.LockdownRoomAbility.RemainingCooldown = value;
                this.LockdownRoomAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets the Remaining Lockdown Duration.
        /// </summary>
        public float RemainingLockdownDuration => this.LockdownRoomAbility.RemainingLockdownDuration;

        /// <summary>
        /// Gets the amount of rooms that SCP-079 has blacked out.
        /// </summary>
        public int BlackoutCount => this.BlackoutRoomAbility.RoomsOnCooldown;

        /// <summary>
        /// Gets the maximum amount of rooms that SCP-079 can black out at its current <see cref="Level"/>.
        /// </summary>
        public int BlackoutCapacity => this.BlackoutRoomAbility.CurrentCapacity;

        /// <summary>
        /// Gets or sets the amount of time until SCP-079 can use its blackout zone ability again.
        /// </summary>
        public float BlackoutZoneCooldown
        {
            get => this.BlackoutZoneAbility._cooldownTimer.Remaining;
            set
            {
                this.BlackoutZoneAbility._cooldownTimer.Remaining = value;
                this.BlackoutZoneAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the amount of time that SCP-2176 will disable SCP-079 for.
        /// </summary>
        public float Scp2176LostTime
        {
            get => this.LostSignalHandler._ghostlightLockoutDuration;
            set => this.LostSignalHandler._ghostlightLockoutDuration = value;
        }

        /// <summary>
        /// Gets the Roll Rotation of SCP-079.
        /// </summary>
        public float RollRotation => this.Base.RollRotation;

        /// <summary>
        /// Gets a value indicating whether SCP-079's signal is lost due to SCP-2176.
        /// </summary>
        public bool IsLost => this.LostSignalHandler.Lost;

        /// <summary>
        /// Gets a value indicating how much more time SCP-079 will be lost.
        /// </summary>
        public float LostTime => this.LostSignalHandler.RemainingTime;

        /// <summary>
        /// Gets SCP-079's energy regeneration speed.
        /// </summary>
        public float EnergyRegenerationSpeed => this.AuxManager.RegenSpeed;

        /// <inheritdoc/>
        public VoiceModuleBase VoiceModule => this.Base.VoiceModule;

        /// <summary>
        /// Gets the game <see cref="Scp079GameRole"/>.
        /// </summary>
        public new Scp079GameRole Base { get; }

        /// <summary>
        /// Unlocks all doors that SCP-079 has locked.
        /// </summary>
        public void UnlockAllDoors() => this.DoorLockChanger.ServerUnlock();

        /// <summary>
        /// Forces SCP-079's signal to be lost for the specified amount of time.
        /// </summary>
        /// <param name="duration">Time to lose SCP-079's signal.</param>
        public void LoseSignal(float duration) => this.LostSignalHandler.ServerLoseSignal(duration);

        /// <summary>
        /// Grants SCP-079 experience.
        /// </summary>
        /// <param name="amount">The amount to grant.</param>
        /// <param name="reason">The reason to grant experience.</param>
        public void AddExperience(int amount, Scp079HudTranslation reason = Scp079HudTranslation.ExpGainAdminCommand) => this.TierManager.ServerGrantExperience(amount, reason);

        /// <summary>
        /// Grants SCP-079 experience.
        /// </summary>
        /// <param name="amount">The amount to grant.</param>
        /// <param name="reason">The reason to grant experience.</param>
        /// <param name="subject">The RoleType of the player that's causing it to happen.</param>
        public void AddExperience(int amount, Scp079HudTranslation reason, RoleTypeId subject) => this.TierManager.ServerGrantExperience(amount, reason, subject);

        /// <summary>
        /// Locks the provided <paramref name="door"/>.
        /// </summary>
        /// <param name="door">The door to lock.</param>
        /// <returns><see langword="true"/> if the door has been lock; otherwise, <see langword="false"/>.</returns>
        public bool LockDoor(Door door)
        {
            if (door is not null)
            {
                this.DoorLockChanger.LockedDoor = door.Base;
                this.DoorLockChanger._lockTime = NetworkTime.time;
                this.DoorLockChanger.LockedDoor.ServerChangeLock(DoorLockReason.Regular079, true);
                if (door.Room is not null)
                    this.MarkRoom(door.Room);
                this.AuxManager.CurrentAux -= this.DoorLockChanger.GetCostForDoor(DoorAction.Locked, this.DoorLockChanger.LockedDoor);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Locks the provided <paramref name="door"/>.
        /// </summary>
        /// <param name="door">The door to lock.</param>
        /// <returns><see langword="true"/> if the door has been lock; otherwise, <see langword="false"/>.</returns>
        /// <param name="consumeEnergy">Indicates if the energy cost should be consumed or not.</param>
        public bool LockDoor(Door door, bool consumeEnergy = true)
        {
            if (door is not null)
            {
                this.DoorLockChanger.LockedDoor = door.Base;
                this.DoorLockChanger._lockTime = NetworkTime.time;
                this.DoorLockChanger.LockedDoor.ServerChangeLock(DoorLockReason.Regular079, true);
                this.MarkRoom(door.Room);
                if (consumeEnergy)
                    this.AuxManager.CurrentAux -= this.GetCost(door, DoorAction.Locked);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unlocks the <see cref="LockedDoor"/>.
        /// </summary>
        public void UnlockDoor() => this.LockedDoor?.Unlock();

        /// <summary>
        /// Unlocks the provided <paramref name="door"/>.
        /// </summary>
        /// <param name="door">The door to unlock.</param>
        public void UnlockDoor(Door door)
        {
            if (door is not null && Door.Get(this.DoorLockChanger.LockedDoor) == door)
            {
                door.Unlock();
            }
        }

        /// <summary>
        /// Marks a room as being modified by SCP-079 (granting experience if a kill happens in the room).
        /// </summary>
        /// <param name="room">The room to mark.</param>
        public void MarkRoom(Room room)
        {
            if (room is not null)
                this.RewardManager.MarkRoom(room.Identifier);
        }

        /// <summary>
        /// Marks a array of rooms as being modified by SCP-079 (granting experience if a kill happens in the room).
        /// </summary>
        /// <param name="rooms">The Array of Rooms to mark.</param>
        public void MarkRooms(IEnumerable<Room> rooms) => this.RewardManager.MarkRooms(rooms.Select(x => x.Identifier).ToArray());

        /// <summary>
        /// Removes a marked room.
        /// </summary>
        /// <param name="room">The room to remove.</param>
        public void UnmarkRoom(Room room)
        {
            if (room is not null && this.RewardManager._markedRooms.ContainsKey(room.Identifier))
                this.RewardManager._markedRooms.Remove(room.Identifier);
        }

        /// <summary>
        /// Clears the list of marked SCP-079 rooms.
        /// </summary>
        public void ClearMarkedRooms() => this.RewardManager._markedRooms.Clear();

        /// <summary>
        /// Gets the cost to switch from the current <see cref="Camera"/> to the provided <paramref name="camera"/>.
        /// </summary>
        /// <param name="camera">The camera to get the cost to switch to.</param>
        /// <returns>The cost to switch from the current camera to the new camera.</returns>
        public int GetSwitchCost(Camera camera) => camera is null ? 0 : this.CurrentCameraSync.GetSwitchCost(camera.Base);

        /// <summary>
        /// Gets the cost to modify a door.
        /// </summary>
        /// <param name="door">The door to get the cost to modify.</param>
        /// <param name="action">The action.</param>
        /// <returns>The cost to modify the door.</returns>
        public int GetCost(Door door, DoorAction action) => action is DoorAction.Locked or DoorAction.Unlocked ? this.DoorLockChanger.GetCostForDoor(action, door.Base) :
            this.DoorStateChanger.GetCostForDoor(action, door.Base);

        /// <summary>
        /// Blackout the current room.
        /// </summary>
        /// <param name="consumeEnergy">Indicates if the energy cost should be consumed or not.</param>
        public void BlackoutRoom(bool consumeEnergy = true)
        {
            if (consumeEnergy)
                this.BlackoutRoomAbility.AuxManager.CurrentAux -= this.BlackoutRoomAbility._cost;

            this.BlackoutRoomAbility.RewardManager.MarkRoom(this.BlackoutRoomAbility._roomController.Room);
            this.BlackoutRoomAbility._blackoutCooldowns[this.BlackoutRoomAbility._roomController.netId] = NetworkTime.time + this.BlackoutRoomAbility._cooldown;
            this.BlackoutRoomAbility._roomController.ServerFlickerLights(this.BlackoutRoomAbility._blackoutDuration);
            this.BlackoutRoomAbility._successfulController = this.BlackoutRoomAbility._roomController;
            this.BlackoutRoomAbility.ServerSendRpc(true);
        }

        /// <summary>
        /// Blackout the current zone.
        /// </summary>
        /// <param name="consumeEnergy">Indicates if the energy cost should be consumed or not.</param>
        public void BlackoutZone(bool consumeEnergy = true)
        {
            foreach (RoomLightController lightController in RoomLightController.Instances)
            {
                if (lightController.Room.Zone == this.BlackoutZoneAbility._syncZone)
                {
                    lightController.ServerFlickerLights(this.BlackoutZoneAbility._duration);
                }
            }

            this.BlackoutZoneAbility._cooldownTimer.Trigger(this.BlackoutZoneAbility._cooldown);

            if (consumeEnergy)
                this.BlackoutZoneAbility.AuxManager.CurrentAux -= this.BlackoutZoneAbility._cost;

            this.BlackoutZoneAbility.ServerSendRpc(true);
        }

        /// <summary>
        /// Trigger the Ping Ability to ping a <see cref="RelativePosition"/>.
        /// </summary>
        /// <param name="position">The SyncNormal Position.</param>
        /// <param name="pingType">The PingType to return.</param>
        /// <param name="consumeEnergy">Indicates if the energy cost should be consumed or not.</param>
        public void Ping(Vector3 position, PingType pingType = PingType.Default, bool consumeEnergy = true)
        {
            this.PingAbility._syncPos = new(position);
            this.PingAbility._syncNormal = position;
            this.PingAbility._syncProcessorIndex = (byte)pingType;

            this.PingAbility.ServerSendRpc(x => this.PingAbility.ServerCheckReceiver(x, this.PingAbility._syncPos.Position, (int)pingType));

            if (consumeEnergy)
                this.PingAbility.AuxManager.CurrentAux -= this.PingAbility._cost;

            this.PingAbility._rateLimiter.RegisterInput();
        }

        /// <summary>
        /// Trigger the Lockdown Room Ability to lock the current room.
        /// </summary>
        public void LockdownRoom() => this.LockdownRoomAbility.ServerInitLockdown();

        /// <summary>
        /// Cancels the Actual Lockdown.
        /// </summary>
        public void CancelLockdown() => this.LockdownRoomAbility.ServerCancelLockdown();

        /// <summary>
        /// Trigger the SCP-079's Tesla Gate Ability.
        /// </summary>
        /// <param name="consumeEnergy">Indicates if the energy cost should be consume or not.</param>
        public void ActivateTesla(bool consumeEnergy = true)
        {
            Scp079Camera cam = this.CurrentCameraSync.CurrentCamera;
            this.RewardManager.MarkRoom(cam.Room);

            if (!global::TeslaGate.AllGates.TryGetFirst(x => cam.Position.TryGetRoom(out RoomIdentifier camRoom) && x.transform.position.TryGetRoom(out RoomIdentifier teslaRoom) && camRoom == teslaRoom, out global::TeslaGate teslaGate))
                return;

            if (consumeEnergy)
                this.AuxManager.CurrentAux -= this.TeslaAbility._cost;

            teslaGate.RpcInstantBurst();
            this.TeslaAbility._nextUseTime = NetworkTime.time + this.TeslaAbility._cooldown;
            this.TeslaAbility.ServerSendRpc(false);
        }

        /// <summary>
        /// Gets the spawn chance of SCP-079.
        /// </summary>
        /// <param name="alreadySpawned">The List of Roles already spawned.</param>
        /// <returns>The Spawn Chance.</returns>
        public float GetSpawnChance(List<RoleTypeId> alreadySpawned) => this.Base.GetSpawnChance(alreadySpawned);
    }
}
