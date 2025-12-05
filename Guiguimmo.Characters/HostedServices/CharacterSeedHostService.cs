using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guiguimmo.Characters.Models;
using Guiguimmo.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Guiguimmo.Characters.HostedServices;

public class CharacterSeedHostedService : IHostedService
{
  private readonly ILogger<CharacterSeedHostedService> _logger;
  private readonly IRepository<Character> _charactersRepository;

  public CharacterSeedHostedService(
    ILogger<CharacterSeedHostedService> logger,
    IRepository<Character> charactersRepository)
  {
    _logger = logger;
    _charactersRepository = charactersRepository;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting Data Seeder...");
    _logger.LogInformation("Data Seeder finished.");
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("MongoDB Data Seeder is stopping.");
    return Task.CompletedTask;
  }
}