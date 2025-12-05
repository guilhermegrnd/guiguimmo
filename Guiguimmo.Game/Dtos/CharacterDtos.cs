using System;
using Guiguimmo.Game.Models;

namespace Guiguimmo.Game.Dtos;

public record CharacterAction(string ConnectionId, string Type, string Payload);
public record CharacterDto(Guid Id, string Name, Position Position, int Health);