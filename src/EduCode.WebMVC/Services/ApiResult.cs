namespace EduCode.WebMVC.Services;

public sealed class ApiResult<T>
{
    public bool Exitoso { get; init; }
    public T? Datos { get; init; }
    public string Error { get; init; } = string.Empty;
    public int StatusCode { get; init; }

    public static ApiResult<T> Ok(T? datos, int statusCode = 200) =>
        new() { Exitoso = true, Datos = datos, StatusCode = statusCode };

    public static ApiResult<T> Fallo(string error, int statusCode = 500) =>
        new() { Exitoso = false, Error = error, StatusCode = statusCode };
}
