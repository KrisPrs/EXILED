// -----------------------------------------------------------------------
// <copyright file="AirlockController.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Doors
{
    using System.Collections.Generic;
    using System.Linq;

    using BaseController = Interactables.Interobjects.AirlockController;

    /// <summary>
    /// Represents airlock.
    /// </summary>
    public class AirlockController
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> containing all known <see cref="BaseController"/>'s and their corresponding <see cref="AirlockController"/>.
        /// </summary>
        internal static readonly Dictionary<BaseController, AirlockController> BaseToExiledControllers = new(new ComponentsEqualityComparer());

        /// <summary>
        /// Initializes a new instance of the <see cref="AirlockController"/> class.
        /// </summary>
        /// <param name="controller">Base-game controller.</param>
        public AirlockController(BaseController controller)
        {
            this.Base = controller;

            BaseToExiledControllers.Add(controller, this);
        }

        /// <summary>
        /// Gets the list with all airlocks.
        /// </summary>
        public static IReadOnlyCollection<AirlockController> List => BaseToExiledControllers.Values;

        /// <summary>
        /// Gets the basegame controller.
        /// </summary>
        public BaseController Base { get; }

        /// <summary>
        /// Gets the first subdoor.
        /// </summary>
        public Door DoorA => Door.Get(this.Base._doorA);

        /// <summary>
        /// Gets the second subdoor.
        /// </summary>
        public Door DoorB => Door.Get(this.Base._doorB);

        /// <summary>
        /// Gets or sets a value indicating whether both subdoors are locked.
        /// </summary>
        public bool DoorsLocked
        {
            get => this.Base._doorsLocked;
            set => this.Base._doorsLocked = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or airlock is disabled.
        /// </summary>
        public bool AirlockDisabled
        {
            get => this.Base.AirlockDisabled;
            set => this.Base.AirlockDisabled = value;
        }

        /// <summary>
        /// Gets the <see cref="AirlockController"/> by its base-game controller.
        /// </summary>
        /// <param name="controller">Base-game controller.</param>
        /// <returns>Instance of <see cref="AirlockController"/>.</returns>
        public static AirlockController Get(BaseController controller) => controller != null ? (BaseToExiledControllers.TryGetValue(controller, out AirlockController airlockController) ? airlockController : new AirlockController(controller)) : null;

        /// <summary>
        /// Gets the <see cref="AirlockController"/> by one of it's subdoors.
        /// </summary>
        /// <param name="door">Subdoor.</param>
        /// <returns>Instance of <see cref="AirlockController"/>.</returns>
        public static AirlockController Get(Door door) => BaseToExiledControllers.Values.FirstOrDefault(x => x.DoorA == door || x.DoorB == door);

        /// <summary>
        /// Toggles airlock.
        /// </summary>
        public void Toggle() => this.Base.ToggleAirlock();

        /// <summary>
        /// Returns the Door in a human-readable format.
        /// </summary>
        /// <returns>A string containing Door-related data.</returns>
        public override string ToString() => $"|{this.DoorA}| /{this.DoorB}/ *{this.DoorsLocked}* ={this.AirlockDisabled}=";
    }
}