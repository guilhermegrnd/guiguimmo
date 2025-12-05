using Guiguiflix.Global.Service.Dtos;
using Guiguimmo.Global.Service.Models;

namespace Guiguiflix.Characters.Service.Extensions;

public static class RaceExtension
{
  public static RaceDto AsDto(this Race item)
  {
    return new RaceDto(item.Id, item.Name);
  }
}