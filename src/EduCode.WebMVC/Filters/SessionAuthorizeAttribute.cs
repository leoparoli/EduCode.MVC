using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EduCode.WebMVC.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (string.IsNullOrWhiteSpace(context.HttpContext.Session.GetString("AccessToken")))
        {
            string retorno = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl = retorno });
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionRoleAttribute : SessionAuthorizeAttribute
{
    private readonly HashSet<string> _roles;
    public SessionRoleAttribute(params string[] roles) => _roles = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        if (context.Result is not null) return;
        string rol = context.HttpContext.Session.GetString("Rol") ?? string.Empty;
        if (!_roles.Contains(rol)) context.Result = new RedirectToActionResult("AccesoDenegado", "Home", null);
    }
}
