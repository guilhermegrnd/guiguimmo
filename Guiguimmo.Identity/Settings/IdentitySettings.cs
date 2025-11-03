namespace Guiguiflix.Identity.Settings;

public class IdentitySettings
{
  public IdentityAdminSettings Admin { get; init; }
  public IdentityManagerSettings Manager { get; init; }
}

public class IdentityAdminSettings
{
  public string Email { get; init; }
  public string Password { get; init; }
}

public class IdentityManagerSettings
{
  public string ClientId { get; init; }
  public string ClientSecret { get; init; }
  public string DisplayName { get; init; }
  public string RedirectUris { get; init; }
  public string PostLogoutRedirectUris { get; init; }
  public string Scopes { get; init; }
}