using System.ComponentModel.DataAnnotations;

namespace EduCode.WebMVC.Models;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Ingresá tu correo o nombre de usuario.")]
    [Display(Name = "Correo o usuario")]
    public string EmailONombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu contraseña.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public sealed class RegistroRequest
{
    [Required, StringLength(80)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(80)] public string Apellido { get; set; } = string.Empty;
    [Required, StringLength(50), Display(Name = "Nombre de usuario")] public string NombreUsuario { get; set; } = string.Empty;
    [Required, EmailAddress, Display(Name = "Correo electrónico")] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden."), Display(Name = "Confirmar contraseña")]
    public string ConfirmacionPassword { get; set; } = string.Empty;
}

public sealed class UsuarioAutenticadoResponse
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string AvatarClave { get; set; } = "codigo";
    public string FotoPerfilRuta { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime FechaExpiracion { get; set; }
    public DateTime FechaExpiracionRefreshToken { get; set; }
}

public sealed class CursoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CantidadTemas { get; set; }
}

public sealed class TemaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int CantidadEjercicios { get; set; }
    public int EjerciciosCompletados { get; set; }
    public decimal PorcentajeProgreso { get; set; }
    public bool Bloqueado { get; set; }
}

public sealed class EjercicioListadoResponse
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public int PuntajeMaximo { get; set; }
    public int Orden { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal MejorPuntaje { get; set; }
    public int CantidadIntentos { get; set; }
    public bool Bloqueado { get; set; }
}

