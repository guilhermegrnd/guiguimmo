using Guiguimmo.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

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
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/chathub");

app.MapHub<GameHub>("/gamehub");

await app.RunAsync();
