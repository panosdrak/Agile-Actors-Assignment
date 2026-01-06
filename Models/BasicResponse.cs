namespace Agile_Actors_Assignment.Models
{
    public class BasicResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }

        public static BasicResponse Ok(string message = "Operation completed successfully")
        {
            return new BasicResponse
            {
                Success = true,
                Message = message
            };
        }

        public static BasicResponse Fail(string message, string? errorCode = null)
        {
            return new BasicResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    public class BasicDataResponse<T> : BasicResponse
    {
        public T? Data { get; set; }

        public static BasicDataResponse<T> Ok(T data, string message = "Operation completed successfully")
        {
            return new BasicDataResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static new BasicDataResponse<T> Fail(string message, string? errorCode = null)
        {
            return new BasicDataResponse<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                Data = default
            };
        }
    }
}
