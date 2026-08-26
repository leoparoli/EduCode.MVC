using EduCode.WebMVC.Filters;
using EduCode.WebMVC.Models;
using EduCode.WebMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebMVC.Controllers;

[SessionRole("Administrador")]
public sealed class AdministracionController : Controller
{
    private readonly IEduCodeApiClient _api;
    public AdministracionController(IEduCodeApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var cursos = await _api.ObtenerCursosAsync(ct);
        var datosCursos = cursos.Datos ?? [];
        var temas = datosCursos.Count == 0
            ? ApiResult<List<TemaResponse>>.Ok([])
            : await _api.ObtenerTemasAsync(datosCursos[0].Id, ct);
        var metricas = await _api.ObtenerPanelAdministracionAsync(ct);
        var estado = await _api.ObtenerEstadoSistemaAsync(ct);

        if (!cursos.Exitoso || !temas.Exitoso || !metricas.Exitoso)
        {
            TempData["Error"] = !metricas.Exitoso
                ? metricas.Error
                : !cursos.Exitoso ? cursos.Error : temas.Error;
        }

        return View(new AdminDashboardViewModel
        {
            Cursos = datosCursos,
            Temas = temas.Datos ?? [],
            Metricas = metricas.Datos ?? new PanelAdministracionResponse(),
            EstadoSistema = estado.Datos ?? new EstadoSistemaResponse { Estado = "Sin verificar" }
        });
    }

