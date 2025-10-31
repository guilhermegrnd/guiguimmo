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


      // Cria um novo principal (identidade) com os claims do usuário
      var principal = await _signInManager.CreateUserPrincipalAsync(user);

      var roles = await _userManager.GetRolesAsync(user);

      // 2. Adiciona as claims de role ao Principal
      var identity = principal.Identities.FirstOrDefault();
      if (identity == null) throw new InvalidOperationException("Identity não encontrada.");

      foreach (var role in roles)
      {
        // Adiciona a claim 'role' (necessária para autorização)
        identity.AddClaim(OpenIddictConstants.Claims.Role, role, OpenIddictConstants.Destinations.AccessToken);
      }

      // Opcional: Adicionar a claim 'name' para fácil identificação
      identity.AddClaim(OpenIddictConstants.Claims.Name, user.UserName!, OpenIddictConstants.Destinations.AccessToken);
      // Retorna a resposta do token para o cliente
      return SignIn(principal, "OpenIddict.Server.AspNetCore");
    }

    // ... (Adicione a lógica para outros fluxos como Client Credentials ou Refresh Token)

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

    // Se o usuário estiver logado e a aplicação cliente for autorizada:
    var principal = result.Principal;

    // Se for uma requisição de consentimento (o cliente não tem acesso aos escopos):
    // TODO: Implementar a lógica de consentimento aqui. Por enquanto, concedemos automaticamente.

    // Cria o principal que será usado para emitir o código de autorização
    var identity = principal.Identities.FirstOrDefault();
    if (identity == null)
    {
      return Forbid(new AuthenticationProperties(), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // Adiciona claims de escopos e outros claims necessários
    identity.SetScopes(request.GetScopes());
    identity.SetResources(request.GetResources());

    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!string.IsNullOrEmpty(userId) && !identity.HasClaim(c => c.Type == OpenIddictConstants.Claims.Subject))
    {
      identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, userId));
    }

    // Finaliza a requisição de autorização, emitindo o código de autorização
    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
  }

  // [HttpGet("logout")]
  // public IActionResult Logout() => View(); // Exibe uma página de confirmação de logout

  [HttpPost("logout")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> LogoutPost()
  {
    // Encerra a sessão de ASP.NET Identity
    await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

    // Retorna a resposta de logout para o OpenIddict
    return SignOut(
        properties: new AuthenticationProperties { RedirectUri = "/" }, // Redireciona para a página inicial
        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
  }

  [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
  [HttpGet("userinfo")]
  [Produces("application/json")]
  public async Task<IActionResult> Userinfo()
  {
    var claims = new Dictionary<string, object>(StringComparer.Ordinal)
    {
      // O "sub" (subject) é obrigatório
      [OpenIddictConstants.Claims.Subject] = User.FindFirstValue(OpenIddictConstants.Claims.Subject)!,

      // Adicione outros claims que foram solicitados pelo cliente (escopos)
      ["local_ip"] = "192.168.1.1" // Exemplo de claim customizado
    };

    if (User.HasScope(OpenIddictConstants.Scopes.Email))
    {
      claims[OpenIddictConstants.Claims.Email] = User.FindFirstValue(OpenIddictConstants.Claims.Email)!;
    }

    // Você pode injetar o UserManager para buscar claims adicionais
    // var user = await _userManager.GetUserAsync(User);
    // claims["nome_completo"] = user.NomeCompleto;

    return Ok(claims);
  }
}