using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CellStore.Filters;

public class RequiereLoginAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext ctx)
    {
        if (ctx.HttpContext.Session.GetString("rol") == null)
            ctx.Result = new JsonResult(new { error = "debes iniciar sesion" }) { StatusCode = 401 };
    }
    public void OnActionExecuted(ActionExecutedContext ctx) { }
}

public class RequiereAdminAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext ctx)
    {
        if (ctx.HttpContext.Session.GetString("rol") != "admin")
            ctx.Result = new JsonResult(new { error = "solo el administrador puede hacer esto" }) { StatusCode = 403 };
    }
    public void OnActionExecuted(ActionExecutedContext ctx) { }
}