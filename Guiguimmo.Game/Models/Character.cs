using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Guiguimmo.Game.Models;

public class Character
{
  [JsonPropertyName("id")]
  public Guid Id { get; set; }
  [JsonPropertyName("name")]
  public string Name { get; set; }
  [JsonPropertyName("raceId")]
  public Guid RaceId { get; set; }
  [JsonPropertyName("classId")]
  public Guid ClassId { get; set; }
  [JsonPropertyName("genderId")]
  public Guid GenderId { get; set; }
  [JsonPropertyName("level")]
  public int Level { get; set; }
  [JsonPropertyName("health")]
  public int Health { get; set; }
  [JsonPropertyName("color")]
  public string Color { get; set; }
  [JsonPropertyName("bg")]
  public string Bg { get; set; }
  [JsonPropertyName("position")]
  public Position Position { get; set; }
  public Queue<(int X, int Y)> CurrentPath { get; set; } = new Queue<(int X, int Y)>();
  public bool IsWalking { get; set; } = false;
}

public class Position
{
  [JsonPropertyName("x")]
  public int X { get; set; }
  [JsonPropertyName("y")]
  public int Y { get; set; }
  [JsonPropertyName("z")]
  public int Z { get; set; }
}