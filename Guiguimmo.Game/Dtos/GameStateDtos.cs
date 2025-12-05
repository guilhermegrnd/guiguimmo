using System;
using System.Collections.Generic;
using Guiguimmo.Game.Models;
using Guiguimmo.Utils;

namespace Guiguimmo.Game.Dtos;

public class GameStateDto
{
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
  public int[][] MapTiles { get; set; } = Helper.ConvertRectangularToJagged(GameMap.CurrentMap);
  public List<CharacterDto> Characters { get; set; } = [];
}