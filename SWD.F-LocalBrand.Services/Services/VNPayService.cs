using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Options;
using Sprache;
using SWD.F_LocalBrand.Business.DTO.VNPay;
using SWD.F_LocalBrand.Business.Helpers;
using SWD.F_LocalBrand.Business.Settings.VNPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class VNPayService
    {
        private readonly VNPaySettings _vnpaySettings;

        public VNPayService(IOptions<VNPaySettings> vnpaySettings)
        {
            _vnpaySettings = vnpaySettings.Value;
        }

        public SortedList<string, string> responseData
           = new SortedList<string, string>(new VnpayCompare());

        //hàm này để sắp các string theo thứ tự trừ trên xuống theo bảng chữ cái
        public SortedList<string, string> requestData
            = new SortedList<string, string>(new VnpayCompare());

        public string GetLink(string baseUrl, string secretKey, CreateVNPayModel createVNPayModel)
        {
            MakeRequestData(createVNPayModel);
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string result = baseUrl + "?" + data.ToString();
            var secureHash = SecurityUtil.HmacSHA512(secretKey, data.ToString().Remove(data.Length - 1, 1));
            return result += "vnp_SecureHash=" + secureHash;
        }

        //Check data if it is not null then add to requestData
        public void MakeRequestData(CreateVNPayModel createVNPayModel)
        {
            if (createVNPayModel.vnp_Amount != null)
                requestData.Add("vnp_Amount", createVNPayModel.vnp_Amount.ToString() ?? string.Empty);
            if (createVNPayModel.vnp_Command != null)
                requestData.Add("vnp_Command", createVNPayModel.vnp_Command);
            if (createVNPayModel.vnp_CreateDate != null)
                requestData.Add("vnp_CreateDate", createVNPayModel.vnp_CreateDate);
            if (createVNPayModel.vnp_CurrCode != null)
                requestData.Add("vnp_CurrCode", createVNPayModel.vnp_CurrCode);
            if (createVNPayModel.vnp_BankCode != null)
                requestData.Add("vnp_BankCode", createVNPayModel.vnp_BankCode);
            if (createVNPayModel.vnp_IpAddr != null)
                requestData.Add("vnp_IpAddr", createVNPayModel.vnp_IpAddr);
            if (createVNPayModel.vnp_Locale != null)
                requestData.Add("vnp_Locale", createVNPayModel.vnp_Locale);
            if (createVNPayModel.vnp_OrderInfo != null)
                requestData.Add("vnp_OrderInfo", createVNPayModel.vnp_OrderInfo);
            if (createVNPayModel.vnp_OrderType != null)
                requestData.Add("vnp_OrderType", createVNPayModel.vnp_OrderType);
            if (createVNPayModel.vnp_ReturnUrl != null)
                requestData.Add("vnp_ReturnUrl", createVNPayModel.vnp_ReturnUrl);
            if (createVNPayModel.vnp_TmnCode != null)
                requestData.Add("vnp_TmnCode", createVNPayModel.vnp_TmnCode);
            if (createVNPayModel.vnp_ExpireDate != null)
                requestData.Add("vnp_ExpireDate", createVNPayModel.vnp_ExpireDate);
            if (createVNPayModel.vnp_TxnRef != null)
                requestData.Add("vnp_TxnRef", createVNPayModel.vnp_TxnRef);
            if (createVNPayModel.vnp_Version != null)
                requestData.Add("vnp_Version", createVNPayModel.vnp_Version);
        }

        //Check Signature response from VNPAY
        public bool IsValidSignature(string secretKey, UpdateVNPayModel updateVNPayModel)
        {
            MakeResponseData(updateVNPayModel);
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in responseData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string checkSum = SecurityUtil.HmacSHA512(secretKey,
                data.ToString().Remove(data.Length - 1, 1));
            return checkSum.Equals(updateVNPayModel.vnp_SecureHash, StringComparison.InvariantCultureIgnoreCase);
        }

        public void MakeResponseData(UpdateVNPayModel updateVNPayModel)
        {
            if (updateVNPayModel.vnp_Amount != null)
                responseData.Add("vnp_Amount", updateVNPayModel.vnp_Amount.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(updateVNPayModel.vnp_TmnCode))
                responseData.Add("vnp_TmnCode", updateVNPayModel.vnp_TmnCode.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(updateVNPayModel.vnp_BankCode))
                responseData.Add("vnp_BankCode", updateVNPayModel.vnp_BankCode.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(updateVNPayModel.vnp_BankTranNo))
                responseData.Add("vnp_BankTranNo", updateVNPayModel.vnp_BankTranNo.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(updateVNPayModel.vnp_CardType))
                responseData.Add("vnp_CardType", updateVNPayModel.vnp_CardType.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(updateVNPayModel.vnp_OrderInfo))
                responseData.Add("vnp_OrderInfo", updateVNPayModel.vnp_OrderInfo.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(updateVNPayModel.vnp_TransactionNo))
                responseData.Add("vnp_TransactionNo", updateVNPayModel.vnp_TransactionNo.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(updateVNPayModel.vnp_TransactionStatus))
                responseData.Add("vnp_TransactionStatus", updateVNPayModel.vnp_TransactionStatus.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(updateVNPayModel.vnp_TxnRef))
                responseData.Add("vnp_TxnRef", updateVNPayModel.vnp_TxnRef.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(updateVNPayModel.vnp_PayDate))
                responseData.Add("vnp_PayDate", updateVNPayModel.vnp_PayDate.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(updateVNPayModel.vnp_ResponseCode))
                responseData.Add("vnp_ResponseCode", updateVNPayModel.vnp_ResponseCode ?? string.Empty);
        }
    }
}
