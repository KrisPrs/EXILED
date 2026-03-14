// -----------------------------------------------------------------------
// <copyright file="ReservedSlotsCheckEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using Exiled.API.Enums;
    using Exiled.Events.EventArgs.Interfaces;

    /// <summary>
    /// Contains all information when checking if a player has a reserved slot.
    /// </summary>
    public class ReservedSlotsCheckEventArgs : IExiledEvent, IDeniableEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservedSlotsCheckEventArgs" /> class.
        /// </summary>
        /// <param name="hasReservedSlot">
        /// <inheritdoc cref="HasReservedSlot" />
        /// </param>
        /// <param name="userId">
        /// <inheritdoc cref="UserId" />
        /// </param>
        public ReservedSlotsCheckEventArgs(bool hasReservedSlot, string userId)
        {
            this.UserId = userId;
            this.HasReservedSlot = hasReservedSlot;
            this.IsAllowed = hasReservedSlot;
            this.Result = ReservedSlotEventResult.UseBaseGameSystem;
        }

        /// <summary>
        /// Gets the UserID of the player that is being checked.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Gets a value indicating whether the player has a reserved slot in the base game system.
        /// </summary>
        public bool HasReservedSlot { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the player is allowed to connect.
        /// </summary>
        public bool IsAllowed { get; set; }

        /// <summary>
        /// Gets or sets the event result.
        /// </summary>
        public ReservedSlotEventResult Result
        {
            get;
            set
            {
                switch (value)
                {
                    case ReservedSlotEventResult.CanUseReservedSlots or ReservedSlotEventResult.UseBaseGameSystem:
                        this.IsAllowed = this.HasReservedSlot;
                        break;
                    case ReservedSlotEventResult.AllowConnectionUnconditionally:
                        this.IsAllowed = true;
                        break;
                    case ReservedSlotEventResult.CannotUseReservedSlots:
                        this.IsAllowed = false;
                        break;
                    default:
                        return;
                }

                field = value;
            }
        }
    }
}
