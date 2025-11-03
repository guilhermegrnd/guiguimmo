
using System;
using Guiguiflix.Identity.Services;
using Guiguiflix.Identity.Settings;
using Guiguimmo.Common.Settings;
using Guiguimmo.Identity.HostedServices;
using Guiguimmo.Identity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

builder.Services.Configure<IdentitySettings>(builder.Configuration.GetSection(nameof(IdentitySettings)))
    .AddDefaultIdentity<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
    (
        mongoDbSettings.ConnectionString,
        serviceSettings.ServiceName
    );

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseMongoDb()
               .UseDatabase(new MongoDB.Driver.MongoClient(mongoDbSettings.ConnectionString).GetDatabase(serviceSettings.ServiceName));
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("connect/authorize")
               .SetTokenEndpointUris("connect/token")
               .SetUserInfoEndpointUris("connect/userinfo")
               .SetEndSessionEndpointUris("connect/logout")
               .AllowAuthorizationCodeFlow()
               .AllowPasswordFlow()
               .AllowRefreshTokenFlow()
               .RequireProofKeyForCodeExchange()
               .AddEphemeralEncryptionKey()
               .AddEphemeralSigningKey()
               .UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableAuthorizationEndpointPassthrough()
               .EnableUserInfoEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough();

        options.RegisterScopes(
            "api_acesso",
            "roles",
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.OfflineAccess,
            OpenIddictConstants.Scopes.OpenId
        );
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddHostedService<IdentitySeedHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    const string allowedOriginsSetting = "AllowedOrigin";
    app.UseCors(policyBuilder =>
    {
        policyBuilder.WithOrigins(builder.Configuration[allowedOriginsSetting])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

await app.RunAsync();
