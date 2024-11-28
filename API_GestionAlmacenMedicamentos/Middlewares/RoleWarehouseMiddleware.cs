using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API_GestionAlmacenMedicamentos.Middlewares
{
    public class RoleWarehouseMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleWarehouseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated ?? false)
            {
                var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(role) || (role != "0" && string.IsNullOrEmpty(context.User.FindFirst("WarehouseId")?.Value)))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Acceso denegado: no se puede determinar el rol o almacén");
                    return;
                }

                if (role != "0" && role != "1" && role != "2")
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Acceso denegado: rol no válido");
                    return;
                }
            }

            await _next(context);
        }

    }
}
