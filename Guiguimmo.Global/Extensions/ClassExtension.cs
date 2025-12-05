using Guiguiflix.Global.Service.Dtos;
using Guiguimmo.Global.Service.Models;

namespace Guiguiflix.Characters.Service.Extensions;

public static class ClassExtension
{
  public static ClassDto AsDto(this Class item)
  {
    return new ClassDto(item.Id, item.Name);
  }
}