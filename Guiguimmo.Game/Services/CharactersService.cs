using System;
using System.Net.Http;
using System.Text.Json;
using Guiguimmo.Game.Models;

namespace Guiguimmo.Game.Services;

public static class CharactersService
{
  public static Character GetCharacterById(string token, Guid characterId)
  {
    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    var response = client.GetAsync($"https://localhost:5005/v1/characters/{characterId}").Result;
    var json = response.Content.ReadAsStringAsync().Result;
    var character = JsonSerializer.Deserialize<Character>(json);
    return character;
  }
}