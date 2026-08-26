using EduCode.WebMVC.Models;

namespace EduCode.WebMVC.Services;

public interface IEduCodeApiClient
{
    Task<ApiResult<UsuarioAutenticadoResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<ApiResult<UsuarioAutenticadoResponse>> RegistrarAsync(RegistroRequest request, CancellationToken ct = default);
    Task<ApiResult<UsuarioAutenticadoResponse>> RegistrarDocenteAsync(RegistroDocenteRequest request, CancellationToken ct = default);
    Task<ApiResult<PerfilUsuarioResponse>> ObtenerPerfilAsync(CancellationToken ct = default);
    Task<ApiResult<PerfilUsuarioResponse>> ActualizarPerfilAsync(ActualizarPerfilRequest request, CancellationToken ct = default);
    Task<ApiResult<List<OpinionUsuarioResponse>>> ObtenerMisOpinionesAsync(CancellationToken ct = default);
    Task<ApiResult<OpinionUsuarioResponse>> CrearOpinionAsync(CrearOpinionRequest request, CancellationToken ct = default);
    Task<ApiResult<List<OpinionUsuarioResponse>>> ObtenerOpinionesAdministracionAsync(CancellationToken ct = default);
    Task<ApiResult<OpinionUsuarioResponse>> ActualizarEstadoOpinionAsync(int opinionId, ActualizarEstadoOpinionRequest request, CancellationToken ct = default);
    Task<ApiResult<List<CursoResponse>>> ObtenerCursosAsync(CancellationToken ct = default);
    Task<ApiResult<List<TemaResponse>>> ObtenerTemasAsync(int cursoId, CancellationToken ct = default);
    Task<ApiResult<List<EjercicioListadoResponse>>> ObtenerEjerciciosAsync(int temaId, CancellationToken ct = default);
    Task<ApiResult<EjercicioDetalleResponse>> ObtenerEjercicioAsync(int id, CancellationToken ct = default);
    Task<ApiResult<ResultadoIntentoResponse>> EnviarIntentoAsync(int ejercicioId, EnviarIntentoRequest request, CancellationToken ct = default);
    Task<ApiResult<ResultadoIntentoResponse>> ReintentarCorreccionAsync(int intentoId, CancellationToken ct = default);
    Task<ApiResult<ResultadoIntentoResponse>> ObtenerResultadoIntentoAsync(int intentoId, CancellationToken ct = default);
    Task<ApiResult<List<IntentoHistorialResponse>>> ObtenerHistorialAsync(int ejercicioId, CancellationToken ct = default);
    Task<ApiResult<PistaResponse>> ObtenerPistaAsync(int ejercicioId, int nivelActual, CancellationToken ct = default);
    Task<ApiResult<ProgresoUsuarioResponse>> ObtenerProgresoAsync(CancellationToken ct = default);
    Task<ApiResult<ResumenGamificacionResponse>> ObtenerGamificacionAsync(CancellationToken ct = default);
    Task<ApiResult<object>> CambiarVisibilidadRankingAsync(bool mostrar, CancellationToken ct = default);
    Task<ApiResult<EstadoTutorResponse>> ObtenerEstadoTutorAsync(CancellationToken ct = default);
    Task<ApiResult<RespuestaTutorResponse>> ConsultarTutorAsync(ConsultaTutorRequest request, CancellationToken ct = default);
    Task<ApiResult<object>> CrearTemaAsync(AltaTemaRequest request, CancellationToken ct = default);
    Task<ApiResult<object>> CrearEjercicioAsync(AltaEjercicioRequest request, CancellationToken ct = default);
    Task<ApiResult<PanelDocenteResponse>> ObtenerPanelDocenteAsync(CancellationToken ct = default);
    Task<ApiResult<List<PropuestaEjercicioResponse>>> ObtenerPropuestasDocenteAsync(CancellationToken ct = default);
    Task<ApiResult<PropuestaEjercicioResponse>> CrearPropuestaDocenteAsync(CrearPropuestaEjercicioRequest request, CancellationToken ct = default);
    Task<ApiResult<PropuestaEjercicioResponse>> ActualizarPropuestaDocenteAsync(int propuestaId, CrearPropuestaEjercicioRequest request, CancellationToken ct = default);
    Task<ApiResult<PanelAdministracionResponse>> ObtenerPanelAdministracionAsync(CancellationToken ct = default);
    Task<ApiResult<EstadoSistemaResponse>> ObtenerEstadoSistemaAsync(CancellationToken ct = default);
    Task<ApiResult<List<InvitacionDocenteResponse>>> ObtenerInvitacionesDocentesAsync(CancellationToken ct = default);
    Task<ApiResult<InvitacionDocenteResponse>> CrearInvitacionDocenteAsync(CrearInvitacionDocenteRequest request, CancellationToken ct = default);
    Task<ApiResult<object>> RevocarInvitacionDocenteAsync(int invitacionId, CancellationToken ct = default);
    Task<ApiResult<List<PropuestaEjercicioResponse>>> ObtenerPropuestasAdministracionAsync(CancellationToken ct = default);
    Task<ApiResult<PropuestaEjercicioResponse>> RevisarPropuestaAsync(int propuestaId, RevisarPropuestaEjercicioRequest request, CancellationToken ct = default);
    Task<ApiResult<EstructuraAcademicaResponse>> ObtenerEstructuraAcademicaAsync(CancellationToken ct = default);
    Task<ApiResult<object>> CrearGrupoAsync(CrearGrupoRequest request, CancellationToken ct = default);
    Task<ApiResult<object>> InscribirAlumnoAsync(int grupoId, InscribirAlumnoRequest request, CancellationToken ct = default);
}
