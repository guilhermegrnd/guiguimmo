using System;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace Guiguimmo.Identity.Models;

[CollectionName("ApplicationUsers")]
public class ApplicationUser : MongoIdentityUser<Guid>
{
  public bool DarkMode { get; set; }
}