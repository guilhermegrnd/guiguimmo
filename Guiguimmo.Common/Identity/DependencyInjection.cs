using System;
using System.Text;
using Guiguimmo.Common.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;

namespace Guiguimmo.Common.Identity;

public static class DependencyInjection
{
  public static IServiceCollection AddOpenIddictAuthentication(this IServiceCollection services, IConfiguration configuration)
  {
    var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>()
      ?? throw new InvalidOperationException("Missing ServiceSettings section in configuration.");

    services.AddOpenIddict()
      .AddValidation(options =>
      {
        options.SetIssuer(serviceSettings.Authority);
        options.AddAudiences(serviceSettings.Audiencies.Split(','));
        options.AddEncryptionKey(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(serviceSettings.JWTSecretKey)));
        options.UseSystemNetHttp();
        options.UseAspNetCore();
      });

    services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

    return services;
  }
}