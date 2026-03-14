// -----------------------------------------------------------------------
// <copyright file="Plugin.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features
{
#pragma warning disable SA1402
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using CommandSystem;
    using Discord;
    using Enums;
    using Extensions;
    using Interfaces;
    using RemoteAdmin;

    /// <summary>
    /// Expose how a plugin has to be made.
    /// </summary>
    /// <typeparam name="TConfig">The config type.</typeparam>
    public abstract class Plugin<TConfig> : IPlugin<TConfig>
        where TConfig : IConfig, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin{TConfig}"/> class.
        /// </summary>
        public Plugin()
        {
            this.Assembly = Assembly.GetCallingAssembly();
            this.Name = this.Assembly.GetName().Name;
            this.Prefix = this.Name.ToSnakeCase();
            this.Author = this.Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            this.Version = this.Assembly.GetName().Version;
        }

        /// <inheritdoc/>
        public Assembly Assembly { get; protected set; }

        /// <inheritdoc/>
        public virtual string Name { get; }

        /// <inheritdoc/>
        public virtual string Prefix { get; }

        /// <inheritdoc/>
        public virtual string Author { get; }

        /// <inheritdoc/>
        public virtual PluginPriority Priority { get; }

        /// <inheritdoc/>
        public virtual Version Version { get; }

        /// <inheritdoc/>
        public virtual Version RequiredExiledVersion { get; } = typeof(IPlugin<>).Assembly.GetName().Version;

        /// <inheritdoc/>
        public virtual bool IgnoreRequiredVersionCheck { get; } = false;

        /// <inheritdoc/>
        public Dictionary<Type, (ICommand, HashSet<Type>)> Commands { get; } = new();

        /// <inheritdoc/>
        public TConfig Config { get; } = new();

        /// <inheritdoc/>
        public ITranslation InternalTranslation { get; protected set; }

        /// <inheritdoc/>
        public string ConfigPath => Paths.GetConfigPath(this.Prefix);

        /// <inheritdoc/>
        public string TranslationPath => Paths.GetTranslationPath(this.Prefix);

        /// <inheritdoc/>
        public virtual void OnEnabled()
        {
            AssemblyInformationalVersionAttribute attribute = this.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            Log.Info($"{this.Name} v{(this.Version is not null ? $"{this.Version.Major}.{this.Version.Minor}.{this.Version.Build}" : attribute is not null ? attribute.InformationalVersion : string.Empty)} by {this.Author} has been enabled!");
        }

        /// <inheritdoc/>
        public virtual void OnDisabled() => Log.Info($"{this.Name} has been disabled!");

        /// <inheritdoc/>
        public virtual void OnReloaded() => Log.Info($"{this.Name} has been reloaded!");

        /// <inheritdoc/>
        public virtual void OnRegisteringCommands()
        {
            foreach (Type type in this.Assembly.GetTypes())
            {
                if (type.GetInterface(nameof(ICommand)) != typeof(ICommand))
                    continue;

                if (!Attribute.IsDefined(type, typeof(CommandHandlerAttribute)))
                    continue;

                foreach (CustomAttributeData customAttributeData in type.CustomAttributes)
                {
                    try
                    {
                        if (customAttributeData.AttributeType != typeof(CommandHandlerAttribute))
                            continue;

                        Type handlerType = (Type)customAttributeData.ConstructorArguments[0].Value;
                        this.RegisterCommand(handlerType, (ICommand)Activator.CreateInstance(type));
                    }
                    catch (Exception exception)
                    {
                        Log.Error($"An error has occurred while registering a command: {exception}");
                    }
                }
            }
        }

        /// <summary>
        /// Удобно регистрирует команду с заносом в важные словарики. Не регистрируйте команды вручную, используйте это.
        /// </summary>
        /// <param name="commandHandlerType">В какой тип консоли будет зарегистрирована команда.</param>
        /// <param name="command">Команда для регистрации.</param>
        public void RegisterCommand(Type commandHandlerType, ICommand command)
        {
            Type commandTypeToRegister = command.GetType();
            if (!this.Commands.TryGetValue(commandTypeToRegister, out (ICommand, HashSet<Type>) commandData))
                commandData = (command, new HashSet<Type>());
            if (commandData.Item2.Contains(commandHandlerType))
                return;

            this.RegisterCommand(commandHandlerType, commandData);
        }

        /// <inheritdoc/>
        public virtual void OnUnregisteringCommands()
        {
            foreach ((ICommand, HashSet<Type>) command in this.Commands.Values)
            {
                if (command.Item2.Contains(typeof(RemoteAdminCommandHandler)))
                    CommandProcessor.RemoteAdminCommandHandler.UnregisterCommand(command.Item1);
                else if (command.Item2.Contains(typeof(GameConsoleCommandHandler)))
                    GameCore.Console.ConsoleCommandHandler.UnregisterCommand(command.Item1);
                else if (command.Item2.Contains(typeof(ClientCommandHandler)))
                    QueryProcessor.DotCommandHandler.UnregisterCommand(command.Item1);
            }
        }

        /// <inheritdoc/>
        public int CompareTo(IPlugin<IConfig> other) => -this.Priority.CompareTo(other.Priority);

        private void RegisterCommand(Type commandHandlerType, (ICommand, HashSet<Type>) commandData)
        {
            ICommand command = commandData.Item1;
            Type commandTypeToRegister = command.GetType();
            try
            {
                if (commandHandlerType == typeof(RemoteAdminCommandHandler))
                {
                    CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(command);
                }
                else if (commandHandlerType == typeof(GameConsoleCommandHandler))
                {
                    GameCore.Console.ConsoleCommandHandler.RegisterCommand(command);
                }
                else if (commandHandlerType == typeof(ClientCommandHandler))
                {
                    QueryProcessor.DotCommandHandler.RegisterCommand(command);
                }
                else
                {
                    Log.Error($"Invalid command handler type provided for command {command.Command} in {this.Name}: {commandHandlerType}");
                    return;
                }
            }
            catch (ArgumentException e)
            {
                Log.Error($"An error has occurred while registering a command: {e}");
                return;
            }

            commandData.Item2.Add(commandHandlerType);
            this.Commands[commandTypeToRegister] = commandData;
            if (command is global::ParentCommand)
                Log.Send($"[{this.Name}.{nameof(this.RegisterCommand)}] Command '{command.Command}' uses obsolete ParentCommand class. Use {typeof(ParentCommand).FullName} instead of {typeof(global::ParentCommand).FullName}.", LogLevel.Debug, ConsoleColor.DarkGray);

            if (!commandTypeToRegister.GetProperty(nameof(ICommand.Description))?.CanWrite ?? true)
                Log.Send($"[{this.Name}.{nameof(this.RegisterCommand)}] Command '{command.Command}' has description without setter, making translation impossible. Consider fixing this.", LogLevel.Debug, ConsoleColor.DarkGray);
        }
    }

    /// <summary>
    /// Expose how a plugin has to be made.
    /// </summary>
    /// <typeparam name="TConfig">The config type.</typeparam>
    /// <typeparam name="TTranslation">The translation type.</typeparam>
    public abstract class Plugin<TConfig, TTranslation> : Plugin<TConfig>
        where TConfig : IConfig, new()
        where TTranslation : ITranslation, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin{TConfig, TTranslation}"/> class.
        /// </summary>
        public Plugin()
        {
            this.Assembly = Assembly.GetCallingAssembly();
            this.InternalTranslation = new TTranslation();
        }

        /// <summary>
        /// Gets the plugin translations.
        /// </summary>
        public TTranslation Translation => (TTranslation)this.InternalTranslation;
    }
}
