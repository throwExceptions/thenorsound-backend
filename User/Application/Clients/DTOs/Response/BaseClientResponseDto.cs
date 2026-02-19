namespace Application.Clients.DTOs.Response;

public class BaseClientResponseDto<T>
{
    public T Result { get; set; }
    public bool Success { get; set; }
}
