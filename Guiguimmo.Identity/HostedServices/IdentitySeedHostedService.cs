using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
  private readonly IdentitySettings _settings;

  public IdentitySeedHostedService(
      IServiceScopeFactory serviceScopeFactory,
      IOptions<IdentitySettings> identityOptions)
  {
    _serviceScopeFactory = serviceScopeFactory;
    _settings = identityOptions.Value;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    using var scope = _serviceScopeFactory.CreateScope();
    var serviceProvider = scope.ServiceProvider;

    var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    if (await manager.FindByClientIdAsync(_settings.Manager.ClientId, cancellationToken) == null)
    {
      var redirectUrisArray = _settings.Manager.RedirectUris.Split(',', StringSplitOptions.RemoveEmptyEntries);
      var postLogoutUrisArray = _settings.Manager.PostLogoutRedirectUris.Split(',', StringSplitOptions.RemoveEmptyEntries);
      var scopesArray = _settings.Manager.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries);
      var descriptor = new OpenIddictApplicationDescriptor
      {
        ClientId = _settings.Manager.ClientId,
        ClientSecret = _settings.Manager.ClientSecret,
        DisplayName = _settings.Manager.DisplayName,
        Permissions =
        {
          OpenIddictConstants.Permissions.Endpoints.Token,
          OpenIddictConstants.Permissions.Endpoints.Authorization,
          OpenIddictConstants.Permissions.Endpoints.EndSession,
          OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
          OpenIddictConstants.Permissions.GrantTypes.Password,
          OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
          OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
          OpenIddictConstants.Permissions.ResponseTypes.Code
        }
      };
      foreach (var uriString in redirectUrisArray)
      {
        if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
        {
          descriptor.RedirectUris.Add(uri);
        }
      }
      foreach (var scp in scopesArray)
      {
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scp);
      }
      foreach (var uriString in postLogoutUrisArray)
      {
        if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
        {
          descriptor.PostLogoutRedirectUris.Add(uri);
        }
      }

      await manager.CreateAsync(descriptor, cancellationToken);
    }

    var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = ["admin", "client", "member"];

    foreach (var roleName in roleNames)
    {
      if (!await roleManager.RoleExistsAsync(roleName))
      {
        await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
      }
    }

    var adminUser = await userManager.FindByNameAsync(_settings.Admin.Email);
    if (adminUser == null)
    {
      adminUser = new ApplicationUser
      {
        UserName = _settings.Admin.Email,
        Email = _settings.Admin.Email,
        EmailConfirmed = true
      };

      var result = await userManager.CreateAsync(adminUser, _settings.Admin.Password);
      if (result.Succeeded)
      {
        await userManager.AddToRoleAsync(adminUser, "admin");
        await userManager.AddToRoleAsync(adminUser, "member");
      }
    }
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}