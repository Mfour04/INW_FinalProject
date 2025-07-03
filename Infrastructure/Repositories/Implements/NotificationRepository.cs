using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implements
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<NotificationEntity> _collection;
        public NotificationRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("notification").Wait();
            _collection = mongoDBHelper.GetCollection<NotificationEntity>("notification");
        }
        public async Task<NotificationEntity> CreateAsync(NotificationEntity notification)
        {
            try
            {
                await _collection.InsertOneAsync(notification);
                return notification;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task DeleteAllAsync(string userId)
        {
            try
            {
                await _collection.DeleteManyAsync(x => x.user_id == userId);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task DeleteAsync(string notificationId)
        {
            try
            {
                await _collection.DeleteOneAsync(x => x.id == notificationId);

            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task DeleteOldReadNotificationsAsync()
        {
            var cutoff = DateTime.UtcNow.AddDays(-30).Ticks;
            var filter = Builders<NotificationEntity>.Filter.And(
                Builders<NotificationEntity>.Filter.Eq(x => x.is_read, true),
                Builders<NotificationEntity>.Filter.Lt(x => x.created_at, cutoff)
            );

            await _collection.DeleteManyAsync(filter);
        }

        public async Task<List<NotificationEntity>> GetUserNotificationsAsync(string userId)
        {
            try
            {
                return await _collection.Find(x => x.user_id == userId).SortByDescending(x => x.created_at).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task MarkAsReadAsync(string notificationId)
        {
            try
            {
                var update = Builders<NotificationEntity>.Update.Set(x => x.is_read, true);
                await _collection.UpdateOneAsync(x => x.id == notificationId, update);
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}
