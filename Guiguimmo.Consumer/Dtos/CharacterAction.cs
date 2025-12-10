using System;

namespace Guiguimmo.Consumer.Dtos;

public class CharacterAction<T>
{
  public string ActionType { get; set; }
  public Guid CharacterId { get; set; }
  public T ActionData { get; set; }
}

public class MoveActionData
{
  public int X { get; set; }
  public int Y { get; set; }
}