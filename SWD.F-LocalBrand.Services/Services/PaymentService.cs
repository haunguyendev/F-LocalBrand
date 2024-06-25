using AutoMapper;
using SWD.F_LocalBrand.Business.DTO.Payment;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Models;
using SWD.F_LocalBrand.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class PaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region create payment
        public async Task<Payment> CreatePaymentAsync(PaymentCreateModel model)
        {
            var payment = new Payment
            {
                OrderId = model.OrderId,
                PaymentDate = DateOnly.FromDateTime(model.PaymentDate),
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = model.PaymentStatus,
                StatusResponseCode = model.StatusResponseCode
            };

            await _unitOfWork.Payments.CreateAsync(payment);
            await _unitOfWork.CommitAsync();

            return payment;
        }
        #endregion


    }
}
