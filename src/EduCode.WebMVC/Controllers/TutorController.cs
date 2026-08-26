using System.Text.Json;
using EduCode.WebMVC.Filters;
using EduCode.WebMVC.Models;
using EduCode.WebMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebMVC.Controllers;

[SessionRole("Alumno")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class TutorController : Controller
{
    private readonly IEduCodeApiClient _api;
    private static readonly JsonSerializerOptions JsonOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public TutorController(IEduCodeApiClient api) => _api = api;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ApiResult<EstadoTutorResponse> resultado = await _api.ObtenerEstadoTutorAsync(ct);
        if (!resultado.Exitoso || resultado.Datos is null)
        {
            TempData["Error"] = MensajeTutorParaUsuario(
                resultado.StatusCode,
                alCargarEstado: true);
            return RedirectToAction("Dashboard", "Alumno");
        }

        return View(new TutorViewModel
        {
            Estado = resultado.Datos,
            UsuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0,
            NombreAlumno = HttpContext.Session.GetString("NombreCompleto") ?? "Estudiante",
            PreguntasSugeridas =
            [
                "¿Cómo recorro un array sin salir de rango?",
                "¿Cómo agrego y elimino elementos de una lista?",
                "¿Qué funcionalidades puedo usar para trabajar con strings?",
                "¿Cuál es la diferencia entre for y while?",
                "¿Por qué una función puede devolver undefined?",
                "Quiero practicar clases y objetos dentro del recorrido"
            ]
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Consultar(
        [FromForm] ConsultaTutorWebRequest formulario,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            string mensaje = ModelState.Values
                .SelectMany(valor => valor.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "La consulta no es válida.";

            return BadRequest(new { error = mensaje });
        }

        List<MensajeTutorRequest> historial = DeserializarHistorial(
            formulario.HistorialJson);

        ApiResult<RespuestaTutorResponse> resultado = await _api.ConsultarTutorAsync(
            new ConsultaTutorRequest
            {
                Consulta = formulario.Consulta.Trim(),
                Historial = historial
            },
            ct);

        if (!resultado.Exitoso || resultado.Datos is null)
        {
            int estado = resultado.StatusCode is >= 400 and <= 599
                ? resultado.StatusCode
                : StatusCodes.Status502BadGateway;

            return StatusCode(estado, new
            {
                error = MensajeTutorParaUsuario(estado, alCargarEstado: false)
            });
        }

        return Json(resultado.Datos);
    }

    private static string MensajeTutorParaUsuario(
        int statusCode,
        bool alCargarEstado)
    {
        return statusCode switch
        {
            StatusCodes.Status401Unauthorized =>
                "Tu sesión venció. Volvé a iniciar sesión.",
            StatusCodes.Status429TooManyRequests =>
                "Alcanzaste el límite diario de consultas. Podrás volver a usar el tutor mañana.",
            StatusCodes.Status502BadGateway or
            StatusCodes.Status503ServiceUnavailable or
            StatusCodes.Status504GatewayTimeout =>
                alCargarEstado
                    ? "El tutor no está disponible en este momento. Intentá ingresar nuevamente en unos segundos."
                    : "El tutor está tardando más de lo esperado. Intentá nuevamente en unos segundos.",
            _ => alCargarEstado
                ? "No pudimos abrir el tutor en este momento. Intentá nuevamente."
                : "El tutor no pudo responder en este momento. Intentá nuevamente."
        };
    }

    private static List<MensajeTutorRequest> DeserializarHistorial(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];

        try
        {
            return (JsonSerializer.Deserialize<List<MensajeTutorRequest>>(
                    json,
                    JsonOptions) ?? [])
                .Where(mensaje =>
                    mensaje is not null &&
                    !string.IsNullOrWhiteSpace(mensaje.Contenido) &&
                    string.Equals(mensaje.Rol, "user", StringComparison.OrdinalIgnoreCase))
                .TakeLast(6)
                .Select(mensaje => new MensajeTutorRequest
                {
                    Rol = "user",
                    Contenido = mensaje.Contenido.Trim().Length <= 1200
                        ? mensaje.Contenido.Trim()
                        : mensaje.Contenido.Trim()[..1200]
                })
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
