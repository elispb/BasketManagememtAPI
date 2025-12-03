using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BasketManagementAPI.Filters;

public sealed class KeyNotFoundExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is KeyNotFoundException notFoundException)
        {
            context.Result = new NotFoundObjectResult(new ProblemDetails
            {
                Title = "Resource not found",
                Detail = notFoundException.Message
            });

            context.ExceptionHandled = true;
        }
    }
}

