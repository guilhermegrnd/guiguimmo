using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Guiguimmo.Common.Interfaces;
using Guiguimmo.Consumer.Dtos;
using Guiguimmo.Consumer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Guiguimmo.Consumer.Services;

public class CharacterActionConsumer : BackgroundService
{
  private readonly IConsumer<Ignore, string> _consumer;
  private readonly IRepository<Character> _characterRepository;
  private const string TopicName = "character-actions";

  public CharacterActionConsumer(IConfiguration config, IRepository<Character> characterRepository)
  {
    var consumerConfig = new ConsumerConfig
    {
      GroupId = "game-persistence-test-new-group-46",
      BootstrapServers = config["Kafka:BootstrapServers"],
      AutoOffsetReset = AutoOffsetReset.Earliest,
      // EnableAutoCommit = true // Auto-commit offsets periodically
    };

    _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

    _characterRepository = characterRepository;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    Console.WriteLine($"Consumer subscribing to {TopicName}...");
    _consumer.Subscribe(TopicName);
    Console.WriteLine($"Consumer successfully subscribed. Entering consume loop...");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(5));

        if (consumeResult == null)
        {
          // Log a status check if nothing was consumed within the timeout
          Console.WriteLine($"INFO: Timeout reached. Waiting for messages or group to stabilize...");
          continue; // Go back to the top of the while loop
        }
        // --- Your Game State Persistence Logic ---
        Console.WriteLine($"Consumed action from: {consumeResult.TopicPartitionOffset}. Value: {consumeResult.Message.Value}");

        var parsedAction = System.Text.Json.JsonSerializer.Deserialize<CharacterAction<MoveActionData>>(consumeResult.Message.Value);
        if (parsedAction != null)
        {
          if (parsedAction.ActionType == "Move")
          {
            Console.WriteLine($"Processing Move Action for CharacterId: {parsedAction.CharacterId} to Position X:{parsedAction.ActionData.X}, Y:{parsedAction.ActionData.Y}");

            var character = await _characterRepository.GetAsync(parsedAction.CharacterId);
            if (character == null)
            {
              Console.WriteLine($"Character with ID {parsedAction.CharacterId} not found.");
              continue;
            }
            character.Position.X = parsedAction.ActionData.X;
            character.Position.Y = parsedAction.ActionData.Y;
            await _characterRepository.UpdateAsync(character);
          }
          else
          {
            Console.WriteLine($"Unknown ActionType: {parsedAction.ActionType}");
          }
        }
      }
      catch (OperationCanceledException)
      {
        // Expected during graceful shutdown
        break;
      }
      catch (ConsumeException ex)
      {
        Console.WriteLine($"Consume error: {ex.Error.Reason}");
        // Implement retry logic or dead-letter queue here
      }
    }

    _consumer.Close();
  }

  public override void Dispose()
  {
    _consumer.Dispose();
    base.Dispose();
  }
}