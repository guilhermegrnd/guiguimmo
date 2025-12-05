namespace Guiguimmo.Common.Settings;

public class ServiceSettings
{
  public string ServiceName { get; init; }
  public string Authority { get; init; }
  public string Audiencies { get; init; }
  public string JWTSecretKey { get; init; }
}