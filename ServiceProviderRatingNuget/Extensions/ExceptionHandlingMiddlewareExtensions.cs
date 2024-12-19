using Microsoft.AspNetCore.Builder;
using ServiceProviderRatingNuget.MiddleWare;

namespace ServiceProviderRatingNuget.Extensions
{
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