    [HttpGet]
    public async Task<IActionResult> AltaTema(CancellationToken ct)
    {
        var cursos = await _api.ObtenerCursosAsync(ct);
        var lista = cursos.Datos ?? [];
        var temas = lista.Count == 0 ? [] : (await _api.ObtenerTemasAsync(lista[0].Id, ct)).Datos ?? [];
        return View(new AltaTemaViewModel
        {
            Cursos = lista,
            Temas = temas,
            Formulario = new AltaTemaRequest { CursoId = lista.FirstOrDefault()?.Id ?? 0, Orden = temas.Count + 1 }
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AltaTema(AltaTemaViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await CargarOpcionesTema(model, ct);
            return View(model);
        }
        var result = await _api.CrearTemaAsync(model.Formulario, ct);
        if (!result.Exitoso)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            await CargarOpcionesTema(model, ct);
            return View(model);
        }
        TempData["Success"] = "El tema fue creado y ya está disponible en el recorrido.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> AltaEjercicio(CancellationToken ct)
    {
        var temas = await ObtenerTodosLosTemas(ct);
        return View(new AltaEjercicioViewModel
        {
            Temas = temas,
            TemaId = temas.FirstOrDefault()?.Id ?? 0,
            Orden = (temas.FirstOrDefault()?.CantidadEjercicios ?? 0) + 1,
            CodigoInicial = "function resolver() {\n    // Tu código\n}"
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AltaEjercicio(AltaEjercicioViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            model.Temas = await ObtenerTodosLosTemas(ct);
            return View(model);
        }
        var request = new AltaEjercicioRequest
        {
            Titulo = model.Titulo,
            Consigna = model.Consigna,
            Dificultad = model.Dificultad,
            PuntajeMaximo = model.PuntajeMaximo,
            PuntajeMinimoAprobacion = model.PuntajeMinimoAprobacion,
            Orden = model.Orden,
            CodigoInicial = model.CodigoInicial,
            TemaId = model.TemaId,
            VersionRubrica = $"admin-{DateTime.UtcNow:yyyyMMdd}-3.0",
            Criterios =
            [
                new AltaCriterioRequest
                {
                    Descripcion = model.CriterioPrincipal,
                    Tipo = 1,
                    Peso = 35,
                    Obligatorio = true,
                    PorcentajeMinimoObligatorio = 72,
                    Orden = 1
                },
                new AltaCriterioRequest
                {
                    Descripcion = "Aplica correctamente la técnica central requerida por la consigna.",
                    Tipo = 3,
                    Peso = 22,
                    Obligatorio = true,
                    PorcentajeMinimoObligatorio = 65,
                    Orden = 2
                },
                new AltaCriterioRequest
                {
                    Descripcion = "Contempla entradas habituales y casos borde sin resultados accidentales.",
                    Tipo = 6,
                    Peso = 18,
                    Obligatorio = false,
                    Orden = 3
                },
                new AltaCriterioRequest
                {
                    Descripcion = "La estructura, los acumuladores y el retorno son coherentes.",
                    Tipo = 2,
                    Peso = 14,
                    Obligatorio = false,
                    Orden = 4
                },
                new AltaCriterioRequest
                {
                    Descripcion = model.CriterioCalidad,
                    Tipo = 5,
                    Peso = 11,
                    Obligatorio = false,
                    Orden = 5
                }
            ]
        };
        if (!string.IsNullOrWhiteSpace(model.PistaNivel1))
            request.Pistas.Add(new AltaPistaRequest
            {
                Nivel = 1,
                Contenido = model.PistaNivel1,
                IntentosMinimosRequeridos = 1,
                PenalizacionPuntaje = 0
            });

        if (!string.IsNullOrWhiteSpace(model.PistaNivel2))
            request.Pistas.Add(new AltaPistaRequest
            {
                Nivel = 2,
                Contenido = model.PistaNivel2,
                IntentosMinimosRequeridos = 2,
                PenalizacionPuntaje = 0
            });

        if (!string.IsNullOrWhiteSpace(model.PistaNivel3))
            request.Pistas.Add(new AltaPistaRequest
            {
                Nivel = 3,
                Contenido = model.PistaNivel3,
                IntentosMinimosRequeridos = 3,
                PenalizacionPuntaje = 0
            });

        var result = await _api.CrearEjercicioAsync(request, ct);
        if (!result.Exitoso)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            model.Temas = await ObtenerTodosLosTemas(ct);
            return View(model);
        }
        TempData["Success"] = "El ejercicio fue creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Opiniones(CancellationToken ct)
    {
        ApiResult<List<OpinionUsuarioResponse>> resultado =
            await _api.ObtenerOpinionesAdministracionAsync(ct);

        if (!resultado.Exitoso)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        return View(new AdminOpinionesViewModel
        {
            Opiniones = resultado.Datos ?? []
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarEstadoOpinion(
        int opinionId,
        int estado,
        CancellationToken ct)
    {
        ApiResult<OpinionUsuarioResponse> resultado =
            await _api.ActualizarEstadoOpinionAsync(
                opinionId,
                new ActualizarEstadoOpinionRequest { Estado = estado },
                ct);

        TempData[resultado.Exitoso ? "Success" : "Error"] = resultado.Exitoso
            ? "El estado de la opinión fue actualizado."
            : resultado.Error;

        return RedirectToAction(nameof(Opiniones));
    }

    [HttpGet]
    public async Task<IActionResult> InvitacionesDocentes(CancellationToken ct)
    {
        ApiResult<List<InvitacionDocenteResponse>> resultado =
            await _api.ObtenerInvitacionesDocentesAsync(ct);
        if (!resultado.Exitoso) TempData["Error"] = resultado.Error;
        return View(new AdminInvitacionesDocentesViewModel
        {
            Invitaciones = resultado.Datos ?? [],
            Formulario = new CrearInvitacionDocenteRequest()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearInvitacionDocente(
        AdminInvitacionesDocentesViewModel model,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            model.Invitaciones = (await _api.ObtenerInvitacionesDocentesAsync(ct)).Datos ?? [];
            return View("InvitacionesDocentes", model);
        }

        ApiResult<InvitacionDocenteResponse> resultado =
            await _api.CrearInvitacionDocenteAsync(model.Formulario, ct);
        TempData[resultado.Exitoso ? "Success" : "Error"] = resultado.Exitoso
            ? $"Invitación creada: {resultado.Datos?.Codigo}"
            : resultado.Error;
        return RedirectToAction(nameof(InvitacionesDocentes));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RevocarInvitacionDocente(
        int invitacionId,
        CancellationToken ct)
    {
        ApiResult<object> resultado = await _api.RevocarInvitacionDocenteAsync(invitacionId, ct);
        TempData[resultado.Exitoso ? "Success" : "Error"] = resultado.Exitoso
            ? "La invitación docente fue revocada."
            : resultado.Error;
        return RedirectToAction(nameof(InvitacionesDocentes));
    }

    [HttpGet]
    public async Task<IActionResult> PropuestasEjercicios(CancellationToken ct)
    {
        ApiResult<List<PropuestaEjercicioResponse>> resultado =
            await _api.ObtenerPropuestasAdministracionAsync(ct);
        if (!resultado.Exitoso) TempData["Error"] = resultado.Error;
        return View(new AdminPropuestasEjerciciosViewModel
        {
            Propuestas = resultado.Datos ?? []
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RevisarPropuesta(
        int propuestaId,
        string decision,
        string observacion,
        CancellationToken ct)
    {
        ApiResult<PropuestaEjercicioResponse> resultado =
            await _api.RevisarPropuestaAsync(
                propuestaId,
                new RevisarPropuestaEjercicioRequest
                {
                    Decision = decision,
                    Observacion = observacion
                },
                ct);
        TempData[resultado.Exitoso ? "Success" : "Error"] = resultado.Exitoso
            ? decision.Equals("aprobar", StringComparison.OrdinalIgnoreCase)
                ? "La propuesta fue aprobada y publicada como ejercicio."
                : "La propuesta fue actualizada."
            : resultado.Error;
        return RedirectToAction(nameof(PropuestasEjercicios));
    }

    [HttpGet]
    public async Task<IActionResult> EstructuraAcademica(CancellationToken ct)
    {
        return View(await CargarEstructuraAsync(ct));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearGrupo(
        AdminEstructuraAcademicaViewModel model,
        CancellationToken ct)
    {
        ModelState.Clear();
        if (string.IsNullOrWhiteSpace(model.NuevoGrupo.Nombre) ||
            model.NuevoGrupo.CursoId <= 0 ||
            model.NuevoGrupo.PeriodoAcademicoId <= 0 ||
            model.NuevoGrupo.DocenteId <= 0)
        {
            TempData["Error"] = "Completá todos los datos del nuevo grupo.";
            return RedirectToAction(nameof(EstructuraAcademica));
        }

        ApiResult<object> resultado = await _api.CrearGrupoAsync(model.NuevoGrupo, ct);
        TempData[resultado.Exitoso ? "Success" : "Error"] = resultado.Exitoso
            ? "El grupo fue creado y quedó asociado al docente."
            : resultado.Error;
        return RedirectToAction(nameof(EstructuraAcademica));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> InscribirAlumno(
        AdminEstructuraAcademicaViewModel model,
        CancellationToken ct)
    {
        if (model.GrupoSeleccionadoId <= 0 || model.NuevaInscripcion.AlumnoId <= 0)
        {
            TempData["Error"] = "Seleccioná un grupo y un alumno.";
            return RedirectToAction(nameof(EstructuraAcademica));
        }

        ApiResult<object> resultado = await _api.InscribirAlumnoAsync(
            model.GrupoSeleccionadoId,
            model.NuevaInscripcion,
            ct);
        TempData[resultado.Exitoso ? "Success" : "Error"] = resultado.Exitoso
            ? "El alumno fue asociado al grupo."
            : resultado.Error;
        return RedirectToAction(nameof(EstructuraAcademica));
    }

    private async Task<AdminEstructuraAcademicaViewModel> CargarEstructuraAsync(
        CancellationToken ct)
    {
        ApiResult<EstructuraAcademicaResponse> resultado =
            await _api.ObtenerEstructuraAcademicaAsync(ct);
        if (!resultado.Exitoso) TempData["Error"] = resultado.Error;
        EstructuraAcademicaResponse estructura = resultado.Datos ?? new();
        return new AdminEstructuraAcademicaViewModel
        {
            Estructura = estructura,
            NuevoGrupo = new CrearGrupoRequest
            {
                CursoId = estructura.Cursos.FirstOrDefault()?.Id ?? 0,
                PeriodoAcademicoId = estructura.Periodos.FirstOrDefault()?.Id ?? 0,
                DocenteId = estructura.Docentes.FirstOrDefault()?.Id ?? 0
            },
            GrupoSeleccionadoId = estructura.Grupos.FirstOrDefault()?.Id ?? 0,
            NuevaInscripcion = new InscribirAlumnoRequest
            {
                AlumnoId = estructura.Alumnos.FirstOrDefault()?.Id ?? 0
            }
        };
    }

    private async Task CargarOpcionesTema(AltaTemaViewModel model, CancellationToken ct)
    {
        model.Cursos = (await _api.ObtenerCursosAsync(ct)).Datos ?? [];
        model.Temas = model.Formulario.CursoId > 0 ? (await _api.ObtenerTemasAsync(model.Formulario.CursoId, ct)).Datos ?? [] : [];
    }

    private async Task<List<TemaResponse>> ObtenerTodosLosTemas(CancellationToken ct)
    {
        var cursos = (await _api.ObtenerCursosAsync(ct)).Datos ?? [];
        var temas = new List<TemaResponse>();
        foreach (var curso in cursos)
            temas.AddRange((await _api.ObtenerTemasAsync(curso.Id, ct)).Datos ?? []);
        return temas;
    }
}
