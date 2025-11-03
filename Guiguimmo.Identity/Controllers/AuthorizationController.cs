using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Guiguimmo.Identity.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Guiguimmo.Identity.Controllers;

[ApiController]
[Route("connect")]
public class AuthorizationController : ControllerBase
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;

  public AuthorizationController(
      UserManager<ApplicationUser> userManager,
      SignInManager<ApplicationUser> signInManager)
  {
    _userManager = userManager;
    _signInManager = signInManager;
  }

  [HttpPost("token"), IgnoreAntiforgeryToken, Produces("application/json")]
  public async Task<IActionResult> Exchange()
  {
    var request = HttpContext.GetOpenIddictServerRequest()
                  ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

    if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
    {
      var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

      return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    if (request.IsPasswordGrantType())
    {
      var user = await _userManager.FindByNameAsync(request.Username);
      if (user == null)
      {
        var properties = new AuthenticationProperties(new Dictionary<string, string?>
        {
          [OpenIddictConstants.Parameters.Error] = OpenIddictConstants.Errors.InvalidGrant,
          [OpenIddictConstants.Parameters.ErrorDescription] = "The username/password is invalid."
        });

        return Forbid(properties, "OpenIddict.Server.AspNetCore");
      }

      var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
      if (!result.Succeeded)
      {
        var properties = new AuthenticationProperties(new Dictionary<string, string?>
        {
          [OpenIddictConstants.Parameters.Error] = OpenIddictConstants.Errors.InvalidGrant,
          [OpenIddictConstants.Parameters.ErrorDescription] = "The username/password is invalid."
        });

        return Forbid(properties, "OpenIddict.Server.AspNetCore");
      }

      var principal = await _signInManager.CreateUserPrincipalAsync(user);

      var roles = await _userManager.GetRolesAsync(user);

      var identity = principal.Identities.FirstOrDefault();
      if (identity == null) throw new InvalidOperationException("Identity não encontrada.");

      foreach (var role in roles)
      {
        identity.AddClaim(OpenIddictConstants.Claims.Role, role, OpenIddictConstants.Destinations.AccessToken);
      }

      identity.AddClaim(OpenIddictConstants.Claims.Name, user.UserName!, OpenIddictConstants.Destinations.AccessToken);

      return SignIn(principal, "OpenIddict.Server.AspNetCore");
    }

    throw new InvalidOperationException("O fluxo de concessão não é suportado.");
  }

  [HttpGet("authorize")]
  [HttpPost("authorize")]
  [IgnoreAntiforgeryToken]
  public async Task<IActionResult> Authorize()
  {
    var request = HttpContext.GetOpenIddictServerRequest()
                  ?? throw new InvalidOperationException("A requisição OpenID Connect não pode ser recuperada.");

    var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
    if (result is null || !result.Succeeded || request.HasPromptValue("login"))
    {
      var properties = new AuthenticationProperties
      {
        RedirectUri = Url.Action(nameof(Authorize), "Authorization") + Request.QueryString
      };

      return Challenge(properties, IdentityConstants.ApplicationScheme);
    }

    var principal = result.Principal;

    var identity = principal.Identities.FirstOrDefault();
    if (identity == null)
    {
      return Forbid(new AuthenticationProperties(), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    identity.SetScopes(request.GetScopes());
    identity.SetResources(request.GetResources());

    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!string.IsNullOrEmpty(userId) && !identity.HasClaim(c => c.Type == OpenIddictConstants.Claims.Subject))
    {
      identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, userId));
    }

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
      return Forbid(new AuthenticationProperties(), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    var roles = await _userManager.GetRolesAsync(user);
    foreach (var role in roles)
    {
      identity.AddClaim(OpenIddictConstants.Claims.Role, role);
    }

    identity.SetDestinations(claim =>
    {
      if (claim.Type == OpenIddictConstants.Claims.Role &&
          principal.HasScope("roles"))
      {
        return [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken];
      }

      return [OpenIddictConstants.Destinations.AccessToken];
    });

    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
  }

  [HttpGet("logout")]
  [HttpPost("logout")]
  public async Task<IActionResult> LogoutPost()
  {
    await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

    return SignOut(
        properties: new AuthenticationProperties { RedirectUri = "/" },
        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
  }

  [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
  [HttpGet("userinfo")]
  [Produces("application/json")]
  public async Task<IActionResult> Userinfo()
  {
    var claims = new Dictionary<string, object>(StringComparer.Ordinal)
    {
      [OpenIddictConstants.Claims.Subject] = User.FindFirstValue(OpenIddictConstants.Claims.Subject)!,
      ["local_ip"] = "192.168.1.1"
    };

    if (User.HasScope(OpenIddictConstants.Scopes.Email))
    {
      claims[OpenIddictConstants.Claims.Email] = User.FindFirstValue(OpenIddictConstants.Claims.Email)!;
    }

    return Ok(claims);
  }
}