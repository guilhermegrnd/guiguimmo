using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Guiguimmo.Game.Dtos;
using Guiguimmo.Game.Models;
using Guiguimmo.Utils;
using Microsoft.Extensions.Hosting;
using IGameHubContext = Microsoft.AspNetCore.SignalR.IHubContext<Guiguimmo.Hubs.GameHub, Guiguimmo.Interfaces.IGameClient>;

namespace Guiguimmo.Game.Services;

public class GameEngine : BackgroundService
{
  private readonly IGameHubContext _hubContext;

  private readonly ConcurrentDictionary<string, Character> _characters = new();
  private readonly ConcurrentQueue<CharacterAction> _actionQueue = new();

  private const int TicksPerSecond = 2; //20 ticks per second
  private readonly TimeSpan _tickTime = TimeSpan.FromMilliseconds(1000 / TicksPerSecond);

  private readonly ProducerConfig config = new() { BootstrapServers = "localhost:9092" };

  public GameEngine(IGameHubContext hubContext)
  {
    _hubContext = hubContext;
  }

  public Task AddCharacter(string token, Guid characterId, string connectionId)
  {
    var character = CharactersService.GetCharacterById(token, characterId);

    var safePosition = FindClosestValidTile(character.Position.X, character.Position.Y);

    if (safePosition.HasValue)
    {
      character.Position.X = safePosition.Value.X;
      character.Position.Y = safePosition.Value.Y;
      _characters.TryAdd(connectionId, character);
      return _hubContext.Clients.Client(connectionId).ReceiveMessage(DateTime.UtcNow, "System", $"Welcome! Spawned at ({safePosition.Value.X}, {safePosition.Value.Y}).");
    }
    else
    {
      return _hubContext.Clients.Client(connectionId).ReceiveMessage(DateTime.UtcNow, "System", "Map is full. Try again later.");
    }
  }

  public void RemoveCharacter(string connectionId)
  {
    _characters.TryRemove(connectionId, out _);
  }

