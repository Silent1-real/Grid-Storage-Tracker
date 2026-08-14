using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.World;
using SpaceEngineers.Game.SessionComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Torch.Commands;
using Torch.Mod;
using Torch.Mod.Messages;

namespace GridStorageTracker
{
    [Category("gridstorage")]
    public class Commands : CommandModule
    {
        [Command("list", "Lists a player's stored grids")]
        public void List(string playerName)
        {
            long identityId = 0;
            // Try to parse the playerName as a Steam ID (ulong). If successful, get the corresponding identity ID.
            if (ulong.TryParse(playerName, out ulong steamId))
            {
                identityId = MySession.Static.Players.TryGetIdentityId(steamId);
            }
            else
            {
                // If playerName is not a Steam ID, search for the identity by display name (case-insensitive).
                foreach (MyIdentity identity in MySession.Static.Players.GetAllIdentities())
                {
                    if (identity.DisplayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {
                        identityId = identity.IdentityId;
                        break;
                    }
                }
            }

            if (identityId == 0)

            {
                Context.Respond($"Player '{playerName}' not found.");
                return;
            }
            var component = MySession.Static.GetComponent<MyGridsStorageSessionComponent>();

            List<Guid> matchingGrids = new List<Guid>();
            // Iterate through the stored grids and collect those that belong to the specified player (by identity ID).
            foreach (KeyValuePair<Guid, MyStoredGridData> pair in component.GetStoredGridsData())
            {
                if (pair.Value.OwnerId == identityId)
                {
                    matchingGrids.Add(pair.Key);
                }
            }
            if (matchingGrids.Count == 0)
            {
                Context.Respond($"No stored grids found for player '{playerName}'.");
                return;
            }

            // Sort the matching grids by their display names (case-insensitive) for better readability.
            matchingGrids.Sort((a, b) => string.Compare(
                component.GetStoredGridsData()[a].SavedGridDetails.DisplayName,
                component.GetStoredGridsData()[b].SavedGridDetails.DisplayName,
                StringComparison.OrdinalIgnoreCase));
            StringBuilder response = new StringBuilder();
            for (int i = 1; i <= matchingGrids.Count; i++)
            {

                // Get the grid ID and corresponding grid data for each matching grid.
                Guid gridId = matchingGrids[i - 1];
                var gridData = component.GetStoredGridsData()[gridId];
                response.AppendLine($"{i}. {gridData.SavedGridDetails.DisplayName}");
            }
            if (Context.Player != null)
            {
                // If the command is executed by a player, send the response as a dialog message to the player's Steam ID.
                var dialog = new DialogMessage("Grid Storage Tracker", $"Player: {playerName}", response.ToString());
                ModCommunication.SendMessageTo(dialog, Context.Player.SteamUserId);
            }
            else
            {
                Context.Respond(response.ToString());
            }
        }

        [Command("delete", "Deletes a stored grid by its index number")]
        public void Delete(string playerName, string indexStr)
        {
            if (!int.TryParse(indexStr, out int index) || index < 1)
            {
                Context.Respond("Invalid index. Please provide a valid positive integer.");
                return;
            }

            long identityId = 0;

            if (ulong.TryParse(playerName, out ulong steamId))
            {
                identityId = MySession.Static.Players.TryGetIdentityId(steamId);
            }
            else
            {
                
                foreach (MyIdentity identity in MySession.Static.Players.GetAllIdentities())
                {
                    if (identity.DisplayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {
                        identityId = identity.IdentityId;
                        break;
                    }
                }
            }

            if (identityId == 0)
            {
                Context.Respond($"Player '{playerName}' not found.");
                return;
            }
            // Get the MyGridsStorageSessionComponent to access stored grid data.
            var component = MySession.Static.GetComponent<MyGridsStorageSessionComponent>();

            List<Guid> matchingGrids = new List<Guid>();

            foreach (KeyValuePair<Guid, MyStoredGridData> pair in component.GetStoredGridsData())
            {
                if (pair.Value.OwnerId == identityId)
                {
                    matchingGrids.Add(pair.Key);
                }
            }

            if (matchingGrids.Count == 0)
            {
                Context.Respond($"No stored grids found for player '{playerName}'.");
                return;
            }

            matchingGrids.Sort((a, b) => string.Compare(
                component.GetStoredGridsData()[a].SavedGridDetails.DisplayName,
                component.GetStoredGridsData()[b].SavedGridDetails.DisplayName,
                StringComparison.OrdinalIgnoreCase));

            if (index > matchingGrids.Count)
            {
                Context.Respond($"Invalid index. Please provide a number between 1 and {matchingGrids.Count}.");
                return;
            }
            // Get the grid ID to delete based on the provided index (1-based).
            Guid gridToDelete = matchingGrids[index - 1];
            string gridName = component.GetStoredGridsData()[gridToDelete].SavedGridDetails.DisplayName;
            // Attempt to delete the grid and log the result.
            bool success = component.DeleteGrid(gridToDelete);
            string adminName = Context.Player != null ? Context.Player.DisplayName : "Console";

            if (success)
            {
                string targetPlayerInfo = EventPatches.GetPlayerName(identityId);
                Context.Respond($"Grid '{gridName}' has been deleted successfully from {playerName}'s grid storage.");
                LogWriter.LogRemoveEvent(adminName, gridName, targetPlayerInfo);
            }
            else
            {
                Context.Respond($"Failed to delete grid '{gridName}' from {playerName}'s grid storage.");
            }
        }
    }
}