public sealed class EjercicioDetalleResponse
{
    public int Id { get; set; }
    public int TemaId { get; set; }
    public string TemaNombre { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Consigna { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public string Lenguaje { get; set; } = string.Empty;
    public string CodigoInicial { get; set; } = string.Empty;
    public int PuntajeMaximo { get; set; }
    public int PuntajeMinimoAprobacion { get; set; }
    public int CantidadIntentosRealizados { get; set; }
    public bool PermiteSolicitarPista { get; set; }
    public int? SiguienteEjercicioId { get; set; }
    public string SiguienteEjercicioTitulo { get; set; } = string.Empty;
    public bool HaySiguienteEjercicioEnTema { get; set; }
    public List<CriterioPublicoResponse> Criterios { get; set; } = [];
}

public sealed class CriterioPublicoResponse
{
    public string Descripcion { get; set; } = string.Empty;
    public decimal Peso { get; set; }
    public bool Obligatorio { get; set; }
    public int PorcentajeMinimoObligatorio { get; set; }
}

public sealed class EnviarIntentoRequest
{
    public int EjercicioId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int? TiempoResolucionSegundos { get; set; }
}

public sealed class ResultadoIntentoResponse
{
    public int IntentoId { get; set; }
    public int NumeroIntento { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal PuntajeExacto { get; set; }
    public int PuntajeMostrado { get; set; }
    public bool EjercicioAprobado { get; set; }
    public bool CumpleCriteriosObligatorios { get; set; }
    public string MotivoNoAprobacion { get; set; } = string.Empty;
    public string Resumen { get; set; } = string.Empty;
    public List<string> AspectosCorrectos { get; set; } = [];
    public List<string> AspectosAMejorar { get; set; } = [];
    public string Pista { get; set; } = string.Empty;
    public string VersionRubrica { get; set; } = string.Empty;
    public string VersionMotorPuntaje { get; set; } = string.Empty;
    public List<ResultadoCriterioResponse> Criterios { get; set; } = [];
    public bool CorreccionPendiente { get; set; }
    public bool EsCorreccionProvisoria { get; set; }
    public bool PuedeReintentar { get; set; }
    public bool AfectaProgreso { get; set; } = true;
    public string MensajeCorreccion { get; set; } = string.Empty;
    public bool FeedbackPersonalizadoPendiente { get; set; }
    public bool FeedbackPersonalizadoDisponible { get; set; }
    public string ProveedorFeedback { get; set; } = string.Empty;
    public int CasosPruebaSuperados { get; set; }
    public int TotalCasosPrueba { get; set; }
    public List<ResultadoCasoPruebaResponse> ResultadosCasosPrueba { get; set; } = [];
    public List<HallazgoTecnicoResponse> HallazgosTecnicos { get; set; } = [];
    public List<LogroDesbloqueadoResponse> NuevosLogros { get; set; } = [];
}

public sealed class ResultadoCasoPruebaResponse
{
    public string Nombre { get; set; } = string.Empty;
    public bool Superado { get; set; }
    public bool EsOculto { get; set; }
    public string SalidaEsperada { get; set; } = string.Empty;
    public string SalidaObtenida { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

public sealed class HallazgoTecnicoResponse
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Critico { get; set; }
}

public sealed class ResultadoCriterioResponse
{
    public int CriterioId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int PorcentajeCumplimiento { get; set; }
    public decimal PesoMaximo { get; set; }
    public decimal PuntajeObtenido { get; set; }
    public bool Obligatorio { get; set; }
    public int PorcentajeMinimoObligatorio { get; set; }
    public bool CumplimientoObligatorioAlcanzado { get; set; }
    public string Evidencia { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
}

public sealed class IntentoHistorialResponse
{
    public int Id { get; set; }
    public int NumeroIntento { get; set; }
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal? Puntaje { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public bool PuedeReintentar { get; set; }
    public bool EsCorreccionProvisoria { get; set; }
    public string MensajeEstado { get; set; } = string.Empty;
}

public sealed class PistaResponse
{
    public int Id { get; set; }
    public int Nivel { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public decimal PenalizacionPuntaje { get; set; }
}

public sealed class ProgresoUsuarioResponse
{
    public int TotalEjercicios { get; set; }
    public int Aprobados { get; set; }
    public int EnProceso { get; set; }
    public decimal PorcentajeGeneral { get; set; }
    public List<ProgresoEjercicioResponse> Ejercicios { get; set; } = [];
}

public sealed class ProgresoEjercicioResponse
{
    public int EjercicioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int CantidadIntentos { get; set; }
    public decimal MejorPuntaje { get; set; }
    public DateTime? UltimaActividad { get; set; }
}

public sealed class AltaTemaRequest
{
    [Required] public string Nombre { get; set; } = string.Empty;
    [Required] public string Descripcion { get; set; } = string.Empty;
    [Range(1, 999)] public int Orden { get; set; }
    [Range(1, int.MaxValue)] public int CursoId { get; set; }
    public int? TemaPrerequisitoId { get; set; }
}

public sealed class AltaEjercicioRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Consigna { get; set; } = string.Empty;
    public int Tipo { get; set; } = 1;
    public int Dificultad { get; set; }
    public int PuntajeMaximo { get; set; } = 100;
    public int PuntajeMinimoAprobacion { get; set; } = 70;
    public int Orden { get; set; }
    public string Lenguaje { get; set; } = "JavaScript";
    public string CodigoInicial { get; set; } = string.Empty;
    public int TemaId { get; set; }
    public int? EjercicioPrerequisitoId { get; set; }
    public bool PermitePistas { get; set; } = true;
    public bool PermiteVerSolucion { get; set; }
    public int CantidadIntentosRecomendada { get; set; } = 4;
    public string VersionRubrica { get; set; } = "rubrica-2.0";
    public List<AltaCriterioRequest> Criterios { get; set; } = [];
    public List<AltaPistaRequest> Pistas { get; set; } = [];
    public List<object> ConceptosPermitidos { get; set; } = [];
    public List<object> ConceptosNoPermitidos { get; set; } = [];
    public List<object> CasosPrueba { get; set; } = [];
}

public sealed class AltaCriterioRequest
{
    public string Descripcion { get; set; } = string.Empty;
    public int Tipo { get; set; }
    public decimal Peso { get; set; }
    public bool Obligatorio { get; set; }
    public int PorcentajeMinimoObligatorio { get; set; } = 60;
    public int Orden { get; set; }
}

public sealed class AltaPistaRequest
{
    public int Nivel { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public int IntentosMinimosRequeridos { get; set; }
    public decimal PenalizacionPuntaje { get; set; }
}

// ======================================================
// TUTOR IA · CONTRATOS WEB MVC 2.8
// ======================================================

public sealed class ConsultaTutorRequest
{
    public string Consulta { get; set; } = string.Empty;
    public List<MensajeTutorRequest> Historial { get; set; } = [];
}

public sealed class MensajeTutorRequest
{
    public string Rol { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
}

public sealed class RespuestaTutorResponse
{
    public bool Aceptada { get; set; }
    public string Respuesta { get; set; } = string.Empty;
    public string MotivoRechazo { get; set; } = string.Empty;
    public int ConsultasRestantes { get; set; }
    public int LimiteDiario { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public bool UsoProveedorIA { get; set; }
    public bool EsFallbackLocal { get; set; }
    public bool RequiereMasContexto { get; set; }
    public string CodigoResultado { get; set; } = string.Empty;
    public string Clasificacion { get; set; } = string.Empty;
    public List<string> TemasDetectados { get; set; } = [];
    public string Advertencia { get; set; } = string.Empty;
    public List<string> Sugerencias { get; set; } = [];
}

public sealed class EstadoTutorResponse
{
    public bool Habilitado { get; set; }
    public int LimiteDiario { get; set; }
    public int ConsultasUtilizadas { get; set; }
    public int ConsultasRestantes { get; set; }
    public int MaximoCaracteresConsulta { get; set; } = 800;
    public List<string> TemasHabilitados { get; set; } = [];
}



public sealed class ResumenGamificacionResponse
{
    public bool VisibleEnRanking { get; set; }
    public int? PosicionRanking { get; set; }
    public decimal PuntajeRanking { get; set; }
    public decimal PuntajeMaximoDisponible { get; set; }
    public decimal PromedioMejoresPuntajes { get; set; }
    public int EjerciciosIntentados { get; set; }
    public int EjerciciosAprobados { get; set; }
    public RachaResolucionResponse RachaActual { get; set; } = new();
    public RachaResolucionResponse MejorRacha { get; set; } = new();
    public List<LogroUsuarioResponse> Insignias { get; set; } = [];
    public List<PosicionRankingResponse> Ranking { get; set; } = [];
}

public sealed class RachaResolucionResponse
{
    public int CantidadEjercicios { get; set; }
    public decimal Promedio { get; set; }
}

public sealed class PosicionRankingResponse
{
    public int Posicion { get; set; }
    public string NombreVisible { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string AvatarClave { get; set; } = "codigo";
    public string FotoPerfilRuta { get; set; } = string.Empty;
    public decimal PuntajeTotal { get; set; }
    public decimal Promedio { get; set; }
    public int EjerciciosAprobados { get; set; }
    public int EjerciciosIntentados { get; set; }
    public bool EsUsuarioActual { get; set; }
}

public sealed class LogroUsuarioResponse
{
    public int LogroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int ValorObjetivo { get; set; }
    public int ValorActual { get; set; }
    public int PorcentajeProgreso { get; set; }
    public bool Obtenido { get; set; }
    public DateTime? FechaObtencion { get; set; }
}

public sealed class LogroDesbloqueadoResponse
{
    public int LogroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
}


public sealed class CambiarVisibilidadRankingRequest
{
    public bool Mostrar { get; set; }
}


// ======================================================
// PERFIL Y OPINIONES · CONTRATOS WEB MVC 2.9.6
// ======================================================

public sealed class PerfilUsuarioResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string AvatarClave { get; set; } = "codigo";
    public string FotoPerfilRuta { get; set; } = string.Empty;
    public int MaximoCambiosNombreUsuario { get; set; }
    public int DiasVentanaCambiosNombreUsuario { get; set; }
    public int CambiosNombreUsuarioUsados { get; set; }
    public int CambiosNombreUsuarioDisponibles { get; set; }
    public bool PuedeCambiarNombreUsuario { get; set; }
    public DateTime? ProximoCambioNombreUsuarioDisponible { get; set; }
    public List<AvatarDisponibleResponse> AvataresDisponibles { get; set; } = [];
}

public sealed class AvatarDisponibleResponse
{
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
}

public sealed class ActualizarPerfilRequest
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string AvatarClave { get; set; } = "codigo";
    public string? NuevaFotoPerfilRuta { get; set; }
    public bool EliminarFotoPerfil { get; set; }
}

public sealed class CrearOpinionRequest
{
    public int Categoria { get; set; }
    public int Calificacion { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public bool AutorizaPublicacion { get; set; }
}

public sealed class OpinionUsuarioResponse
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreVisible { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string AvatarClave { get; set; } = "codigo";
    public string FotoPerfilRuta { get; set; } = string.Empty;
    public int Categoria { get; set; }
    public string CategoriaTexto { get; set; } = string.Empty;
    public int Calificacion { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public bool AutorizaPublicacion { get; set; }
    public int Estado { get; set; }
    public string EstadoTexto { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaRevision { get; set; }
}

public sealed class ActualizarEstadoOpinionRequest
{
    public int Estado { get; set; }
}

public sealed class RegistroDocenteRequest
{
    [Required, Display(Name = "Código de invitación")]
    public string CodigoInvitacion { get; set; } = string.Empty;
    [Required, StringLength(80)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(80)] public string Apellido { get; set; } = string.Empty;
    [Required, StringLength(50), Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;
    [Required, EmailAddress, Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;
    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden."), Display(Name = "Confirmar contraseña")]
    public string ConfirmacionPassword { get; set; } = string.Empty;
    [StringLength(1200), Display(Name = "Presentación profesional")]
    public string Biografia { get; set; } = string.Empty;
}

public sealed class CrearInvitacionDocenteRequest
{
    [Required, StringLength(150)] public string Especialidad { get; set; } = "Programación 1";
    [EmailAddress, Display(Name = "Correo permitido (opcional)")]
    public string? EmailPermitido { get; set; }
    [Range(1, 90), Display(Name = "Días de vigencia")]
    public int DiasVigencia { get; set; } = 15;
}

public sealed class InvitacionDocenteResponse
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public string EmailPermitido { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public string UtilizadaPor { get; set; } = string.Empty;
}

public sealed class CrearPropuestaEjercicioRequest
{
    public int PropuestaId { get; set; }
    [Range(1, int.MaxValue)] public int TemaId { get; set; }
    [Required, StringLength(150)] public string Titulo { get; set; } = string.Empty;
    [Required, StringLength(5000)] public string Consigna { get; set; } = string.Empty;
    [Range(1, 5)] public int Dificultad { get; set; } = 2;
    [Required] public string CodigoInicial { get; set; } = "function resolver() {\n    // Tu propuesta\n}";
    [Required, StringLength(1000), Display(Name = "Qué debería lograr la solución")]
    public string CriterioPrincipal { get; set; } = string.Empty;
    [StringLength(1200), Display(Name = "Pista orientativa")]
    public string OrientacionPista { get; set; } = string.Empty;
    [StringLength(1600), Display(Name = "Por qué sería útil para el alumno")]
    public string FundamentoPedagogico { get; set; } = string.Empty;
}

public sealed class RevisarPropuestaEjercicioRequest
{
    public string Decision { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
}

public sealed class PropuestaEjercicioResponse
{
    public int Id { get; set; }
    public int TemaId { get; set; }
    public string Tema { get; set; } = string.Empty;
    public int DocenteId { get; set; }
    public string Docente { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Consigna { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public string CodigoInicial { get; set; } = string.Empty;
    public string CriterioPrincipal { get; set; } = string.Empty;
    public string OrientacionPista { get; set; } = string.Empty;
    public string FundamentoPedagogico { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaRevision { get; set; }
    public string ObservacionRevision { get; set; } = string.Empty;
    public int? EjercicioPublicadoId { get; set; }
}

public sealed class PanelDocenteResponse
{
    public int TotalGrupos { get; set; }
    public int TotalAlumnos { get; set; }
    public int AlumnosActivosUltimos7Dias { get; set; }
    public int AlumnosActivosUltimos30Dias { get; set; }
    public int IntentosUltimos30Dias { get; set; }
    public decimal TasaAprobacionUltimos30Dias { get; set; }
    public decimal PuntajePromedioUltimos30Dias { get; set; }
    public string LecturaGeneral { get; set; } = string.Empty;
    public List<GrupoObservacionResponse> Grupos { get; set; } = [];
    public List<TemaObservacionResponse> Temas { get; set; } = [];
    public List<AlumnoActividadResponse> ActividadAlumnos { get; set; } = [];
}

public sealed class GrupoObservacionResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Curso { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public int Alumnos { get; set; }
    public int ActivosUltimos7Dias { get; set; }
    public int IntentosUltimos30Dias { get; set; }
    public decimal TasaAprobacion { get; set; }
}

public sealed class TemaObservacionResponse
{
    public int TemaId { get; set; }
    public string Tema { get; set; } = string.Empty;
    public int Intentos { get; set; }
    public int AlumnosQuePracticaron { get; set; }
    public decimal TasaAprobacion { get; set; }
    public decimal PuntajePromedio { get; set; }
    public string Senal { get; set; } = string.Empty;
}

public sealed class AlumnoActividadResponse
{
    public int AlumnoId { get; set; }
    public string Alumno { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Grupo { get; set; } = string.Empty;
    public DateTime? UltimaActividad { get; set; }
    public int IntentosUltimos30Dias { get; set; }
    public int EjerciciosAprobados { get; set; }
    public string Ritmo { get; set; } = string.Empty;
}

public sealed class PanelAdministracionResponse
{
    public int TotalUsuarios { get; set; }
    public int TotalAlumnos { get; set; }
    public int TotalDocentes { get; set; }
    public int TotalGrupos { get; set; }
    public int UsuariosActivosUltimos30Dias { get; set; }
    public int IntentosUltimos30Dias { get; set; }
    public decimal TasaAprobacionUltimos30Dias { get; set; }
    public decimal PuntajePromedioUltimos30Dias { get; set; }
    public int SolicitudesIAUltimos30Dias { get; set; }
    public int FallosIAUltimos30Dias { get; set; }
    public int PropuestasPendientes { get; set; }
    public int InvitacionesDisponibles { get; set; }
    public List<TemaObservacionResponse> Temas { get; set; } = [];
    public List<AuditoriaResumenResponse> AuditoriaReciente { get; set; } = [];
}

public sealed class AuditoriaResumenResponse
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

public sealed class EstructuraAcademicaResponse
{
    public List<UsuarioAcademicoResponse> Docentes { get; set; } = [];
    public List<UsuarioAcademicoResponse> Alumnos { get; set; } = [];
    public List<CursoAcademicoResponse> Cursos { get; set; } = [];
    public List<PeriodoAcademicoResponse> Periodos { get; set; } = [];
    public List<GrupoAcademicoResponse> Grupos { get; set; } = [];
}

public sealed class UsuarioAcademicoResponse
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
}

public sealed class CursoAcademicoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public sealed class PeriodoAcademicoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public sealed class GrupoAcademicoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CursoId { get; set; }
    public string Curso { get; set; } = string.Empty;
    public int PeriodoId { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public int? DocenteId { get; set; }
    public string Docente { get; set; } = string.Empty;
    public int CantidadAlumnos { get; set; }
}

public sealed class CrearGrupoRequest
{
    [Required, StringLength(80)] public string Nombre { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int CursoId { get; set; }
    [Range(1, int.MaxValue)] public int PeriodoAcademicoId { get; set; }
    [Range(1, int.MaxValue)] public int DocenteId { get; set; }
}

public sealed class InscribirAlumnoRequest
{
    [Range(1, int.MaxValue)] public int AlumnoId { get; set; }
}

public sealed class EstadoSistemaResponse
{
    public string Servicio { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaUtc { get; set; }
    public string Version { get; set; } = string.Empty;
    public ComponentesEstadoResponse Componentes { get; set; } = new();
}

public sealed class ComponentesEstadoResponse
{
    public ComponenteEstadoResponse Api { get; set; } = new();
    public ComponenteEstadoResponse BaseDatos { get; set; } = new();
}

public sealed class ComponenteEstadoResponse
{
    public string Estado { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
}
