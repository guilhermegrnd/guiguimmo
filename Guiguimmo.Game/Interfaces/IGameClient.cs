using System;
using System.Threading.Tasks;
using Guiguimmo.Game.Dtos;

namespace Guiguimmo.Interfaces;

public interface IGameClient
{
  Task JoinCharacter(Guid characterId);
  Task MoveCharacter(string direction);
  Task ReceiveMessage(DateTime dateTime, string name, string message);
  Task ReceiveGameState(GameStateDto gameState);
  Task SendMessage(string message);
  Task StartAutoWalk(int destX, int destY);
}