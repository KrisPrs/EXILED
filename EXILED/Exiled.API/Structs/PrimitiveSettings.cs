// -----------------------------------------------------------------------
// <copyright file="PrimitiveSettings.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Structs
{
    using AdminToys;
    using UnityEngine;

    /// <summary>
    /// Settings for primitives.
    /// </summary>
    public struct PrimitiveSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveSettings"/> struct.
        /// </summary>
        /// <param name="primitiveType">The type of the primitive.</param>
        /// <param name="color">The color of the primitive.</param>
        /// <param name="position">The position of the primitive.</param>
        /// <param name="rotation">The rotation of the primitive.</param>
        /// <param name="scale">The scale of the primitive.</param>
        /// <param name="spawn">Whether the primitive should be spawned.</param>
        /// <param name="isStatic">Whether the primitive should be static.</param>
        public PrimitiveSettings(PrimitiveType primitiveType, Color color, Vector3 position, Vector3 rotation, Vector3 scale, bool spawn, bool isStatic)
        {
            this.PrimitiveType = primitiveType;
            this.Flags = PrimitiveFlags.Collidable | PrimitiveFlags.Visible;
            this.Color = color;
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
            this.Spawn = spawn;
            this.IsStatic = isStatic;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveSettings"/> struct.
        /// </summary>
        /// <param name="primitiveType">The type of the primitive.</param>
        /// <param name="color">The color of the primitive.</param>
        /// <param name="position">The position of the primitive.</param>
        /// <param name="rotation">The rotation of the primitive.</param>
        /// <param name="scale">The scale of the primitive.</param>
        /// <param name="spawn">Whether the primitive should be spawned.</param>
        public PrimitiveSettings(PrimitiveType primitiveType, Color color, Vector3 position, Vector3 rotation, Vector3 scale, bool spawn)
        {
            this.PrimitiveType = primitiveType;
            this.Flags = PrimitiveFlags.Collidable | PrimitiveFlags.Visible;
            this.Color = color;
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
            this.Spawn = spawn;
            this.IsStatic = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveSettings"/> struct.
        /// </summary>
        /// <param name="primitiveType">The type of the primitive.</param>
        /// <param name="primitiveFlags">The flags of the primitive.</param>
        /// <param name="color">The color of the primitive.</param>
        /// <param name="position">The position of the primitive.</param>
        /// <param name="rotation">The rotation of the primitive.</param>
        /// <param name="scale">The scale of the primitive.</param>
        /// <param name="spawn">Whether the primitive should be spawned.</param>
        public PrimitiveSettings(PrimitiveType primitiveType, PrimitiveFlags primitiveFlags, Color color, Vector3 position, Vector3 rotation, Vector3 scale, bool spawn)
        {
            this.PrimitiveType = primitiveType;
            this.Flags = primitiveFlags;
            this.Color = color;
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
            this.Spawn = spawn;
            this.IsStatic = false;
        }

        /// <summary>
        /// Gets the primitive type.
        /// </summary>
        public PrimitiveType PrimitiveType { get; }

        /// <summary>
        /// Gets the primitive flags.
        /// </summary>
        public PrimitiveFlags Flags { get; }

        /// <summary>
        /// Gets the primitive color.
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// Gets the primitive position.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Gets the primitive rotation.
        /// </summary>
        public Vector3 Rotation { get; }

        /// <summary>
        /// Gets the primitive scale.
        /// </summary>
        public Vector3 Scale { get; }

        /// <summary>
        /// Gets a value indicating whether the primitive should be spawned.
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the primitive should be spawned.
        /// </summary>
        public bool Spawn { get; }
    }
}
