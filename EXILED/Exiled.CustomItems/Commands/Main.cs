// -----------------------------------------------------------------------
// <copyright file="Main.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.Commands
{
    using System;
    using System.Collections.Generic;

    using CommandSystem;
    using Exiled.API.Features;

    /// <summary>
    /// The main command.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal sealed class Main : ParentCommand, IUsageProvider
    {
        /// <inheritdoc/>
        public override string Command { get; } = "customitems";

        /// <inheritdoc/>
        public override string[] Aliases { get; set; } = { "ci", "cis" };

        /// <inheritdoc/>
        public override string Description { get; set; } = "Кастомные предметы: give (g), spawn (sp), info (i), list (l)";

        /// <summary>
        /// Gets the usage hint shown in the Remote Admin console.
        /// </summary>
        public string[] Usage { get; } = { "give/spawn/info/list", "Название/ID предмета", "Игрок/all" };

        /// <inheritdoc/>
        protected override IEnumerable<Type> CommandsToRegister()
        {
            yield return typeof(Give);
            yield return typeof(Spawn);
            yield return typeof(Info);
            yield return typeof(List.List);
        }
    }
}
