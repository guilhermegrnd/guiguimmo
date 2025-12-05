using Guiguimmo.Characters.HostedServices;
using Guiguimmo.Characters.Models;
using Guiguimmo.Common.Database;
using Guiguimmo.Common.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenIddictAuthentication(builder.Configuration);

builder.Services.AddMongo()
                .AddMongoRepository<Character>(nameof(Character));

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddHostedService<CharacterSeedHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    const string allowedOriginsSetting = "AllowedOrigin";
    var origins = builder.Configuration[allowedOriginsSetting].Split(';', System.StringSplitOptions.RemoveEmptyEntries);
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

await app.RunAsync();
