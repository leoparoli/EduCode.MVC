using EduCode.WebMVC.Filters;
using EduCode.WebMVC.Models;
using EduCode.WebMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebMVC.Controllers;

[SessionRole("Alumno")]
public sealed class ComunidadController : Controller
{
    private readonly IEduCodeApiClient _api;

    public ComunidadController(IEduCodeApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ApiResult<ResumenGamificacionResponse> resultado =
            await _api.ObtenerGamificacionAsync(ct);

        if (!resultado.Exitoso || resultado.Datos is null)
        {
            TempData["Error"] = resultado.Error.Length > 0
                ? resultado.Error
                : "No fue posible obtener el ranking y las insignias.";
            return RedirectToAction("Dashboard", "Alumno");
        }

        return View(new ComunidadViewModel
        {
            Resumen = resultado.Datos
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarVisibilidad(
        bool mostrar,
        CancellationToken ct)
    {
        ApiResult<object> resultado =
            await _api.CambiarVisibilidadRankingAsync(mostrar, ct);

        TempData[resultado.Exitoso ? "Success" : "Error"] = resultado.Exitoso
            ? mostrar
                ? "Tu perfil vuelve a participar del ranking."
                : "Tu perfil quedó oculto del ranking. Tus notas y logros se conservan."
            : resultado.Error;

        return RedirectToAction(nameof(Index));
    }
}
