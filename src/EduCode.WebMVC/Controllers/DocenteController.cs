using System.Text;
using EduCode.WebMVC.Filters;
using EduCode.WebMVC.Models;
using EduCode.WebMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebMVC.Controllers;

[SessionRole("Docente")]
public sealed class DocenteController : Controller
{
    private readonly IEduCodeApiClient _api;
    public DocenteController(IEduCodeApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ApiResult<PanelDocenteResponse> resultado =
            await _api.ObtenerPanelDocenteAsync(ct);
        if (!resultado.Exitoso)
        {
            TempData["Error"] = resultado.Error;
        }

        return View(new DocenteDashboardViewModel
        {
            Panel = resultado.Datos ?? new PanelDocenteResponse(),
            NombreDocente = HttpContext.Session.GetString("NombreCompleto") ?? "Docente"
        });
    }

    [HttpGet]
    public async Task<IActionResult> Propuestas(int? editar, CancellationToken ct)
    {
        DocentePropuestasViewModel model = await CargarPropuestasAsync(
            new CrearPropuestaEjercicioRequest(),
            ct);
        if (editar.HasValue)
        {
            PropuestaEjercicioResponse? propuesta = model.Propuestas.FirstOrDefault(
                p => p.Id == editar.Value && p.Estado == "RequiereAjustes");
            if (propuesta is not null)
            {
                model.Formulario = new CrearPropuestaEjercicioRequest
                {
                    PropuestaId = propuesta.Id,
                    TemaId = propuesta.TemaId,
                    Titulo = propuesta.Titulo,
                    Consigna = propuesta.Consigna,
                    Dificultad = propuesta.Dificultad switch
                    {
                        "Inicial" => 1,
                        "Basico" => 2,
                        "Intermedio" => 3,
                        "Avanzado" => 4,
                        "Desafio" => 5,
                        _ => 2
                    },
                    CodigoInicial = propuesta.CodigoInicial,
                    CriterioPrincipal = propuesta.CriterioPrincipal,
                    OrientacionPista = propuesta.OrientacionPista,
                    FundamentoPedagogico = propuesta.FundamentoPedagogico
                };
            }
        }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Propuestas(
        DocentePropuestasViewModel model,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(await CargarPropuestasAsync(model.Formulario, ct));

        ApiResult<PropuestaEjercicioResponse> resultado = model.Formulario.PropuestaId > 0
            ? await _api.ActualizarPropuestaDocenteAsync(
                model.Formulario.PropuestaId,
                model.Formulario,
                ct)
            : await _api.CrearPropuestaDocenteAsync(model.Formulario, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.Error);
            return View(await CargarPropuestasAsync(model.Formulario, ct));
        }

        TempData["Success"] = model.Formulario.PropuestaId > 0
            ? "La propuesta fue ajustada y reenviada a revisión."
            : "La propuesta fue enviada. Administración podrá revisarla antes de publicarla.";
        return RedirectToAction(nameof(Propuestas));
    }

    [HttpGet]
    public async Task<IActionResult> ExportarActividad(CancellationToken ct)
    {
        ApiResult<PanelDocenteResponse> resultado = await _api.ObtenerPanelDocenteAsync(ct);
        if (!resultado.Exitoso || resultado.Datos is null)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        StringBuilder csv = new();
        csv.AppendLine("Alumno;Usuario;Grupo;Ultima actividad;Intentos 30 dias;Ejercicios aprobados;Ritmo");
        foreach (AlumnoActividadResponse alumno in resultado.Datos.ActividadAlumnos)
        {
            string[] columnas =
            [
                Escapar(alumno.Alumno),
                Escapar(alumno.NombreUsuario),
                Escapar(alumno.Grupo),
                alumno.UltimaActividad?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "Sin actividad",
                alumno.IntentosUltimos30Dias.ToString(),
                alumno.EjerciciosAprobados.ToString(),
                Escapar(alumno.Ritmo)
            ];
            csv.AppendLine(string.Join(";", columnas));
        }

        return File(
            Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(),
            "text/csv; charset=utf-8",
            $"educode-actividad-{DateTime.Now:yyyyMMdd}.csv");
    }

    private async Task<DocentePropuestasViewModel> CargarPropuestasAsync(
        CrearPropuestaEjercicioRequest formulario,
        CancellationToken ct)
    {
        ApiResult<List<PropuestaEjercicioResponse>> propuestas =
            await _api.ObtenerPropuestasDocenteAsync(ct);
        List<CursoResponse> cursos = (await _api.ObtenerCursosAsync(ct)).Datos ?? [];
        List<TemaResponse> temas = [];
        foreach (CursoResponse curso in cursos)
            temas.AddRange((await _api.ObtenerTemasAsync(curso.Id, ct)).Datos ?? []);

        if (formulario.TemaId == 0)
            formulario.TemaId = temas.FirstOrDefault()?.Id ?? 0;

        return new DocentePropuestasViewModel
        {
            Propuestas = propuestas.Datos ?? [],
            Temas = temas,
            Formulario = formulario
        };
    }

    private static string Escapar(object? valor)
    {
        string texto = valor?.ToString() ?? string.Empty;
        return texto.Contains(';') || texto.Contains('"') || texto.Contains('\n')
            ? "\"" + texto.Replace("\"", "\"\"") + "\""
            : texto;
    }
}
