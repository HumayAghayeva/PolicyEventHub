using DataBridge.API.Enums;
using DataBridge.API.Models;

namespace DataBridge.API.DTO
{
    /// <summary>
    /// Base class used by API responses
    /// </summary>
    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public ApiResponse() : base()
        {
        }

        public ApiResponse(T? data) : base()
        {
            Data = data;
        }

        public ApiResponse(T? data, string message) : base()
        {
            Data = data;
            Message = message;
        }

        private ApiResponse(string message, ResponseCode responseCode) : base()
        {
            this.Message = message;
            this.Code = responseCode;
        }

        public static ApiResponse<T> AuthFailure(string message)
        {
            return new ApiResponse<T>(message, ResponseCode.AuthenticationError);
        }

        public static ApiResponse<T> Success(T value)
        {
            return new ApiResponse<T>(value);
        }

        public static ApiResponse<T> Success(T value, string message)
        {
            return new ApiResponse<T>(value, message);
        }

        public ApiResponse(T data, ResponseCode code, string message)
        {
            Data = data;
            Code = code;
            Message = message;
        }

        public new static ApiResponse<T> Failure(string message, ResponseCode code)
        {
            return new ApiResponse<T>(message, code);
        }
    }

    public class ApiResponse : BaseMessage
    {
        public ResponseCode Code { get; set; }

        public string Message { get; set; }

        public DateTime TimeStamp { get; set; } = DateTime.Now;


        public ApiResponse() : base()
        {
            Code = ResponseCode.Success;
            Message = "Success";
        }

        public ApiResponse(ResponseCode code, string message) : base()
        {
            Code = code;
            Message = message;
        }

        public static ApiResponse Success(string message)
        {
            return new ApiResponse(ResponseCode.Success, message);
        }

        public static ApiResponse Failure(string message, ResponseCode code)
        {
            return new ApiResponse(code, message);
        }
    }
}