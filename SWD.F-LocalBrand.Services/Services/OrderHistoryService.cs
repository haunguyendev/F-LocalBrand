using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SWD.F_LocalBrand.Business.Common.Shared;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class OrderHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly NotificationService _notificationService;
        private readonly FirebaseService _firebaseService;
        public OrderHistoryService(IUnitOfWork unitOfWork, NotificationService notificationService, FirebaseService firebaseService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _firebaseService = firebaseService;
        }
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus, string userRole)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return false;
            }

            var currentStatus = order.OrderHistories.FirstOrDefault(oh => oh.IsCurrent);

            if (currentStatus == null)
            {
                return false;
            }

            if (!IsValidStatusTransition(currentStatus.Status, newStatus, userRole))
            {
                return false;
            }

            currentStatus.IsCurrent = false;

            var newOrderHistory = new OrderHistory
            {
                OrderId = orderId,
                Status = newStatus,
                ChangeTime = DateTime.UtcNow,
                IsCurrent = true
            };
            if(newStatus == "Prepared")
            {
                // Send notification to Admin
                var roles = await _unitOfWork.Roles.FindByCondition(r => r.RoleName == "Shipper").ToListAsync();
                foreach (var role in roles)
                {
                    var userList = await _unitOfWork.Users.FindByCondition(u => u.RoleId == role.Id).ToListAsync();
                    var customer = await _unitOfWork.Customers.FindByCondition(c => c.Id == order.CustomerId.GetValueOrDefault()).FirstOrDefaultAsync();
                    foreach (var user in userList)
                    {
                        if (user.DeviceId != null)
                            await _notificationService.SendNotification(user.DeviceId, "F-LocalBrand", $"Have order by {order.CustomerId}, please check and ship!");
                    }
                        await _notificationService.PushNotificationToRedis(order.CustomerId.GetValueOrDefault(), order.Id, $"Order {order.Id} is prepared", "Prepared", customer.FullName, customer.Image);

                }


            }
            if(newStatus == "Delivered")
            {
                // Send notification to Customer
                var customer = await _unitOfWork.Customers.FindByCondition(u => u.Id == order.CustomerId).FirstOrDefaultAsync();
                await _notificationService.SendNotification(customer.DeviceId, "F-LocalBrand", $"Order {order.Id} is delivered!");
                await _notificationService.PushNotificationToRedis(customer.Id, order.Id, $"Order {order.Id} is delivered", "Delivered", customer.FullName ,customer.Image);
            }
            if(newStatus == "ShipperReceived")
            {
                var customer = await _unitOfWork.Customers.FindByCondition(u => u.Id == order.CustomerId).FirstOrDefaultAsync();
                await _notificationService.SendNotification(customer.DeviceId, "F-LocalBrand", $"Shipper received the order {order.Id}");
                await _notificationService.PushNotificationToRedis(customer.Id, order.Id, $"Shipper received the order {order.Id}", "ShipperReceived", customer.FullName, customer.Image);
            }
            if (newStatus == "InTransit")
            {
                var customer = await _unitOfWork.Customers.FindByCondition(u => u.Id == order.CustomerId).FirstOrDefaultAsync();
                await _notificationService.SendNotification(customer.DeviceId, "F-LocalBrand", $"Shipper transits the order {order.Id}");
                await _notificationService.PushNotificationToRedis(customer.Id, order.Id, $"Shipper transits the order {order.Id}", "InTransit", customer.FullName, customer.Image);
            }
            await _unitOfWork.OrderHistories.UpdateAsync(currentStatus);
            await _unitOfWork.OrderHistories.CreateAsync(newOrderHistory);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private bool IsValidStatusTransition(string currentStatus, string newStatus, string userRole)
        {
            if (userRole == "Admin" && newStatus == OrderHistoryStatusTypeEnum.Prepared && currentStatus == OrderHistoryStatusTypeEnum.Preparing)
            {
                return true;
            }

            if (userRole == "Shipper")
            {
                switch (currentStatus)
                {
                    case OrderHistoryStatusTypeEnum.Prepared:
                        return newStatus == OrderHistoryStatusTypeEnum.ShipperReceived;
                    case OrderHistoryStatusTypeEnum.ShipperReceived:
                        return newStatus == OrderHistoryStatusTypeEnum.InTransit;
                    case OrderHistoryStatusTypeEnum.InTransit:
                        return newStatus == OrderHistoryStatusTypeEnum.Delivered;
                    default:
                        return false;
                }
            }

            return false;
        }
        //update status payment delivered
        public async Task<bool> UpdatePaymentStatusDeliveredAsync(int orderId, IFormFile image)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return false;
            }

            var currentStatus = order.OrderHistories.FirstOrDefault(oh => oh.IsCurrent);

            if (currentStatus == null)
            {
                return false;
            }

            currentStatus.IsCurrent = false;

            var newOrderHistory = new OrderHistory
            {
                OrderId = orderId,
                Status = OrderHistoryStatusTypeEnum.Delivered,
                ChangeTime = DateTime.UtcNow,
                IsCurrent = true
            };
            Random random = new Random();
            int randomNumber = random.Next(1000, 10000);
            var imageUrl = $"ORDER/{randomNumber}";
            var pathUrl = await _firebaseService.UploadFileToFirebase(image, imageUrl);
            order.Image = pathUrl;

            // Send notification to Customer
            var customer = await _unitOfWork.Customers.FindByCondition(u => u.Id == order.CustomerId).FirstOrDefaultAsync();
                await _notificationService.SendNotification(customer.DeviceId, "F-LocalBrand", $"Order {order.Id} is delivered!");
                await _notificationService.PushNotificationToRedis(customer.Id, order.Id, $"Order {order.Id} is delivered", "Delivered", customer.FullName, customer.Image);


            await _unitOfWork.OrderHistories.UpdateAsync(currentStatus);
            await _unitOfWork.OrderHistories.CreateAsync(newOrderHistory);
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

            return true;
        }
    }
}
