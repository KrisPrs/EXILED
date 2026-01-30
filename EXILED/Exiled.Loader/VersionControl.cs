// -----------------------------------------------------------------------
// <copyright file="VersionControl.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Loader;

using System.Reflection;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

/// <summary>
/// Класс, отвечающий за контроль версий зависимостей.
/// </summary>
internal static class VersionControl
{
    private static string LabApiDeps => LabApi.Loader.Features.Paths.PathManager.Dependencies.ToString();

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

        foreach (Assembly assembly in Loader.Dependencies)
        {
            try
            {
                if(string.IsNullOrEmpty(assembly.GetPath()))
                    continue;
                string destFile = Path.Combine(labApiDepsDir, $"{assembly.GetName().Name}.dll");
                if (File.Exists(destFile))
                    File.Delete(destFile);
                File.Copy(assembly.GetPath(), destFile);
            }
            catch (Exception e)
            {
                Log.Error($"не удалось переместить dll {assembly}. \n {e}");
            }
        }
    }

    /// <summary>
    /// Обновляет хэш и определяет, нужно ли обновлять зависимости.
    /// </summary>
    /// <returns>Нужно ли обновляться или нет.</returns>
    public static bool ProceedHashChecks()
    {
        string exiledHashPath = Path.Combine(Paths.Dependencies, "version.txt");
        string labApiHashPath = Path.Combine(LabApiDeps, "global", "version.txt");

        List<string> exiledDepsPath = Loader.Dependencies.Select(assembly => assembly.Location).ToList();
        string hash = GetFilesHash(exiledDepsPath);
        File.WriteAllText(exiledHashPath, hash);

        if (!File.Exists(labApiHashPath))
        {
            File.WriteAllText(labApiHashPath, hash);
            return true;
        }

        string labApiHash = File.ReadAllText(labApiHashPath);
        if (labApiHash != hash)
            File.WriteAllText(labApiHashPath, hash);

        return labApiHash != hash;
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
            writer.Write(fileInfo.LastWriteTimeUtc.Ticks);
            byte[] content = File.ReadAllBytes(filePath);
            writer.Write(content);
        }

        ms.Position = 0;
        byte[] hashBytes = sha256.ComputeHash(ms);
        return Convert.ToBase64String(hashBytes);
    }
}