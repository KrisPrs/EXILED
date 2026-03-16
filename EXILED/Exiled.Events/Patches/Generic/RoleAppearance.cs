// -----------------------------------------------------------------------
// <copyright file="RoleAppearance.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Generic
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features;

    using Exiled.API.Extensions;
    using Exiled.API.Features.Pools;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Player;

    using HarmonyLib;

    using Mirror;

    using PlayerRoles;
    using PlayerRoles.FirstPersonControl;
    using PlayerRoles.PlayableScps.Scp049.Zombies;
    using PlayerRoles.SpawnData;

    using RelativePositioning;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Патч для <see cref="RoleSyncInfo.Write(NetworkWriter)"/>.
    /// Реализует систему подмены внешности через <see cref="Role.GlobalAppearance"/>,
    /// <see cref="Role.TeamAppearances"/>, <see cref="Role.RoleAppearances"/> и <see cref="Role.IndividualAppearances"/>.
    /// </summary>
    [HarmonyPatch(typeof(RoleSyncInfo), nameof(RoleSyncInfo.Write))]
    internal static class RoleAppearance
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);
            Label continueOriginal = generator.DefineLabel();
            int insertIndex = newInstructions.FindIndex(i =>
                i.Calls(Method(typeof(NetworkWriterExtensions), nameof(NetworkWriterExtensions.WriteUInt)))) + 1;

            // Вставляем перехват:
            // if (TryHandleAppearance(ref this, writer))
            //     return;
            newInstructions.InsertRange(insertIndex, new[]
            {
                // Загружаем ref this (для struct Ldarg_0 = указатель на структуру)
                new CodeInstruction(OpCodes.Ldarg_0),

                // Загружаем writer
                new CodeInstruction(OpCodes.Ldarg_1),

                // Вызываем наш обработчик
                new CodeInstruction(OpCodes.Call, Method(typeof(RoleAppearance), nameof(TryHandleAppearance))),

                // Если вернул false — переходим к оригинальному коду
                new CodeInstruction(OpCodes.Brfalse_S, continueOriginal),

                // Если вернул true — выходим из метода (всё уже записано)
                new CodeInstruction(OpCodes.Ret),
            });

            // Добавляем метку на следующую инструкцию после вставки
            newInstructions[insertIndex + 5].labels.Add(continueOriginal);

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }

        private static bool TryHandleAppearance(ref RoleSyncInfo info, NetworkWriter writer)
        {
            // Не перехватываем если игрок смотрит на себя, это вызовет десинк
            if (info._targetNetId == info._receiverNetId)
                return false;

            Player targetPlayer = Player.Get(info._targetNetId);
            if (targetPlayer?.Role is not { } role)
                return false;

            Player receiverPlayer = Player.Get(info._receiverNetId);
            if (receiverPlayer == null)
                return false;

            RoleTypeId appearance = role.GetAppearanceForPlayer(receiverPlayer);
            SendingRoleEventArgs ev = new(targetPlayer, receiverPlayer, appearance);
            Handlers.Player.OnSendingRole(ev);
            appearance = ev.RoleType;

            writer.WriteRoleType(appearance);
            WritePrependedData(ref info, writer);
            WritePublicSpawnData(info._role, writer, appearance);
            return true;
        }

        /// <summary>
        /// Записывает prepended spoofed data, если они присутствуют.
        /// </summary>
        /// <param name="info">Ссылка на структуру RoleSyncInfo.</param>
        /// <param name="writer">NetworkWriter для записи данных.</param>
        /// <remarks>
        /// Prepended data используется для особых случаев спуфинга на уровне игры.
        /// Копируем байты как есть, не интерпретируя их содержимое.
        /// </remarks>
        private static void WritePrependedData(ref RoleSyncInfo info, NetworkWriter writer)
        {
            if (info._prependedSpoofedData == null)
                return;

            foreach (byte b in info._prependedSpoofedData.ToArraySegment())
                writer.WriteByte(b);
        }

        private static void WritePublicSpawnData(PlayerRoleBase realRole, NetworkWriter writer, RoleTypeId appearance)
        {
            if (realRole == null)
                return;

            if (realRole.RoleTypeId == appearance || !appearance.TryGetRoleBase(out PlayerRoleBase fakeBase))
            {
                if (realRole is IPublicSpawnDataWriter spawnWriter)
                    spawnWriter.WritePublicSpawnData(writer);

                return;
            }

            switch (fakeBase)
            {
                case PlayerRoles.HumanRole role when role.UsesUnitNames:
                {
                    byte unitId = realRole is PlayerRoles.HumanRole { UsesUnitNames: true } humanReal ? humanReal.UnitNameId : (byte)0;
                    writer.WriteByte(unitId);

                    WriteFpcSpawnData(realRole, writer);
                    return;
                }

                case ZombieRole:
                {
                    if (realRole is ZombieRole zombieReal)
                    {
                        writer.WriteUShort(zombieReal._syncMaxHealth);
                        writer.WriteBool(zombieReal._showConfirmationBox);
                    }
                    else
                    {
                        writer.WriteUShort(400);
                        writer.WriteBool(false);
                    }

                    WriteFpcSpawnData(realRole, writer);
                    return;
                }

                case PlayerRoles.PlayableScps.Scp1507.Scp1507Role:
                {
                    byte reason = realRole is PlayerRoles.PlayableScps.Scp1507.Scp1507Role scp1507Real ? (byte)scp1507Real.ServerSpawnReason
                        : (byte)RoleChangeReason.ItemUsage;

                    writer.WriteByte(reason);
                    WriteFpcSpawnData(realRole, writer);
                    return;
                }
            }

            if (fakeBase is not FpcStandardRoleBase)
                return;

            WriteFpcSpawnData(realRole, writer);
        }

        private static void WriteFpcSpawnData(PlayerRoleBase realRole, NetworkWriter writer)
        {
            if (realRole is not FpcStandardRoleBase fpcReal)
                return;

            fpcReal.FpcModule.MouseLook.GetSyncValues(0, out ushort syncH, out _);
            writer.WriteRelativePosition(new(fpcReal._hubTransform.position));
            writer.WriteUShort(syncH);
        }
    }
}