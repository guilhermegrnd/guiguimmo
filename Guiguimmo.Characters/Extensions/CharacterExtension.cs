using Guiguiflix.Characters.Dtos;
using Guiguimmo.Characters.Models;

namespace Guiguiflix.Characters.Extensions;

public static class CharacterExtension
{
  public static CharacterDto AsDto(this Character item)
  {
    return new CharacterDto(item.Id, item.Name, item.RaceId, item.ClassId, item.GenderId, item.Health, item.Level, item.Color, item.Bg, item.Position);
  }
}