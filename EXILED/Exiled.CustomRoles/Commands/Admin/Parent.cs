// -----------------------------------------------------------------------
// <copyright file="Parent.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Commands.Admin
{
    using System;
    using System.Collections.Generic;

    using CommandSystem;
    using Exiled.API.Features;

    /// <summary>
    ///     The main parent command.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Parent : ParentCommand, IUsageProvider
    {
        /// <inheritdoc />
        public override string Command { get; } = "customroles";

        /// <inheritdoc />
        public override string[] Aliases { get; set; } = { "cr", "crs" };

        /// <inheritdoc />
        public override string Description { get; set; } = "Кастомные роли: give (g), info (i), list (l)";

        /// <summary>
        /// Gets the usage hint shown in the Remote Admin console.
        /// </summary>
        public string[] Usage { get; } = { "give/info/list", "Название/ID роли", "Игрок/all" };

        /// <inheritdoc />
        protected override IEnumerable<Type> CommandsToRegister()
        {
            yield return typeof(Give);
            yield return typeof(Info);
            yield return typeof(List.List);
        }
    }
}