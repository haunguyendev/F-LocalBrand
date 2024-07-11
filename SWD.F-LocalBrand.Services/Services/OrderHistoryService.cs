using Microsoft.VisualBasic;
using SWD.F_LocalBrand.Business.Common.Shared;
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
        public OrderHistoryService(IUnitOfWork unitOfWork)
        {
                _unitOfWork = unitOfWork;
            
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
    }
}
