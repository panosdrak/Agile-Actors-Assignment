namespace Agile_Actors_Assignment.Models
{
    public class BasicDataResponse<T> : BasicResponse
    {
        public T Data { get; set; }

        public static BasicDataResponse<T> Ok(T data, string message = "Operation completed successfully")
        {
            return new BasicDataResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static new BasicDataResponse<T> Fail(string message, string errorCode = null)
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
