using Guiguimmo.Common.Database;
using Guiguimmo.Consumer.Models;
using Guiguimmo.Consumer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMongo()
                .AddMongoRepository<Character>(nameof(Character));

builder.Services.AddHostedService<CharacterActionConsumer>();

var host = builder.Build();

await host.RunAsync();
