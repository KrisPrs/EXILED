// -----------------------------------------------------------------------
// <copyright file="LoaderMessages.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Loader.Features
{
    /// <summary>
    /// A class that contains the different EXILED loader messages.
    /// </summary>
    public static class LoaderMessages
    {
        /// <summary>
        /// Gets the default loader message.
        /// </summary>
        public static string Default => @"
   ▄████████ ▀████    ▐████▀  ▄█   ▄█          ▄████████ ████████▄
  ███    ███   ███▌   ████▀  ███  ███         ███    ███ ███   ▀███
  ███    █▀     ███  ▐███    ███▌ ███         ███    █▀  ███    ███
 ▄███▄▄▄        ▀███▄███▀    ███▌ ███        ▄███▄▄▄     ███    ███
▀▀███▀▀▀        ████▀██▄     ███▌ ███       ▀▀███▀▀▀     ███    ███
  ███    █▄    ▐███  ▀███    ███  ███         ███    █▄  ███    ███
  ███    ███  ▄███     ███▄  ███  ███▌    ▄   ███    ███ ███   ▄███
  ██████████ ████       ███▄ █▀   █████▄▄██   ██████████ ████████▀
                                  ▀                                 ";

        /// <summary>
        /// Gets the easter egg loader message.
        /// </summary>
        public static string EasterEgg => @"
   ▄████████    ▄████████ ▀████    ▐████▀  ▄█   ▄█          ▄████████ ████████▄
  ███    ███   ███    ███   ███▌   ████▀  ███  ███         ███    ███ ███   ▀███
  ███    █▀    ███    █▀     ███  ▐███    ███▌ ███         ███    █▀  ███    ███
  ███         ▄███▄▄▄        ▀███▄███▀    ███▌ ███        ▄███▄▄▄     ███    ███
▀███████████ ▀▀███▀▀▀        ████▀██▄     ███▌ ███       ▀▀███▀▀▀     ███    ███
         ███   ███    █▄    ▐███  ▀███    ███  ███         ███    █▄  ███    ███
   ▄█    ███   ███    ███  ▄███     ███▄  ███  ███▌    ▄   ███    ███ ███   ▄███
 ▄████████▀    ██████████ ████       ███▄ █▀   █████▄▄██   ██████████ ████████▀
                                                                                ";

        /// <summary>
        /// Gets the loader message according to the actual month.
        /// </summary>
        /// <returns>The correspondent loader message.</returns>
        public static string GetMessage()
        {
            if (Loader.Version.ToString().Contains("6.9") || Loader.Random.NextDouble() <= 0.069)
                return EasterEgg;

            return Default;
        }
    }
}