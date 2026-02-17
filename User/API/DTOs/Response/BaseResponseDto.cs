using Error = Domain.Models.Error;

namespace API.DTOs.Response
{
    public class BaseResponseDto<T>
    {
        public T Result { get; set; }
        public Error Error { get; set; }
        public bool Success => Error == null;
    }
}
