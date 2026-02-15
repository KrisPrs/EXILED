// -----------------------------------------------------------------------
// <copyright file="VersionControl.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Loader;

using System.Linq;
using LabApi.Loader.Features.Paths;
using System.Reflection;
using Exiled.API.Features;
using System;
using MEC;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

/// <summary>
/// Класс, отвечающий за контроль версий зависимостей.
/// </summary>
internal static class VersionControl
{
    private static string LabApiDeps => PathManager.Dependencies.ToString();

    private static string LabApiPlugins => PathManager.Plugins.ToString();

    /// <summary>
    /// Обновляет зависимости.
    /// </summary>
    public static void Update()
    {
        string labApiDepsDir = Path.Combine(LabApiDeps, "global");
        foreach (Assembly asm in LabApi.Loader.PluginLoader.Dependencies)
        {
            Log.Info($"{asm} ");
        }

        foreach (FileInfo fileInfo in Loader.DepsPaths.Select(dep => new FileInfo(dep)))
        {
            try
            {
                string destFile = Path.Combine(labApiDepsDir, $"{fileInfo.Name}");
                if (File.Exists(destFile))
                    File.Delete(destFile);
                File.Copy(fileInfo.FullName, destFile);
            }
            catch (Exception e)
            {
                Log.Error($"не удалось переместить dll {fileInfo.Name}. \n {e}");
            }
        }
    }

    /// <summary>
    /// Обновляет хэш и определяет, нужно ли обновлять зависимости.
    /// </summary>
    /// <returns>Нужно ли обновляться или нет.</returns>
    public static bool ProceedHashChecks()
    {
        string labApiDepsPath = Path.Combine(LabApiDeps, "global");
        List<string> correctLabApiDeps = Directory.GetFiles(labApiDepsPath, "*dll").ToList();
        List<string> correctExiledDeps = Directory.GetFiles(Paths.Dependencies, "*.dll").ToList();

        string depsExiledHash = GetFilesHash(correctExiledDeps);
        string depLabApiHash = GetFilesHash(correctLabApiDeps);

        return depLabApiHash != depsExiledHash;
    }

    /// <summary>
    /// Обновляет сам лоадер, если надо.
    /// </summary>
    public static void SelfUpdateIfNeed()
    {
        using SHA256 sha256 = SHA256.Create();

        FileInfo exiledLoaderPath = new(Path.Combine(Paths.Exiled, "Exiled.Loader.dll"));
        FileInfo labApiLoaderPath = new(Path.Combine(LabApiPlugins, "global", "Exiled.Loader.dll"));

        string exiledApi = Convert.ToBase64String(File.ReadAllBytes(exiledLoaderPath.FullName));
        string labApi = Convert.ToBase64String(File.ReadAllBytes(labApiLoaderPath.FullName));
        if (labApi == exiledApi)
            return;
        Log.SendRaw("Устаревшая версия Exiled.Loader.dll. Произвожу обновление через 8 секунд", ConsoleColor.Green);
        Timing.CallDelayed(8f, () =>
        {
            File.Delete(labApiLoaderPath.FullName);
            File.Copy(exiledLoaderPath.FullName, labApiLoaderPath.FullName);
            Server.Restart();
        });
    }

    /// <summary>
    /// Вычисляет Хэш всех файлов.
    /// </summary>
    /// <param name="filePaths">Список всех файлов.</param>
    /// <returns>Возвращает хэш всех файлов.</returns>
    private static string GetFilesHash(List<string> filePaths)
    {
        using SHA256 sha256 = SHA256.Create();
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        filePaths.Sort();

        foreach (string filePath in filePaths)
        {
            if (!File.Exists(filePath))
                continue;

            FileInfo fileInfo = new(filePath);

            writer.Write(Path.GetFileName(filePath));
            writer.Write(fileInfo.Length);
            byte[] content = File.ReadAllBytes(filePath);
            writer.Write(content);
        }

        ms.Position = 0;
        byte[] hashBytes = sha256.ComputeHash(ms);
        return Convert.ToBase64String(hashBytes);
    }
}