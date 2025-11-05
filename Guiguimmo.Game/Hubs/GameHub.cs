using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Guiguimmo.Models;
using Microsoft.AspNetCore.SignalR;

namespace Guiguimmo.Hubs;

public static class GameState
{
  // Thread-safe dictionary to store all active players
  public static readonly Dictionary<string, Player> Players = new Dictionary<string, Player>();
}

public class GameHub : Hub
{
  // Client sends an updated position
  public async Task SendPosition(float x, float y, float z)
  {
    var playerId = Context.ConnectionId;

    // 1. Authoritative Server Logic: Update the authoritative game state
    if (GameState.Players.ContainsKey(playerId))
    {
      GameState.Players[playerId].X = x;
      GameState.Players[playerId].Y = y;
      GameState.Players[playerId].Z = z;
    }

    // 2. Broadcast to others (excluding the sender)
    // 'Others' sends to all clients except the calling client. This saves bandwidth.
    await Clients.Others.SendAsync("ReceivePosition", playerId, x, y, z);
  }

  // When a player connects, add them to the game state
  public override Task OnConnectedAsync()
  {
    Console.WriteLine($"Player connected: {Context.ConnectionId}");
    var newPlayer = new Player { Id = Context.ConnectionId, X = 0, Y = 0, Z = 0 };
    GameState.Players.Add(newPlayer.Id, newPlayer);

    // Notify the new player of all existing players
    Clients.Caller.SendAsync("LoadExistingPlayers", GameState.Players.Values);

    // Notify all existing players of the new player
    Clients.Others.SendAsync("NewPlayerJoined", newPlayer.Id, newPlayer.X, newPlayer.Y, newPlayer.Z);

    return base.OnConnectedAsync();
  }

  // When a player disconnects, remove them from the game state
  public override Task OnDisconnectedAsync(Exception exception)
  {
    GameState.Players.Remove(Context.ConnectionId);

    // Notify all other players to remove the disconnected player
    Clients.Others.SendAsync("PlayerLeft", Context.ConnectionId);

    return base.OnDisconnectedAsync(exception);
  }
}