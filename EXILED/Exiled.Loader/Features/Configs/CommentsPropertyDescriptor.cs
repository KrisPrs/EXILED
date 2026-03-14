// -----------------------------------------------------------------------
// <copyright file="CommentsPropertyDescriptor.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Loader.Features.Configs
{
    using System;
    using System.ComponentModel;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Source: https://dotnetfiddle.net/8M6iIE.
    /// </summary>
    public sealed class CommentsPropertyDescriptor : IPropertyDescriptor
    {
        private readonly IPropertyDescriptor baseDescriptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentsPropertyDescriptor"/> class.
        /// </summary>
        /// <param name="baseDescriptor">The base descriptor instance.</param>
        public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
        {
            this.baseDescriptor = baseDescriptor;
            this.Name = baseDescriptor.Name;
        }

        /// <inheritdoc cref="IPropertyDescriptor"/>
        public string Name { get; set; }

        /// <inheritdoc cref="IPropertyDescriptor"/>
        public Type Type => this.baseDescriptor.Type;

        /// <inheritdoc cref="IPropertyDescriptor"/>
        public Type TypeOverride
        {
            get => this.baseDescriptor.TypeOverride;
            set => this.baseDescriptor.TypeOverride = value;
        }

        /// <inheritdoc cref="IPropertyDescriptor"/>
        public int Order { get; set; }

        /// <inheritdoc cref="IPropertyDescriptor"/>
        public ScalarStyle ScalarStyle
        {
            get => this.baseDescriptor.ScalarStyle;
            set => this.baseDescriptor.ScalarStyle = value;
        }

        /// <inheritdoc cref="IPropertyDescriptor"/>
        public bool CanWrite => this.baseDescriptor.CanWrite;

        /// <inheritdoc cref="IPropertyDescriptor"/>
        public void Write(object target, object value) => this.baseDescriptor.Write(target, value);

        /// <inheritdoc cref="IPropertyDescriptor"/>
        public T GetCustomAttribute<T>()
            where T : Attribute => this.baseDescriptor.GetCustomAttribute<T>();

        /// <inheritdoc cref="IPropertyDescriptor"/>
        public IObjectDescriptor Read(object target)
        {
            DescriptionAttribute description = this.baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
            return description is not null
                ? new CommentsObjectDescriptor(this.baseDescriptor.Read(target), description.Description)
                : this.baseDescriptor.Read(target);
        }
    }
}