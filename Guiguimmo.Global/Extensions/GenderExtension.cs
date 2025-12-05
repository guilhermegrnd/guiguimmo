using Guiguiflix.Global.Service.Dtos;
using Guiguimmo.Global.Service.Models;

namespace Guiguiflix.Characters.Service.Extensions;

public static class GenderExtension
{
  public static GenderDto AsDto(this Gender item)
  {
    return new GenderDto(item.Id, item.Name);
  }
}