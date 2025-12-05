using System.Threading;
using System.Threading.Tasks;
using Guiguimmo.Common.Interfaces;
using Guiguimmo.Global.Service.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Guiguimmo.Global.HostedServices;

public class GlobalSeedHostedService : IHostedService
{
  private readonly ILogger<GlobalSeedHostedService> _logger;
  private readonly IRepository<Class> _classRepository;
  private readonly IRepository<Race> _raceRepository;
  private readonly IRepository<Gender> _genderRepository;

  public GlobalSeedHostedService(
    ILogger<GlobalSeedHostedService> logger,
    IRepository<Class> classRepository,
    IRepository<Race> raceRepository,
    IRepository<Gender> genderRepository)
  {
    _logger = logger;
    _classRepository = classRepository;
    _raceRepository = raceRepository;
    _genderRepository = genderRepository;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting Data Seeder...");

    var initialRaces = new string[] { "Human", "Elf", "Dwarf", "Orc" };
    if (await _raceRepository.GetAllAsync() is var dbraces && dbraces.Count == 0)
    {
      _logger.LogInformation("No races found. Seeding initial data.");
      foreach (var race in initialRaces)
      {
        await _raceRepository.CreateAsync(new Race { Name = race });
      }
      _logger.LogInformation($"Successfully seeded {initialRaces.Length} races.");
    }
    else
    {
      _logger.LogInformation("Races already exist. Skipping seeding for races.");
    }

    var initialClasses = new string[] { "Knight", "Archer", "Mage", "Druid" };
    if (await _classRepository.GetAllAsync() is var dbclasses && dbclasses.Count == 0)
    {
      _logger.LogInformation("No classes found. Seeding initial data.");
      foreach (var cls in initialClasses)
      {
        await _classRepository.CreateAsync(new Class { Name = cls });
      }
      _logger.LogInformation($"Successfully seeded {initialClasses.Length} classes.");
    }
    else
    {
      _logger.LogInformation("Classes already exist. Skipping seeding for classes.");
    }

    var initialGenders = new string[] { "Male", "Female" };
    if (await _genderRepository.GetAllAsync() is var dbgenders && dbgenders.Count == 0)
    {
      _logger.LogInformation("No genders found. Seeding initial data.");
      foreach (var gender in initialGenders)
      {
        await _genderRepository.CreateAsync(new Gender { Name = gender });
      }
      _logger.LogInformation($"Successfully seeded {initialGenders.Length} genders.");
    }
    else
    {
      _logger.LogInformation("Genders already exist. Skipping seeding for genders.");
    }

    _logger.LogInformation("Data Seeder finished.");
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("MongoDB Data Seeder is stopping.");
    return Task.CompletedTask;
  }
}