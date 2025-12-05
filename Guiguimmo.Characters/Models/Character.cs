using System;
using Guiguimmo.Common.Interfaces;

namespace Guiguimmo.Characters.Models;

public class Character : IEntity
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public string Name { get; set; }
  public Guid RaceId { get; set; }
  public Guid ClassId { get; set; }
  public Guid GenderId { get; set; }
  public int Health { get; set; }
  public int Level { get; set; }
  public string Color { get; set; }
  public string Bg { get; set; }
  public Position Position { get; set; }
}

public class Position
{
  public int X { get; set; }
  public int Y { get; set; }
  public int Z { get; set; }
}