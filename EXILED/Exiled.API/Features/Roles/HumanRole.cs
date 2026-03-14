// -----------------------------------------------------------------------
// <copyright file="HumanRole.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Roles
{
    using Mirror;

    using PlayerRoles;
    using PlayerRoles.PlayableScps.HumeShield;
    using Respawning;
    using Respawning.NamingRules;

    using HumanGameRole = PlayerRoles.HumanRole;

    /// <summary>
    /// Defines a role that represents a human class.
    /// </summary>
    public class HumanRole : FpcRole, IHumeShieldRole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HumanRole"/> class.
        /// </summary>
        /// <param name="baseRole">the base <see cref="HumanGameRole"/>.</param>
        internal HumanRole(HumanGameRole baseRole)
            : base(baseRole)
        {
            this.Base = baseRole;
            this.HumeShieldModule = baseRole.HumeShieldModule;
        }

        /// <inheritdoc/>
        public override RoleTypeId Type => this.Base.RoleTypeId;

        /// <summary>
        /// Gets the player's unit name.
        /// </summary>
        public string UnitName => NamingRulesManager.ClientFetchReceived(this.Team, this.UnitNameId);

        /// <summary>
        /// Gets or sets the <see cref="UnitNameId"/>.
        /// </summary>
        public byte UnitNameId
        {
            get => this.Base.UnitNameId;
            set => this.Base.UnitNameId = value;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="HumanRole"/> uses unit names or not.
        /// </summary>
        public bool UsesUnitNames => this.Base.UsesUnitNames;

        /// <summary>
        /// Gets the game <see cref="HumanGameRole"/>.
        /// </summary>
        public new HumanGameRole Base { get; }

        /// <inheritdoc/>
        public HumeShieldModuleBase HumeShieldModule { get; }

        /// <summary>
        /// Gets the <see cref="HumanRole"/> armor efficacy based on a specific <see cref="HitboxType"/> and the armor the <see cref="Role.Owner"/> is wearing.
        /// </summary>
        /// <param name="hitbox">The <see cref="HitboxType"/>.</param>
        /// <returns>The armor efficacy.</returns>
        public int GetArmorEfficacy(HitboxType hitbox) => this.Base.GetArmorEfficacy(hitbox);

        /// <inheritdoc/>
        internal override void SendAppearanceSpawnMessage(NetworkWriter writer, PlayerRoleBase basicRole)
        {
            if (this.UsesUnitNames)
                writer.WriteByte(basicRole is HumanGameRole humanRole && humanRole.UsesUnitNames ? humanRole.UnitNameId : (byte)0);

            base.SendAppearanceSpawnMessage(writer, basicRole);
        }
    }
}