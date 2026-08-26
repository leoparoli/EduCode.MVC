using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EduCode.WebMVC.Models;

public sealed class DashboardViewModel
{
    public CursoResponse? Curso { get; set; }
    public ProgresoUsuarioResponse Progreso { get; set; } = new();
    public List<TemaResponse> Temas { get; set; } = [];
    public ResumenGamificacionResponse Gamificacion { get; set; } = new();
    public string NombreCompleto { get; set; } = string.Empty;
    public ProgresoEjercicioResponse? UltimaActividad => Progreso.Ejercicios
        .Where(e => e.UltimaActividad.HasValue)
        .OrderByDescending(e => e.UltimaActividad)
        .FirstOrDefault();
}

public sealed class TemasViewModel
{
    public CursoResponse Curso { get; set; } = new();
    public List<CursoResponse> Cursos { get; set; } = [];
    public List<TemaResponse> Temas { get; set; } = [];
}

public sealed class TemaDetalleViewModel
{
    public int TemaId { get; set; }
    public string NombreTema { get; set; } = string.Empty;
    public string? DescripcionTema { get; set; }
    public List<EjercicioListadoResponse> Ejercicios { get; set; } = [];
}

public sealed class ResolverEjercicioViewModel
{
    public EjercicioDetalleResponse Ejercicio { get; set; } = new();
    [Required(ErrorMessage = "Escribí una solución antes de enviarla.")]
    public string Codigo { get; set; } = string.Empty;
    public int NivelPistaActual { get; set; }
    public PistaResponse? PistaActual { get; set; }
}

public sealed class ResultadoViewModel
{
    public int EjercicioId { get; set; }
    public int TemaId { get; set; }
    public string TemaNombre { get; set; } = string.Empty;
    public string TituloEjercicio { get; set; } = string.Empty;
    public int? SiguienteEjercicioId { get; set; }
    public string SiguienteEjercicioTitulo { get; set; } = string.Empty;
    public bool HaySiguienteEjercicioEnTema { get; set; }
    public ResultadoIntentoResponse Resultado { get; set; } = new();
}

public sealed class HistorialViewModel
{
    public int EjercicioId { get; set; }
    public string TituloEjercicio { get; set; } = string.Empty;
    public List<IntentoHistorialResponse> Intentos { get; set; } = [];
}

public sealed class AdminDashboardViewModel
{
    public List<CursoResponse> Cursos { get; set; } = [];
    public List<TemaResponse> Temas { get; set; } = [];
    public PanelAdministracionResponse Metricas { get; set; } = new();
    public EstadoSistemaResponse EstadoSistema { get; set; } = new();
    public int TotalEjercicios => Temas.Sum(t => t.CantidadEjercicios);
}

public sealed class AltaTemaViewModel
{
    public AltaTemaRequest Formulario { get; set; } = new();
    public List<CursoResponse> Cursos { get; set; } = [];
    public List<TemaResponse> Temas { get; set; } = [];
}

public sealed class AltaEjercicioViewModel
{
    [Required] public string Titulo { get; set; } = string.Empty;
    [Required] public string Consigna { get; set; } = string.Empty;
    [Range(1, 5)] public int Dificultad { get; set; } = 2;
    [Range(1, 100)] public int PuntajeMaximo { get; set; } = 100;
    [Range(0, 100)] public int PuntajeMinimoAprobacion { get; set; } = 70;
    [Range(1, 999)] public int Orden { get; set; } = 1;
    [Required] public string CodigoInicial { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int TemaId { get; set; }
    [Required, Display(Name = "Criterio principal")] public string CriterioPrincipal { get; set; } = string.Empty;
    [Required, Display(Name = "Criterio de calidad")] public string CriterioCalidad { get; set; } = "La solución es clara y utiliza los conceptos del nivel.";
    [Display(Name = "Pista 1 · orientación conceptual")] public string PistaNivel1 { get; set; } = string.Empty;
    [Display(Name = "Pista 2 · estructura sugerida")] public string PistaNivel2 { get; set; } = string.Empty;
    [Display(Name = "Pista 3 · revisión puntual")] public string PistaNivel3 { get; set; } = string.Empty;
    public List<TemaResponse> Temas { get; set; } = [];
}

public sealed class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

public sealed class TutorViewModel
{
    public EstadoTutorResponse Estado { get; set; } = new();
    public int UsuarioId { get; set; }
    public string NombreAlumno { get; set; } = string.Empty;
    public List<string> PreguntasSugeridas { get; set; } = [];
}

public sealed class ConsultaTutorWebRequest
{
    [Required(ErrorMessage = "Escribí una consulta antes de enviarla.")]
    [StringLength(800, ErrorMessage = "La consulta puede tener un máximo de 800 caracteres.")]
    public string Consulta { get; set; } = string.Empty;

