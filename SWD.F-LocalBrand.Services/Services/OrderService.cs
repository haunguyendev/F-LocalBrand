using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class OrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //get list order have payment status is true
        public async Task<List<OrderModel>> GetOrdersWithPaymentStatusTrueAsync()
        {
            var orders = await _unitOfWork.Orders.FindAll()
                .Include(o => o.Payments)
                .Where(o => o.Payments.Any(p => p.PaymentStatus == "true")).ToListAsync();
            return _mapper.Map<List<OrderModel>>(orders);
        }


        //get order or list order have status from request
        public async Task<IEnumerable<OrderModel>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _unitOfWork.Orders.FindByCondition(o => o.OrderStatus == status).ToListAsync();
            return _mapper.Map<IEnumerable<OrderModel>>(orders);
        }
    }
}
