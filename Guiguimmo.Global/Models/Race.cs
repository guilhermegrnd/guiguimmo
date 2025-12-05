using System;
using Guiguimmo.Common.Interfaces;

namespace Guiguimmo.Global.Service.Models;

public class Race : IEntity
{
  public Guid Id { get; set; }
  public string Name { get; set; }
}