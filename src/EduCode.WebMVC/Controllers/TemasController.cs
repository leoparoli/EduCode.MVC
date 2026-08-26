using EduCode.WebMVC.Filters;
using EduCode.WebMVC.Models;
using EduCode.WebMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebMVC.Controllers;

[SessionAuthorize]
public sealed class TemasController : Controller
{
    private readonly IEduCodeApiClient _api;
    public TemasController(IEduCodeApiClient api) => _api = api;

    public async Task<IActionResult> Index(int? cursoId, CancellationToken ct)
    {
        var cursosResult = await _api.ObtenerCursosAsync(ct);
        if (!cursosResult.Exitoso || cursosResult.Datos is null || cursosResult.Datos.Count == 0)
        {
            TempData["Error"] = cursosResult.Error;
            return RedirigirAlPanel();
        }
        var curso = cursosResult.Datos.FirstOrDefault(c => c.Id == cursoId) ?? cursosResult.Datos[0];
        var temasResult = await _api.ObtenerTemasAsync(curso.Id, ct);
        if (!temasResult.Exitoso)
        {
            TempData["Error"] = temasResult.Error;
            return RedirigirAlPanel();
        }
        return View(new TemasViewModel { Curso = curso, Cursos = cursosResult.Datos, Temas = temasResult.Datos ?? [] });
    }

    public async Task<IActionResult> Detalle(int id, string? nombre, string? descripcion, CancellationToken ct)
    {
        var result = await _api.ObtenerEjerciciosAsync(id, ct);
        if (!result.Exitoso)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(new TemaDetalleViewModel
        {
            TemaId = id,
            NombreTema = string.IsNullOrWhiteSpace(nombre) ? "Tema" : nombre,
            DescripcionTema = descripcion,
            Ejercicios = result.Datos ?? []
        });
    }
    private IActionResult RedirigirAlPanel()
    {
        string rol = HttpContext.Session.GetString("Rol") ?? string.Empty;
        if (rol.Equals("Docente", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "Docente");
        if (rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "Administracion");
        return RedirectToAction("Dashboard", "Alumno");
    }

}
