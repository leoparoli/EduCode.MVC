using EduCode.WebMVC.Models;
using EduCode.WebMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebMVC.Controllers;

public sealed class AuthController : Controller
{
    private readonly IEduCodeApiClient _api;
    public AuthController(IEduCodeApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginRequest());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model, string? returnUrl, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _api.LoginAsync(model, ct);
        if (!result.Exitoso || result.Datos is null)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }
        GuardarSesion(result.Datos);
        TempData["Success"] = $"Bienvenido/a, {result.Datos.NombreCompleto}.";
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        return result.Datos.Rol switch
        {
            "Administrador" => RedirectToAction("Index", "Administracion"),
            "Docente" => RedirectToAction("Index", "Docente"),
            _ => RedirectToAction("Dashboard", "Alumno")
        };
    }

    [HttpGet]
    public IActionResult Registro() => View(new RegistroRequest());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroRequest model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _api.RegistrarAsync(model, ct);
        if (!result.Exitoso || result.Datos is null)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }
        GuardarSesion(result.Datos);
        TempData["Success"] = "Tu cuenta fue creada correctamente. Ya podés comenzar el recorrido.";
        return RedirectToAction("Dashboard", "Alumno");
    }

    [HttpGet]
    public IActionResult RegistroDocente(string? codigo = null) =>
        View(new RegistroDocenteRequest { CodigoInvitacion = codigo ?? string.Empty });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistroDocente(
        RegistroDocenteRequest model,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _api.RegistrarDocenteAsync(model, ct);
        if (!result.Exitoso || result.Datos is null)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }
        GuardarSesion(result.Datos);
        TempData["Success"] = "Tu acceso docente fue activado. Ya podés observar la práctica de tus grupos.";
        return RedirectToAction("Index", "Docente");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AccesoDemoAlumno(CancellationToken ct)
    {
        var result = await _api.LoginAsync(new LoginRequest
        {
            EmailONombreUsuario = "alumno@educode.local",
            Password = "Alumno123!"
        }, ct);
        if (!result.Exitoso || result.Datos is null)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Login));
        }
        GuardarSesion(result.Datos);
        return RedirectToAction("Dashboard", "Alumno");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AccesoDemoDocente(CancellationToken ct)
    {
        var result = await _api.LoginAsync(new LoginRequest
        {
            EmailONombreUsuario = "docente@educode.local",
            Password = "Docente123!"
        }, ct);
        if (!result.Exitoso || result.Datos is null)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Login));
        }
        GuardarSesion(result.Datos);
        return RedirectToAction("Index", "Docente");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "La sesión se cerró correctamente.";
        return RedirectToAction("Index", "Home");
    }

    private void GuardarSesion(UsuarioAutenticadoResponse usuario)
    {
        HttpContext.Session.SetString("AccessToken", usuario.AccessToken);
        HttpContext.Session.SetString("RefreshToken", usuario.RefreshToken);
        HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto);
        HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
        HttpContext.Session.SetString("Email", usuario.Email);
        HttpContext.Session.SetString("Rol", usuario.Rol);
        HttpContext.Session.SetString("AvatarClave", usuario.AvatarClave);
        HttpContext.Session.SetString("FotoPerfilRuta", usuario.FotoPerfilRuta ?? string.Empty);
        HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
    }
}
