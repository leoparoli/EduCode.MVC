using EduCode.WebMVC.Filters;
using EduCode.WebMVC.Models;
using EduCode.WebMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebMVC.Controllers;

[SessionRole("Alumno")]
public sealed class AlumnoController : Controller
{
    private readonly IEduCodeApiClient _api;
    public AlumnoController(IEduCodeApiClient api) => _api = api;

    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var cursos = await _api.ObtenerCursosAsync(ct);
        if (!cursos.Exitoso || cursos.Datos is null || cursos.Datos.Count == 0)
            return VistaError(cursos.Error.Length > 0 ? cursos.Error : "No hay cursos disponibles.");

        CursoResponse curso = cursos.Datos[0];
        var progreso = await _api.ObtenerProgresoAsync(ct);
        var temas = await _api.ObtenerTemasAsync(curso.Id, ct);
        var gamificacion = await _api.ObtenerGamificacionAsync(ct);
        if (!progreso.Exitoso) return VistaError(progreso.Error);
        if (!temas.Exitoso) return VistaError(temas.Error);

        return View(new DashboardViewModel
        {
            Curso = curso,
            Progreso = progreso.Datos ?? new ProgresoUsuarioResponse(),
            Temas = temas.Datos ?? [],
            Gamificacion = gamificacion.Exitoso
                ? gamificacion.Datos ?? new ResumenGamificacionResponse()
                : new ResumenGamificacionResponse(),
            NombreCompleto = HttpContext.Session.GetString("NombreCompleto") ?? "Estudiante"
        });
    }

    private IActionResult VistaError(string mensaje)
    {
        TempData["Error"] = mensaje;
        return RedirectToAction("Index", "Home");
    }
}
