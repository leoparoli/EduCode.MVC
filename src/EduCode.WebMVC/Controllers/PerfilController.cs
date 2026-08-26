using EduCode.WebMVC.Filters;
using EduCode.WebMVC.Models;
using EduCode.WebMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebMVC.Controllers;

[SessionAuthorize]
public sealed class PerfilController : Controller
{
    private const long MaximoFotoBytes = 2 * 1024 * 1024;
    private readonly IEduCodeApiClient _api;
    private readonly IWebHostEnvironment _entorno;
    private readonly ILogger<PerfilController> _logger;

    public PerfilController(
        IEduCodeApiClient api,
        IWebHostEnvironment entorno,
        ILogger<PerfilController> logger)
    {
        _api = api;
        _entorno = entorno;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ApiResult<PerfilUsuarioResponse> perfil = await _api.ObtenerPerfilAsync(ct);
        if (!perfil.Exitoso || perfil.Datos is null)
        {
            TempData["Error"] = perfil.Error.Length > 0
                ? perfil.Error
                : "No pudimos cargar tu cuenta.";
            return RedirigirAlPanel();
        }

        List<OpinionUsuarioResponse> opiniones = [];
        if (!EsAdministrador())
        {
            ApiResult<List<OpinionUsuarioResponse>> resultadoOpiniones =
                await _api.ObtenerMisOpinionesAsync(ct);
            if (!resultadoOpiniones.Exitoso)
            {
                TempData["Info"] =
                    "Tu cuenta se cargó, pero no pudimos recuperar las opiniones anteriores.";
            }
            else
            {
                opiniones = resultadoOpiniones.Datos ?? [];
            }
        }

        return View(ConstruirModelo(perfil.Datos, opiniones));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaximoFotoBytes + 256 * 1024)]
    public async Task<IActionResult> Actualizar(
        [Bind(Prefix = "PerfilForm")] EditarPerfilFormModel formulario,
        CancellationToken ct)
    {
        string? nuevaRuta = null;
        string rutaAnterior = HttpContext.Session.GetString("FotoPerfilRuta") ?? string.Empty;

        if (formulario.FotoPerfil is not null)
        {
            ResultadoValidacionFoto validacion =
                await ValidarFotoAsync(formulario.FotoPerfil, ct);
            if (!validacion.EsValida)
                ModelState.AddModelError("PerfilForm.FotoPerfil", validacion.Error);
        }

        if (!ModelState.IsValid)
            return await RecargarVistaAsync(formulario, new CrearOpinionFormModel(), ct);

        try
        {
            if (formulario.FotoPerfil is not null)
                nuevaRuta = await GuardarFotoAsync(
                    formulario.FotoPerfil,
                    ObtenerExtensionSegura(formulario.FotoPerfil),
                    ct);

            bool eliminarFoto = formulario.FotoPerfil is null && formulario.EliminarFotoPerfil;
            var request = new ActualizarPerfilRequest
            {
                NombreUsuario = formulario.NombreUsuario,
                AvatarClave = formulario.AvatarClave,
                NuevaFotoPerfilRuta = nuevaRuta,
                EliminarFotoPerfil = eliminarFoto
            };

            ApiResult<PerfilUsuarioResponse> resultado =
                await _api.ActualizarPerfilAsync(request, ct);

            if (!resultado.Exitoso || resultado.Datos is null)
            {
                EliminarFotoLocalSegura(nuevaRuta);
                ModelState.AddModelError(string.Empty, resultado.Error);
                return await RecargarVistaAsync(formulario, new CrearOpinionFormModel(), ct);
            }

            bool fotoReemplazada = !string.IsNullOrWhiteSpace(nuevaRuta);
            if ((fotoReemplazada || eliminarFoto) &&
                !string.Equals(rutaAnterior, nuevaRuta, StringComparison.Ordinal))
            {
                EliminarFotoLocalSegura(rutaAnterior);
            }

            ActualizarSesion(resultado.Datos);
            TempData["Success"] = "Tu cuenta fue actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (IOException ex)
        {
            EliminarFotoLocalSegura(nuevaRuta);
            _logger.LogError(ex, "No fue posible guardar la foto de perfil del usuario {UsuarioId}.",
                HttpContext.Session.GetInt32("UsuarioId"));
            ModelState.AddModelError(
                "PerfilForm.FotoPerfil",
                "No pudimos guardar la foto. Probá con otra imagen.");
            return await RecargarVistaAsync(formulario, new CrearOpinionFormModel(), ct);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionRole("Alumno", "Docente")]
    public async Task<IActionResult> EnviarOpinion(
        [Bind(Prefix = "OpinionForm")] CrearOpinionFormModel formulario,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return await RecargarVistaAsync(new EditarPerfilFormModel(), formulario, ct, conservarPerfilApi: true);

        ApiResult<OpinionUsuarioResponse> resultado = await _api.CrearOpinionAsync(
            new CrearOpinionRequest
            {
                Categoria = formulario.Categoria,
                Calificacion = formulario.Calificacion,
                Comentario = formulario.Comentario,
                AutorizaPublicacion = formulario.AutorizaPublicacion
            },
            ct);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.Error);
            return await RecargarVistaAsync(new EditarPerfilFormModel(), formulario, ct, conservarPerfilApi: true);
        }

        TempData["Success"] = "Gracias por tu opinión. Quedó registrada para ser revisada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> RecargarVistaAsync(
        EditarPerfilFormModel perfilForm,
        CrearOpinionFormModel opinionForm,
        CancellationToken ct,
        bool conservarPerfilApi = false)
    {
        ApiResult<PerfilUsuarioResponse> perfil = await _api.ObtenerPerfilAsync(ct);
        List<OpinionUsuarioResponse> opiniones = [];
        if (!EsAdministrador())
        {
            ApiResult<List<OpinionUsuarioResponse>> resultadoOpiniones =
                await _api.ObtenerMisOpinionesAsync(ct);
            opiniones = resultadoOpiniones.Datos ?? [];
        }

        if (!perfil.Exitoso || perfil.Datos is null)
        {
            TempData["Error"] = perfil.Error.Length > 0
                ? perfil.Error
                : "No pudimos cargar tu cuenta.";
            return RedirigirAlPanel();
        }

        PerfilViewModel modelo = ConstruirModelo(perfil.Datos, opiniones);
        if (conservarPerfilApi)
        {
            perfilForm = modelo.PerfilForm;
        }
        else
        {
            perfilForm.AvatarClave = string.IsNullOrWhiteSpace(perfilForm.AvatarClave)
                ? perfil.Datos.AvatarClave
                : perfilForm.AvatarClave;
        }

        modelo.PerfilForm = perfilForm;
        modelo.OpinionForm = opinionForm;
        return View(nameof(Index), modelo);
    }

    private static PerfilViewModel ConstruirModelo(
        PerfilUsuarioResponse perfil,
        List<OpinionUsuarioResponse> opiniones) => new()
    {
        Perfil = perfil,
        PerfilForm = new EditarPerfilFormModel
        {
            NombreUsuario = perfil.NombreUsuario,
            AvatarClave = perfil.AvatarClave
        },
        OpinionForm = new CrearOpinionFormModel(),
        Opiniones = opiniones
    };

    private void ActualizarSesion(PerfilUsuarioResponse perfil)
    {
        HttpContext.Session.SetString("NombreCompleto", perfil.NombreCompleto);
        HttpContext.Session.SetString("NombreUsuario", perfil.NombreUsuario);
        HttpContext.Session.SetString("AvatarClave", perfil.AvatarClave);
        HttpContext.Session.SetString("FotoPerfilRuta", perfil.FotoPerfilRuta ?? string.Empty);
    }

    private async Task<ResultadoValidacionFoto> ValidarFotoAsync(
        IFormFile foto,
        CancellationToken ct)
    {
        if (foto.Length <= 0)
            return ResultadoValidacionFoto.Fallo("La imagen seleccionada está vacía.");
        if (foto.Length > MaximoFotoBytes)
            return ResultadoValidacionFoto.Fallo("La foto no puede superar 2 MB.");

        string tipo = foto.ContentType.Trim().ToLowerInvariant();
        if (tipo is not ("image/jpeg" or "image/png" or "image/webp"))
            return ResultadoValidacionFoto.Fallo("Usá una imagen JPG, PNG o WebP.");

        byte[] cabecera = new byte[12];
        await using Stream stream = foto.OpenReadStream();
        int leidos = await stream.ReadAsync(cabecera.AsMemory(0, cabecera.Length), ct);

        bool jpeg = leidos >= 3 && cabecera[0] == 0xFF && cabecera[1] == 0xD8 && cabecera[2] == 0xFF;
        bool png = leidos >= 8 &&
            cabecera[0] == 0x89 && cabecera[1] == 0x50 && cabecera[2] == 0x4E && cabecera[3] == 0x47 &&
            cabecera[4] == 0x0D && cabecera[5] == 0x0A && cabecera[6] == 0x1A && cabecera[7] == 0x0A;
        bool webp = leidos >= 12 &&
            cabecera[0] == 0x52 && cabecera[1] == 0x49 && cabecera[2] == 0x46 && cabecera[3] == 0x46 &&
            cabecera[8] == 0x57 && cabecera[9] == 0x45 && cabecera[10] == 0x42 && cabecera[11] == 0x50;

        bool coincide = tipo switch
        {
            "image/jpeg" => jpeg,
            "image/png" => png,
            "image/webp" => webp,
            _ => false
        };

        return coincide
            ? ResultadoValidacionFoto.Ok()
            : ResultadoValidacionFoto.Fallo("El contenido del archivo no coincide con una imagen válida.");
    }

    private async Task<string> GuardarFotoAsync(
        IFormFile foto,
        string extension,
        CancellationToken ct)
    {
        int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
        string carpeta = Path.Combine(_entorno.WebRootPath, "uploads", "perfiles");
        Directory.CreateDirectory(carpeta);

        string nombreArchivo = $"perfil-{usuarioId}-{Guid.NewGuid():N}{extension}";
        string rutaFisica = Path.Combine(carpeta, nombreArchivo);

        await using var destino = new FileStream(
            rutaFisica,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);
        await foto.CopyToAsync(destino, ct);

        return $"/uploads/perfiles/{nombreArchivo}";
    }

    private static string ObtenerExtensionSegura(IFormFile foto) =>
        foto.ContentType.Trim().ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };

    private void EliminarFotoLocalSegura(string? rutaPublica)
    {
        if (string.IsNullOrWhiteSpace(rutaPublica) ||
            !rutaPublica.StartsWith("/uploads/perfiles/", StringComparison.Ordinal) ||
            rutaPublica.Contains("..", StringComparison.Ordinal))
        {
            return;
        }

        string nombre = Path.GetFileName(rutaPublica);
        if (string.IsNullOrWhiteSpace(nombre)) return;

        string carpeta = Path.GetFullPath(
            Path.Combine(_entorno.WebRootPath, "uploads", "perfiles"));
        string archivo = Path.GetFullPath(Path.Combine(carpeta, nombre));
        if (!archivo.StartsWith(carpeta, StringComparison.OrdinalIgnoreCase)) return;

        try
        {
            if (System.IO.File.Exists(archivo))
                System.IO.File.Delete(archivo);
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar una foto anterior de perfil: {Ruta}", archivo);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar una foto anterior de perfil: {Ruta}", archivo);
        }
    }

    private bool EsAdministrador() =>
        string.Equals(
            HttpContext.Session.GetString("Rol"),
            "Administrador",
            StringComparison.OrdinalIgnoreCase);

    private IActionResult RedirigirAlPanel()
    {
        string rol = HttpContext.Session.GetString("Rol") ?? string.Empty;
        if (rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "Administracion");
        if (rol.Equals("Docente", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "Docente");
        return RedirectToAction("Dashboard", "Alumno");
    }

    private sealed record ResultadoValidacionFoto(bool EsValida, string Error)
    {
        public static ResultadoValidacionFoto Ok() => new(true, string.Empty);
        public static ResultadoValidacionFoto Fallo(string error) => new(false, error);
    }
}
