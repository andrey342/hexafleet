using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GtMotive.Estimate.Microservice.Api.Filters
{
    /// <summary>
    /// Adds the request path and the trace id to every <see cref="ProblemDetails"/> response, including the ones
    /// built by presenters and exception filters.
    /// </summary>
    public sealed class ProblemDetailsResultFilter : IAlwaysRunResultFilter
    {
        private const string TraceIdKey = "traceId";

        public void OnResultExecuting(ResultExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Result is not ObjectResult { Value: ProblemDetails problemDetails })
            {
                return;
            }

            problemDetails.Instance ??= context.HttpContext.Request.Path;
            problemDetails.Extensions.TryAdd(TraceIdKey, Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
