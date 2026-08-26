using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EduCode.WebMVC.Models;

namespace EduCode.WebMVC.Services;

public sealed class EduCodeApiClient : IEduCodeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<EduCodeApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public EduCodeApiClient(HttpClient httpClient, IHttpContextAccessor contextAccessor,
        ILogger<EduCodeApiClient> logger)
    {
        _httpClient = httpClient;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }

    public Task<ApiResult<UsuarioAutenticadoResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default) =>
        EnviarAsync<UsuarioAutenticadoResponse>(HttpMethod.Post, "api/auth/login", request, false, ct);

    public Task<ApiResult<UsuarioAutenticadoResponse>> RegistrarAsync(RegistroRequest request, CancellationToken ct = default) =>
        EnviarAsync<UsuarioAutenticadoResponse>(HttpMethod.Post, "api/auth/registro", request, false, ct);

    public Task<ApiResult<UsuarioAutenticadoResponse>> RegistrarDocenteAsync(RegistroDocenteRequest request, CancellationToken ct = default) =>
        EnviarAsync<UsuarioAutenticadoResponse>(HttpMethod.Post, "api/auth/registro-docente", request, false, ct);

    public Task<ApiResult<PerfilUsuarioResponse>> ObtenerPerfilAsync(CancellationToken ct = default) =>
        EnviarAsync<PerfilUsuarioResponse>(HttpMethod.Get, "api/perfil", null, true, ct);

    public Task<ApiResult<PerfilUsuarioResponse>> ActualizarPerfilAsync(
        ActualizarPerfilRequest request,
        CancellationToken ct = default) =>
        EnviarAsync<PerfilUsuarioResponse>(HttpMethod.Put, "api/perfil", request, true, ct);

    public Task<ApiResult<List<OpinionUsuarioResponse>>> ObtenerMisOpinionesAsync(
        CancellationToken ct = default) =>
        EnviarAsync<List<OpinionUsuarioResponse>>(HttpMethod.Get, "api/perfil/opiniones", null, true, ct);

    public Task<ApiResult<OpinionUsuarioResponse>> CrearOpinionAsync(
        CrearOpinionRequest request,
        CancellationToken ct = default) =>
        EnviarAsync<OpinionUsuarioResponse>(HttpMethod.Post, "api/perfil/opiniones", request, true, ct);

    public Task<ApiResult<List<OpinionUsuarioResponse>>> ObtenerOpinionesAdministracionAsync(
        CancellationToken ct = default) =>
        EnviarAsync<List<OpinionUsuarioResponse>>(HttpMethod.Get, "api/administracion/opiniones", null, true, ct);

    public Task<ApiResult<OpinionUsuarioResponse>> ActualizarEstadoOpinionAsync(
        int opinionId,
        ActualizarEstadoOpinionRequest request,
        CancellationToken ct = default) =>
        EnviarAsync<OpinionUsuarioResponse>(
            HttpMethod.Put,
            $"api/administracion/opiniones/{opinionId}/estado",
            request,
            true,
            ct);

    public Task<ApiResult<List<CursoResponse>>> ObtenerCursosAsync(CancellationToken ct = default) =>
        EnviarAsync<List<CursoResponse>>(HttpMethod.Get, "api/cursos", null, true, ct);

    public Task<ApiResult<List<TemaResponse>>> ObtenerTemasAsync(int cursoId, CancellationToken ct = default) =>
        EnviarAsync<List<TemaResponse>>(HttpMethod.Get, $"api/temas/curso/{cursoId}", null, true, ct);

    public Task<ApiResult<List<EjercicioListadoResponse>>> ObtenerEjerciciosAsync(int temaId, CancellationToken ct = default) =>
        EnviarAsync<List<EjercicioListadoResponse>>(HttpMethod.Get, $"api/ejercicios/tema/{temaId}", null, true, ct);

    public Task<ApiResult<EjercicioDetalleResponse>> ObtenerEjercicioAsync(int id, CancellationToken ct = default) =>
        EnviarAsync<EjercicioDetalleResponse>(HttpMethod.Get, $"api/ejercicios/{id}", null, true, ct);

    public Task<ApiResult<ResultadoIntentoResponse>> EnviarIntentoAsync(int ejercicioId, EnviarIntentoRequest request, CancellationToken ct = default) =>
        EnviarAsync<ResultadoIntentoResponse>(HttpMethod.Post, $"api/ejercicios/{ejercicioId}/intentos", request, true, ct);

    public Task<ApiResult<ResultadoIntentoResponse>> ReintentarCorreccionAsync(int intentoId, CancellationToken ct = default) =>
        EnviarAsync<ResultadoIntentoResponse>(HttpMethod.Post, $"api/ejercicios/intentos/{intentoId}/reintentar", null, true, ct);

    public Task<ApiResult<ResultadoIntentoResponse>> ObtenerResultadoIntentoAsync(int intentoId, CancellationToken ct = default) =>
        EnviarAsync<ResultadoIntentoResponse>(HttpMethod.Get, $"api/ejercicios/intentos/{intentoId}/resultado", null, true, ct);

    public Task<ApiResult<List<IntentoHistorialResponse>>> ObtenerHistorialAsync(int ejercicioId, CancellationToken ct = default) =>
        EnviarAsync<List<IntentoHistorialResponse>>(HttpMethod.Get, $"api/ejercicios/{ejercicioId}/intentos", null, true, ct);

    public Task<ApiResult<PistaResponse>> ObtenerPistaAsync(int ejercicioId, int nivelActual, CancellationToken ct = default) =>
        EnviarAsync<PistaResponse>(HttpMethod.Get, $"api/ejercicios/{ejercicioId}/pista?nivelActual={nivelActual}", null, true, ct);

    public Task<ApiResult<ProgresoUsuarioResponse>> ObtenerProgresoAsync(CancellationToken ct = default) =>
        EnviarAsync<ProgresoUsuarioResponse>(HttpMethod.Get, "api/progreso/mi-progreso", null, true, ct);

    public Task<ApiResult<ResumenGamificacionResponse>> ObtenerGamificacionAsync(CancellationToken ct = default) =>
        EnviarAsync<ResumenGamificacionResponse>(HttpMethod.Get, "api/gamificacion/resumen", null, true, ct);

    public Task<ApiResult<object>> CambiarVisibilidadRankingAsync(
        bool mostrar,
        CancellationToken ct = default) =>
        EnviarAsync<object>(
            HttpMethod.Put,
            "api/gamificacion/visibilidad",
            new CambiarVisibilidadRankingRequest { Mostrar = mostrar },
            true,
            ct);

    public Task<ApiResult<EstadoTutorResponse>> ObtenerEstadoTutorAsync(CancellationToken ct = default) =>
        EnviarAsync<EstadoTutorResponse>(HttpMethod.Get, "api/tutor-ia/estado", null, true, ct);

    public Task<ApiResult<RespuestaTutorResponse>> ConsultarTutorAsync(
        ConsultaTutorRequest request,
        CancellationToken ct = default) =>
        EnviarAsync<RespuestaTutorResponse>(HttpMethod.Post, "api/tutor-ia/consultar", request, true, ct);

    public Task<ApiResult<object>> CrearTemaAsync(AltaTemaRequest request, CancellationToken ct = default) =>
        EnviarAsync<object>(HttpMethod.Post, "api/temas", request, true, ct);

    public Task<ApiResult<object>> CrearEjercicioAsync(AltaEjercicioRequest request, CancellationToken ct = default) =>
        EnviarAsync<object>(HttpMethod.Post, "api/ejercicios", request, true, ct);

    public Task<ApiResult<PanelDocenteResponse>> ObtenerPanelDocenteAsync(CancellationToken ct = default) =>
        EnviarAsync<PanelDocenteResponse>(HttpMethod.Get, "api/docente/panel", null, true, ct);

    public Task<ApiResult<List<PropuestaEjercicioResponse>>> ObtenerPropuestasDocenteAsync(CancellationToken ct = default) =>
        EnviarAsync<List<PropuestaEjercicioResponse>>(HttpMethod.Get, "api/docente/propuestas", null, true, ct);

    public Task<ApiResult<PropuestaEjercicioResponse>> CrearPropuestaDocenteAsync(CrearPropuestaEjercicioRequest request, CancellationToken ct = default) =>
        EnviarAsync<PropuestaEjercicioResponse>(HttpMethod.Post, "api/docente/propuestas", request, true, ct);

    public Task<ApiResult<PropuestaEjercicioResponse>> ActualizarPropuestaDocenteAsync(int propuestaId, CrearPropuestaEjercicioRequest request, CancellationToken ct = default) =>
        EnviarAsync<PropuestaEjercicioResponse>(HttpMethod.Put, $"api/docente/propuestas/{propuestaId}", request, true, ct);

    public Task<ApiResult<PanelAdministracionResponse>> ObtenerPanelAdministracionAsync(CancellationToken ct = default) =>
        EnviarAsync<PanelAdministracionResponse>(HttpMethod.Get, "api/administracion/panel", null, true, ct);

    public Task<ApiResult<EstadoSistemaResponse>> ObtenerEstadoSistemaAsync(CancellationToken ct = default) =>
        EnviarAsync<EstadoSistemaResponse>(HttpMethod.Get, "api/estado", null, false, ct);

    public Task<ApiResult<List<InvitacionDocenteResponse>>> ObtenerInvitacionesDocentesAsync(CancellationToken ct = default) =>
        EnviarAsync<List<InvitacionDocenteResponse>>(HttpMethod.Get, "api/administracion/invitaciones-docentes", null, true, ct);

    public Task<ApiResult<InvitacionDocenteResponse>> CrearInvitacionDocenteAsync(CrearInvitacionDocenteRequest request, CancellationToken ct = default) =>
        EnviarAsync<InvitacionDocenteResponse>(HttpMethod.Post, "api/administracion/invitaciones-docentes", request, true, ct);

    public Task<ApiResult<object>> RevocarInvitacionDocenteAsync(int invitacionId, CancellationToken ct = default) =>
        EnviarAsync<object>(HttpMethod.Put, $"api/administracion/invitaciones-docentes/{invitacionId}/revocar", null, true, ct);

    public Task<ApiResult<List<PropuestaEjercicioResponse>>> ObtenerPropuestasAdministracionAsync(CancellationToken ct = default) =>
        EnviarAsync<List<PropuestaEjercicioResponse>>(HttpMethod.Get, "api/administracion/propuestas-ejercicios", null, true, ct);

    public Task<ApiResult<PropuestaEjercicioResponse>> RevisarPropuestaAsync(int propuestaId, RevisarPropuestaEjercicioRequest request, CancellationToken ct = default) =>
        EnviarAsync<PropuestaEjercicioResponse>(HttpMethod.Put, $"api/administracion/propuestas-ejercicios/{propuestaId}", request, true, ct);

    public Task<ApiResult<EstructuraAcademicaResponse>> ObtenerEstructuraAcademicaAsync(CancellationToken ct = default) =>
        EnviarAsync<EstructuraAcademicaResponse>(HttpMethod.Get, "api/administracion/estructura-academica", null, true, ct);

    public Task<ApiResult<object>> CrearGrupoAsync(CrearGrupoRequest request, CancellationToken ct = default) =>
        EnviarAsync<object>(HttpMethod.Post, "api/administracion/grupos", request, true, ct);

    public Task<ApiResult<object>> InscribirAlumnoAsync(int grupoId, InscribirAlumnoRequest request, CancellationToken ct = default) =>
        EnviarAsync<object>(HttpMethod.Post, $"api/administracion/grupos/{grupoId}/inscripciones", request, true, ct);

    private async Task<ApiResult<T>> EnviarAsync<T>(HttpMethod method, string route, object? body,
        bool requiereToken, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, route);
            if (body is not null) request.Content = JsonContent.Create(body, options: JsonOptions);

            if (requiereToken)
            {
                string? token = _contextAccessor.HttpContext?.Session.GetString("AccessToken");
                if (string.IsNullOrWhiteSpace(token))
                    return ApiResult<T>.Fallo("Tu sesión venció. Volvé a iniciar sesión.", 401);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using HttpResponseMessage response = await _httpClient.SendAsync(request, ct);
            string content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                string? detalleInterno = ExtraerError(content);
                _logger.LogWarning(
                    "Respuesta no exitosa al consumir {Route}. Estado: {StatusCode}. Detalle: {Detalle}",
                    route,
                    (int)response.StatusCode,
                    detalleInterno ?? "Sin detalle");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    _contextAccessor.HttpContext?.Session.Clear();

                string mensajePublico = CrearMensajePublico(
                    response.StatusCode,
                    detalleInterno,
                    requiereToken);
                return ApiResult<T>.Fallo(mensajePublico, (int)response.StatusCode);
            }

            if (string.IsNullOrWhiteSpace(content)) return ApiResult<T>.Ok(default, (int)response.StatusCode);
            T? data = JsonSerializer.Deserialize<T>(content, JsonOptions);
            return ApiResult<T>.Ok(data, (int)response.StatusCode);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "La solicitud a {Route} superó el tiempo de espera.", route);
            return ApiResult<T>.Fallo(
                "La operación está tardando más de lo esperado. Intentá nuevamente en unos segundos.",
                504);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "No fue posible conectar con EduCode API en {Route}.", route);
            return ApiResult<T>.Fallo(
                "No pudimos completar la operación en este momento. Intentá nuevamente en unos segundos.",
                503);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado consumiendo {Route}", route);
            return ApiResult<T>.Fallo("Ocurrió un error inesperado al procesar la solicitud.", 500);
        }
    }

    private static string CrearMensajePublico(
        System.Net.HttpStatusCode statusCode,
        string? detalleInterno,
        bool requiereToken)
    {
        if (statusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            if (!requiereToken &&
                !string.IsNullOrWhiteSpace(detalleInterno) &&
                !ContieneDetalleTecnico(detalleInterno))
            {
                return detalleInterno;
            }

            return "Tu sesión venció. Volvé a iniciar sesión.";
        }

        if (statusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            return !string.IsNullOrWhiteSpace(detalleInterno) &&
                   !ContieneDetalleTecnico(detalleInterno)
                ? detalleInterno
                : "Alcanzaste el límite disponible. Intentá nuevamente más adelante.";
        }

        if ((int)statusCode >= 500)
        {
            return statusCode == System.Net.HttpStatusCode.GatewayTimeout
                ? "La operación está tardando más de lo esperado. Intentá nuevamente en unos segundos."
                : "No pudimos completar la operación en este momento. Intentá nuevamente en unos segundos.";
        }

        if (!string.IsNullOrWhiteSpace(detalleInterno) &&
            !ContieneDetalleTecnico(detalleInterno))
        {
            return detalleInterno;
        }

        return statusCode switch
        {
            System.Net.HttpStatusCode.BadRequest =>
                "No pudimos procesar la información enviada. Revisala e intentá nuevamente.",
            System.Net.HttpStatusCode.NotFound =>
                "No encontramos la información solicitada.",
            System.Net.HttpStatusCode.Conflict =>
                "La operación entra en conflicto con el estado actual. Actualizá la página e intentá nuevamente.",
            _ => "No pudimos completar la operación. Intentá nuevamente."
        };
    }

    private static bool ContieneDetalleTecnico(string mensaje)
    {
        string normalizado = mensaje.ToLowerInvariant();
        string[] marcasTecnicas =
        [
            "api", "webapi", "endpoint", "http", "status code", "puerto",
            "gemini", "openai", "proveedor", "modelo", "api key", "token",
            "exception", "stack trace", "sql", "entity framework", "jwt",
            "<html", "<!doctype", "<body", "traceid", "kestrel", "asp.net",
            "localhost"
        ];

        return marcasTecnicas.Any(marca =>
            normalizado.Contains(marca, StringComparison.Ordinal));
    }

    private static string? ExtraerError(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;
        try
        {
            using JsonDocument doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("error", out JsonElement error)) return error.GetString();
            if (doc.RootElement.TryGetProperty("title", out JsonElement title)) return title.GetString();
        }
        catch (JsonException) { }
        return content.Length <= 250 ? content : null;
    }
}
