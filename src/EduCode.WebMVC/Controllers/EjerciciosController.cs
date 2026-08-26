using EduCode.WebMVC.Filters;
using EduCode.WebMVC.Models;
using EduCode.WebMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebMVC.Controllers;

[SessionAuthorize]
public sealed class EjerciciosController : Controller
{
    private readonly IEduCodeApiClient _api;

    public EjerciciosController(IEduCodeApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(
        int id,
        int nivelPista = 0,
        CancellationToken ct = default)
    {
        ApiResult<EjercicioDetalleResponse> result =
            await _api.ObtenerEjercicioAsync(id, ct);

        if (!result.Exitoso || result.Datos is null)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction("Index", "Temas");
        }

        return View(new ResolverEjercicioViewModel
        {
            Ejercicio = result.Datos,
            Codigo = result.Datos.CodigoInicial,
            NivelPistaActual = nivelPista
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionRole("Alumno")]
    public async Task<IActionResult> Resolver(
        int ejercicioId,
        string codigo,
        int? tiempoResolucionSegundos,
        CancellationToken ct)
    {
        ApiResult<EjercicioDetalleResponse> ejercicio =
            await _api.ObtenerEjercicioAsync(ejercicioId, ct);

        if (!ejercicio.Exitoso || ejercicio.Datos is null)
        {
            TempData["Error"] = ejercicio.Error;
            return RedirectToAction(nameof(Detalle), new { id = ejercicioId });
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            ModelState.AddModelError(
                nameof(codigo),
                "Escribí una solución antes de enviarla.");

            return View("Detalle", new ResolverEjercicioViewModel
            {
                Ejercicio = ejercicio.Datos,
                Codigo = codigo
            });
        }

        ApiResult<ResultadoIntentoResponse> result =
            await _api.EnviarIntentoAsync(
                ejercicioId,
                new EnviarIntentoRequest
                {
                    EjercicioId = ejercicioId,
                    Codigo = codigo,
                    TiempoResolucionSegundos = tiempoResolucionSegundos
                },
                ct);

        if (!result.Exitoso || result.Datos is null)
        {
            ModelState.AddModelError(string.Empty, result.Error);

            return View("Detalle", new ResolverEjercicioViewModel
            {
                Ejercicio = ejercicio.Datos,
                Codigo = codigo
            });
        }

        return MostrarResultado(
            ejercicio.Datos,
            result.Datos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionRole("Alumno")]
    public async Task<IActionResult> ReintentarCorreccion(
        int intentoId,
        int ejercicioId,
        CancellationToken ct)
    {
        ApiResult<EjercicioDetalleResponse> ejercicio =
            await _api.ObtenerEjercicioAsync(ejercicioId, ct);

        if (!ejercicio.Exitoso || ejercicio.Datos is null)
        {
            TempData["Error"] = ejercicio.Error;
            return RedirectToAction(nameof(Historial), new { id = ejercicioId });
        }

        ApiResult<ResultadoIntentoResponse> result =
            await _api.ReintentarCorreccionAsync(intentoId, ct);

        if (!result.Exitoso || result.Datos is null)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Historial), new { id = ejercicioId });
        }

        return MostrarResultado(
            ejercicio.Datos,
            result.Datos);
    }


    [HttpGet]
    [SessionRole("Alumno")]
    public async Task<IActionResult> EstadoFeedback(
        int intentoId,
        CancellationToken ct)
    {
        ApiResult<ResultadoIntentoResponse> result =
            await _api.ObtenerResultadoIntentoAsync(intentoId, ct);

        if (!result.Exitoso || result.Datos is null)
        {
            return Json(new
            {
                disponible = false,
                error = result.Error
            });
        }

        return Json(new
        {
            disponible = result.Datos.FeedbackPersonalizadoDisponible,
            pendiente = result.Datos.FeedbackPersonalizadoPendiente,
            proveedor = result.Datos.ProveedorFeedback
        });
    }

    [HttpGet]
    [SessionRole("Alumno")]
    public async Task<IActionResult> Pista(
        int id,
        int nivelActual,
        CancellationToken ct)
    {
        ApiResult<EjercicioDetalleResponse> ejercicio =
            await _api.ObtenerEjercicioAsync(id, ct);
        ApiResult<PistaResponse> pista =
            await _api.ObtenerPistaAsync(id, nivelActual, ct);

        if (!ejercicio.Exitoso || ejercicio.Datos is null)
        {
            TempData["Error"] = ejercicio.Error;
            return RedirectToAction("Index", "Temas");
        }

        if (!pista.Exitoso || pista.Datos is null)
        {
            TempData["Info"] = pista.Error;
            return RedirectToAction(
                nameof(Detalle),
                new { id, nivelPista = nivelActual });
        }

        return View("Detalle", new ResolverEjercicioViewModel
        {
            Ejercicio = ejercicio.Datos,
            Codigo = ejercicio.Datos.CodigoInicial,
            NivelPistaActual = pista.Datos.Nivel,
            PistaActual = pista.Datos
        });
    }

    [HttpGet]
    [SessionRole("Alumno")]
    public async Task<IActionResult> Historial(
        int id,
        CancellationToken ct)
    {
        ApiResult<EjercicioDetalleResponse> ejercicio =
            await _api.ObtenerEjercicioAsync(id, ct);
        ApiResult<List<IntentoHistorialResponse>> historial =
            await _api.ObtenerHistorialAsync(id, ct);

        if (!historial.Exitoso)
        {
            TempData["Error"] = historial.Error;
            return RedirectToAction(nameof(Detalle), new { id });
        }

        return View(new HistorialViewModel
        {
            EjercicioId = id,
            TituloEjercicio = ejercicio.Datos?.Titulo ?? "Ejercicio",
            Intentos = historial.Datos ?? []
        });
    }

    private ViewResult MostrarResultado(
        EjercicioDetalleResponse ejercicio,
        ResultadoIntentoResponse resultado) =>
        View("Resultado", new ResultadoViewModel
        {
            EjercicioId = ejercicio.Id,
            TemaId = ejercicio.TemaId,
            TemaNombre = ejercicio.TemaNombre,
            TituloEjercicio = ejercicio.Titulo,
            SiguienteEjercicioId = ejercicio.SiguienteEjercicioId,
            SiguienteEjercicioTitulo = ejercicio.SiguienteEjercicioTitulo,
            HaySiguienteEjercicioEnTema = ejercicio.HaySiguienteEjercicioEnTema,
            Resultado = resultado
        });
}
