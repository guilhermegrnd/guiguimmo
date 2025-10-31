using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Models;
using Guiguiflix.Identity.Settings;
using Guiguimmo.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace Guiguimmo.Identity.HostedServices;

public class IdentitySeedHostedService : IHostedService
{
  private readonly IServiceScopeFactory _serviceScopeFactory;
  private readonly ILogger<IdentitySeedHostedService> _logger;
  private readonly IdentitySettings _settings;

  public IdentitySeedHostedService(
      IServiceScopeFactory serviceScopeFactory,
      ILogger<IdentitySeedHostedService> logger,
      IOptions<IdentitySettings> identityOptions)
  {
    _serviceScopeFactory = serviceScopeFactory;
    _logger = logger;
    _settings = identityOptions.Value;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    using var scope = _serviceScopeFactory.CreateScope();
    var serviceProvider = scope.ServiceProvider;

    var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    if (await manager.FindByClientIdAsync("my-client-app", cancellationToken) == null)
    {
      await manager.CreateAsync(new OpenIddictApplicationDescriptor
      {
        ClientId = "my-client-app",
        ClientSecret = "uma-chave-secreta-forte",
        DisplayName = "Minha Aplicação Web/API",
        RedirectUris =
          {
            new Uri("http://localhost:3000/callback"),
            new Uri("http://localhost:3000/silent-renew.html")
          },
        Permissions =
          {
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.Endpoints.Authorization,
            OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
            OpenIddictConstants.Permissions.GrantTypes.Password,
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
            OpenIddictConstants.Permissions.ResponseTypes.Code,
            OpenIddictConstants.Permissions.Prefixes.Scope + "api_acesso",
            OpenIddictConstants.Permissions.Prefixes.Scope + "offline_access",
            OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
            OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
          }
      }, cancellationToken);
    }

    var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "Admin", "Client", "Member" };

    foreach (var roleName in roleNames)
    {
      if (!await roleManager.RoleExistsAsync(roleName))
      {
        await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
      }
    }

    var adminUser = await userManager.FindByNameAsync(_settings.AdminUserEmail);
    if (adminUser == null)
    {
      adminUser = new ApplicationUser
      {
        UserName = _settings.AdminUserEmail,
        Email = _settings.AdminUserEmail,
        EmailConfirmed = true
      };

      var result = await userManager.CreateAsync(adminUser, _settings.AdminUserPassword);
      if (result.Succeeded)
      {
        await userManager.AddToRoleAsync(adminUser, "Admin");
        await userManager.AddToRoleAsync(adminUser, "Member");
      }
    }
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}