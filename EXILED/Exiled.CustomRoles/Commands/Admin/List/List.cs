// -----------------------------------------------------------------------
// <copyright file="List.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Commands.Admin.List
{
    using System;
    using System.Collections.Generic;

    using Exiled.API.Features;

    /// <summary>
    ///     The command to list all registered roles.
    /// </summary>
    internal sealed class List : ParentCommand
    {
        /// <inheritdoc />
        public override string Command { get; } = "list";

        /// <inheritdoc />
        public override string[] Aliases { get; set; } = { "l" };

        /// <inheritdoc />
        public override string Description { get; set; } = "Списки кастомных ролей.";

        /// <inheritdoc />
        protected override IEnumerable<Type> CommandsToRegister()
        {
            yield return typeof(Registered);
            yield return typeof(InGame);
        }
    }
}