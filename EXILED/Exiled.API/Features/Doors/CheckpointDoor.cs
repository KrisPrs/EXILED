// -----------------------------------------------------------------------
// <copyright file="CheckpointDoor.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Doors
{
    using System.Collections.Generic;
    using System.Linq;

    using Interactables.Interobjects.DoorUtils;

    /// <summary>
    /// Represents a checkpoint door.
    /// </summary>
    public class CheckpointDoor : Door, Interfaces.IDamageableDoor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckpointDoor"/> class.
        /// </summary>
        /// <param name="door">The base <see cref="Interactables.Interobjects.CheckpointDoor"/> for this door.</param>
        /// <param name="room">The <see cref="Room"/> for this door.</param>
        internal CheckpointDoor(Interactables.Interobjects.CheckpointDoor door, List<Room> room)
            : base(door, room)
        {
            this.Base = door;
            this.Subdoors = this.SubDoorsValue.AsReadOnly();
        }

        /// <summary>
        /// Gets the base <see cref="Interactables.Interobjects.CheckpointDoor"/>.
        /// </summary>
        public new Interactables.Interobjects.CheckpointDoor Base { get; }

        /// <summary>
        /// Gets the list of all sub doors belonging to this <see cref="CheckpointDoor"/>.
        /// </summary>
        public IReadOnlyCollection<BreakableDoor> Subdoors { get; }

        /// <summary>
        /// Gets or sets the current checkpoint stage.
        /// </summary>
        public Interactables.Interobjects.CheckpointDoor.SequenceState CurrentStage
        {
            get => this.Base.CurSequence;
            set => this.Base.CurSequence = value;
        }

        /// <summary>
        /// Gets or sets a time in seconds for main timer.
        /// </summary>
        public float MainTimer
        {
            get => this.Base.SequenceCtrl.RemainingTime;
            set => this.Base.SequenceCtrl.RemainingTime = value;
        }

        /// <summary>
        /// Gets or sets time before doors close.
        /// </summary>
        public float WaitTime
        {
            get => this.Base.SequenceCtrl.OpenLoopTime;
            set => this.Base.SequenceCtrl.OpenLoopTime = value;
        }

        /// <summary>
        /// Gets or sets time in seconds when warning will be shown.
        /// </summary>
        public float WarningTime
        {
            get => this.Base.SequenceCtrl.WarningTime;
            set => this.Base.SequenceCtrl.WarningTime = value;
        }

        /// <inheritdoc/>
        public bool IsDestroyed
        {
            get => this.Base.IsDestroyed;
            set => this.Base.IsDestroyed = value;
        }

        /// <inheritdoc/>
        public bool IsBreakable => !this.IsDestroyed;

        /// <inheritdoc/>
        public float Health
        {
            get => this.Base.GetHealthPercent();
            set
            {
                float health = value / this.Subdoors.Count;

                foreach (BreakableDoor door in this.Subdoors)
                {
                    door.Health = health;
                }
            }
        }

        /// <inheritdoc/>
        public float MaxHealth
        {
            get => this.Subdoors.Sum(door => door.MaxHealth);
            set
            {
                float health = value / this.Subdoors.Count;

                foreach (BreakableDoor door in this.Subdoors)
                {
                    door.MaxHealth = health;
                }
            }
        }

        /// <inheritdoc/>
        public DoorDamageType IgnoredDamage
        {
            get => this.Subdoors.Aggregate(DoorDamageType.None, (current, door) => current | door.IgnoredDamage);
            set
            {
                foreach (BreakableDoor door in this.Subdoors)
                {
                    door.IgnoredDamage = value;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing all known subdoors <see cref="Door"/>s.
        /// </summary>
        internal List<BreakableDoor> SubDoorsValue { get; } = new();

        /// <summary>
        /// Repair the door.
        /// </summary>
        public void Repair() => this.Base.ServerRepair();

        /// <summary>
        /// Toggles the state of the doors from <see cref="Subdoors"/>.
        /// </summary>
        /// <param name="newState">New state for the subdoors.</param>
        public void ToggleAllDoors(bool newState) => this.Base.ToggleAllDoors(newState);

        /// <inheritdoc/>
        public bool Damage(float amount, DoorDamageType damageType = DoorDamageType.ServerCommand) => this.Base.ServerDamage(amount, damageType);

        /// <inheritdoc/>
        public bool Break(DoorDamageType type = DoorDamageType.ServerCommand) => this.Base.ServerDamage(float.MaxValue, type);

        /// <summary>
        /// Returns the Door in a human-readable format.
        /// </summary>
        /// <returns>A string containing Door-related data.</returns>
        public override string ToString() => $"{base.ToString()} |{this.WaitTime}| -{this.WarningTime}-";
    }
}