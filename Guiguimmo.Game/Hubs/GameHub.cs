using System;
using System.Threading.Tasks;
using Guiguimmo.Game.Dtos;
using Guiguimmo.Game.Services;
using Guiguimmo.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Guiguimmo.Hubs;

[Authorize]
public class GameHub : Hub<IGameClient>
{
  private readonly GameEngine _engine;

  public GameHub(GameEngine engine)
  {
    _engine = engine;
  }

  public override async Task OnConnectedAsync()
  {
    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    _engine.RemoveCharacter(Context.ConnectionId);
    await base.OnDisconnectedAsync(exception);
  }

  public async Task JoinCharacter(Guid characterId)
  {
    var httpContext = Context.GetHttpContext();
    var token = httpContext.Request.Query["access_token"].ToString();

    await _engine.AddCharacter(token, characterId, Context.ConnectionId);
  }

  public async Task<Task> MoveCharacter(string direction)
  {
    _engine.QueueCharacterAction(new CharacterAction(Context.ConnectionId, "Move", direction));

    return Task.CompletedTask;
  }

  public async Task<Task> SendMessage(string message)
  {
    _engine.QueueCharacterAction(new CharacterAction(Context.ConnectionId, "Message", message));

    return Task.CompletedTask;
  }

  public Task StartAutoWalk(int destX, int destY)
  {
    _engine.QueueCharacterAction(new CharacterAction(Context.ConnectionId, "Pathfind", $"{destX},{destY}"));

    return Task.CompletedTask;
  }
}