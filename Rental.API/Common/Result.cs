namespace Rental.API.Common
{
    public class Result
    {
        public bool IsSuccess { get; private set; }

        public List<string> Errors { get; private set; }

        public Result(bool isSuccess, List<string>? errors)
        {
            IsSuccess = isSuccess;
            Errors = errors ?? new List<string>();
        }

        public static Result Success()
        {
            return new Result(true, null);
        }

        public static Result Failure(string error) { 
            return new Result(false, new List<string> { error });
        }

        public static Result Failure(List<string> errors) {
            return new Result(false, errors);
        }
    }

    public class Result<T>
    {
        public T? Data { get; private set;}

        public bool IsSuccess { get; private set; }

        public List<string> Errors { get; private set; }

        public Result(T? data, bool isSuccess, List<string>? errors)
        {
            Data = data;
            IsSuccess = isSuccess;
            Errors = errors ?? new List<string>();
        }

        public static Result<T> Success(T data)
        {
            return new Result<T>(data, true, null);
        }

        public static Result<T> Failure(string error)
        {
            return new Result<T>(default, false, new List<string> { error });
        }

        public static Result<T> Failure(List<string> errors)
        {
            return new Result<T>(default, false, errors);
        }
    }
}
