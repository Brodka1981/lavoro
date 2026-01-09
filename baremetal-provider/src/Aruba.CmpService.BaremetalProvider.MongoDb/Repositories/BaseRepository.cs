//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
//using MongoDB.Bson;
//using MongoDB.Driver;

//namespace Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;
//public abstract class BaseRepository<TModel, TEntity>
//    where TModel : class, IResourceBase
//    where TEntity : IEntity
//{
//    private readonly IMapper mapper;

//    protected BaseRepository(IMapper mapper)
//    {
//        this.mapper = mapper;
//    }

//    public abstract IMongoCollection<TEntity> MongoCollection { get; }

//    public async Task<TModel?> GetById(string? id, bool bypassDeletedStatus = false)
//    {
//        if (id is null)
//            return null;

//        if (!ObjectId.TryParse(id, out _))
//            return null;

//        var filter = Builders<TEntity>.Filter.Where(d => d.Id == id);

//        if (!bypassDeletedStatus)
//        {
//            filter = Builders<TEntity>.Filter.And(filter, Builders<TEntity>.Filter.Ne("status.state", StatusValues.Deleted));
//        }

//        var results = await MongoCollection.Find(filter).ToListAsync().ConfigureAwait(false);
//        var entity = results.FirstOrDefault();

//        if (entity is null)
//            return default;

//        return mapper.Map<TModel>(entity);
//    }

//    public async Task<TModel?> GetByIdAsync(string? id)
//    {
//        if (string.IsNullOrWhiteSpace(id))
//            return null;

//        if (!ObjectId.TryParse(id, out _))
//            return null;

//        var filter = Builders<TEntity>.Filter.Where(d => d.Id == id);
//        var results = await MongoCollection.Find(filter).ToListAsync().ConfigureAwait(false);
//        var entity = results.FirstOrDefault();

//        if (entity is null)
//            return default;

//        return mapper.Map<TModel>(entity);
//    }

//    public async Task<List<TModel>> GetByUserAsync(string? userId)
//    {
//        if (string.IsNullOrWhiteSpace(userId))
//            return new();

//        var filter = Builders<TEntity>.Filter.Where(d => d.CreatedBy == userId);
//        var results = await MongoCollection.Find(filter).ToListAsync().ConfigureAwait(false);
//        if (results is null)
//            return new();

//        return results.Select(r => mapper.Map<TModel>(r)).ToList();
//    }

//    public async Task Save(TModel model)
//    {
//        if (model is null)
//            return;

//        var entity = mapper.Map<TEntity>(model);

//        if (entity.Id is null)
//        {
//            entity.Id = ObjectId.GenerateNewId().ToString();
//            entity.CreationDate = DateTimeOffset.UtcNow;
//            await MongoCollection.InsertOneAsync(entity).ConfigureAwait(false);
//            model.Id = entity.Id;
//        }
//        else
//        {
//            entity.UpdateDate = DateTimeOffset.UtcNow;
//            var filter = Builders<TEntity>.Filter.Where(d => d.Id == entity.Id);
//            _ = await MongoCollection.ReplaceOneAsync(filter, entity).ConfigureAwait(false);
//        }
//    }

//    protected async Task<bool> UpdateField<TField>(TEntity entity, Expression<Func<TEntity, TField>> field, TField value)
//    {
//        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

//        var filter = Builders<TEntity>.Filter.Where(p => p.Id == entity.Id);

//        entity.UpdateDate = DateTimeOffset.UtcNow;

//        var update = Builders<TEntity>.Update.Set(field, value)
//            .Set(d => d.UpdateDate, entity.UpdateDate)
//            .Set(d => d.UpdatedBy, entity.UpdatedBy);

//        var result = await MongoCollection.UpdateOneAsync(filter,
//                                                                update,
//                                                                new UpdateOptions
//                                                                {
//                                                                    IsUpsert = false,
//                                                                    BypassDocumentValidation = false
//                                                                })
//                                                .ConfigureAwait(false);

//        return result.IsAcknowledged;
//    }
//}
