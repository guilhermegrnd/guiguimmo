using System;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace Guiguimmo.Identity.Models;

[CollectionName("ApplicationRoles")]
public class ApplicationRole : MongoIdentityRole<Guid>
{
}
