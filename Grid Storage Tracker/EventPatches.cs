using Sandbox.Game.EntityComponents.Interfaces;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SpaceEngineers.Game.SessionComponents;
using System;
using System.Reflection;
using Sandbox.Game.Entities;
using VRage.ModAPI;

namespace GridStorageTracker
{
    // This class contains method patches for grid storage events in Space Engineers.
    public static class EventPatches
    {
        public static readonly MethodInfo StoreGridMethod =
            typeof(MyGridsStorageSessionComponent).GetMethod("StoreGrid");

        public static readonly MethodInfo RetrieveGridMethod =
            typeof(MyGridsStorageSessionComponent).GetMethod("RetrieveGrid");

        public static readonly MethodInfo StoreGridPrefix =
            typeof(EventPatches).GetMethod(nameof(StoreGrid_Prefix), BindingFlags.Static | BindingFlags.NonPublic);

        public static readonly MethodInfo RetrieveGridPrefix =
            typeof(EventPatches).GetMethod(nameof(RetrieveGrid_Prefix), BindingFlags.Static | BindingFlags.NonPublic);

        // This method retrieves the player's display name and Steam ID based on their identity ID.
        internal static string GetPlayerName(long identityId)
        {
            var identity = MySession.Static.Players.TryGetIdentity(identityId);
            ulong steamId = MySession.Static.Players.TryGetSteamId(identityId);
            string displayName = identity?.DisplayName ?? identityId.ToString();
            return $"{displayName} ({steamId})";
        }
        // This method searches for a spawned grid entity by its name and owner ID, returning its entity ID if found.
        private static long FindSpawnedGridEntityId(string gridName, long ownerId)
        {
            foreach (IMyEntity entity in MyEntities.GetEntities())
            {
                //kunam pare shood
                if (entity is MyCubeGrid grid &&
                    grid.DisplayName == gridName &&
                    grid.BigOwners.Contains(ownerId))
                {
                    return grid.EntityId;
                }
            }
            return 0;
        }

        // Prefix method for the StoreGrid method. It wraps the original callback to log the store event when the grid is successfully stored.
        private static void StoreGrid_Prefix(VRage.Game.ModAPI.IMyCubeGrid  grid, IMyGridStorageProxy callerEntity, long ownerId, ulong? callerEndpointId, string name, ref Action<MyGridStorageRequestResult> callback)
        {
            var originalCallback = callback;
            string gridName = grid.CustomName;
            long entityId = grid.EntityId;
            string playerName = GetPlayerName(ownerId);
            // Wrap the original callback to log the store event when the grid is successfully stored.
            callback = (result) =>
            {
                if (result == MyGridStorageRequestResult.Success)
                {
                    LogWriter.LogStoreEvent(playerName, gridName, entityId);
                }
                originalCallback?.Invoke(result);
            };
        }
        // Prefix method for the RetrieveGrid method. It wraps the original callback to log the retrieve event when the grid is successfully retrieved.
        private static void RetrieveGrid_Prefix(Guid id, IMyGridStorageProxy callerEntity, long owner, VRageMath.Vector3D? suggestedWorldPosition, ulong? callerEndpointId, ref Action<MyGridStorageRequestResult> callback)
        {
            var originalCallback = callback;
            string playerName = GetPlayerName(owner);

            string gridName = "(unknown)";
            var component = MySession.Static.GetComponent<MyGridsStorageSessionComponent>();
            if (component != null && component.TryGetStoredGridData(id, out var data))
            {
                gridName = data.SavedGridDetails?.DisplayName ?? "(unknown)";
            }
            // Wrap the original callback to log the retrieve event when the grid is successfully retrieved.
            callback = (result) =>
            {
                if (result == MyGridStorageRequestResult.Success)
                {
                    long entityId = FindSpawnedGridEntityId(gridName, owner);
                    LogWriter.LogRetrieveEvent(playerName, gridName, entityId);
                }
                originalCallback?.Invoke(result);
            };
        }
    }
}