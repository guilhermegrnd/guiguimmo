using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Guiguimmo.Common.Interfaces;
using MongoDB.Driver;

namespace Guiguimmo.Common.Database;

public class MongoRepository<T> : IRepository<T> where T : IEntity
{
  private readonly IMongoCollection<T> dbCollection;
  private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

  public MongoRepository(IMongoDatabase database, string collectionName)
  {
    dbCollection = database.GetCollection<T>(collectionName);
  }

  public async Task<IReadOnlyCollection<T>> GetAllAsync()
  {
    return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
  }

  public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
  {
    return await dbCollection.Find(filter).ToListAsync();
  }

  public async Task<T> GetAsync(Guid id)
  {
    var filter = filterBuilder.Eq(item => item.Id, id);
    return await dbCollection.Find(filter).FirstOrDefaultAsync();
  }

  public Task<T> GetAsync(Expression<Func<T, bool>> filter)
  {
    return dbCollection.Find(filter).FirstOrDefaultAsync();
  }

  public async Task CreateAsync(T item)
  {
    ArgumentNullException.ThrowIfNull(item);

    await dbCollection.InsertOneAsync(item);
  }

  public async Task UpdateAsync(T item)
  {
    ArgumentNullException.ThrowIfNull(item);

    var filter = filterBuilder.Eq(existingItem => existingItem.Id, item.Id);
    await dbCollection.ReplaceOneAsync(filter, item);
  }

  public async Task RemoveAsync(Guid id)
  {
    var filter = filterBuilder.Eq(item => item.Id, id);
    await dbCollection.DeleteOneAsync(filter);
  }
}