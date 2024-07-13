using FirebaseAdmin.Messaging;
using MailKit.Search;
using Newtonsoft.Json;
using StackExchange.Redis;
using SWD.F_LocalBrand.Business.Attributes;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class NotificationService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IResponseCacheService _cache;

        public NotificationService(IConnectionMultiplexer redis, IResponseCacheService cache)
        {
            _redis = redis;
            _cache = cache;
        }
        public async Task<string> SendNotification(string token, string titile, string body)
        {
            var message = new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Title = titile,
                    Body = body
                },
                Data = new Dictionary<string, string>()
                {
                    { "key1", "value1" }
                }
            };
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return response;
        }

        //push notification to redis
        public async Task<bool> PushNotificationToRedis(int customerId, int orderId, string message, string status)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = $"customer:{customerId}:notifications";

                var notification = new NotificationModel
                {
                    CustomerId = customerId,
                    OrderId = orderId,
                    Message = message,
                    Status = status,
                    Timestamp = DateTime.UtcNow
                };
                var serializedNotification = JsonConvert.SerializeObject(notification);

                // Use a transaction to ensure atomicity
                var tran = db.CreateTransaction();
                _ = tran.ListRightPushAsync(key, serializedNotification);
                _ = tran.ListTrimAsync(key, -30, -1); // Keep only the last 30 items
                var committed = await tran.ExecuteAsync();

                return committed;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine(ex.Message);
                return false;
            }
        }


        //get notification from redis with notificationModel filter
        public async Task<List<NotificationModel>> GetNotificationFromRedis(int customerId, NotificationModel? notificationModelFilter)
        {
            //var db = _redis.GetDatabase();
            var key = $"customer:{customerId}:notifications";
            //var notifications = await db.ListRangeAsync(key);
            var result = new List<NotificationModel>();
            //foreach (var notification in notifications)
            //{
            //    while (notification.HasValue)
            //    {
            //        var notification = JsonConvert.DeserializeObject<string>(notification);
            //        result.Add(notificationModel);
            //    }
            //    var notificationModel = JsonConvert.DeserializeObject<NotificationModel>(notification);
            //    result.Add(notificationModel);
            //}
            var cachedProduct = await _cache.GetCachedResponseAsync(key);
            if (cachedProduct != null)
            {
                try
                {
                    // Check if the string needs multiple deserializations
                    while (cachedProduct.StartsWith("\"") && cachedProduct.EndsWith("\""))
                    {
                        cachedProduct = JsonConvert.DeserializeObject<string>(cachedProduct);
                    }

                    var deserializedProduct = JsonConvert.DeserializeObject<List<NotificationModel>>(cachedProduct);
                    result = deserializedProduct;
                }
                catch (JsonSerializationException ex)
                {
                    // Log or handle the exception
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
            if (notificationModelFilter.CustomerId != null)
            {
                result = result.Where(x => x.CustomerId == notificationModelFilter.CustomerId).ToList();
            }
            if (notificationModelFilter.OrderId != null)
            {
                result = result.Where(x => x.OrderId == notificationModelFilter.OrderId).ToList();
            }
            if (notificationModelFilter.Message != null)
            {
                result = result.Where(x => x.Message == notificationModelFilter.Message).ToList();
            }
            if (notificationModelFilter.Status != null)
            {
                result = result.Where(x => x.Status == notificationModelFilter.Status).ToList();
            }
            if (notificationModelFilter.Timestamp != null)
            {
                result = result.Where(x => x.Timestamp == notificationModelFilter.Timestamp).ToList();
            }
            return result;
        }
        
    }
}
