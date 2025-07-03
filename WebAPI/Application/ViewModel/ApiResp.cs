using Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{           
    public class ApiResp
    {
        public HttpStatusCode StatusCode { get; private set; }
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public object? Result { get; private set; }

        public ApiResp SetOk(object? result = null) // Changed 'object' to 'object?' to allow null values  
        {   
            IsSuccess = true;
            StatusCode = HttpStatusCode.OK;
            Result = result;
            return this;
        }

        public ApiResp SetNotFound(object? result = null, string? message = null) // Changed 'object' and 'string' to nullable types  
        {
            IsSuccess = false;
            StatusCode = HttpStatusCode.NotFound;
            if (!string.IsNullOrEmpty(message))
            {
                ErrorMessage = message;
            }
            Result = result;
            return this;
        }

        public ApiResp SetBadRequest(object? result = null, string? message = null) // Changed 'object' and 'string' to nullable types  
        {
            IsSuccess = false;
            StatusCode = HttpStatusCode.BadRequest;
            if (!string.IsNullOrEmpty(message))
            {
                ErrorMessage = message;
            }
            Result = result;
            return this;
        }
        public ApiResp SetUnauthorized(object? result = null, string? message = null) // Changed 'object' and 'string' to nullable types  
        {
            IsSuccess = false;
            StatusCode = HttpStatusCode.Unauthorized;
            if (!string.IsNullOrEmpty(message))
            {
                ErrorMessage = message;
            }
            Result = result;
            return this;
        }

        public ApiResp SetApiResponse(HttpStatusCode statusCode, bool isSuccess, string? message = null, object? result = null) // Changed 'string' and 'object' to nullable types  
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            if (!string.IsNullOrEmpty(message))
            {
                ErrorMessage = message;
            }
            Result = result;
            return this;
        }

        public static implicit operator double(ApiResp v)
        {
            throw new NotImplementedException();
        }
    }
}