    public string HistorialJson { get; set; } = "[]";
}



public sealed class ComunidadViewModel
{
    public ResumenGamificacionResponse Resumen { get; set; } = new();
}


public sealed class PerfilViewModel
{
    public PerfilUsuarioResponse Perfil { get; set; } = new();
    public EditarPerfilFormModel PerfilForm { get; set; } = new();
    public CrearOpinionFormModel OpinionForm { get; set; } = new();
    public List<OpinionUsuarioResponse> Opiniones { get; set; } = [];
}

public sealed class EditarPerfilFormModel
{
    [Required(ErrorMessage = "Elegí un nombre de usuario.")]
    [StringLength(50, MinimumLength = 3,
        ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    [RegularExpression(@"^[\p{L}\p{N}](?:[\p{L}\p{N}._-]*[\p{L}\p{N}])?$",
        ErrorMessage = "Usá letras, números, punto, guion o guion bajo; sin separadores al inicio o al final.")]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    public string AvatarClave { get; set; } = "codigo";

    [Display(Name = "Foto de perfil")]
    public IFormFile? FotoPerfil { get; set; }

    public bool EliminarFotoPerfil { get; set; }
}

public sealed class CrearOpinionFormModel
{
    [Range(1, 6, ErrorMessage = "Seleccioná una categoría.")]
    [Display(Name = "Tema de la opinión")]
    public int Categoria { get; set; } = 1;

    [Range(1, 5, ErrorMessage = "Elegí una calificación entre 1 y 5 estrellas.")]
    public int Calificacion { get; set; } = 5;

    [Required(ErrorMessage = "Contanos tu experiencia o sugerencia.")]
    [StringLength(1200, MinimumLength = 20,
        ErrorMessage = "La opinión debe tener entre 20 y 1200 caracteres.")]
    [Display(Name = "Tu opinión")]
    public string Comentario { get; set; } = string.Empty;

    [Display(Name = "Autorizo a mostrar esta reseña públicamente de forma asociada a mi perfil")]
    public bool AutorizaPublicacion { get; set; }
}

public sealed class AdminOpinionesViewModel
{
    public List<OpinionUsuarioResponse> Opiniones { get; set; } = [];
    public int Pendientes => Opiniones.Count(o => o.Estado == 1);
    public int Revisadas => Opiniones.Count(o => o.Estado == 2);
    public int Consideradas => Opiniones.Count(o => o.Estado == 3);
    public decimal Promedio => Opiniones.Count == 0
        ? 0m
        : Math.Round(Opiniones.Average(o => (decimal)o.Calificacion), 1);
}

public static class AvatarUi
{
    public static string Icono(string? clave) => (clave ?? string.Empty).ToLowerInvariant() switch
    {
        "terminal" => ">_",
        "cohete" => "🚀",
        "robot" => "🤖",
        "cerebro" => "🧠",
        "laptop" => "💻",
        "bug" => "🐞",
        "braces" => "{}",
        _ => "</>"
    };
}


public sealed class DocenteDashboardViewModel
{
    public PanelDocenteResponse Panel { get; set; } = new();
    public string NombreDocente { get; set; } = string.Empty;
}

public sealed class DocentePropuestasViewModel
{
    public List<PropuestaEjercicioResponse> Propuestas { get; set; } = [];
    public List<TemaResponse> Temas { get; set; } = [];
    public CrearPropuestaEjercicioRequest Formulario { get; set; } = new();
}

public sealed class AdminInvitacionesDocentesViewModel
{
    public List<InvitacionDocenteResponse> Invitaciones { get; set; } = [];
    public CrearInvitacionDocenteRequest Formulario { get; set; } = new();
    public int Disponibles => Invitaciones.Count(i => i.Estado == "Disponible");
}

public sealed class AdminPropuestasEjerciciosViewModel
{
    public List<PropuestaEjercicioResponse> Propuestas { get; set; } = [];
    public int Pendientes => Propuestas.Count(p => p.Estado is "Enviada" or "EnRevision");
}

public sealed class AdminEstructuraAcademicaViewModel
{
    public EstructuraAcademicaResponse Estructura { get; set; } = new();
    public CrearGrupoRequest NuevoGrupo { get; set; } = new();
    public int GrupoSeleccionadoId { get; set; }
    public InscribirAlumnoRequest NuevaInscripcion { get; set; } = new();
}
