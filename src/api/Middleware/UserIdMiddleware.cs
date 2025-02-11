using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RequestValidationService _requestValidationService;

    public RequestValidationMiddleware(RequestDelegate next, RequestValidationService requestValidationService)
    {
        _next = next;
        _requestValidationService = requestValidationService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (await _requestValidationService.ValidateRequestAsync(context))
        {
            await _next(context);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid signature or missing required headers.");
        }
    }
}