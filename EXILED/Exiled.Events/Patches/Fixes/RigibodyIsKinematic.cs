namespace Exiled.Events.Patches.Fixes
{
    using HarmonyLib;
    using InventorySystem.Items.Pickups;

    /// <summary>
    /// Fixing RigidBody.isKinematic.
    /// </summary>
    [HarmonyPatch(typeof(PickupStandardPhysics), nameof(PickupStandardPhysics.ServerSendFreeze), MethodType.Getter)]
    internal static class RigidBodyIsKinematicFix
    {
        static void Postfix(PickupStandardPhysics __instance, ref bool __result) => __result = __result || __instance.Rb.isKinematic;
    }
}