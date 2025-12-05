using System;
using Guiguimmo.Common.Interfaces;

namespace Guiguimmo.Global.Service.Models;

public class Class : IEntity
{
  public Guid Id { get; set; }
  public string Name { get; set; }
}