  public void QueueCharacterAction(CharacterAction action)
  {
    _actionQueue.Enqueue(action);
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      var startTime = DateTime.UtcNow;

      await ProcessCharacterActionsAsync();

      UpdateWorldState();

      await BroadcastGameState();

      var timeTaken = DateTime.UtcNow - startTime;
      var timeToWait = _tickTime - timeTaken;

      if (timeToWait > TimeSpan.Zero)
      {
        await Task.Delay(timeToWait, stoppingToken);
      }
      // Optional: Log a warning if timeTaken > _tickTime (lag/slow performance)
    }
  }

  private async Task ProcessCharacterActionsAsync()
  {
    while (_actionQueue.TryDequeue(out var action))
    {
      if (_characters.TryGetValue(action.ConnectionId, out var Character))
      {
        if (action.Type == "Move")
        {
          int newX = Character.Position.X;
          int newY = Character.Position.Y;

          string direction = action.Payload.ToLower();

          switch (direction)
          {
            case "up":
              newY--;
              break;
            case "down":
              newY++;
              break;
            case "left":
              newX--;
              break;
            case "right":
              newX++;
              break;
            default:
              continue; // Unknown direction, skip
          }

          int destinationTileId = GameMap.GetTile(newX, newY);

          if (!GameMap.IsTileSolid(destinationTileId))
          {
            Character.Position.X = newX;
            Character.Position.Y = newY;
            // Optional: You could add logic here for walking on special tiles (e.g., healing on a fire tile)

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
              var messageJson = JsonSerializer.Serialize(new
              {
                ActionType = "Move",
                CharacterId = Character.Id,
                ActionData = new
                {
                  X = newX,
                  Y = newY
                }
              });

              var result = await producer.ProduceAsync("character-actions", new Message<Null, string> { Value = messageJson });

              Console.WriteLine($"Delivered '{result.Value}' to {result.TopicPartitionOffset}");
            }
          }
          else
          {
            // The tile is solid (Wall, Water)
            // The action is rejected, and the player's position remains unchanged.
            // The client (React UI) will see no change in the next GameState broadcast.
          }
        }
        else if (action.Type == "Attack")
        {
          // Attack logic (not implemented)
        }
        else if (action.Type == "Message")
        {
          await _hubContext.Clients.All.ReceiveMessage(DateTime.UtcNow, Character.Name, action.Payload);
        }
        else if (action.Type == "UseItem")
        {
          // Item usage logic (not implemented)
        }
        else if (action.Type == "Pathfind")
        {
          var parts = action.Payload.Split(',');
          if (parts.Length == 2 && int.TryParse(parts[0], out int destX) && int.TryParse(parts[1], out int destY))
          {
            // Calculate the new path
            var newPathSteps = FindPath(Character.Position.X, Character.Position.Y, destX, destY);

            // Clear any existing path and set the new one
            Character.CurrentPath.Clear();
            foreach (var step in newPathSteps)
            {
              Character.CurrentPath.Enqueue(step);
            }
            Character.IsWalking = Character.CurrentPath.Count > 0;
          }
        }
      }
    }
    ExecuteActivePaths();
  }


  private void ExecuteActivePaths()
  {
    foreach (var character in _characters.Values)
    {
      if (character.IsWalking && character.CurrentPath.Count > 0)
      {
        // Get the next step from the queue
        var (X, Y) = character.CurrentPath.Peek(); // Use Peek to look, not Dequeue yet

        // Re-check for collision at the moment of movement (e.g., another character might have just moved there)
        if (IsTileValid(X, Y))
        {
          // Tile is valid, move the character
          character.CurrentPath.Dequeue(); // Now officially move to the next tile
          character.Position.X = X;
          character.Position.Y = Y;

          // Stop walking if the path is complete
          if (character.CurrentPath.Count == 0)
          {
            character.IsWalking = false;
          }
        }
        else
        {
          var dest = character.CurrentPath.Last(); // Get the original destination
          character.CurrentPath.Clear();
          character.IsWalking = false; // Stop movement until a new path is calculated

          // Re-queue the pathfinding action for the next tick
          var connectionId = _characters.FirstOrDefault(kv => kv.Value == character).Key;
          _actionQueue.Enqueue(new CharacterAction(connectionId, "Pathfind", $"{dest.X},{dest.Y}"));
        }
      }
    }
  }

  private void UpdateWorldState()
  {
    // Logic for monster AI, projectile movement, spell cooldowns, etc.
    // E.g.: Monsters move one tile towards the nearest Character
  }

  private async Task BroadcastGameState()
  {
    var gameStateDto = new GameStateDto
    {
      MapTiles = Helper.ConvertRectangularToJagged(GameMap.CurrentMap),
      Timestamp = DateTime.UtcNow,
      Characters = _characters.Values.Select(p => new CharacterDto(p.Id, p.Name, p.Position, p.Health)).ToList()
      // ... include other entities (monsters, items)
    };

    await _hubContext.Clients.All.ReceiveGameState(gameStateDto);
  }

  private bool IsTileValid(int x, int y)
  {
    int tileId = GameMap.GetTile(x, y);
    if (GameMap.IsTileSolid(tileId))
    {
      return false;
    }

    bool isOccupied = _characters.Values.Any(p => p.Position.X == x && p.Position.Y == y);

    return !isOccupied;
  }

  public (int X, int Y)? FindClosestValidTile(int startX, int startY, int maxRadius = 10)
  {
    if (IsTileValid(startX, startY))
    {
      return (startX, startY);
    }

    for (int r = 1; r <= maxRadius; r++)
    {
      int x = startX - r;
      int y = startY - r;

      for (int i = 0; i < 2 * r; i++)
      {
        x++;
        if (IsTileValid(x, y)) return (x, y);
      }

      for (int i = 0; i < 2 * r; i++)
      {
        y++;
        if (IsTileValid(x, y)) return (x, y);
      }

      for (int i = 0; i < 2 * r; i++)
      {
        x--;
        if (IsTileValid(x, y)) return (x, y);
      }

      for (int i = 0; i < 2 * r; i++)
      {
        y--;
        if (IsTileValid(x, y)) return (x, y);
      }
    }

    return null;
  }

  public List<(int X, int Y)> FindPath(int startX, int startY, int endX, int endY)
  {
    // Simplified A* implementation using lists (for game servers, specialized priority queues are faster)
    List<PathNode> openList = new List<PathNode>();
    HashSet<PathNode> closedSet = new HashSet<PathNode>();

    PathNode startNode = new PathNode(startX, startY);
    PathNode endNode = new PathNode(endX, endY);

    if (GameMap.IsTileSolid(GameMap.GetTile(endX, endY)))
    {
      return new List<(int X, int Y)>();
    }

    openList.Add(startNode);

    while (openList.Count > 0)
    {
      // Get the node with the lowest F-Cost
      PathNode currentNode = openList.OrderBy(n => n.FCost).First();

      openList.Remove(currentNode);
      closedSet.Add(currentNode);

      if (currentNode.Equals(endNode))
      {
        return ReconstructPath(currentNode); // Found the path!
      }

      // Check neighbors (up, down, left, right)
      foreach (var neighborPos in GetWalkableNeighbors(currentNode.X, currentNode.Y))
      {
        PathNode neighbor = new PathNode(neighborPos.X, neighborPos.Y);

        if (closedSet.Contains(neighbor) || GameMap.IsTileSolid(GameMap.GetTile(neighbor.X, neighbor.Y)))
        {
          continue;
        }

        int newGCost = currentNode.GCost + 1; // All steps cost 1

        if (newGCost < neighbor.GCost || !openList.Contains(neighbor))
        {
          neighbor.GCost = newGCost;
          // Manhattan distance as heuristic: |dx| + |dy|
          neighbor.HCost = Math.Abs(neighbor.X - endX) + Math.Abs(neighbor.Y - endY);
          neighbor.Parent = currentNode;

          if (!openList.Contains(neighbor))
          {
            openList.Add(neighbor);
          }
        }
      }
    }

    return new List<(int X, int Y)>(); // Path not found
  }

  private List<(int X, int Y)> ReconstructPath(PathNode endNode)
  {
    List<(int X, int Y)> path = new List<(int X, int Y)>();
    PathNode current = endNode;

    while (current != null)
    {
      path.Add((current.X, current.Y));
      current = current.Parent;
    }

    path.Reverse();
    // Exclude the starting tile from the path steps
    if (path.Count > 0) path.RemoveAt(0);
    return path;
  }

  private IEnumerable<(int X, int Y)> GetWalkableNeighbors(int x, int y)
  {
    yield return (x, y - 1); // Up
    yield return (x, y + 1); // Down
    yield return (x - 1, y); // Left
    yield return (x + 1, y); // Right
  }
}
