using System.Diagnostics;
using EduCode.WebMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebMVC.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("AccessToken")))
            return RedirectToAction("Dashboard", "Alumno");
        return View();
    }

    public IActionResult AcercaDe() => View();
    public IActionResult AccesoDenegado() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
