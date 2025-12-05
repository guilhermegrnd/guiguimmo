using System;
using Guiguimmo.Common.Identity;
using Guiguimmo.Game.Services;
using Guiguimmo.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenIddictAuthentication(builder.Configuration);

builder.Services.AddControllers(options => options.SuppressAsyncSuffixInActionNames = false);

builder.Services.AddSignalR();

builder.Services.AddSingleton<GameEngine>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<GameEngine>());

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    const string allowedOriginsSetting = "AllowedOrigin";
    var origins = builder.Configuration[allowedOriginsSetting].Split(';', StringSplitOptions.RemoveEmptyEntries);
    app.UseCors(policyBuilder =>
    {
        policyBuilder.WithOrigins(origins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<GameHub>("/gamehub").RequireAuthorization();

await app.RunAsync();
