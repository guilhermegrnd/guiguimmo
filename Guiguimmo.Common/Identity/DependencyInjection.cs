using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace Guiguimmo.Common.Identity;

public static class DependencyInjection
{
  public static AuthenticationBuilder AddJwtBearerAuthentication(this IServiceCollection services)
  {
    return services.ConfigureOptions<ConfigureJwtBearerOptions>()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
  }
}